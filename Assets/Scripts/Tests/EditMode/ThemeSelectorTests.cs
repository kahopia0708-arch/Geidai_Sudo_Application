using System;
using NUnit.Framework;
using FsCheck;
using Geidai.Common.Content;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// ThemeSelector のプロパティベース＋例示テスト（U5 / NFR-U5-04 / P1）。
    /// 不変条件：count&lt;=0 は -1／それ以外は 0..count-1／決定的／剰余一致。純粋関数。
    /// </summary>
    public class ThemeSelectorTests
    {
        [Test]
        public void NonPositiveCount_Returns_MinusOne()
        {
            var date = new DateTime(2026, 7, 16);
            Assert.AreEqual(-1, ThemeSelector.SelectIndex(date, 0));
            Assert.AreEqual(-1, ThemeSelector.SelectIndex(date, -5));
        }

        [Test]
        public void Index_Is_In_Range()
        {
            Prop.ForAll<int, int>((dayOffset, rawCount) =>
            {
                int count = (Math.Abs(rawCount) % 50) + 1; // 1..50
                DateTime date = new DateTime(2000, 1, 1).AddDays(Math.Abs(dayOffset) % 20000);
                int index = ThemeSelector.SelectIndex(date, count);
                return index >= 0 && index < count;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void Is_Deterministic()
        {
            Prop.ForAll<int, int>((dayOffset, rawCount) =>
            {
                int count = (Math.Abs(rawCount) % 50) + 1;
                DateTime date = new DateTime(2000, 1, 1).AddDays(Math.Abs(dayOffset) % 20000);
                return ThemeSelector.SelectIndex(date, count) == ThemeSelector.SelectIndex(date, count);
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void Index_Matches_WeekModulo()
        {
            Prop.ForAll<int, int>((dayOffset, rawCount) =>
            {
                int count = (Math.Abs(rawCount) % 50) + 1;
                DateTime date = new DateTime(2000, 1, 1).AddDays(Math.Abs(dayOffset) % 20000);
                int week = ThemeSelector.WeekOfYear(date);
                int expected = ((week % count) + count) % count;
                return ThemeSelector.SelectIndex(date, count) == expected;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void WeekOfYear_Is_Positive_And_Reasonable()
        {
            // 代表日付：年初・年央・年末で 1..53 の範囲に収まる。
            foreach (var d in new[]
            {
                new DateTime(2026, 1, 1),
                new DateTime(2026, 6, 15),
                new DateTime(2026, 12, 31),
                new DateTime(2024, 2, 29), // 閏年
            })
            {
                int w = ThemeSelector.WeekOfYear(d);
                Assert.IsTrue(w >= 1 && w <= 53, $"week={w} for {d:yyyy-MM-dd}");
            }
        }

        [Test]
        public void Consecutive_Weeks_Rotate_Index()
        {
            // count=3 のとき、7 日ごとに index が変化する（同一週内は不変）。
            var baseDate = new DateTime(2026, 3, 2); // 月曜
            int i0 = ThemeSelector.SelectIndex(baseDate, 3);
            int i0b = ThemeSelector.SelectIndex(baseDate.AddDays(6), 3); // 同一週（日曜まで）
            int i1 = ThemeSelector.SelectIndex(baseDate.AddDays(7), 3); // 翌週
            Assert.AreEqual(i0, i0b);
            Assert.AreEqual((i0 + 1) % 3, i1);
        }
    }
}
