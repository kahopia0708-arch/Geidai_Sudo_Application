using System;
using UnityEngine;
using Geidai.Common.Audio;
using Geidai.Common.Create;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.Utils;

namespace Geidai.Services.Audio
{
    /// <summary>
    /// 録音/再生の共有実装（U4 + U7/U8）。
    /// Create 用にデュアル AudioSource リグを遅延生成する。
    /// </summary>
    public class AudioService : IAudioService
    {
        private const int RecordSeconds = 4;

        private readonly AudioBuffer _buffer = new AudioBuffer();
        private AudioClip _recordingClip;
        private AudioClip _playbackClip;
        private string _device;

        private GameObject _rig;
        private EffectChain _effectChain;
        private AudioSource _playbackSource;

        private GameObject _layerRig;
        private AudioSource _layerSourceA;
        private AudioSource _layerSourceB;
        private EffectChain _layerEffectA;
        private EffectChain _layerEffectB;

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

        private void EnsureLayerRig()
        {
            if (_layerRig != null && _layerSourceA != null && _layerSourceB != null) return;

            if (_layerRig == null)
            {
                _layerRig = new GameObject("Geidai.AudioService.LayerRig");
                UnityEngine.Object.DontDestroyOnLoad(_layerRig);
            }

            var goA = _layerRig.transform.Find("LayerA");
            if (goA == null)
            {
                var child = new GameObject("LayerA");
                child.transform.SetParent(_layerRig.transform, false);
                goA = child.transform;
            }
            var goB = _layerRig.transform.Find("LayerB");
            if (goB == null)
            {
                var child = new GameObject("LayerB");
                child.transform.SetParent(_layerRig.transform, false);
                goB = child.transform;
            }

            _layerEffectA = goA.GetComponent<EffectChain>() ?? goA.gameObject.AddComponent<EffectChain>();
            _layerEffectA.EnsureComponents();
            _layerSourceA = _layerEffectA.Source;

            _layerEffectB = goB.GetComponent<EffectChain>() ?? goB.gameObject.AddComponent<EffectChain>();
            _layerEffectB.EnsureComponents();
            _layerSourceB = _layerEffectB.Source;
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

        public Result Play(AudioBuffer buffer) => PlayInternal(buffer, null, false);

        public Result Play(AudioBuffer buffer, SoundEffectSettingsData settings)
            => PlayInternal(buffer, settings, true);

        private Result PlayInternal(AudioBuffer buffer, SoundEffectSettingsData settings, bool applyEffects)
        {
            try
            {
                if (buffer == null || buffer.Samples == null)
                    return Result.Fail(ResultCode.ValidationError, "さいせいする おとが ないよ");

                EnsureRig();
                if (_playbackSource == null)
                    return Result.Fail(ResultCode.Unknown, "さいせいの じゅんびが できてないよ");

                StopLayersQuiet();

                if (applyEffects && settings != null)
                    _effectChain.Apply(settings, true, true, true, true, true);
                else
                    _effectChain.Apply(settings ?? new SoundEffectSettingsData(), false, false, false, false, false);

                if (_playbackClip == null || _playbackClip.samples != buffer.Samples.Length)
                    _playbackClip = AudioClip.Create("audio_playback", buffer.Samples.Length, AudioBuffer.Channels, AudioBuffer.SampleRate, false);

                _playbackClip.SetData(buffer.Samples, 0);
                _playbackSource.clip = _playbackClip;
                _playbackSource.volume = 1f;
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
                StopLayersQuiet();
                return Result.Ok();
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Audio] Stop failed: " + e.Message);
                return Result.Fail(ResultCode.Unknown, "とめられなかったよ");
            }
        }

        public bool IsPlaying
        {
            get
            {
                if (_playbackSource != null && _playbackSource.isPlaying) return true;
                if (_layerSourceA != null && _layerSourceA.isPlaying) return true;
                if (_layerSourceB != null && _layerSourceB.isPlaying) return true;
                return false;
            }
        }

        public AudioSource GetPlaybackSource()
        {
            EnsureRig();
            return _playbackSource;
        }

        public Result PlayCuratedClip(AudioClip clip)
        {
            try
            {
                if (clip == null)
                    return Result.Fail(ResultCode.ValidationError, "おとが ないよ");

                EnsureRig();
                StopLayersQuiet();
                _effectChain.Apply(new SoundEffectSettingsData(), false, false, false, false, false);
                _playbackSource.clip = clip;
                _playbackSource.volume = 1f;
                _playbackSource.Play();
                return Result.Ok();
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Audio] PlayCuratedClip failed: " + e.Message);
                return Result.Fail(ResultCode.IOError, "さいせいできなかったよ");
            }
        }

        public Result PlayLayers(AudioClip clipA, SoundRecipeLayer layerA, AudioClip clipB, SoundRecipeLayer layerB)
        {
            try
            {
                bool hasA = clipA != null && layerA != null && !string.IsNullOrEmpty(layerA.curatedSoundId);
                bool hasB = clipB != null && layerB != null && !string.IsNullOrEmpty(layerB.curatedSoundId);
                if (!hasA && !hasB)
                    return Result.Fail(ResultCode.ValidationError, "おとを えらんでね");

                EnsureLayerRig();
                if (_playbackSource != null && _playbackSource.isPlaying)
                    _playbackSource.Stop();

                StopLayersQuiet();

                if (hasA)
                    StartLayer(_layerSourceA, _layerEffectA, clipA, layerA);
                if (hasB)
                    StartLayer(_layerSourceB, _layerEffectB, clipB, layerB);

                return Result.Ok();
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Audio] PlayLayers failed: " + e.Message);
                return Result.Fail(ResultCode.IOError, "さいせいできなかったよ");
            }
        }

        public Result<byte[]> RenderRecipeToWav(AudioClip clipA, SoundRecipeLayer layerA, AudioClip clipB, SoundRecipeLayer layerB)
        {
            try
            {
                bool hasA = clipA != null && layerA != null && !string.IsNullOrEmpty(layerA.curatedSoundId);
                bool hasB = clipB != null && layerB != null && !string.IsNullOrEmpty(layerB.curatedSoundId);
                if (!hasA && !hasB)
                    return Result<byte[]>.Fail(ResultCode.ValidationError, "おとを えらんでね");

                float[] mixed = MixOffline(
                    hasA ? clipA : null, hasA ? RecipeValidator.Clamp(new SoundRecipe { layerA = layerA }).layerA : null,
                    hasB ? clipB : null, hasB ? RecipeValidator.Clamp(new SoundRecipe { layerB = layerB }).layerB : null);

                if (mixed == null || mixed.Length == 0)
                    return Result<byte[]>.Fail(ResultCode.ValidationError, "おとが ないよ");

                byte[] wav = WavCodec.Encode(mixed, AudioBuffer.SampleRate, 1);
                return Result<byte[]>.Ok(wav);
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Audio] RenderRecipeToWav failed: " + e.Message);
                return Result<byte[]>.Fail(ResultCode.IOError, "かきだしに しっぱいしたよ");
            }
        }

        private static void StartLayer(AudioSource source, EffectChain chain, AudioClip clip, SoundRecipeLayer layer)
        {
            var clamped = layer.Clone();
            RecipeValidator.ClampLayer(clamped);
            var settings = ToSettings(clamped);
            chain.Apply(settings, true, true, true, true, true);
            source.clip = clip;
            source.volume = clamped.volume;
            source.Play();
        }

        private void StopLayersQuiet()
        {
            if (_layerSourceA != null && _layerSourceA.isPlaying) _layerSourceA.Stop();
            if (_layerSourceB != null && _layerSourceB.isPlaying) _layerSourceB.Stop();
        }

        private static SoundEffectSettingsData ToSettings(SoundRecipeLayer layer)
        {
            return new SoundEffectSettingsData
            {
                pitchSemitones = layer.pitchSemitones,
                reverb = layer.reverb,
                noiseLevel = NoiseLevel.None,
                timbre = MapTimbre(layer.timbre)
            };
        }

        private static TimbreType MapTimbre(RecipeTimbreKind kind)
        {
            switch (kind)
            {
                case RecipeTimbreKind.Robot: return TimbreType.Hard;
                case RecipeTimbreKind.Chorus: return TimbreType.Soft;
                default: return TimbreType.Original;
            }
        }

        /// <summary>
        /// 簡易オフラインミックス: 各クリップを GetData → volume 乗算 → pitch は線形再サンプル近似 → 加算クランプ。
        /// MVP 品質。DSP 精度は後続改善。
        /// </summary>
        private static float[] MixOffline(AudioClip clipA, SoundRecipeLayer layerA, AudioClip clipB, SoundRecipeLayer layerB)
        {
            float[] a = clipA != null ? ExtractMono(clipA, layerA) : null;
            float[] b = clipB != null ? ExtractMono(clipB, layerB) : null;
            int len = Math.Max(a?.Length ?? 0, b?.Length ?? 0);
            if (len == 0) return Array.Empty<float>();

            var mixed = new float[len];
            for (int i = 0; i < len; i++)
            {
                float s = 0f;
                if (a != null && i < a.Length) s += a[i];
                if (b != null && i < b.Length) s += b[i];
                if (s > 1f) s = 1f;
                if (s < -1f) s = -1f;
                mixed[i] = s;
            }
            return mixed;
        }

        private static float[] ExtractMono(AudioClip clip, SoundRecipeLayer layer)
        {
            int samples = clip.samples * clip.channels;
            var data = new float[samples];
            clip.GetData(data, 0);

            // モノラル化（先頭チャンネル相当）
            float[] mono;
            if (clip.channels <= 1)
            {
                mono = data;
            }
            else
            {
                mono = new float[clip.samples];
                for (int i = 0; i < clip.samples; i++)
                    mono[i] = data[i * clip.channels];
            }

            float vol = layer != null ? RecipeClamp.ClampVolume(layer.volume) : 1f;
            int pitch = layer != null ? RecipeClamp.ClampPitch(layer.pitchSemitones) : 0;
            double ratio = PitchMath.SemitonesToRatio(pitch);

            // ピッチ変更: 出力長 = 入力長 / ratio
            int outLen = Math.Max(1, (int)(mono.Length / ratio));
            var pitched = new float[outLen];
            for (int i = 0; i < outLen; i++)
            {
                double srcIndex = i * ratio;
                int i0 = (int)srcIndex;
                int i1 = Math.Min(i0 + 1, mono.Length - 1);
                double t = srcIndex - i0;
                float sample = (float)((1.0 - t) * mono[Math.Min(i0, mono.Length - 1)] + t * mono[i1]);
                pitched[i] = sample * vol;
            }
            return pitched;
        }
    }
}
