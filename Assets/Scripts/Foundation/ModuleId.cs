namespace Geidai.Foundation
{
    /// <summary>
    /// ホームから遷移可能な MVP モジュール（domain-entities.md / US-NAV-02）。
    /// Place・テストは含めない（BR-10/11）。ProfileEdit は登録シーンの編集モードを開く導線。
    /// </summary>
    public enum ModuleId
    {
        Rec,
        Collection,
        GameSelect,
        WeeklyTheme,
        ProfileEdit,
        Library,
        Create
    }
}
