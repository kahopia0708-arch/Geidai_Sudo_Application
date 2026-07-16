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
    /// StorageService.SaveSound の単体テスト（US-REC-03 / BR-REC-30）。
    /// wav＋meta の対生成、往復（保存→一覧/読込）、および入力検証を確認する。
    /// 生成物は TearDown で確実に削除する。
    /// </summary>
    public class SaveSoundTests
    {
        private IStorageService _storage;
        private readonly List<string> _createdIds = new List<string>();
        private string SoundsDir => Path.Combine(Application.persistentDataPath, "sounds");

        [SetUp]
        public void SetUp()
        {
            _storage = new StorageService();
            _createdIds.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var id in _createdIds)
            {
                TryDelete(Path.Combine(SoundsDir, id + ".wav"));
                TryDelete(Path.Combine(SoundsDir, id + ".meta.json"));
            }
        }

        private static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch { /* テスト後始末のため無視 */ }
        }

        private static AudioBuffer MakeBuffer(float value = 0.25f)
        {
            var buf = new AudioBuffer();
            for (int i = 0; i < 100; i++) buf.Samples[i] = value;
            return buf;
        }

        [Test]
        public void SaveSound_Creates_Wav_And_Meta_Pair()
        {
            var meta = SoundClipMeta.CreateNew("てすと");
            _createdIds.Add(meta.id);
            var sound = new SavedSound(meta, new SoundEffectSettingsData { pitchSemitones = 3, reverb = 0.5f });

            Result result = _storage.SaveSound(sound, MakeBuffer());

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.IsTrue(File.Exists(Path.Combine(SoundsDir, meta.wavFileName)), "wav が生成されること");
            Assert.IsTrue(File.Exists(Path.Combine(SoundsDir, meta.id + ".meta.json")), "meta が生成されること");
        }

        [Test]
        public void SaveSound_Then_Load_RoundTrips_Settings()
        {
            var meta = SoundClipMeta.CreateNew("まるまる");
            _createdIds.Add(meta.id);
            var settings = new SoundEffectSettingsData
            {
                pitchSemitones = -5,
                noiseLevel = NoiseLevel.High,
                timbre = TimbreType.Hard,
                reverb = 0.8f
            };
            var sound = new SavedSound(meta, settings);

            Assert.IsTrue(_storage.SaveSound(sound, MakeBuffer()).IsSuccess);

            Result<SavedSound> loaded = _storage.LoadSound(meta.id);
            Assert.IsTrue(loaded.IsSuccess, loaded.Message);
            Assert.AreEqual(-5, loaded.Value.settings.pitchSemitones);
            Assert.AreEqual(NoiseLevel.High, loaded.Value.settings.noiseLevel);
            Assert.AreEqual(TimbreType.Hard, loaded.Value.settings.timbre);
            Assert.AreEqual(0.8f, loaded.Value.settings.reverb, 1e-4f);
        }

        [Test]
        public void SaveSound_Appears_In_ListSounds()
        {
            var meta = SoundClipMeta.CreateNew("いちらん");
            _createdIds.Add(meta.id);
            var sound = new SavedSound(meta, new SoundEffectSettingsData());

            Assert.IsTrue(_storage.SaveSound(sound, MakeBuffer()).IsSuccess);

            Result<List<SavedSound>> list = _storage.ListSounds();
            Assert.IsTrue(list.IsSuccess);
            Assert.IsTrue(list.Value.Exists(s => s.meta != null && s.meta.id == meta.id), "保存音が一覧に含まれること");
        }

        [Test]
        public void SaveSound_Null_Buffer_Fails_Without_Writing()
        {
            var meta = SoundClipMeta.CreateNew("なし");
            var sound = new SavedSound(meta, new SoundEffectSettingsData());

            Result result = _storage.SaveSound(sound, null);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ResultCode.ValidationError, result.Code);
            Assert.IsFalse(File.Exists(Path.Combine(SoundsDir, meta.wavFileName)), "失敗時に wav を作らないこと");
        }

        [Test]
        public void SaveSound_Null_Sound_Fails()
        {
            Result result = _storage.SaveSound(null, MakeBuffer());
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ResultCode.ValidationError, result.Code);
        }
    }
}
