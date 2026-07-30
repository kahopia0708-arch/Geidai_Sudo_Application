using Geidai.Common.Library;
using Geidai.Services;
using Geidai.Services.Content;
using Geidai.Services.Progression;
using UnityEngine;

namespace Geidai.Library
{
    /// <summary>
    /// Library モジュール初期化。カタログ／解除表 SO を ContentService へ注入し、初期解除を適用する。
    /// </summary>
    public static class LibraryBootstrap
    {
        public static void EnsureCatalogs(CuratedSoundCatalog curated, UnlockRulesCatalog rules)
        {
            var content = ServiceRegistry.Resolve<IContentService>();
            if (content == null) return;

            if (curated != null) content.SetCuratedCatalog(curated);
            if (rules != null) content.SetUnlockRules(rules);

            var progression = ServiceRegistry.Resolve<IProgressionService>();
            progression?.ApplyInitialUnlocks();
        }
    }
}
