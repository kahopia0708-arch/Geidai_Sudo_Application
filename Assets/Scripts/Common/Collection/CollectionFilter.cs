using System;
using System.Collections.Generic;
using Geidai.Common.Models;

namespace Geidai.Common.Collection
{
    /// <summary>
    /// コレクション一覧の絞込・検索（純粋関数 / Q5=A・NFR-COL-T1）。
    /// 副作用なし・決定的。PBT の不変条件＝「結果⊆入力・条件空→全件・冪等・AND 合成」。
    /// - 月別: <see cref="SoundClipMeta.createdAtIso"/> から "YYYY-MM" を導出し一致判定。
    /// - 検索: title/memo/nickname を正規化（Trim・小文字化）して部分一致。
    /// - 月＋検索は AND。
    /// </summary>
    public static class CollectionFilter
    {
        /// <summary>
        /// <paramref name="items"/> を <paramref name="query"/> で絞り込む。
        /// null 安全（items が null なら空リスト）。入力の順序を保持する。
        /// </summary>
        public static List<SavedSound> Filter(IReadOnlyList<SavedSound> items, CollectionQuery query)
        {
            var result = new List<SavedSound>();
            if (items == null) return result;

            string month = Normalize(query.yearMonth);
            string keyword = Normalize(query.keyword);
            bool hasMonth = !string.IsNullOrEmpty(month);
            bool hasKeyword = !string.IsNullOrEmpty(keyword);

            for (int i = 0; i < items.Count; i++)
            {
                var s = items[i];
                if (s == null || s.meta == null) continue; // 無効項目は対象外

                if (hasMonth && !MatchesMonth(s.meta, month)) continue;
                if (hasKeyword && !MatchesKeyword(s.meta, keyword)) continue;

                result.Add(s);
            }

            return result;
        }

        /// <summary>createdAtIso（ISO 8601）から "YYYY-MM" を導出する。導出不能なら空。</summary>
        public static string ToYearMonth(string createdAtIso)
        {
            if (string.IsNullOrWhiteSpace(createdAtIso)) return string.Empty;

            // まずは厳密パース、失敗時は先頭 "YYYY-MM" を素直に取り出す（後方互換）。
            if (DateTime.TryParse(createdAtIso, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
            {
                return dt.ToString("yyyy-MM");
            }

            var trimmed = createdAtIso.Trim();
            if (trimmed.Length >= 7 && trimmed[4] == '-')
                return trimmed.Substring(0, 7);

            return string.Empty;
        }

        private static bool MatchesMonth(SoundClipMeta meta, string month)
        {
            return string.Equals(ToYearMonth(meta.createdAtIso), month, StringComparison.Ordinal);
        }

        private static bool MatchesKeyword(SoundClipMeta meta, string keyword)
        {
            return Contains(meta.title, keyword)
                || Contains(meta.memo, keyword)
                || Contains(meta.nickname, keyword);
        }

        private static bool Contains(string field, string keyword)
        {
            if (string.IsNullOrEmpty(field)) return false;
            return Normalize(field).Contains(keyword);
        }

        /// <summary>比較用の正規化（Trim＋小文字化）。null は空文字へ。</summary>
        private static string Normalize(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Trim().ToLowerInvariant();
        }
    }
}
