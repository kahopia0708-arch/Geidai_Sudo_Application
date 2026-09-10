using System;
using UnityEngine;
using NUnit.Framework;
using FsCheck;
using Geidai.Common.Audio;
using Geidai.Common.Models;
using Geidai.Common.Utils;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// 設定/プロフィール JSON のプロパティベーステスト（NFR-09）。
    /// JsonUtility のシリアライズ→デシリアライズがラウンドトリップすることを検証する。
    /// </summary>
    public class SerializationTests
    {
        [Test]
        public void SoundEffectSettingsData_Json_RoundTrips()
        {
            Prop.ForAll<int>(seed =>
            {
                int pitch = PitchMath.Clamp((seed % 25) - 12, -12, 12);
                int noise = ((seed % 4) + 4) % 4;
                int timbre = ((seed % 3) + 3) % 3;
                float reverb = (Math.Abs(seed) % 1001) / 1000f;

                var s = new SoundEffectSettingsData
                {
                    pitchSemitones = pitch,
                    noiseLevel = (NoiseLevel)noise,
                    timbre = (TimbreType)timbre,
                    reverb = reverb
                };

                string json = JsonUtility.ToJson(s);
                var back = JsonUtility.FromJson<SoundEffectSettingsData>(json);

                return back != null
                    && back.pitchSemitones == s.pitchSemitones
                    && back.noiseLevel == s.noiseLevel
                    && back.timbre == s.timbre
                    && Mathf.Abs(back.reverb - s.reverb) < 1e-6f;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void UserProfile_Json_RoundTrips()
        {
            Prop.ForAll<int>(seed =>
            {
                int age = ValidationUtil.MinAge + (Math.Abs(seed) % (ValidationUtil.MaxAge - ValidationUtil.MinAge + 1));
                int year = DateTime.Now.Year - age;
                string nickname = "user" + (Math.Abs(seed) % 100000);

                var p = new UserProfile(year, nickname);
                string json = JsonUtility.ToJson(p);
                var back = JsonUtility.FromJson<UserProfile>(json);

                return back != null && back.birthYear == p.birthYear && back.nickname == p.nickname;
            }).QuickCheckThrowOnFailure();
        }
    }
}
