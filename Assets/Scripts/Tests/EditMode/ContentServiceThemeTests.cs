using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Geidai.Common.Content;
using Geidai.Common.Results;
using Geidai.Services.Content;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// ContentService（お題取得）の単体テスト（U5 / NFR-U5-04 / P2）。
    /// 空/無効カタログ→Fail(NotFound)・有効カタログ→今週のお題・GetText("theme.current")・text 空除外。
    /// </summary>
    public class ContentServiceThemeTests
    {
        private static ThemeCatalog MakeCatalog(params ThemeItem[] items)
        {
            var catalog = ScriptableObject.CreateInstance<ThemeCatalog>();
            catalog.SetItems(new List<ThemeItem>(items));
            return catalog;
        }

        private static readonly DateTime FixedDate = new DateTime(2026, 3, 2); // 月曜

        [Test]
        public void NullCatalog_Returns_NotFound()
        {
            var service = new ContentService(null, () => FixedDate);
            var result = service.GetCurrentTheme();
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ResultCode.NotFound, result.Code);
        }

        [Test]
        public void EmptyCatalog_Returns_NotFound()
        {
            var service = new ContentService(MakeCatalog(), () => FixedDate);
            var result = service.GetCurrentTheme();
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ResultCode.NotFound, result.Code);
        }

        [Test]
        public void AllInvalidItems_Returns_NotFound()
        {
            var service = new ContentService(
                MakeCatalog(new ThemeItem(""), new ThemeItem("   ")), () => FixedDate);
            var result = service.GetCurrentTheme();
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ResultCode.NotFound, result.Code);
        }

        [Test]
        public void ValidCatalog_Returns_Item_By_Week()
        {
            var catalog = MakeCatalog(
                new ThemeItem("Kirakira"),
                new ThemeItem("DonDon"),
                new ThemeItem("FuwaFuwa"));
            var service = new ContentService(catalog, () => FixedDate);

            var result = service.GetCurrentTheme();
            Assert.IsTrue(result.IsSuccess);

            int expectedIndex = ThemeSelector.SelectIndex(FixedDate, 3);
            Assert.AreEqual(catalog.ValidItems()[expectedIndex].text, result.Value.text);
        }

        [Test]
        public void InvalidItems_Are_Excluded_From_Selection()
        {
            // 無効（空）を混ぜても、有効2件からのみ選ばれる。
            var catalog = MakeCatalog(
                new ThemeItem("A"),
                new ThemeItem(""),   // 無効
                new ThemeItem("B"));
            var service = new ContentService(catalog, () => FixedDate);

            var result = service.GetCurrentTheme();
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.text == "A" || result.Value.text == "B");
        }

        [Test]
        public void GetText_ThemeCurrent_Returns_Body_On_Success()
        {
            var catalog = MakeCatalog(new ThemeItem("Kirakira"));
            var service = new ContentService(catalog, () => FixedDate);

            var text = service.GetText("theme.current");
            Assert.IsTrue(text.IsSuccess);
            Assert.AreEqual("Kirakira", text.Value);
        }

        [Test]
        public void GetText_ThemeCurrent_Fails_On_Empty_Catalog()
        {
            var service = new ContentService(MakeCatalog(), () => FixedDate);
            var text = service.GetText("theme.current");
            Assert.IsFalse(text.IsSuccess);
            Assert.AreEqual(ResultCode.NotFound, text.Code);
        }

        [Test]
        public void GetText_Unknown_Key_Returns_NotImplemented()
        {
            var service = new ContentService(MakeCatalog(new ThemeItem("X")), () => FixedDate);
            var text = service.GetText("game.level");
            Assert.IsFalse(text.IsSuccess);
            Assert.AreEqual(ResultCode.NotImplemented, text.Code);
        }

        [Test]
        public void SetCatalog_Updates_Source()
        {
            var service = new ContentService();
            service.SetCatalog(MakeCatalog(new ThemeItem("Later")));
            // now provider は既定 DateTime.Now だが、1件なら index=0 で必ず "Later"。
            var result = service.GetCurrentTheme();
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Later", result.Value.text);
        }
    }
}
