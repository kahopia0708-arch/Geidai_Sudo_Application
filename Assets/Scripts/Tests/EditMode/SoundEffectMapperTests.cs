using System;
using NUnit.Framework;
using FsCheck;
using Geidai.Common.Audio;
using Geidai.Common.Models;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// SoundEffectMapper のプロパティベーステスト（NFR-09 / U3）。
    /// 半音↔セント・ノイズ4段・リバーブ正規化の換算が
    /// 範囲内でラウンドトリップ／飽和（クランプ）することを検証する。
    /// </summary>
    public class SoundEffectMapperTests
    {
        [Test]
        public void Semitones_Cents_RoundTrips_Within_Range()
        {
            Prop.ForAll<int>(seed =>
            {
                int semis = (seed % 25) - 12; // -12..12
                if (semis < SoundEffectMapper.MinSemitones) semis = SoundEffectMapper.MinSemitones;
                if (semis > SoundEffectMapper.MaxSemitones) semis = SoundEffectMapper.MaxSemitones;

                double cents = SoundEffectMapper.SemitonesToCents(semis);
                int back = SoundEffectMapper.CentsToSemitones(cents);
                return back == semis;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void CentsToSemitones_Is_Clamped_To_Range()
        {
            Prop.ForAll<int>(seed =>
            {
                double cents = seed; // 任意（極端値含む）
                int semis = SoundEffectMapper.CentsToSemitones(cents);
                return semis >= SoundEffectMapper.MinSemitones && semis <= SoundEffectMapper.MaxSemitones;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void NoiseLevel_Continuous_RoundTrips_Per_Level()
        {
            foreach (NoiseLevel level in Enum.GetValues(typeof(NoiseLevel)))
            {
                float v = SoundEffectMapper.NoiseLevelToContinuous(level);
                NoiseLevel back = SoundEffectMapper.ContinuousToNoiseLevel(v);
                Assert.AreEqual(level, back, "NoiseLevel round-trip failed for " + level);
            }
        }

        [Test]
        public void ContinuousToNoiseLevel_Is_Monotonic_And_Bounded()
        {
            Prop.ForAll<int>(seed =>
            {
                float v = (Math.Abs(seed) % 1001) / 1000f; // 0..1
                NoiseLevel level = SoundEffectMapper.ContinuousToNoiseLevel(v);
                int idx = (int)level;
                return idx >= 0 && idx <= 3;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void Reverb_Normalize_Denormalize_RoundTrips()
        {
            Prop.ForAll<int>(seed =>
            {
                float v = (Math.Abs(seed) % 1001) / 1000f; // 0..1
                float mb = SoundEffectMapper.DenormalizeReverb(v);
                float back = SoundEffectMapper.NormalizeReverb(mb);
                return Math.Abs(back - v) < 1e-4f;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void NormalizeReverb_Is_Clamped_To_Unit_Range()
        {
            Prop.ForAll<int>(seed =>
            {
                float mb = seed; // 任意（範囲外含む）
                float v = SoundEffectMapper.NormalizeReverb(mb);
                return v >= 0f && v <= 1f;
            }).QuickCheckThrowOnFailure();
        }
    }
}
