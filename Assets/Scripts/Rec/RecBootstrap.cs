using Geidai.Services;
using Geidai.Services.Audio;

namespace Geidai.Rec
{
    /// <summary>
    /// Rec モジュールの初期化（logical-components §1）。
    /// U4 で <see cref="IAudioService"/> の実装は Services 層の共有 <see cref="AudioService"/> に一本化した
    /// （Q4=A / NFR-COL-M4）。通常は <see cref="AppManager"/> が起動時に登録するが、Rec シーン単独起動でも
    /// 動くよう、未登録なら共有実装を登録して返す（録音側の挙動は不変）。
    /// </summary>
    public static class RecBootstrap
    {
        /// <summary>共有 <see cref="IAudioService"/> を保証して返す。未登録なら <see cref="AudioService"/> を登録する。</summary>
        public static IAudioService EnsureAudioService()
        {
            var audio = ServiceRegistry.Resolve<IAudioService>();
            if (audio == null)
            {
                audio = new AudioService();
                ServiceRegistry.Register<IAudioService>(audio);
            }
            return audio;
        }
    }
}
