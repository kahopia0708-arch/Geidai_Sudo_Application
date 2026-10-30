using System;
using NUnit.Framework;
using Geidai.Common.Audio;
using Geidai.Common.Models;

namespace Geidai.Tests.EditMode
{
    public class GameAudioTrimTests
    {
        [Test]
        public void ApplyFadeOut_EndSampleApproachesZero()
        {
            var buffer = new float[4410];
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = 0.5f;

            GameAudioTrim.ApplyFadeOut(buffer, AudioBuffer.SampleRate, 0.1f);

            Assert.Less(Math.Abs(buffer[buffer.Length - 1]), 1e-4f);
            Assert.Greater(Math.Abs(buffer[0]), 0.4f);
        }

        [Test]
        public void TrimForGameplay_LongTail_EndsWithFade()
        {
            int rate = AudioBuffer.SampleRate;
            int len = rate * 3;
            var samples = new float[len];
            int attack = rate / 10;
            for (int i = attack; i < attack + 200; i++)
                samples[i] = 0.8f;
            for (int i = attack + 200; i < len; i++)
                samples[i] = 0.02f;

            var trimmed = GameAudioTrim.TrimForGameplay(
                samples,
                rate,
                amplitudeThreshold: 0.015f,
                maxSeconds: 0.5f,
                fadeOutSeconds: 0.1f);

            Assert.Greater(trimmed.Length, 0);
            Assert.Less(Math.Abs(trimmed[trimmed.Length - 1]), 1e-3f);
        }
    }
}
