using Geidai.Services;
using Geidai.Services.Audio;

namespace Geidai.Game1
{
    /// <summary>
    /// Game1（①音合わせ）モジュールの初期化（U6 / P1）。
    /// RecBootstrap/ThemeBootstrap と同パターン。IPitchVariationService を解決し、未登録なら登録する。
    /// </summary>
    public static class Game1Bootstrap
    {
        public static IPitchVariationService EnsurePitchVariationService()
        {
            var svc = ServiceRegistry.Resolve<IPitchVariationService>();
            if (svc == null)
            {
                svc = new PitchVariationService();
                ServiceRegistry.Register<IPitchVariationService>(svc);
            }
            return svc;
        }
    }
}
