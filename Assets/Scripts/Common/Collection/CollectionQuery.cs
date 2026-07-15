using System;

namespace Geidai.Common.Collection
{
    /// <summary>
    /// コレクション一覧の絞込・検索条件（純粋関数 <see cref="CollectionFilter"/> の入力 / Q5=A）。
    /// 技術非依存の値オブジェクト。UI からの変更で新しいインスタンスを作って渡す。
    /// </summary>
    [Serializable]
    public struct CollectionQuery
    {
        /// <summary>月別絞込（"YYYY-MM"）。null/空＝全月。</summary>
        public string yearMonth;

        /// <summary>キーワード（title/memo/nickname 部分一致）。null/空＝検索なし。</summary>
        public string keyword;

        public CollectionQuery(string yearMonth, string keyword)
        {
            this.yearMonth = yearMonth;
            this.keyword = keyword;
        }

        /// <summary>無条件（全件）を表すクエリ。</summary>
        public static CollectionQuery Empty => new CollectionQuery(string.Empty, string.Empty);

        /// <summary>月・キーワードともに未指定なら true（＝全件）。</summary>
        public bool IsEmpty =>
            string.IsNullOrWhiteSpace(yearMonth) && string.IsNullOrWhiteSpace(keyword);
    }
}
