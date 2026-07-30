using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Geidai.Common.Audio;
using Geidai.Common.Collection;
using Geidai.Common.Create;
using Geidai.Common.Library;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.Utils;
using Geidai.Services.IO;

namespace Geidai.Services.Storage
{
    /// <summary>
    /// ローカル永続化の本実装（U4 / nfr-design §1・§2・§6 + U7/U8）。
    /// - 全書込（profile/meta/wav/写真/UnlockState/Recipe）を <see cref="AtomicFile"/> の原子的置換へ統一（NFR-COL-R1）。
    /// - sounds/{id}.wav と sounds/{id}.meta.json は対で扱い、破損/欠損はスキップ（NFR-COL-R2）。
    /// - 空/ディレクトリ無しは空リストへフォールバック（NFR-COL-R3）。
    /// - 失敗は Result（理由コード）で返し、クラッシュさせない（SECURITY-15）。ログに PII を出さない。
    /// </summary>
    public class StorageService : IStorageService
    {
        private const string ProfileFileName = "profile.json";
        private const string SoundsDirName = "sounds";
        private const string MetaSuffix = ".meta.json";
        private const string PhotoPrefix = ".photo"; // {id}.photo.<ext>
        private const string ProgressionDirName = "progression";
        private const string UnlockStateFileName = "unlock-state.json";
        private const string RecipesDirName = "recipes";
        private const string ExportsDirName = "exports";

        private static string Root => Application.persistentDataPath;
        private static string ProfilePath => Path.Combine(Root, ProfileFileName);
        private static string SoundsPath => Path.Combine(Root, SoundsDirName);
        private static string ProgressionPath => Path.Combine(Root, ProgressionDirName);
        private static string UnlockStatePath => Path.Combine(ProgressionPath, UnlockStateFileName);
        private static string RecipesPath => Path.Combine(Root, RecipesDirName);
        private static string ExportsPath => Path.Combine(Root, ExportsDirName);

        // ---------------------------------------------------------------- Profile

        public Result<UserProfile> LoadProfile()
        {
            try
            {
                if (!File.Exists(ProfilePath))
                    return Result<UserProfile>.Fail(ResultCode.NotFound, "プロフィールが未登録です。");

                string json = File.ReadAllText(ProfilePath);
                var profile = JsonUtility.FromJson<UserProfile>(json);
                if (profile == null)
                    return Result<UserProfile>.Fail(ResultCode.Corrupted, "プロフィールが壊れています。");

                return Result<UserProfile>.Ok(profile);
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] LoadProfile failed: " + e.Message);
                return Result<UserProfile>.Fail(ResultCode.IOError, "読み込みに失敗しました。");
            }
        }

        public Result SaveProfile(UserProfile profile)
        {
            if (profile == null)
                return Result.Fail(ResultCode.ValidationError, "プロフィールが空です。");

            // U4: 原子的置換（一時ファイル→置換）で既存を壊さない。
            string json = JsonUtility.ToJson(profile);
            return AtomicFile.WriteAllTextAtomic(ProfilePath, json);
        }

        // ---------------------------------------------------------------- List / Load

        public Result<List<SavedSound>> ListSounds()
        {
            var outcome = ListSoundsDetailed();
            return Result<List<SavedSound>>.Ok(outcome.items);
        }

        /// <summary>
        /// 破損/対 wav 欠損を安全にスキップしつつ有効項目と読み飛ばし件数を返す（NFR-COL-R2/R3）。
        /// どの段階の例外も最悪は空リストへフォールバックする。
        /// </summary>
        public LoadOutcome ListSoundsDetailed()
        {
            var outcome = new LoadOutcome();
            try
            {
                if (!Directory.Exists(SoundsPath))
                    return outcome; // 空フォールバック

                var metaFiles = Directory.GetFiles(SoundsPath, "*" + MetaSuffix);
                foreach (var metaFile in metaFiles)
                {
                    try
                    {
                        string json = File.ReadAllText(metaFile);
                        var saved = JsonUtility.FromJson<SavedSound>(json);
                        if (saved == null || saved.meta == null)
                        {
                            outcome.skippedCount++;
                            SafeLogger.Warn("[Storage] corrupted meta skipped: " + Path.GetFileName(metaFile));
                            continue;
                        }

                        string wavName = saved.meta.wavFileName ?? string.Empty;
                        string wavPath = Path.Combine(SoundsPath, wavName);
                        if (string.IsNullOrEmpty(wavName) || !File.Exists(wavPath))
                        {
                            outcome.skippedCount++;
                            SafeLogger.Warn("[Storage] paired wav missing, skip: " + Path.GetFileName(metaFile));
                            continue;
                        }

                        outcome.items.Add(saved);
                    }
                    catch (Exception inner)
                    {
                        outcome.skippedCount++;
                        SafeLogger.Warn("[Storage] meta read failed, skip: " + Path.GetFileName(metaFile) + " " + inner.Message);
                    }
                }
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] ListSounds failed: " + e.Message);
                // 最悪でも空フォールバック（クラッシュしない）。
            }
            return outcome;
        }

        public Result<SavedSound> LoadSound(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Result<SavedSound>.Fail(ResultCode.ValidationError, "id が空です。");

            try
            {
                string metaPath = Path.Combine(SoundsPath, id + MetaSuffix);
                if (!File.Exists(metaPath))
                    return Result<SavedSound>.Fail(ResultCode.NotFound, "見つかりませんでした。");

                string json = File.ReadAllText(metaPath);
                var saved = JsonUtility.FromJson<SavedSound>(json);
                if (saved == null || saved.meta == null)
                    return Result<SavedSound>.Fail(ResultCode.Corrupted, "データが壊れています。");

                return Result<SavedSound>.Ok(saved);
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] LoadSound failed: " + e.Message);
                return Result<SavedSound>.Fail(ResultCode.IOError, "読み込みに失敗しました。");
            }
        }

        // ---------------------------------------------------------------- Save (pair)

        public Result SaveSound(SavedSound sound, AudioBuffer buffer)
        {
            if (sound == null || sound.meta == null || sound.settings == null)
                return Result.Fail(ResultCode.ValidationError, "保存データが空です。");
            if (buffer == null || buffer.Samples == null)
                return Result.Fail(ResultCode.ValidationError, "録音データが空です。");
            if (string.IsNullOrEmpty(sound.meta.id))
                return Result.Fail(ResultCode.ValidationError, "id が空です。");

            string wavName = string.IsNullOrEmpty(sound.meta.wavFileName)
                ? sound.meta.id + ".wav"
                : sound.meta.wavFileName;
            string wavPath = Path.Combine(SoundsPath, wavName);
            string metaPath = Path.Combine(SoundsPath, sound.meta.id + MetaSuffix);

            bool wavWritten = File.Exists(wavPath);
            try
            {
                Directory.CreateDirectory(SoundsPath);

                // wav → meta の順に原子的置換で書き込む（対整合 / NFR-COL-R4）。
                byte[] wav = WavCodec.Encode(buffer.Samples, AudioBuffer.SampleRate, AudioBuffer.Channels);
                var wavResult = AtomicFile.WriteAllBytesAtomic(wavPath, wav);
                if (!wavResult.IsSuccess)
                    return wavResult;
                wavWritten = true;

                string json = JsonUtility.ToJson(sound);
                var metaResult = AtomicFile.WriteAllTextAtomic(metaPath, json);
                if (!metaResult.IsSuccess)
                {
                    // meta 失敗時は今回書いた wav を残さない（新規時のみ削除・既存更新は保持）。
                    if (!File.Exists(metaPath) && wavWritten)
                        TryDelete(wavPath);
                    return metaResult;
                }

                return Result.Ok();
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] SaveSound failed: " + e.Message);
                return Result.Fail(ResultCode.IOError, "保存に失敗しました。");
            }
        }

        // ---------------------------------------------------------------- Meta (edit)

        public Result SaveMeta(SoundClipMeta meta)
        {
            if (meta == null || string.IsNullOrEmpty(meta.id))
                return Result.Fail(ResultCode.ValidationError, "メタが空です。");

            try
            {
                string metaPath = Path.Combine(SoundsPath, meta.id + MetaSuffix);
                if (!File.Exists(metaPath))
                    return Result.Fail(ResultCode.NotFound, "対象が見つかりませんでした。");

                // 既存の settings を保持したまま meta を差し替える。
                string existingJson = File.ReadAllText(metaPath);
                var saved = JsonUtility.FromJson<SavedSound>(existingJson);
                if (saved == null)
                    return Result.Fail(ResultCode.Corrupted, "データが壊れています。");
                if (saved.settings == null)
                    saved.settings = new SoundEffectSettingsData();

                saved.meta = meta;

                string json = JsonUtility.ToJson(saved);
                return AtomicFile.WriteAllTextAtomic(metaPath, json);
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] SaveMeta failed: " + e.Message);
                return Result.Fail(ResultCode.IOError, "保存に失敗しました。");
            }
        }

        // ---------------------------------------------------------------- Photo

        public Result<string> SavePhoto(string id, string sourceTempPath)
        {
            if (string.IsNullOrEmpty(id))
                return Result<string>.Fail(ResultCode.ValidationError, "id が空です。");
            if (string.IsNullOrEmpty(sourceTempPath) || !File.Exists(sourceTempPath))
                return Result<string>.Fail(ResultCode.NotFound, "写真が見つかりませんでした。");

            try
            {
                Directory.CreateDirectory(SoundsPath);

                string ext = Path.GetExtension(sourceTempPath);
                if (string.IsNullOrEmpty(ext)) ext = ".jpg";
                ext = ext.ToLowerInvariant();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                    return Result<string>.Fail(ResultCode.ValidationError, "たいおうしていない しゃしんです。");

                // 既存の異なる拡張子の写真は掃除してから書く（1音1写真）。
                RemovePhotoFiles(id);

                string photoName = id + PhotoPrefix + ext;
                string destPath = Path.Combine(SoundsPath, photoName);
                var copyResult = AtomicFile.CopyAtomic(sourceTempPath, destPath);
                if (!copyResult.IsSuccess)
                    return Result<string>.Fail(copyResult.Code, copyResult.Message);

                return Result<string>.Ok(photoName);
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] SavePhoto failed: " + e.Message);
                return Result<string>.Fail(ResultCode.IOError, "しゃしんの ほぞんに しっぱいしました。");
            }
        }

        public Result RemovePhoto(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Result.Fail(ResultCode.ValidationError, "id が空です。");

            try
            {
                RemovePhotoFiles(id);
                return Result.Ok();
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] RemovePhoto failed: " + e.Message);
                return Result.Fail(ResultCode.IOError, "しゃしんの さくじょに しっぱいしました。");
            }
        }

        public Result<byte[]> LoadPhoto(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Result<byte[]>.Fail(ResultCode.ValidationError, "id が空です。");

            try
            {
                if (!Directory.Exists(SoundsPath))
                    return Result<byte[]>.Fail(ResultCode.NotFound, "しゃしんが ないよ");

                var photos = Directory.GetFiles(SoundsPath, id + PhotoPrefix + ".*");
                if (photos.Length == 0)
                    return Result<byte[]>.Fail(ResultCode.NotFound, "しゃしんが ないよ");

                byte[] bytes = File.ReadAllBytes(photos[0]);
                return Result<byte[]>.Ok(bytes);
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] LoadPhoto failed: " + e.Message);
                return Result<byte[]>.Fail(ResultCode.IOError, "しゃしんを よみこめなかったよ");
            }
        }

        public Result<AudioBuffer> LoadSoundBuffer(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Result<AudioBuffer>.Fail(ResultCode.ValidationError, "id が空です。");

            try
            {
                // meta 記載の wav 名（無ければ既定 {id}.wav）を解決。
                string wavName = id + ".wav";
                string metaPath = Path.Combine(SoundsPath, id + MetaSuffix);
                if (File.Exists(metaPath))
                {
                    try
                    {
                        var saved = JsonUtility.FromJson<SavedSound>(File.ReadAllText(metaPath));
                        if (saved != null && saved.meta != null && !string.IsNullOrEmpty(saved.meta.wavFileName))
                            wavName = saved.meta.wavFileName;
                    }
                    catch (Exception inner)
                    {
                        SafeLogger.Warn("[Storage] LoadSoundBuffer: meta read failed (use default wav): " + inner.Message);
                    }
                }

                string wavPath = Path.Combine(SoundsPath, wavName);
                if (!File.Exists(wavPath))
                    return Result<AudioBuffer>.Fail(ResultCode.NotFound, "おとが みつからないよ");

                byte[] wav = File.ReadAllBytes(wavPath);
                var decoded = WavCodec.Decode(wav);
                return Result<AudioBuffer>.Ok(new AudioBuffer(decoded.Samples));
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] LoadSoundBuffer failed: " + e.Message);
                return Result<AudioBuffer>.Fail(ResultCode.Corrupted, "おとを よみこめなかったよ");
            }
        }

        // ---------------------------------------------------------------- Delete

        public Result DeleteSound(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Result.Fail(ResultCode.ValidationError, "id が空です。");

            try
            {
                if (!Directory.Exists(SoundsPath))
                    return Result.Ok(); // 何も無ければ成功扱い

                // wav（meta 記載名 or 既定名）を削除。
                string metaPath = Path.Combine(SoundsPath, id + MetaSuffix);
                string wavName = id + ".wav";
                if (File.Exists(metaPath))
                {
                    try
                    {
                        var saved = JsonUtility.FromJson<SavedSound>(File.ReadAllText(metaPath));
                        if (saved != null && saved.meta != null && !string.IsNullOrEmpty(saved.meta.wavFileName))
                            wavName = saved.meta.wavFileName;
                    }
                    catch (Exception inner)
                    {
                        SafeLogger.Warn("[Storage] delete: meta read failed (use default wav name): " + inner.Message);
                    }
                }

                TryDelete(Path.Combine(SoundsPath, wavName));
                TryDelete(metaPath);
                RemovePhotoFiles(id);

                return Result.Ok();
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] DeleteSound failed: " + e.Message);
                return Result.Fail(ResultCode.IOError, "さくじょに しっぱいしました。");
            }
        }

        // ---------------------------------------------------------------- UnlockState (U7)

        public Result<UnlockState> LoadUnlockState()
        {
            try
            {
                if (!File.Exists(UnlockStatePath))
                    return Result<UnlockState>.Ok(UnlockState.Empty());

                string json = File.ReadAllText(UnlockStatePath);
                var state = JsonUtility.FromJson<UnlockState>(json);
                if (state == null)
                {
                    SafeLogger.Warn("[Storage] unlock-state corrupted; empty fallback");
                    return Result<UnlockState>.Ok(UnlockState.Empty());
                }
                if (state.unlockedIds == null) state.unlockedIds = Array.Empty<string>();
                if (state.achievedGameKeys == null) state.achievedGameKeys = Array.Empty<string>();
                if (state.achievedRecordingKeys == null) state.achievedRecordingKeys = Array.Empty<string>();
                return Result<UnlockState>.Ok(state);
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] LoadUnlockState failed: " + e.Message);
                return Result<UnlockState>.Ok(UnlockState.Empty());
            }
        }

        public Result SaveUnlockState(UnlockState state)
        {
            if (state == null)
                return Result.Fail(ResultCode.ValidationError, "じょうたいが ないよ");
            try
            {
                Directory.CreateDirectory(ProgressionPath);
                string json = JsonUtility.ToJson(state);
                return AtomicFile.WriteAllTextAtomic(UnlockStatePath, json);
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] SaveUnlockState failed: " + e.Message);
                return Result.Fail(ResultCode.IOError, "ほぞんに しっぱいしたよ");
            }
        }

        // ---------------------------------------------------------------- Recipes (U8)

        public Result SaveRecipe(SoundRecipe recipe)
        {
            if (recipe == null || string.IsNullOrWhiteSpace(recipe.id))
                return Result.Fail(ResultCode.ValidationError, "レシピが ないよ");

            var clamped = RecipeValidator.Clamp(recipe);
            try
            {
                Directory.CreateDirectory(RecipesPath);
                string path = Path.Combine(RecipesPath, clamped.id + ".json");
                string json = JsonUtility.ToJson(clamped);
                return AtomicFile.WriteAllTextAtomic(path, json);
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] SaveRecipe failed: " + e.Message);
                return Result.Fail(ResultCode.IOError, "ほぞんに しっぱいしたよ");
            }
        }

        public Result DeleteRecipe(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Result.Fail(ResultCode.ValidationError, "IDが ないよ");
            try
            {
                string path = Path.Combine(RecipesPath, id + ".json");
                TryDelete(path);
                string export = Path.Combine(ExportsPath, id + ".wav");
                TryDelete(export);
                return Result.Ok();
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] DeleteRecipe failed: " + e.Message);
                return Result.Fail(ResultCode.IOError, "さくじょに しっぱいしたよ");
            }
        }

        public Result<List<SoundRecipe>> ListRecipes()
        {
            var list = new List<SoundRecipe>();
            try
            {
                if (!Directory.Exists(RecipesPath))
                    return Result<List<SoundRecipe>>.Ok(list);

                foreach (var file in Directory.GetFiles(RecipesPath, "*.json"))
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var recipe = JsonUtility.FromJson<SoundRecipe>(json);
                        if (recipe == null || string.IsNullOrEmpty(recipe.id))
                        {
                            SafeLogger.Warn("[Storage] recipe skipped: " + Path.GetFileName(file));
                            continue;
                        }
                        list.Add(recipe);
                    }
                    catch (Exception inner)
                    {
                        SafeLogger.Warn("[Storage] recipe read failed: " + Path.GetFileName(file) + " " + inner.Message);
                    }
                }
                return Result<List<SoundRecipe>>.Ok(list);
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] ListRecipes failed: " + e.Message);
                return Result<List<SoundRecipe>>.Ok(list);
            }
        }

        public Result<SoundRecipe> LoadRecipe(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Result<SoundRecipe>.Fail(ResultCode.ValidationError, "IDが ないよ");
            try
            {
                string path = Path.Combine(RecipesPath, id + ".json");
                if (!File.Exists(path))
                    return Result<SoundRecipe>.Fail(ResultCode.NotFound, "レシピが みつからないよ");
                var recipe = JsonUtility.FromJson<SoundRecipe>(File.ReadAllText(path));
                if (recipe == null)
                    return Result<SoundRecipe>.Fail(ResultCode.Corrupted, "レシピが こわれてるよ");
                return Result<SoundRecipe>.Ok(recipe);
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] LoadRecipe failed: " + e.Message);
                return Result<SoundRecipe>.Fail(ResultCode.IOError, "よめなかったよ");
            }
        }

        public Result SaveRecipeExport(string id, byte[] wavBytes)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Result.Fail(ResultCode.ValidationError, "IDが ないよ");
            if (wavBytes == null || wavBytes.Length == 0)
                return Result.Fail(ResultCode.ValidationError, "データが ないよ");
            try
            {
                Directory.CreateDirectory(ExportsPath);
                string path = Path.Combine(ExportsPath, id + ".wav");
                return AtomicFile.WriteAllBytesAtomic(path, wavBytes);
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] SaveRecipeExport failed: " + e.Message);
                return Result.Fail(ResultCode.IOError, "かきだしに しっぱいしたよ");
            }
        }

        // ---------------------------------------------------------------- helpers

        private static void RemovePhotoFiles(string id)
        {
            if (!Directory.Exists(SoundsPath)) return;
            var photos = Directory.GetFiles(SoundsPath, id + PhotoPrefix + ".*");
            foreach (var p in photos)
                TryDelete(p);
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception e)
            {
                SafeLogger.Warn("[Storage] delete failed: " + Path.GetFileName(path) + " " + e.Message);
            }
        }
    }
}
