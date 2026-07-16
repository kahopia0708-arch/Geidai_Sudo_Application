using Geidai.Common.Results;
using Geidai.Common.Content;

namespace Geidai.Services.Content
{
    /// <summary>
    /// コンテンツ取得サービスの器（お題テキスト/ゲームパラメータ等 / NFR-05）。
    /// Sさん が差し替えるデータ（ThemeCatalog 等）へのアクセスを抽象化。お題本実装は U5。
    /// </summary>
    public interface IContentService
    {
        /// <summary>キーに対応するテキストを返す（例: "theme.current"＝今週のお題本文）。後方互換 IF。</summary>
        Result<string> GetText(string key);

        /// <summary>今週のお題（ThemeItem）を返す（U5 / P2）。空/無効カタログは Fail(NotFound)。</summary>
        Result<ThemeItem> GetCurrentTheme();

        /// <summary>お題カタログを注入する（起動時 DI / ThemeBootstrap 等から呼ぶ）。</summary>
        void SetCatalog(ThemeCatalog catalog);
    }
}
