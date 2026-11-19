using System;
using Geidai.Common.Audio;
using UnityEngine;

namespace Geidai.Game3
{
    /// <summary>
    /// Game3 専用の図鑑音再生。
    /// 元の AudioClip を直接使い、図鑑 WAV の 48,000 Hz を維持したまま
    /// 合意済みの cents 値だけを AudioSource.pitch に適用する。
    /// </summary>
    public sealed class Game3PitchPlayer : IDisposable
    {
        private GameObject _rig;
        private AudioSource _source;

        public int BaseFrequency => _source != null && _source.clip != null
            ? _source.clip.frequency
            : 0;

        public float CurrentPitch => _source != null ? _source.pitch : 1f;

        public float GetPlaybackDurationSeconds(int cents)
        {
            if (_source == null || _source.clip == null)
                return 0f;

            float pitch = (float)PitchMath.CentsToRatio(cents);
            return _source.clip.length / Mathf.Max(pitch, 0.001f);
        }

        public void SetVolume(float volume)
        {
            if (_source != null)
                _source.volume = Mathf.Clamp01(volume);
        }

        public bool SetBase(AudioClip clip)
        {
            if (clip == null || clip.samples <= 0)
                return false;

            EnsureRig();
            _source.Stop();
            _source.clip = clip;
            _source.pitch = 1f;
            _source.volume = 1f;
            return true;
        }

        public bool Play(int cents)
        {
            if (_source == null || _source.clip == null)
                return false;

            _source.Stop();
            _source.pitch = (float)PitchMath.CentsToRatio(cents);
            _source.Play();
            return true;
        }

        public void Stop()
        {
            if (_source != null && _source.isPlaying)
                _source.Stop();

            if (_source != null)
                _source.volume = 1f;
        }

        private void EnsureRig()
        {
            if (_rig != null && _source != null)
                return;

            _rig = new GameObject("Geidai.Game3.PitchPlayer");
            if (Application.isPlaying)
                UnityEngine.Object.DontDestroyOnLoad(_rig);

            _source = _rig.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = false;
            _source.spatialBlend = 0f;
        }

        public void Dispose()
        {
            if (_rig == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(_rig);
            else
                UnityEngine.Object.DestroyImmediate(_rig);

            _source = null;
            _rig = null;
        }
    }
}
