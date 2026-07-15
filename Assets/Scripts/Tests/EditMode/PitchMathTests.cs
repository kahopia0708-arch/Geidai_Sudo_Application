using System;
using NUnit.Framework;
using FsCheck;
using Geidai.Common.Audio;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// PitchMath のプロパティベーステスト（NFR-09）。
    /// cents ↔ ratio の逆変換と semitone→cents の線形性を検証する。
    /// </summary>
    public class PitchMathTests
    {
        [Test]
        public void CentsToRatio_RatioToCents_RoundTrips()
        {
            Prop.ForAll<int>(seed =>
            {
                double cents = seed % 12000; // ±約10オクターブに制限
                double back = PitchMath.RatioToCents(PitchMath.CentsToRatio(cents));
                return Math.Abs(back - cents) < 1e-6;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void SemitonesToCents_Is_Linear()
        {
            Prop.ForAll<int>(seed =>
            {
                int semitones = seed % 128;
                return Math.Abs(PitchMath.SemitonesToCents(semitones) - semitones * 100.0) < 1e-9;
            }).QuickCheckThrowOnFailure();
        }
    }
}
