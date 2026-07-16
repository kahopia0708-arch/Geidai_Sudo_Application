using Geidai.Common.Content;
using Geidai.Services;
using Geidai.Services.Content;

namespace Geidai.Theme
{
    /// <summary>
    /// Theme モジュールの初期化（U5 / P4）。RecBootstrap/CollectionBootstrap と同パターン。
    /// IContentService を解決（未登録なら ContentService を登録）し、ThemeCatalog を注入する。
    /// カタログ未注入でも ContentService は Fail(NotFound) で安全に動く（BR-THEME-43）。
    /// </summary>
    public static class ThemeBootstrap
    {
        /// <summary>IContentService を確保し、必要ならカタログを注入して返す。</summary>
        public static IContentService EnsureContentService(ThemeCatalog catalog = null)
        {
            var content = ServiceRegistry.Resolve<IContentService>();
            if (content == null)
            {
                content = new ContentService();
                ServiceRegistry.Register<IContentService>(content);
            }
            if (catalog != null) content.SetCatalog(catalog);
            return content;
        }

        /// <summary>ThemeContext を確保する（未登録なら生成・登録）。</summary>
        public static ThemeContext EnsureThemeContext()
        {
            var ctx = ServiceRegistry.Resolve<ThemeContext>();
            if (ctx == null)
            {
                ctx = new ThemeContext();
                ServiceRegistry.Register<ThemeContext>(ctx);
            }
            return ctx;
        }
    }
}
