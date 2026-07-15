namespace Geidai.Common.Models
{
    /// <summary>
    /// 録音バッファ（NFR-03）。44100Hz / モノラル / 16bit / 3秒固定 = 132300 サンプル。
    /// GC 削減のためサンプル配列は再利用可能（NFR-06 / nfr-design §2）。
    /// </summary>
    public class AudioBuffer
    {
        public const int SampleRate = 44100;
        public const int Channels = 1;
        public const float DurationSeconds = 3f;
        public const int SampleCount = 132300; // 44100 * 3

        public float[] Samples;

        public AudioBuffer()
        {
            Samples = new float[SampleCount];
        }

        public AudioBuffer(float[] samples)
        {
            Samples = samples;
        }
    }
}
