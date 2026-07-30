namespace Geidai.Common.Models
{
    /// <summary>
    /// 型安全な画面遷移のための論理シーン識別子（US-TECH-04 / BR-12〜15）。
    /// Place は MVP 導線から除外するため列挙に含めない（BR-15）。
    /// GameSelect（U2 追加）はゲーム選択画面（既存 game_Home）に対応する。
    /// 既存値の順序は不変（後方互換のため末尾に追加）。
    /// </summary>
    public enum SceneId
    {
        Boot,
        Home,
        Register,
        Rec,
        Collection,
        Theme,
        Game1,
        GameSelect,
        Library,
        Create
    }
}
