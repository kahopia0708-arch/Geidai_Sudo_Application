using UnityEngine;
using Geidai.Services;
using Geidai.Services.Audio;

namespace Geidai.Rec
{
    /// <summary>
    /// Rec モジュールの初期化（logical-components §1）。
    /// <see cref="IAudioService"/> の本実装（<see cref="RecAudioService"/>）を Rec 側で
    /// <see cref="ServiceRegistry"/> へ登録する（Services→Rec の循環依存を作らないため、
    /// AppManager ではなく Rec 側で登録する）。
    /// </summary>
    public static class RecBootstrap
    {
        /// <summary>
        /// IAudioService（RecAudioService）を保証し、再生用 AudioSource を結線して返す。
        /// 既に登録済みならそれを再利用する。
        /// </summary>
        public static RecAudioService EnsureAudioService(AudioSource playbackSource)
        {
            var audio = ServiceRegistry.Resolve<IAudioService>() as RecAudioService;
            if (audio == null)
            {
                audio = new RecAudioService();
                ServiceRegistry.Register<IAudioService>(audio);
            }
            audio.SetPlaybackSource(playbackSource);
            return audio;
        }
    }
}
