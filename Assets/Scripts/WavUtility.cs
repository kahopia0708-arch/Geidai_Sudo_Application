using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class WavUtility
{
    // Save AudioClip to 16-bit PCM WAV
    public static void Save(string filePath, AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("保存する AudioClip がありません。");
            return;
        }

        int channels = clip.channels;
        int frequency = clip.frequency;
        int samplesPerChannel = clip.samples;
        float[] samples = new float[samplesPerChannel * channels];
        clip.GetData(samples, 0);

        byte[] wavData = ConvertAudioClipToWav(samples, channels, frequency);
        File.WriteAllBytes(filePath, wavData);

        Debug.Log("WAVファイルを保存しました: " + filePath);
    }

    // Load WAV file (supports standard PCM WAV with 'data' chunk)
    public static AudioClip Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning("WAVファイルが見つかりません: " + filePath);
            return null;
        }

        byte[] wavData = File.ReadAllBytes(filePath);
        return ConvertWavToAudioClip(wavData, Path.GetFileNameWithoutExtension(filePath));
    }

    private static byte[] ConvertAudioClipToWav(float[] samples, int channels, int sampleRate)
    {
        using (var ms = new MemoryStream())
        using (var bw = new BinaryWriter(ms, Encoding.ASCII))
        {
            int sampleCount = samples.Length;
            int byteRate = sampleRate * channels * 2; // 16 bit = 2 bytes
            int dataSize = sampleCount * 2;
            // RIFF chunk size = 36 + SubChunk2Size (dataSize)
            int riffChunkSize = 36 + dataSize;

            // RIFF header
            bw.Write(Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(riffChunkSize); // 4 bytes little-endian
            bw.Write(Encoding.ASCII.GetBytes("WAVE"));

            // fmt subchunk
            bw.Write(Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16);                    // Subchunk1Size for PCM
            bw.Write((short)1);              // AudioFormat = 1 (PCM)
            bw.Write((short)channels);       // NumChannels
            bw.Write(sampleRate);            // SampleRate
            bw.Write(byteRate);              // ByteRate
            bw.Write((short)(channels * 2)); // BlockAlign
            bw.Write((short)16);             // BitsPerSample

            // data subchunk
            bw.Write(Encoding.ASCII.GetBytes("data"));
            bw.Write(dataSize);

            // samples: float [-1,1] -> Int16
            for (int i = 0; i < sampleCount; i++)
            {
                float f = samples[i];
                // clamp and convert
                int intSample = Mathf.Clamp(Mathf.RoundToInt(f * 32767f), short.MinValue, short.MaxValue);
                bw.Write((short)intSample);
            }

            bw.Flush();
            return ms.ToArray();
        }
    }

    private static AudioClip ConvertWavToAudioClip(byte[] wavData, string clipName)
    {
        if (wavData == null || wavData.Length < 44)
        {
            Debug.LogWarning("WAVデータが正しくありません。");
            return null;
        }

        try
        {
            // read basic fmt info (offsets fixed for standard WAV)
            int channels = BitConverter.ToInt16(wavData, 22);
            int sampleRate = BitConverter.ToInt32(wavData, 24);

            // find "data" chunk (handles other chunks and padding)
            int pos = 12; // skip "RIFF" + size + "WAVE"
            int dataChunkPos = -1;
            int dataChunkSize = 0;

            while (pos + 8 <= wavData.Length)
            {
                string chunkId = Encoding.ASCII.GetString(wavData, pos, 4);
                int chunkSize = BitConverter.ToInt32(wavData, pos + 4);

                pos += 8; // move to chunk data start

                if (chunkId == "data")
                {
                    dataChunkPos = pos;
                    dataChunkSize = chunkSize;
                    break;
                }

                // advance to next chunk (account for padding if chunkSize is odd)
                pos += chunkSize;
                if (chunkSize % 2 == 1) pos++;
            }

            if (dataChunkPos < 0 || dataChunkPos + dataChunkSize > wavData.Length)
            {
                Debug.LogWarning("WAVデータ内に正しい data チャンクが見つかりません。");
                return null;
            }

            int sampleCount = dataChunkSize / 2; // 16-bit samples
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                int idx = dataChunkPos + i * 2;
                if (idx + 1 >= wavData.Length) break;
                short s = BitConverter.ToInt16(wavData, idx);
                samples[i] = s / 32768f;
            }

            int samplesPerChannel = sampleCount / channels;
            AudioClip clip = AudioClip.Create(clipName, samplesPerChannel, channels, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
        catch (Exception ex)
        {
            Debug.LogError("WAV読み込みでエラー: " + ex.Message);
            return null;
        }
    }
}