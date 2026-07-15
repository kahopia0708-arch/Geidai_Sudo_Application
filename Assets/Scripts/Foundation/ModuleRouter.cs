using Geidai.Common.Models;

namespace Geidai.Foundation
{
    /// <summary>
    /// ホーム導線（ModuleId）を論理シーン（SceneId）へ変換する（BR-10/13）。
    /// 遷移自体は NavigationService に委譲し、ここは対応表のみを担う（純粋）。
    /// </summary>
    public static class ModuleRouter
    {
        /// <summary>ModuleId に対応する SceneId を返す。</summary>
        public static SceneId ToSceneId(ModuleId moduleId)
        {
            switch (moduleId)
            {
                case ModuleId.Rec: return SceneId.Rec;
                case ModuleId.Collection: return SceneId.Collection;
                case ModuleId.GameSelect: return SceneId.GameSelect;
                case ModuleId.WeeklyTheme: return SceneId.Theme;
                case ModuleId.ProfileEdit: return SceneId.Register;
                default: return SceneId.Home;
            }
        }
    }
}
