using Geidai.Common.Library;
using Geidai.Common.Results;

namespace Geidai.Services.Progression
{
    /// <summary>
    /// 進行イベント受付→解除判定→UnlockState 更新（U7 / FR-22/23）。
    /// 通貨・XP・ライフは扱わない。
    /// </summary>
    public interface IProgressionService
    {
        Result NotifyGameCleared(string gameKey);
        Result NotifyRecordingChallenge(string challengeKey);
        Result ApplyInitialUnlocks();
        UnlockState CurrentUnlockState { get; }
        /// <summary>ディスクから再読込してキャッシュを更新する。</summary>
        Result Reload();
    }
}
