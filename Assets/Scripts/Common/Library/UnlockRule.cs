using System;

namespace Geidai.Common.Library
{
    /// <summary>1素材の解除条件（データ駆動 / BR-UNLOCK-01）。</summary>
    [Serializable]
    public class UnlockRule
    {
        public string soundId;
        public UnlockConditionKind kind;
        public string gameKey;
        public string recordingChallengeKey;
        /// <summary>Combined 時: true=全条件、false=いずれか（BR-UNLOCK-02）。</summary>
        public bool requireAll = true;

        public bool IsValid => !string.IsNullOrWhiteSpace(soundId);
    }
}
