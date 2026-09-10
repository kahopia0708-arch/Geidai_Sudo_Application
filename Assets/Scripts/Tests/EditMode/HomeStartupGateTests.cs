using NUnit.Framework;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Foundation;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// GeidaiHome 起動ゲートの分岐テスト（FR-HOME-06）。
    /// </summary>
    public class HomeStartupGateTests
    {
        [Test]
        public void ShouldStayOnHome_WhenProfileExists()
        {
            var load = Result<UserProfile>.Ok(new UserProfile(2010, "かほ"));
            Assert.IsTrue(HomeStartupGate.ShouldStayOnHome(load));
            Assert.AreEqual(SceneId.Home, HomeStartupGate.Evaluate(load).Target);
        }

        [Test]
        public void ShouldRedirectRegister_WhenProfileNotFound()
        {
            var load = Result<UserProfile>.Fail(ResultCode.NotFound, "not found");
            Assert.IsFalse(HomeStartupGate.ShouldStayOnHome(load));
            Assert.AreEqual(SceneId.Register, HomeStartupGate.Evaluate(load).Target);
        }

        [Test]
        public void ShouldRedirectRegisterWithWarning_WhenProfileCorrupted()
        {
            var load = Result<UserProfile>.Fail(ResultCode.Corrupted, "corrupted");
            var decision = HomeStartupGate.Evaluate(load);
            Assert.AreEqual(SceneId.Register, decision.Target);
            Assert.IsTrue(decision.ShowFallbackWarning);
        }
    }
}
