using System;

namespace Geidai.Common.Audio
{
    /// <summary>
    /// ピッチ関連の純粋関数（NFR-09）。cents ↔ ratio ↔ semitone の変換。
    /// 副作用なし。RatioToCents(CentsToRatio(c)) は数値誤差内で c に戻る。
    /// </summary>
    public static class PitchMath
    {
        public const double CentsPerOctave = 1200.0;
        public const double CentsPerSemitone = 100.0;

        /// <summary>半音 → セント。</summary>
        public static double SemitonesToCents(double semitones)
        {
            return semitones * CentsPerSemitone;
        }

        /// <summary>セント → 半音。</summary>
        public static double CentsToSemitones(double cents)
        {
            return cents / CentsPerSemitone;
        }

        /// <summary>セント → 周波数比。</summary>
        public static double CentsToRatio(double cents)
        {
            return Math.Pow(2.0, cents / CentsPerOctave);
        }

        /// <summary>周波数比 → セント。ratio は正の値である必要がある。</summary>
        public static double RatioToCents(double ratio)
        {
            if (ratio <= 0.0) throw new ArgumentOutOfRangeException(nameof(ratio), "ratio must be > 0");
            return CentsPerOctave * Math.Log(ratio, 2.0);
        }

        /// <summary>半音 → 周波数比。</summary>
        public static double SemitonesToRatio(double semitones)
        {
            return CentsToRatio(SemitonesToCents(semitones));
        }

        /// <summary>値を [min, max] にクランプする。</summary>
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
