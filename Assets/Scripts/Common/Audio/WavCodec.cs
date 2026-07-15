using System;
using System.IO;
using System.Text;

namespace Geidai.Common.Audio
{
    /// <summary>デコード結果（サンプル＋フォーマット）。</summary>
    public struct WavData
    {
        public float[] Samples;
        public int SampleRate;
        public int Channels;
    }

    /// <summary>
    /// 16bit PCM WAV のエンコード/デコード（純粋関数 / NFR-09）。
    /// 副作用なし・入出力のみに依存。Encode→Decode は 16bit 量子化誤差内でラウンドトリップする。
    /// </summary>
    public static class WavCodec
    {
        private const int BitsPerSample = 16;
        private const float MaxAmplitude = 32767f;

        /// <summary>float[-1,1] のモノラル/多chサンプルを 16bit PCM WAV バイト列へ。</summary>
        public static byte[] Encode(float[] samples, int sampleRate = 44100, int channels = 1)
        {
            if (samples == null) samples = Array.Empty<float>();
            if (sampleRate <= 0) throw new ArgumentOutOfRangeException(nameof(sampleRate));
            if (channels <= 0) throw new ArgumentOutOfRangeException(nameof(channels));

            int byteRate = sampleRate * channels * (BitsPerSample / 8);
            int blockAlign = channels * (BitsPerSample / 8);
            int dataSize = samples.Length * (BitsPerSample / 8);

            using (var stream = new MemoryStream(44 + dataSize))
            using (var writer = new BinaryWriter(stream, Encoding.ASCII))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + dataSize);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));

                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);                       // fmt chunk size
                writer.Write((short)1);                 // PCM
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write((short)blockAlign);
                writer.Write((short)BitsPerSample);

                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(dataSize);

                for (int i = 0; i < samples.Length; i++)
                {
                    float clamped = samples[i];
                    if (clamped > 1f) clamped = 1f;
                    else if (clamped < -1f) clamped = -1f;
                    short s = (short)Math.Round(clamped * MaxAmplitude);
                    writer.Write(s);
                }

                writer.Flush();
                return stream.ToArray();
            }
        }

        /// <summary>16bit PCM WAV バイト列を float サンプルへ復元する。</summary>
        public static WavData Decode(byte[] wav)
        {
            if (wav == null) throw new ArgumentNullException(nameof(wav));
            if (wav.Length < 44) throw new InvalidDataException("WAV data too short.");

            int sampleRate = BitConverter.ToInt32(wav, 24);
            int channels = BitConverter.ToInt16(wav, 22);

            int pos = 12; // skip RIFF + size + WAVE
            int dataOffset = -1;
            int dataSize = 0;
            while (pos + 8 <= wav.Length)
            {
                string chunkId = Encoding.ASCII.GetString(wav, pos, 4);
                int chunkSize = BitConverter.ToInt32(wav, pos + 4);
                if (chunkId == "data")
                {
                    dataOffset = pos + 8;
                    dataSize = chunkSize;
                    break;
                }
                pos += 8 + chunkSize + (chunkSize & 1); // chunks are word-aligned
            }

            if (dataOffset < 0) throw new InvalidDataException("WAV data chunk not found.");
            if (dataOffset + dataSize > wav.Length) dataSize = wav.Length - dataOffset;

            int sampleCount = dataSize / 2;
            var samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                short s = BitConverter.ToInt16(wav, dataOffset + i * 2);
                samples[i] = s / MaxAmplitude;
            }

            return new WavData
            {
                Samples = samples,
                SampleRate = sampleRate <= 0 ? 44100 : sampleRate,
                Channels = channels <= 0 ? 1 : channels
            };
        }
    }
}
