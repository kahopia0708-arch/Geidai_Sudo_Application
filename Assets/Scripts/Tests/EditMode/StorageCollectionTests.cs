using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Services.Storage;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// StorageService のコレクション系テスト（NFR-COL-R2/R3・US-COL-01/02 / U4）。
    /// 破損 meta / 対 wav 欠損のスキップ、DeleteSound の一括削除、SaveMeta の settings 保持を検証する。
    /// 各テストは一意 id を用い、TearDown で生成物を削除する。
    /// </summary>
    public class StorageCollectionTests
    {
        private IStorageService _storage;
        private readonly List<string> _ids = new List<string>();
        private string SoundsDir => Path.Combine(Application.persistentDataPath, "sounds");

        [SetUp]
        public void SetUp()
        {
            _storage = new StorageService();
            _ids.Clear();
            Directory.CreateDirectory(SoundsDir);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var id in _ids)
            {
                TryDelete(Path.Combine(SoundsDir, id + ".wav"));
                TryDelete(Path.Combine(SoundsDir, id + ".meta.json"));
                foreach (var ext in new[] { ".jpg", ".jpeg", ".png" })
                    TryDelete(Path.Combine(SoundsDir, id + ".photo" + ext));
            }
        }

        private static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch { /* ignore */ }
        }

        private static AudioBuffer MakeBuffer()
        {
            var buf = new AudioBuffer();
            for (int i = 0; i < 200; i++) buf.Samples[i] = 0.3f;
            return buf;
        }

        private string NewId(string tag)
        {
            string id = "uT_" + tag + "_" + System.Guid.NewGuid().ToString("N").Substring(0, 8);
            _ids.Add(id);
            return id;
        }

        private string SaveValidSound(string tag, SoundEffectSettingsData settings, string title = "")
        {
            string id = NewId(tag);
            var meta = new SoundClipMeta
            {
                id = id,
                createdAtIso = "2026-04-01T00:00:00.0000000Z",
                wavFileName = id + ".wav",
                title = title
            };
            var sound = new SavedSound(meta, settings);
            Assert.IsTrue(_storage.SaveSound(sound, MakeBuffer()).IsSuccess, "セットアップ保存に成功すること");
            return id;
        }

        // --- ListSounds skip ---

        [Test]
        public void ListSounds_Skips_Corrupted_Meta()
        {
            string id = NewId("corrupt");
            // 壊れた JSON の meta＋対 wav（ダミー）を書く。
            File.WriteAllText(Path.Combine(SoundsDir, id + ".meta.json"), "{ this is not valid json");
            File.WriteAllBytes(Path.Combine(SoundsDir, id + ".wav"), new byte[] { 0, 1, 2 });

            var list = _storage.ListSounds();
            Assert.IsTrue(list.IsSuccess);
            Assert.IsFalse(list.Value.Exists(s => s.meta != null && s.meta.id == id), "壊れた meta はスキップされること");
        }

        [Test]
        public void ListSounds_Skips_Meta_With_Missing_Wav()
        {
            string id = NewId("nowav");
            var meta = new SoundClipMeta
            {
                id = id,
                createdAtIso = "2026-04-02T00:00:00.0000000Z",
                wavFileName = id + ".wav" // wav は作らない
            };
            var sound = new SavedSound(meta, new SoundEffectSettingsData());
            File.WriteAllText(Path.Combine(SoundsDir, id + ".meta.json"), JsonUtility.ToJson(sound));

            var list = _storage.ListSounds();
            Assert.IsTrue(list.IsSuccess);
            Assert.IsFalse(list.Value.Exists(s => s.meta != null && s.meta.id == id), "対 wav 欠損はスキップされること");
        }

        [Test]
        public void ListSounds_Includes_Valid_Pair()
        {
            string id = SaveValidSound("valid", new SoundEffectSettingsData());
            var list = _storage.ListSounds();
            Assert.IsTrue(list.IsSuccess);
            Assert.IsTrue(list.Value.Exists(s => s.meta != null && s.meta.id == id), "有効な対は含まれること");
        }

        // --- DeleteSound ---

        [Test]
        public void DeleteSound_Removes_Wav_Meta_Photo()
        {
            string id = SaveValidSound("del", new SoundEffectSettingsData());
            string photoPath = Path.Combine(SoundsDir, id + ".photo.jpg");
            File.WriteAllBytes(photoPath, new byte[] { 9, 9, 9 });

            var result = _storage.DeleteSound(id);
            Assert.IsTrue(result.IsSuccess, result.Message);

            Assert.IsFalse(File.Exists(Path.Combine(SoundsDir, id + ".wav")), "wav が消えること");
            Assert.IsFalse(File.Exists(Path.Combine(SoundsDir, id + ".meta.json")), "meta が消えること");
            Assert.IsFalse(File.Exists(photoPath), "photo が消えること");
        }

        [Test]
        public void DeleteSound_Missing_Is_Success()
        {
            var result = _storage.DeleteSound("does_not_exist_xyz");
            Assert.IsTrue(result.IsSuccess, "存在しない削除は成功扱い（ベストエフォート）");
        }

        // --- SaveMeta ---

        [Test]
        public void SaveMeta_Preserves_Settings_And_Updates_Meta()
        {
            var settings = new SoundEffectSettingsData
            {
                pitchSemitones = -4,
                noiseLevel = NoiseLevel.High,
                timbre = TimbreType.Hard,
                reverb = 0.7f
            };
            string id = SaveValidSound("meta", settings, "まえ");

            var loaded = _storage.LoadSound(id);
            Assert.IsTrue(loaded.IsSuccess);
            var newMeta = loaded.Value.meta;
            newMeta.title = "あと";
            newMeta.memo = "へんしゅうした";

            var saveResult = _storage.SaveMeta(newMeta);
            Assert.IsTrue(saveResult.IsSuccess, saveResult.Message);

            var reloaded = _storage.LoadSound(id);
            Assert.IsTrue(reloaded.IsSuccess);
            Assert.AreEqual("あと", reloaded.Value.meta.title);
            Assert.AreEqual("へんしゅうした", reloaded.Value.meta.memo);
            // settings は保持されていること
            Assert.AreEqual(-4, reloaded.Value.settings.pitchSemitones);
            Assert.AreEqual(NoiseLevel.High, reloaded.Value.settings.noiseLevel);
            Assert.AreEqual(TimbreType.Hard, reloaded.Value.settings.timbre);
            Assert.AreEqual(0.7f, reloaded.Value.settings.reverb, 1e-4f);
        }

        [Test]
        public void SaveMeta_NotFound_When_No_Existing_Meta()
        {
            var meta = new SoundClipMeta { id = "ghost_meta_id", wavFileName = "ghost.wav" };
            var result = _storage.SaveMeta(meta);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ResultCode.NotFound, result.Code);
        }

        // --- LoadSoundBuffer ---

        [Test]
        public void LoadSoundBuffer_Returns_Decoded_Audio()
        {
            string id = SaveValidSound("buf", new SoundEffectSettingsData());
            var buf = _storage.LoadSoundBuffer(id);
            Assert.IsTrue(buf.IsSuccess, buf.Message);
            Assert.IsNotNull(buf.Value);
            Assert.IsNotNull(buf.Value.Samples);
            Assert.Greater(buf.Value.Samples.Length, 0);
        }
    }
}
