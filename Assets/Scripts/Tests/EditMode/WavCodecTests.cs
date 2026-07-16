using System;
using UnityEngine;
using NUnit.Framework;
using FsCheck;
using Geidai.Common.Audio;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// WavCodec のプロパティベーステスト（NFR-09）。
    /// Encode→Decode が 16bit 量子化誤差内でラウンドトリップすることを検証する。
    /// </summary>
    public class WavCodecTests
    {
        [Test]
        public void Encode_Then_Decode_RoundTrips_Within_16bit_Tolerance()
        {
            Prop.ForAll<short[]>(shorts =>
            {
                if (shorts == null) shorts = Array.Empty<short>();

                var src = new float[shorts.Length];
                for (int i = 0; i < shorts.Length; i++)
                {
                    float v = shorts[i] / 32767f;
                    if (v > 1f) v = 1f;
                    else if (v < -1f) v = -1f;
                    src[i] = v;
                }

                byte[] wav = WavCodec.Encode(src, 44100, 1);
                WavData decoded = WavCodec.Decode(wav);

                if (decoded.Samples.Length != src.Length) return false;
                for (int i = 0; i < src.Length; i++)
                {
                    if (Mathf.Abs(decoded.Samples[i] - src[i]) > 1e-4f) return false;
                }
                return true;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void Decoded_Format_Matches_Encoded()
        {
            Prop.ForAll<short[]>(shorts =>
            {
                if (shorts == null) shorts = Array.Empty<short>();
                var src = new float[shorts.Length];
                byte[] wav = WavCodec.Encode(src, 44100, 1);
                WavData decoded = WavCodec.Decode(wav);
                return decoded.SampleRate == 44100 && decoded.Channels == 1;
            }).QuickCheckThrowOnFailure();
        }
    }
}
