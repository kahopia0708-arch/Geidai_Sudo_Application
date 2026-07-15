using NUnit.Framework;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Foundation;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// StartupRouter の分岐網羅テスト（NFR-09 / BR-01〜04）。
    /// 純粋関数のため副作用なしで起動遷移決定を検証する。
    /// </summary>
    public class StartupRouterTests
    {
        [Test]
        public void Resolve_WithProfile_GoesHome_NoWarning()
        {
            var load = Result<UserProfile>.Ok(new UserProfile(2000, "ねこ"));

            var decision = StartupRouter.Resolve(load);

            Assert.AreEqual(SceneId.Home, decision.Target);
            Assert.IsFalse(decision.ShowFallbackWarning);
        }

        [Test]
        public void Resolve_NotFound_GoesRegister_NoWarning()
        {
            var load = Result<UserProfile>.Fail(ResultCode.NotFound, "not found");

            var decision = StartupRouter.Resolve(load);

            Assert.AreEqual(SceneId.Register, decision.Target);
            Assert.IsFalse(decision.ShowFallbackWarning);
        }

        [Test]
        public void Resolve_Corrupted_GoesRegister_WithWarning()
        {
            var load = Result<UserProfile>.Fail(ResultCode.Corrupted, "corrupted");

            var decision = StartupRouter.Resolve(load);

            Assert.AreEqual(SceneId.Register, decision.Target);
            Assert.IsTrue(decision.ShowFallbackWarning);
        }

        [Test]
        public void Resolve_IOError_GoesRegister_WithWarning()
        {
            var load = Result<UserProfile>.Fail(ResultCode.IOError, "io error");

            var decision = StartupRouter.Resolve(load);

            Assert.AreEqual(SceneId.Register, decision.Target);
            Assert.IsTrue(decision.ShowFallbackWarning);
        }
    }
}
