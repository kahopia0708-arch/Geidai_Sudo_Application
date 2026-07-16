using System;
using UnityEngine;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.Utils;

namespace Geidai.Services.Audio
{
    /// <summary>
    /// 録音/再生の共有実装（U4 / IAudioService / nfr-design §4・NFR-COL-M4・Q4=A）。
    /// - 再生リグ（AudioSource＋<see cref="EffectChain"/>）を遅延生成し DontDestroyOnLoad で常駐させ、
    ///   シーンをまたいで発音できる（Collection 視聴・Rec プレビュー/再生を一本化）。
    /// - 録音は Unity 標準 <see cref="Microphone"/>（3秒・44100・モノラル）。停止時に固定長
    ///   <see cref="AudioBuffer"/>（再利用）へコピーして GC を抑制する（U3 の挙動を移設・不変）。
    /// - すべての失敗は <see cref="Result"/> で表現しクラッシュさせない（SECURITY-15）。
    /// </summary>
    public class AudioService : IAudioService
    {
        // Microphone の録音長（自動停止は 3秒だが、ゆとりを持って確保）
        private const int RecordSeconds = 4;

        private readonly AudioBuffer _buffer = new AudioBuffer(); // 再利用（132300 サンプル）
        private AudioClip _recordingClip;
        private AudioClip _playbackClip;
        private string _device;

        private GameObject _rig;
        private EffectChain _effectChain;
        private AudioSource _playbackSource;

        // --------------------------------------------------------------- rig (lazy)

        /// <summary>再生リグ（GameObject＋AudioSource＋EffectChain）を必要時に生成・常駐させる。</summary>
        private void EnsureRig()
        {
            if (_rig != null && _effectChain != null && _playbackSource != null) return;

            if (_rig == null)
            {
                _rig = new GameObject("Geidai.AudioService.Rig");
                UnityEngine.Object.DontDestroyOnLoad(_rig);
            }
            if (_effectChain == null)
            {
                _effectChain = _rig.GetComponent<EffectChain>();
                if (_effectChain == null) _effectChain = _rig.AddComponent<EffectChain>();
                _effectChain.EnsureComponents();
            }
            _playbackSource = _effectChain.Source;
        }

        // --------------------------------------------------------------- recording

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
                SafeLogger.Error("[Audio] StartRecording failed: " + e.Message);
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
                SafeLogger.Error("[Audio] StopRecording failed: " + e.Message);
                return Result<AudioBuffer>.Fail(ResultCode.IOError, "ろくおんを とめられなかったよ");
            }
        }

        // --------------------------------------------------------------- playback

        public Result Play(AudioBuffer buffer)
        {
            // 素の再生（エフェクト中立）。
            return PlayInternal(buffer, null, false);
        }

        public Result Play(AudioBuffer buffer, SoundEffectSettingsData settings)
        {
            // 保存エフェクトを全 on で再適用して再生（Collection 視聴）。
            return PlayInternal(buffer, settings, true);
        }

        private Result PlayInternal(AudioBuffer buffer, SoundEffectSettingsData settings, bool applyEffects)
        {
            try
            {
                if (buffer == null || buffer.Samples == null)
                    return Result.Fail(ResultCode.ValidationError, "さいせいする おとが ないよ");

                EnsureRig();
                if (_playbackSource == null)
                    return Result.Fail(ResultCode.Unknown, "さいせいの じゅんびが できてないよ");

                // エフェクト反映（適用しない場合は中立化）。
                if (applyEffects && settings != null)
                    _effectChain.Apply(settings, true, true, true, true, true);
                else
                    _effectChain.Apply(settings ?? new SoundEffectSettingsData(), false, false, false, false, false);

                if (_playbackClip == null || _playbackClip.samples != buffer.Samples.Length)
                    _playbackClip = AudioClip.Create("audio_playback", buffer.Samples.Length, AudioBuffer.Channels, AudioBuffer.SampleRate, false);

                _playbackClip.SetData(buffer.Samples, 0);
                _playbackSource.clip = _playbackClip;
                _playbackSource.Play();
                return Result.Ok();
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Audio] Play failed: " + e.Message);
                return Result.Fail(ResultCode.IOError, "さいせいできなかったよ");
            }
        }

        public Result ApplyEffects(SoundEffectSettingsData settings, bool allOn, bool pitchOn, bool noiseOn, bool timbreOn, bool reverbOn)
        {
            try
            {
                if (settings == null)
                    return Result.Fail(ResultCode.ValidationError, "せっていが ないよ");

                EnsureRig();
                _effectChain.Apply(settings, allOn, pitchOn, noiseOn, timbreOn, reverbOn);
                return Result.Ok();
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Audio] ApplyEffects failed: " + e.Message);
                return Result.Fail(ResultCode.Unknown, "こうかを はんえいできなかったよ");
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
                SafeLogger.Error("[Audio] Stop failed: " + e.Message);
                return Result.Fail(ResultCode.Unknown, "とめられなかったよ");
            }
        }

        public bool IsPlaying => _playbackSource != null && _playbackSource.isPlaying;

        /// <summary>再生に用いる AudioSource を取得する（Rec が録音プレビュー等で参照する場合に使用）。</summary>
        public AudioSource GetPlaybackSource()
        {
            EnsureRig();
            return _playbackSource;
        }
    }
}
