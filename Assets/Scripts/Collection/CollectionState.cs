namespace Geidai.Collection
{
    /// <summary>
    /// コレクション画面の状態（frontend-components §2 / business-logic-model §1）。
    /// 1画面（一覧＋絞込/検索＋詳細・編集＋空状態）の遷移を表す。
    /// </summary>
    public enum CollectionState
    {
        Loading,
        Empty,
        Listing,
        Playing,
        Detail,
        Editing,
        Confirm
    }
}
