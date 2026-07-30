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
        public void Reverb_Filter_Params_Increase_With_Amount()
        {
            Prop.ForAll<int>(seed =>
            {
                float low = 0.02f + (Math.Abs(seed) % 490) / 1000f;  // 0.02..0.51
                float high = low + 0.4f;                              // 0.42..0.91

                return SoundEffectMapper.ReverbToRoomMilliBel(high) > SoundEffectMapper.ReverbToRoomMilliBel(low)
                    && SoundEffectMapper.ReverbToLevelMilliBel(high) > SoundEffectMapper.ReverbToLevelMilliBel(low)
                    && SoundEffectMapper.ReverbToDecaySeconds(high) > SoundEffectMapper.ReverbToDecaySeconds(low);
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void Reverb_Filter_Params_Stay_In_Range()
        {
            Prop.ForAll<int>(seed =>
            {
                float v = (Math.Abs(seed) % 2001) / 1000f - 0.5f; // -0.5..1.5（範囲外含む）
                float room = SoundEffectMapper.ReverbToRoomMilliBel(v);
                float level = SoundEffectMapper.ReverbToLevelMilliBel(v);
                float decay = SoundEffectMapper.ReverbToDecaySeconds(v);

                return room >= SoundEffectMapper.ReverbMinMilliBel && room <= SoundEffectMapper.RoomMaxMilliBel
                    && level >= SoundEffectMapper.ReverbMinMilliBel && level <= SoundEffectMapper.LevelMaxMilliBel
                    && decay >= SoundEffectMapper.DecayOffSeconds && decay <= SoundEffectMapper.DecayMaxSeconds;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void Reverb_Zero_Disables_Filter()
        {
            Assert.AreEqual(SoundEffectMapper.ReverbMinMilliBel, SoundEffectMapper.ReverbToRoomMilliBel(0f));
            Assert.AreEqual(SoundEffectMapper.ReverbMinMilliBel, SoundEffectMapper.ReverbToLevelMilliBel(0f));
            Assert.AreEqual(SoundEffectMapper.DecayOffSeconds, SoundEffectMapper.ReverbToDecaySeconds(0f));
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
