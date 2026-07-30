using Geidai.Common.Results;
using Geidai.Common.Content;
using Geidai.Common.Library;

namespace Geidai.Services.Content
{
    /// <summary>
    /// コンテンツ取得サービスの器（お題/ゲームパラメータ/音図鑑 / NFR-05）。
    /// 既存シグネチャは不変。U7 でカタログ／解除表を後方互換追加。
    /// </summary>
    public interface IContentService
    {
        Result<string> GetText(string key);
        Result<ThemeItem> GetCurrentTheme();
        void SetCatalog(ThemeCatalog catalog);

        /// <summary>音図鑑カタログを注入する（U7）。</summary>
        void SetCuratedCatalog(CuratedSoundCatalog catalog);

        /// <summary>音図鑑カタログを返す。未設定は Fail(NotFound)。</summary>
        Result<CuratedSoundCatalog> GetCuratedCatalog();

        /// <summary>解除条件表を注入する（U7）。</summary>
        void SetUnlockRules(UnlockRulesCatalog rules);

        /// <summary>解除条件表を返す。未設定は Fail(NotFound)。</summary>
        Result<UnlockRulesCatalog> GetUnlockRules();
    }
}
