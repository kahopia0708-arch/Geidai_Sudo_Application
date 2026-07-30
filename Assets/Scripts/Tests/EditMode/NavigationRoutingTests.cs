using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Services.Navigation;
using Geidai.Foundation;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// ナビゲーションの安全処理テスト（NFR-07 / BR-14 / nfr-design §2）。
    /// 実シーンのロードを伴う経路は Build & Test（PlayMode）で検証する。
    /// EditMode では SceneManager.LoadScene が失敗し Error ログが出るため LogAssert で受容する。
    /// </summary>
    public class NavigationRoutingTests
    {
        [Test]
        public void GoTo_Theme_IsMapped_NotNotFound()
        {
            var nav = new NavigationService();
            LogAssert.Expect(LogType.Error, new Regex(@"\[Navigation\] load failed"));
            // Theme は MCP フォローアップで SceneMap 登録済み。
            // EditMode では LoadScene が失敗して IOError になり得るが、未定義（NotFound）ではない。
            var result = nav.GoTo(SceneId.Theme);
            Assert.AreNotEqual(ResultCode.NotFound, result.Code);
        }

        [Test]
        public void GoTo_DoesNotThrow_OnMappedScene()
        {
            var nav = new NavigationService();
            LogAssert.Expect(LogType.Error, new Regex(@"\[Navigation\] load failed"));
            LogAssert.Expect(LogType.Error, new Regex(@"\[Navigation\] load failed"));
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
            Assert.AreEqual(SceneId.Library, ModuleRouter.ToSceneId(ModuleId.Library));
            Assert.AreEqual(SceneId.Create, ModuleRouter.ToSceneId(ModuleId.Create));
        }
    }
}
