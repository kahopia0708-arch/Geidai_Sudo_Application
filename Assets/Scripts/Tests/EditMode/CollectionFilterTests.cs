using System;
using System.Collections.Generic;
using NUnit.Framework;
using FsCheck;
using Geidai.Common.Collection;
using Geidai.Common.Models;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// CollectionFilter のプロパティベース＋例示テスト（NFR-COL-T1 / U4）。
    /// 不変条件：結果⊆入力・条件空→全件・冪等・AND 合成・順序保持。純粋関数のため決定的。
    /// </summary>
    public class CollectionFilterTests
    {
        private static SavedSound Make(string id, string createdAtIso, string title = "", string memo = "", string nickname = "")
        {
            var meta = new SoundClipMeta
            {
                id = id,
                createdAtIso = createdAtIso,
                wavFileName = id + ".wav",
                title = title,
                memo = memo,
                nickname = nickname
            };
            return new SavedSound(meta, new SoundEffectSettingsData());
        }

        private static List<SavedSound> Sample()
        {
            return new List<SavedSound>
            {
                Make("a", "2026-01-15T10:00:00.0000000Z", "ねこ", "かわいい", "たろう"),
                Make("b", "2026-01-20T10:00:00.0000000Z", "いぬ", "げんき", "はなこ"),
                Make("c", "2026-02-05T10:00:00.0000000Z", "とり", "そら", "たろう"),
                Make("d", "2026-02-28T10:00:00.0000000Z", "", "ねこの ものまね", "じろう"),
            };
        }

        [Test]
        public void EmptyQuery_Returns_All()
        {
            var items = Sample();
            var result = CollectionFilter.Filter(items, CollectionQuery.Empty);
            Assert.AreEqual(items.Count, result.Count);
        }

        [Test]
        public void Null_Items_Returns_Empty()
        {
            var result = CollectionFilter.Filter(null, CollectionQuery.Empty);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Month_Filter_Selects_Only_That_Month()
        {
            var items = Sample();
            var jan = CollectionFilter.Filter(items, new CollectionQuery("2026-01", string.Empty));
            Assert.AreEqual(2, jan.Count);
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, jan.ConvertAll(s => s.meta.id));
        }

        [Test]
        public void Keyword_Matches_Title_Memo_Nickname_CaseInsensitive()
        {
            var items = Sample();

            // タイトル一致（ねこ）＋メモ一致（ねこの ものまね）
            var neko = CollectionFilter.Filter(items, new CollectionQuery(string.Empty, "ねこ"));
            CollectionAssert.AreEquivalent(new[] { "a", "d" }, neko.ConvertAll(s => s.meta.id));

            // ニックネーム一致（たろう）
            var taro = CollectionFilter.Filter(items, new CollectionQuery(string.Empty, "たろう"));
            CollectionAssert.AreEquivalent(new[] { "a", "c" }, taro.ConvertAll(s => s.meta.id));
        }

        [Test]
        public void Month_And_Keyword_Are_AndCombined()
        {
            var items = Sample();
            var r = CollectionFilter.Filter(items, new CollectionQuery("2026-02", "たろう"));
            CollectionAssert.AreEquivalent(new[] { "c" }, r.ConvertAll(s => s.meta.id));
        }

        [Test]
        public void ToYearMonth_Parses_Iso()
        {
            Assert.AreEqual("2026-01", CollectionFilter.ToYearMonth("2026-01-15T10:00:00.0000000Z"));
            Assert.AreEqual(string.Empty, CollectionFilter.ToYearMonth(""));
        }

        // --- Property-based ---

        [Test]
        public void Result_Is_Subset_Of_Input()
        {
            var items = Sample();
            Prop.ForAll<int>(seed =>
            {
                string kw = KeywordFor(seed);
                var result = CollectionFilter.Filter(items, new CollectionQuery(string.Empty, kw));
                foreach (var r in result)
                    if (!items.Contains(r)) return false;
                return result.Count <= items.Count;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void Filter_Is_Idempotent()
        {
            var items = Sample();
            Prop.ForAll<int>(seed =>
            {
                var q = new CollectionQuery(string.Empty, KeywordFor(seed));
                var once = CollectionFilter.Filter(items, q);
                var twice = CollectionFilter.Filter(once, q);
                if (once.Count != twice.Count) return false;
                for (int i = 0; i < once.Count; i++)
                    if (once[i] != twice[i]) return false;
                return true;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void Filter_Preserves_Input_Order()
        {
            var items = Sample();
            var result = CollectionFilter.Filter(items, new CollectionQuery(string.Empty, "たろう"));
            // a(index0) は c(index2) より前に来る
            int ia = result.FindIndex(s => s.meta.id == "a");
            int ic = result.FindIndex(s => s.meta.id == "c");
            Assert.IsTrue(ia >= 0 && ic >= 0 && ia < ic);
        }

        private static string KeywordFor(int seed)
        {
            switch (Math.Abs(seed) % 5)
            {
                case 0: return string.Empty;
                case 1: return "ねこ";
                case 2: return "たろう";
                case 3: return "そら";
                default: return "ずずず"; // 一致なし
            }
        }
    }
}
