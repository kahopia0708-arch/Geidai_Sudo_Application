using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Geidai.Foundation;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// HomeMenuConfig の可視項目フィルタ/並びのテスト（US-NAV-02 / BR-10）。
    /// 非表示は除外され、order 昇順で返ることを確認する。
    /// </summary>
    public class HomeMenuConfigTests
    {
        private static HomeMenuConfig BuildConfig(List<HomeMenuItem> items)
        {
            var config = ScriptableObject.CreateInstance<HomeMenuConfig>();
            var field = typeof(HomeMenuConfig).GetField("items",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(config, items);
            return config;
        }

        [Test]
        public void VisibleSorted_ExcludesHidden_AndSortsByOrder()
        {
            var items = new List<HomeMenuItem>
            {
                new HomeMenuItem { moduleId = ModuleId.GameSelect, visible = true, order = 2 },
                new HomeMenuItem { moduleId = ModuleId.Rec, visible = true, order = 0 },
                new HomeMenuItem { moduleId = ModuleId.Collection, visible = false, order = 1 },
                new HomeMenuItem { moduleId = ModuleId.WeeklyTheme, visible = true, order = 1 }
            };
            var config = BuildConfig(items);

            var result = config.VisibleSorted();

            Assert.AreEqual(3, result.Count, "非表示は除外される");
            Assert.AreEqual(ModuleId.Rec, result[0].moduleId);
            Assert.AreEqual(ModuleId.WeeklyTheme, result[1].moduleId);
            Assert.AreEqual(ModuleId.GameSelect, result[2].moduleId);

            Object.DestroyImmediate(config);
        }

        [Test]
        public void VisibleSorted_EmptyConfig_ReturnsEmpty()
        {
            var config = BuildConfig(new List<HomeMenuItem>());

            var result = config.VisibleSorted();

            Assert.AreEqual(0, result.Count);

            Object.DestroyImmediate(config);
        }
    }
}
