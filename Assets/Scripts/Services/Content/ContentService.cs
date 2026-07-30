using System;
using System.Collections.Generic;
using Geidai.Common.Results;
using Geidai.Common.Content;
using Geidai.Common.Library;

namespace Geidai.Services.Content
{
    /// <summary>
    /// コンテンツ取得実装（U5 お題 + U7 音図鑑）。
    /// </summary>
    public class ContentService : IContentService
    {
        private ThemeCatalog _catalog;
        private CuratedSoundCatalog _curatedCatalog;
        private UnlockRulesCatalog _unlockRules;
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

        public void SetCuratedCatalog(CuratedSoundCatalog catalog)
        {
            _curatedCatalog = catalog;
        }

        public Result<CuratedSoundCatalog> GetCuratedCatalog()
        {
            if (_curatedCatalog == null)
                return Result<CuratedSoundCatalog>.Fail(ResultCode.NotFound, "おとのずかんが まだ ないよ");
            return Result<CuratedSoundCatalog>.Ok(_curatedCatalog);
        }

        public void SetUnlockRules(UnlockRulesCatalog rules)
        {
            _unlockRules = rules;
        }

        public Result<UnlockRulesCatalog> GetUnlockRules()
        {
            if (_unlockRules == null)
                return Result<UnlockRulesCatalog>.Fail(ResultCode.NotFound, "かいじょじょうけんが まだ ないよ");
            return Result<UnlockRulesCatalog>.Ok(_unlockRules);
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
