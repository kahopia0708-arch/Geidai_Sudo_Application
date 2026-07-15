using NUnit.Framework;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Services.Navigation;
using Geidai.Foundation;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// ナビゲーションの安全処理テスト（NFR-07 / BR-14 / nfr-design §2）。
    /// 未登録シーンへの遷移は例外を投げず NotFound を返すことを確認する。
    /// 実シーンのロードを伴う経路は Build & Test（PlayMode）で検証する。
    /// </summary>
    public class NavigationRoutingTests
    {
        [Test]
        public void GoTo_UnmappedScene_ReturnsNotFound()
        {
            var nav = new NavigationService();

            // Theme は U5 でシーン整備するまで未登録（安全に NotFound を返す）。
            var result = nav.GoTo(SceneId.Theme);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ResultCode.NotFound, result.Code);
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
