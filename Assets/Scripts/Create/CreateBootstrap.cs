using Geidai.Common.Library;
using Geidai.Services;
using Geidai.Services.Content;
using Geidai.Services.Progression;

namespace Geidai.Create
{
    /// <summary>Create モジュール初期化（カタログ注入は Library と同様に Content 経由）。</summary>
    public static class CreateBootstrap
    {
        public static void EnsureCatalogs(CuratedSoundCatalog curated, UnlockRulesCatalog rules)
        {
            var content = ServiceRegistry.Resolve<IContentService>();
            if (content == null) return;
            if (curated != null) content.SetCuratedCatalog(curated);
            if (rules != null) content.SetUnlockRules(rules);
            ServiceRegistry.Resolve<IProgressionService>()?.ApplyInitialUnlocks();
        }
    }
}
