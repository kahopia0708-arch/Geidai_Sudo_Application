using Geidai.Common.Models;
using Geidai.Common.Results;

namespace Geidai.Services.Audio
{
    /// <summary>
    /// 録音/再生サービスの器（IF のみ / 実装は U3 Rec）。
    /// U1 では契約のみを確定し、以降のユニットが実装する。
    /// </summary>
    public interface IAudioService
    {
        Result StartRecording();
        Result<AudioBuffer> StopRecording();
        Result Play(AudioBuffer buffer);
        Result Stop();
    }
}
