using Geidai.Services;
using Geidai.Services.Audio;

namespace Geidai.Game3
{
    /// <summary>
    /// Game3（③音ならべ）で必要なサービスの初期化。
    /// </summary>
    public static class Game3Bootstrap
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