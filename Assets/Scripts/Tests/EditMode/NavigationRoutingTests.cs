using NUnit.Framework;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Services.Navigation;
using Geidai.Foundation;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// ナビゲーションの安全処理テスト（NFR-07 / BR-14 / nfr-design §2）。
    /// 実シーンのロードを伴う経路は Build & Test（PlayMode）で検証する。
    /// </summary>
    public class NavigationRoutingTests
    {
        [Test]
        public void GoTo_Theme_IsMapped_NotNotFound()
        {
            var nav = new NavigationService();
            // Theme は MCP フォローアップで SceneMap 登録済み。
            // EditMode では LoadScene が失敗して IOError になり得るが、未定義（NotFound）ではない。
            var result = nav.GoTo(SceneId.Theme);
            Assert.AreNotEqual(ResultCode.NotFound, result.Code);
        }

        [Test]
        public void GoTo_DoesNotThrow_OnMappedScene()
        {
            var nav = new NavigationService();
            Assert.DoesNotThrow(() => nav.GoTo(SceneId.Home));
            Assert.DoesNotThrow(() => nav.GoTo(SceneId.Game1));
        }

        [Test]
        public void ModuleRouter_MapsModulesToScenes()
        {
            Assert.AreEqual(SceneId.Rec, ModuleRouter.ToSceneId(ModuleId.Rec));
            Assert.AreEqual(SceneId.Collection, ModuleRouter.ToSceneId(ModuleId.Collection));
            Assert.AreEqual(SceneId.GameSelect, ModuleRouter.ToSceneId(ModuleId.GameSelect));
            Assert.AreEqual(SceneId.Theme, ModuleRouter.ToSceneId(ModuleId.WeeklyTheme));
            Assert.AreEqual(SceneId.Register, ModuleRouter.ToSceneId(ModuleId.ProfileEdit));
        }
    }
}
