using UnityEngine;
using Geidai.Common.Audio;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.Utils;

namespace Geidai.Services.Audio
{
    /// <summary>
    /// 出題用リアルタイムピッチ加工の実装（U6 / P1 / NFR-U6-01/02）。
    /// - 専用リグ（GameObject＋AudioSource）を遅延生成し DontDestroyOnLoad で常駐（シーンをまたいで発音）。
    /// - 基準 AudioBuffer から AudioClip を一度だけ生成しキャッシュ、再生時に
    ///   AudioSource.pitch = PitchMath.CentsToRatio(cents) を設定して発音（加工音は非生成・非保存）。
    /// - 失敗は Result で表現しクラッシュさせない。
    /// </summary>
    public class PitchVariationService : IPitchVariationService
    {
        private GameObject _rig;
        private AudioSource _source;
        private AudioClip _clip;
        private AudioBuffer _base;

        private void EnsureRig()
        {
            if (_rig != null && _source != null) return;

            if (_rig == null)
            {
                _rig = new GameObject("Geidai.PitchVariationService.Rig");
                UnityEngine.Object.DontDestroyOnLoad(_rig);
            }
            if (_source == null)
            {
                _source = _rig.GetComponent<AudioSource>();
                if (_source == null) _source = _rig.AddComponent<AudioSource>();
                _source.playOnAwake = false;
            }
        }

        public Result SetBase(AudioBuffer baseBuffer)
        {
            if (baseBuffer == null || baseBuffer.Samples == null || baseBuffer.Samples.Length == 0)
                return Result.Fail(ResultCode.ValidationError, "おとの もとが ないよ");

            try
            {
                EnsureRig();
                _base = baseBuffer;

                if (_clip == null || _clip.samples != baseBuffer.Samples.Length)
                    _clip = AudioClip.Create("pitch_variation", baseBuffer.Samples.Length, AudioBuffer.Channels, AudioBuffer.SampleRate, false);

                _clip.SetData(baseBuffer.Samples, 0);
                _source.clip = _clip;
                return Result.Ok();
            }
            catch (System.Exception e)
            {
                SafeLogger.Warn("[PitchVariation] SetBase failed.");
                return Result.Fail(ResultCode.Unknown, e.Message);
            }
        }

        public Result Play(int cents)
        {
            if (_base == null || _clip == null || _source == null)
                return Result.Fail(ResultCode.NotFound, "おとの もとが ないよ");

            try
            {
                _source.Stop();
                _source.pitch = (float)PitchMath.CentsToRatio(cents);
                _source.Play();
                return Result.Ok();
            }
            catch (System.Exception e)
            {
                SafeLogger.Warn("[PitchVariation] Play failed.");
                return Result.Fail(ResultCode.Unknown, e.Message);
            }
        }

        public Result Play(AudioBuffer baseBuffer, int cents)
        {
            var set = SetBase(baseBuffer);
            if (!set.IsSuccess) return set;
            return Play(cents);
        }

        public Result Stop()
        {
            if (_source != null && _source.isPlaying) _source.Stop();
            return Result.Ok();
        }

        public bool IsPlaying => _source != null && _source.isPlaying;
    }
}
