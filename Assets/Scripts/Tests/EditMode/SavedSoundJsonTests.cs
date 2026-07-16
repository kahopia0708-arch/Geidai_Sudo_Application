using System;
using NUnit.Framework;
using FsCheck;
using UnityEngine;
using Geidai.Common.Models;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// SavedSound / SoundClipMeta の JSON 往復と後方互換テスト（NFR-COL-T2 / U4）。
    /// JsonUtility での serialize↔deserialize が値を保持し、拡張フィールド欠損の旧 JSON も
    /// 既定値で安全に読めること（後方互換）を検証する。
    /// </summary>
    public class SavedSoundJsonTests
    {
        [Test]
        public void SavedSound_RoundTrips_Through_Json()
        {
            var meta = new SoundClipMeta
            {
                id = "abc123",
                displayName = "ひょうじ",
                createdAtIso = "2026-02-05T10:00:00.0000000Z",
                wavFileName = "abc123.wav",
                title = "とりのうた",
                photoFileName = "abc123.photo.jpg",
                memo = "こうえんで ろくおん",
                nickname = "たろう"
            };
            var settings = new SoundEffectSettingsData
            {
                pitchSemitones = 7,
                noiseLevel = NoiseLevel.Medium,
                timbre = TimbreType.Soft,
                reverb = 0.42f
            };
            var original = new SavedSound(meta, settings);

            string json = JsonUtility.ToJson(original);
            var back = JsonUtility.FromJson<SavedSound>(json);

            Assert.IsNotNull(back);
            Assert.AreEqual(meta.id, back.meta.id);
            Assert.AreEqual(meta.title, back.meta.title);
            Assert.AreEqual(meta.photoFileName, back.meta.photoFileName);
            Assert.AreEqual(meta.memo, back.meta.memo);
            Assert.AreEqual(meta.nickname, back.meta.nickname);
            Assert.AreEqual(settings.pitchSemitones, back.settings.pitchSemitones);
            Assert.AreEqual(settings.noiseLevel, back.settings.noiseLevel);
            Assert.AreEqual(settings.timbre, back.settings.timbre);
            Assert.AreEqual(settings.reverb, back.settings.reverb, 1e-4f);
        }

        [Test]
        public void Legacy_Json_Without_U4_Fields_Loads_With_Defaults()
        {
            // U1/U3 時代の meta（title/photoFileName/memo/nickname が無い）を模した JSON。
            string legacyJson =
                "{\"meta\":{\"id\":\"old1\",\"displayName\":\"きゅう\",\"createdAtIso\":\"2026-01-01T00:00:00.0000000Z\",\"wavFileName\":\"old1.wav\"}," +
                "\"settings\":{\"pitchSemitones\":0,\"noiseLevel\":0,\"timbre\":0,\"reverb\":0}}";

            var back = JsonUtility.FromJson<SavedSound>(legacyJson);

            Assert.IsNotNull(back);
            Assert.AreEqual("old1", back.meta.id);
            // 欠損フィールドは既定（空 or null）で読めること＝後方互換
            Assert.IsTrue(string.IsNullOrEmpty(back.meta.title));
            Assert.IsTrue(string.IsNullOrEmpty(back.meta.photoFileName));
            Assert.IsTrue(string.IsNullOrEmpty(back.meta.memo));
            Assert.IsTrue(string.IsNullOrEmpty(back.meta.nickname));
        }

        [Test]
        public void Meta_Fields_RoundTrip_PropertyBased()
        {
            Prop.ForAll<int>(seed =>
            {
                var meta = new SoundClipMeta
                {
                    id = "id" + seed,
                    createdAtIso = "2026-03-01T00:00:00.0000000Z",
                    wavFileName = "id" + seed + ".wav",
                    title = "t" + (seed % 7),
                    memo = "m" + (seed % 5),
                    nickname = "n" + (seed % 3)
                };
                var s = new SavedSound(meta, new SoundEffectSettingsData());

                string json = JsonUtility.ToJson(s);
                var back = JsonUtility.FromJson<SavedSound>(json);

                return back != null
                    && back.meta.id == meta.id
                    && back.meta.title == meta.title
                    && back.meta.memo == meta.memo
                    && back.meta.nickname == meta.nickname;
            }).QuickCheckThrowOnFailure();
        }
    }
}
