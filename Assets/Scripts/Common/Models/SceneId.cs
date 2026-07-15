namespace Geidai.Common.Models
{
    /// <summary>
    /// 型安全な画面遷移のための論理シーン識別子（US-TECH-04 / BR-12〜15）。
    /// Place は MVP 導線から除外するため列挙に含めない（BR-15）。
    /// </summary>
    public enum SceneId
    {
        Boot,
        Home,
        Register,
        Rec,
        Collection,
        Theme,
        Game1
    }
}
