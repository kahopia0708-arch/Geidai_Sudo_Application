using Geidai.Common.Models;
using Geidai.Common.Results;

namespace Geidai.Foundation
{
    /// <summary>
    /// 起動判定の結果（遷移先＋フォールバック警告の要否）。
    /// </summary>
    public struct StartupDecision
    {
        public SceneId Target;
        public bool ShowFallbackWarning;

        public StartupDecision(SceneId target, bool showFallbackWarning)
        {
            Target = target;
            ShowFallbackWarning = showFallbackWarning;
        }
    }

    /// <summary>
    /// プロフィール読込結果から初回起動の遷移先を決める純粋関数（nfr-design §1 / BR-01〜04）。
    /// 副作用なし＝EditMode テストで分岐網羅可能（NFR-09）。
    /// - 成功かつ値あり           → Home（警告なし）
    /// - NotFound（未登録＝初回）  → Register（警告なし）
    /// - Corrupted/IOError/その他 → Register（警告あり＝安全誘導 / BR-04）
    /// </summary>
    public static class StartupRouter
    {
        public static StartupDecision Resolve(Result<UserProfile> loadResult)
        {
            if (loadResult.IsSuccess && loadResult.Value != null)
            {
                return new StartupDecision(SceneId.Home, false);
            }

            if (loadResult.Code == ResultCode.NotFound)
            {
                return new StartupDecision(SceneId.Register, false);
            }

            // Corrupted / IOError / Unknown などは破損とみなし、警告のうえ登録へ安全誘導する。
            return new StartupDecision(SceneId.Register, true);
        }
    }
}
