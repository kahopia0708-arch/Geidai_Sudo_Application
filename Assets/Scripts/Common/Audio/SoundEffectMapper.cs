using System;
using Geidai.Common.Models;

namespace Geidai.Common.Audio
{
    /// <summary>
    /// 加工設定の数値換算（nfr-design §5 / NFR-09）。純粋関数・副作用なしで PBT 対象。
    /// UI/内部の連続値 ↔ 正準モデル <see cref="SoundEffectSettingsData"/> の離散/正規化を集約する。
    /// ※旧グローバル SoundEffectSettings（Assembly-CSharp）との相互変換は含めない
    ///   （U3 は新形式のみ・旧データ移行は U4/対象外）。
    /// </summary>
    public static class SoundEffectMapper
    {
        public const int MinSemitones = -12;
        public const int MaxSemitones = 12;

        public const float ReverbMinMilliBel = -10000f; // 無し
        public const float ReverbMaxMilliBel = 0f;      // 最大

        /// <summary>セント → 半音（100 セント=1 半音・最寄り丸め・±12 クランプ）。</summary>
        public static int CentsToSemitones(double cents)
        {
            int semis = (int)Math.Round(cents / PitchMath.CentsPerSemitone, MidpointRounding.AwayFromZero);
            return PitchMath.Clamp(semis, MinSemitones, MaxSemitones);
        }

        /// <summary>半音 → セント。</summary>
        public static double SemitonesToCents(int semitones)
        {
            int clamped = PitchMath.Clamp(semitones, MinSemitones, MaxSemitones);
            return PitchMath.SemitonesToCents(clamped);
        }

        /// <summary>連続値(0〜1) → ノイズ低減 4 段（最寄り段へ離散化）。</summary>
        public static NoiseLevel ContinuousToNoiseLevel(float v01)
        {
            float v = Clamp01(v01);
            if (v < 1f / 6f) return NoiseLevel.None;
            if (v < 1f / 2f) return NoiseLevel.Low;
            if (v < 5f / 6f) return NoiseLevel.Medium;
            return NoiseLevel.High;
        }

        /// <summary>ノイズ低減 4 段 → 連続値(0〜1)。</summary>
        public static float NoiseLevelToContinuous(NoiseLevel level)
        {
            switch (level)
            {
                case NoiseLevel.None: return 0f;
                case NoiseLevel.Low: return 1f / 3f;
                case NoiseLevel.Medium: return 2f / 3f;
                case NoiseLevel.High: return 1f;
                default: return 0f;
            }
        }

        /// <summary>リバーブレベル(mB, -10000〜0) → 正規化(0〜1)。</summary>
        public static float NormalizeReverb(float milliBel)
        {
            float v = (milliBel - ReverbMinMilliBel) / (ReverbMaxMilliBel - ReverbMinMilliBel);
            return Clamp01(v);
        }

        /// <summary>正規化(0〜1) → リバーブレベル(mB, -10000〜0)。</summary>
        public static float DenormalizeReverb(float v01)
        {
            float v = Clamp01(v01);
            return ReverbMinMilliBel + v * (ReverbMaxMilliBel - ReverbMinMilliBel);
        }

        private static float Clamp01(float v)
        {
            if (v < 0f) return 0f;
            if (v > 1f) return 1f;
            return v;
        }
    }
}
