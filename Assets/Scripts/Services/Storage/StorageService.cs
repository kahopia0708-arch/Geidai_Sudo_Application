using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.Utils;

namespace Geidai.Services.Storage
{
    /// <summary>
    /// U1 最小の永続化実装（NFR-07 / nfr-design §1.2）。
    /// - profile.json の単純保存/読込（原子的置換は U4）。
    /// - sounds/{id}.wav と sounds/{id}.meta.json は対で扱い、破損/欠損はスキップ（BR-05）。
    /// - 失敗は Result（理由コード）で返し、クラッシュさせない。
    /// </summary>
    public class StorageService : IStorageService
    {
        private const string ProfileFileName = "profile.json";
        private const string SoundsDirName = "sounds";
        private const string MetaSuffix = ".meta.json";

        private static string Root => Application.persistentDataPath;
        private static string ProfilePath => Path.Combine(Root, ProfileFileName);
        private static string SoundsPath => Path.Combine(Root, SoundsDirName);

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

            try
            {
                Directory.CreateDirectory(Root);
                string json = JsonUtility.ToJson(profile);
                File.WriteAllText(ProfilePath, json); // U1: 単純保存。原子的置換は U4。
                return Result.Ok();
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] SaveProfile failed: " + e.Message);
                return Result.Fail(ResultCode.IOError, "保存に失敗しました。");
            }
        }

        public Result<List<SavedSound>> ListSounds()
        {
            var list = new List<SavedSound>();
            try
            {
                if (!Directory.Exists(SoundsPath))
                    return Result<List<SavedSound>>.Ok(list);

                var metaFiles = Directory.GetFiles(SoundsPath, "*" + MetaSuffix);
                foreach (var metaFile in metaFiles)
                {
                    try
                    {
                        string json = File.ReadAllText(metaFile);
                        var saved = JsonUtility.FromJson<SavedSound>(json);
                        if (saved == null || saved.meta == null)
                        {
                            SafeLogger.Warn("[Storage] corrupted meta skipped: " + Path.GetFileName(metaFile));
                            continue; // 破損メタはスキップ（BR-05）
                        }

                        string wavName = saved.meta.wavFileName ?? string.Empty;
                        string wavPath = Path.Combine(SoundsPath, wavName);
                        if (string.IsNullOrEmpty(wavName) || !File.Exists(wavPath))
                        {
                            SafeLogger.Warn("[Storage] paired wav missing, skip: " + Path.GetFileName(metaFile));
                            continue; // 対 wav 欠損はスキップ（BR-05）
                        }

                        list.Add(saved);
                    }
                    catch (Exception inner)
                    {
                        SafeLogger.Warn("[Storage] meta read failed, skip: " + Path.GetFileName(metaFile) + " " + inner.Message);
                    }
                }

                return Result<List<SavedSound>>.Ok(list);
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Storage] ListSounds failed: " + e.Message);
                return Result<List<SavedSound>>.Fail(ResultCode.IOError, "一覧の取得に失敗しました。");
            }
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
    }
}
