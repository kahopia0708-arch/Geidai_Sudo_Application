using UnityEngine;

namespace Geidai.Services.Audio
{
    /// <summary>
    /// 図鑑 <see cref="AudioClip"/> をピッチ 1.0 のまま再生する（Game1/2 原音方針）。
    /// </summary>
    public sealed class CatalogClipPlayer
    {
        private readonly AudioSource _source;
        private float _oneShotEndsAtUnscaled;

        public CatalogClipPlayer(GameObject host)
        {
            if (host == null) return;
            _source = host.GetComponent<AudioSource>();
            if (_source == null)
                _source = host.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.pitch = 1f;
        }

        public bool IsReady => _source != null;

        public void Play(AudioClip clip)
        {
            if (_source == null || clip == null) return;
            _source.pitch = 1f;
            _source.PlayOneShot(clip);
            _oneShotEndsAtUnscaled = Time.unscaledTime + Mathf.Max(0.01f, clip.length);
        }

        public void Stop()
        {
            if (_source == null) return;
            _source.Stop();
            _oneShotEndsAtUnscaled = 0f;
        }

        public bool IsPlaying =>
            _source != null &&
            (_source.isPlaying || Time.unscaledTime < _oneShotEndsAtUnscaled);

        public float PlaybackSecondsRemaining
        {
            get
            {
                if (_source == null) return 0f;
                if (!_source.isPlaying && Time.unscaledTime >= _oneShotEndsAtUnscaled)
                    return 0f;
                return Mathf.Max(0f, _oneShotEndsAtUnscaled - Time.unscaledTime);
            }
        }
    }
}