using System;
using Geidai.Common.Models;

namespace Geidai.Common.Audio
{
    /// <summary>
    /// 長い WAV の無音・余韻を除き、ゲーム再生用に短くする。
    /// </summary>
    public static class GameAudioTrim
    {
        public const float DefaultAmplitudeThreshold = 0.012f;
        public const float PaddingBeforeSeconds = 0.03f;
        public const float PaddingAfterSeconds = 0.12f;
        public const float DefaultMaxGameplaySeconds = 1.6f;

        public static float[] TrimForGameplay(
            float[] samples,
            int sampleRate = AudioBuffer.SampleRate,
            float amplitudeThreshold = DefaultAmplitudeThreshold,
            float maxSeconds = DefaultMaxGameplaySeconds)
        {
            if (samples == null || samples.Length == 0 || sampleRate <= 0)
                return samples ?? Array.Empty<float>();

            int start = 0;
            int end = samples.Length - 1;
            for (int i = 0; i < samples.Length; i++)
            {
                if (Math.Abs(samples[i]) >= amplitudeThreshold)
                {
                    start = i;
                    break;
                }
            }

            for (int i = samples.Length - 1; i >= 0; i--)
            {
                if (Math.Abs(samples[i]) >= amplitudeThreshold)
                {
                    end = i;
                    break;
                }
            }

            if (end < start)
                return CopyHead(samples, Math.Min(samples.Length, (int)(maxSeconds * sampleRate)));

            int padBefore = (int)(PaddingBeforeSeconds * sampleRate);
            int padAfter = (int)(PaddingAfterSeconds * sampleRate);
            start = Math.Max(0, start - padBefore);
            end = Math.Min(samples.Length - 1, end + padAfter);

            int length = end - start + 1;
            int maxSamples = Math.Max(1, (int)(maxSeconds * sampleRate));
            if (length > maxSamples)
                length = maxSamples;

            var trimmed = new float[length];
            Array.Copy(samples, start, trimmed, 0, length);
            return trimmed;
        }

        private static float[] CopyHead(float[] samples, int count)
        {
            count = Math.Max(0, Math.Min(count, samples.Length));
            if (count == samples.Length) return samples;
            var head = new float[count];
            Array.Copy(samples, head, count);
            return head;
        }
    }
}
