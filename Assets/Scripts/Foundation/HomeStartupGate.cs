using Geidai.Common.Models;
using Geidai.Common.Results;

namespace Geidai.Foundation
{
    /// <summary>
    /// GeidaiHome 入場時のプロフィールゲート（StartupRouter 再利用 / FR-HOME-06）。
    /// </summary>
    public static class HomeStartupGate
    {
        public static StartupDecision Evaluate(Result<UserProfile> loadResult)
        {
            return StartupRouter.Resolve(loadResult);
        }

        public static bool ShouldStayOnHome(Result<UserProfile> loadResult)
        {
            return Evaluate(loadResult).Target == SceneId.Home;
        }
    }
}
