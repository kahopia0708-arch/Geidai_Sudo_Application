using System;
using System.Collections.Generic;
using Geidai.Common.Results;
using Geidai.Common.Content;

namespace Geidai.Services.Content
{
    /// <summary>
    /// お題コンテンツの取得実装（U5 / P2/P4）。
    /// ThemeCatalog を参照し、ThemeSelector（純粋）で「今週のお題」を導出する。
    /// カタログ未注入/空/全項目無効は Fail(NotFound) を返しクラッシュしない（BR-THEME-21/41）。
    /// 時刻は注入可能（テスト時は固定日付）。ゲーム用パラメータ取得は U6。
    /// </summary>
    public class ContentService : IContentService
    {
        private ThemeCatalog _catalog;
        private readonly Func<DateTime> _now;

        public ContentService()
        {
            _now = () => DateTime.Now;
        }

        public ContentService(ThemeCatalog catalog, Func<DateTime> nowProvider = null)
        {
            _catalog = catalog;
            _now = nowProvider ?? (() => DateTime.Now);
        }

        public void SetCatalog(ThemeCatalog catalog)
        {
            _catalog = catalog;
        }

        public Result<ThemeItem> GetCurrentTheme()
        {
            if (_catalog == null)
                return Result<ThemeItem>.Fail(ResultCode.NotFound, "おだいが まだ ないよ");

            List<ThemeItem> valid = _catalog.ValidItems();
            if (valid == null || valid.Count == 0)
                return Result<ThemeItem>.Fail(ResultCode.NotFound, "おだいが まだ ないよ");

            int index = ThemeSelector.SelectIndex(_now(), valid.Count);
            if (index < 0 || index >= valid.Count)
                return Result<ThemeItem>.Fail(ResultCode.NotFound, "おだいが まだ ないよ");

            return Result<ThemeItem>.Ok(valid[index]);
        }

        public Result<string> GetText(string key)
        {
            if (key == "theme.current")
            {
                var theme = GetCurrentTheme();
                if (!theme.IsSuccess)
                    return Result<string>.Fail(theme.Code, theme.Message);
                return Result<string>.Ok(theme.Value.text);
            }

            return Result<string>.Fail(ResultCode.NotImplemented, "このコンテンツは後続ユニットで実装します。");
        }
    }
}
