using System;
using Geidai.Common.Models;
using UnityEngine;

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
        /// <summary>切り出し末尾のフェード（リバーブ／ディレイの長い尾を急切りしない）。</summary>
        public const float FadeOutSeconds = 0.12f;

        public static float[] TrimForGameplay(
            float[] samples,
            int sampleRate = AudioBuffer.SampleRate,
            float amplitudeThreshold = DefaultAmplitudeThreshold,
            float maxSeconds = DefaultMaxGameplaySeconds,
            float fadeOutSeconds = FadeOutSeconds)
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
            {
                var head = CopyHead(samples, Math.Min(samples.Length, (int)(maxSeconds * sampleRate)));
                ApplyFadeOut(head, sampleRate, fadeOutSeconds);
                return head;
            }

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
            ApplyFadeOut(trimmed, sampleRate, fadeOutSeconds);
            return trimmed;
        }

        /// <summary>バッファ末尾をコサインカーブで 0 へ落とす。</summary>
        public static void ApplyFadeOut(float[] buffer, int sampleRate, float fadeOutSeconds = FadeOutSeconds)
        {
            if (buffer == null || buffer.Length == 0 || sampleRate <= 0 || fadeOutSeconds <= 0f)
                return;

            int fadeSamples = Math.Min(buffer.Length, Math.Max(1, (int)(fadeOutSeconds * sampleRate)));
            int fadeStart = buffer.Length - fadeSamples;
            if (fadeSamples == 1)
            {
                buffer[buffer.Length - 1] = 0f;
                return;
            }

            for (int i = 0; i < fadeSamples; i++)
            {
                float t = i / (float)(fadeSamples - 1);
                float gain = 0.5f * (1f + (float)Math.Cos(Math.PI * t));
                buffer[fadeStart + i] *= gain;
            }
        }

        private static float[] CopyHead(float[] samples, int count)
        {
            count = Math.Max(0, Math.Min(count, samples.Length));
            if (count == samples.Length) return samples;
            var head = new float[count];
            Array.Copy(samples, head, count);
            return head;
        }

        /// <summary>
        /// 図鑑 <see cref="AudioClip"/> のゲーム再生に相当する長さ（秒）。ファイル末尾の無音を除く。
        /// </summary>
        public static float EstimateGameplaySeconds(AudioClip clip)
        {
            if (clip == null) return 0.01f;

            int sampleRate = clip.frequency;
            if (sampleRate <= 0 || clip.samples <= 0)
                return Mathf.Clamp(clip.length, 0.01f, DefaultMaxGameplaySeconds);

            int channels = Math.Max(1, clip.channels);
            int interleavedLength = clip.samples * channels;
            var interleaved = new float[interleavedLength];
            if (!clip.GetData(interleaved, 0))
                return Mathf.Clamp(clip.length, 0.01f, DefaultMaxGameplaySeconds);

            float[] mono;
            if (channels == 1)
            {
                mono = interleaved;
            }
            else
            {
                mono = new float[clip.samples];
                for (int i = 0; i < clip.samples; i++)
                {
                    float peak = 0f;
                    int baseIndex = i * channels;
                    for (int c = 0; c < channels; c++)
                        peak = Math.Max(peak, Math.Abs(interleaved[baseIndex + c]));
                    mono[i] = peak;
                }
            }

            var trimmed = TrimForGameplay(mono, sampleRate);
            if (trimmed == null || trimmed.Length == 0)
                return Mathf.Clamp(clip.length, 0.01f, DefaultMaxGameplaySeconds);

            return Mathf.Clamp(trimmed.Length / (float)sampleRate, 0.01f, DefaultMaxGameplaySeconds);
        }
    }
}
