using System;
using UnityEngine;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.Utils;
using Geidai.Services.Audio;

namespace Geidai.Rec
{
    /// <summary>
    /// 録音/再生サービスの本実装（U3 / IAudioService）。
    /// 録音は Unity 標準 <see cref="Microphone"/>（3秒・44100・モノラル）。
    /// 停止時に固定長 <see cref="AudioBuffer"/>（再利用）へコピーして GC を抑制する（nfr-design §2）。
    /// 再生は外部注入の AudioSource（EffectChain 共有）を用いる（非破壊・加工は再生系で適用）。
    /// すべての失敗は <see cref="Result"/> で表現しクラッシュさせない（SECURITY-15）。
    /// </summary>
    public class RecAudioService : IAudioService
    {
        // Microphone の録音長（自動停止は 3秒だが、ゆとりを持って確保）
        private const int RecordSeconds = 4;

        private readonly AudioBuffer _buffer = new AudioBuffer(); // 再利用（132300 サンプル）
        private AudioSource _playbackSource;
        private AudioClip _recordingClip;
        private AudioClip _playbackClip;
        private string _device;

        /// <summary>再生に用いる AudioSource を設定する（EffectChain と共有）。</summary>
        public void SetPlaybackSource(AudioSource source)
        {
            _playbackSource = source;
        }

        public Result StartRecording()
        {
            try
            {
                if (Microphone.devices == null || Microphone.devices.Length == 0)
                    return Result.Fail(ResultCode.NotFound, "マイクが みつからないよ");

                _device = Microphone.devices[0];

                if (Microphone.IsRecording(_device))
                    Microphone.End(_device);

                _recordingClip = Microphone.Start(_device, false, RecordSeconds, AudioBuffer.SampleRate);
                if (_recordingClip == null)
                    return Result.Fail(ResultCode.IOError, "ろくおんを はじめられなかったよ");

                return Result.Ok();
            }
            catch (Exception e)
            {
                SafeLogger.Error("[RecAudio] StartRecording failed: " + e.Message);
                return Result.Fail(ResultCode.IOError, "ろくおんを はじめられなかったよ");
            }
        }

        public Result<AudioBuffer> StopRecording()
        {
            try
            {
                if (_recordingClip == null)
                    return Result<AudioBuffer>.Fail(ResultCode.NotFound, "ろくおんデータが ないよ");

                if (!string.IsNullOrEmpty(_device) && Microphone.IsRecording(_device))
                    Microphone.End(_device);

                int total = _recordingClip.samples * _recordingClip.channels;
                var temp = new float[total];
                _recordingClip.GetData(temp, 0);

                // 先頭 3秒（SampleCount）を再利用バッファへコピー。不足分は 0 埋め。
                int copy = Math.Min(AudioBuffer.SampleCount, temp.Length);
                Array.Copy(temp, 0, _buffer.Samples, 0, copy);
                for (int i = copy; i < AudioBuffer.SampleCount; i++)
                    _buffer.Samples[i] = 0f;

                return Result<AudioBuffer>.Ok(_buffer);
            }
            catch (Exception e)
            {
                SafeLogger.Error("[RecAudio] StopRecording failed: " + e.Message);
                return Result<AudioBuffer>.Fail(ResultCode.IOError, "ろくおんを とめられなかったよ");
            }
        }

        public Result Play(AudioBuffer buffer)
        {
            try
            {
                if (buffer == null || buffer.Samples == null)
                    return Result.Fail(ResultCode.ValidationError, "さいせいする おとが ないよ");
                if (_playbackSource == null)
                    return Result.Fail(ResultCode.Unknown, "さいせいの じゅんびが できてないよ");

                if (_playbackClip == null || _playbackClip.samples != buffer.Samples.Length)
                    _playbackClip = AudioClip.Create("rec_playback", buffer.Samples.Length, AudioBuffer.Channels, AudioBuffer.SampleRate, false);

                _playbackClip.SetData(buffer.Samples, 0);
                _playbackSource.clip = _playbackClip;
                _playbackSource.Play();
                return Result.Ok();
            }
            catch (Exception e)
            {
                SafeLogger.Error("[RecAudio] Play failed: " + e.Message);
                return Result.Fail(ResultCode.IOError, "さいせいできなかったよ");
            }
        }

        public Result Stop()
        {
            try
            {
                if (_playbackSource != null && _playbackSource.isPlaying)
                    _playbackSource.Stop();
                return Result.Ok();
            }
            catch (Exception e)
            {
                SafeLogger.Error("[RecAudio] Stop failed: " + e.Message);
                return Result.Fail(ResultCode.Unknown, "とめられなかったよ");
            }
        }
    }
}
