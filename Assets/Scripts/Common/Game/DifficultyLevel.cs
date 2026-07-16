using System;

namespace Geidai.Common.Game
{
    /// <summary>
    /// 難易度1段階（U6 / FR-18）。聞き分けの難しさを「選択肢間の最小ピッチ間隔（セント）」で表す。
    /// 例: かんたん=200 / ふつう=100 / むずかしい=50 / とても難しい=20。値は SO で Sさん が調整可能。
    /// </summary>
    [Serializable]
    public struct DifficultyLevel
    {
        /// <summary>表示名（例: "ふつう"）。</summary>
        public string label;

        /// <summary>選択肢間の最小ピッチ間隔（セント。小さいほど難しい）。</summary>
        public int centsStep;

        public DifficultyLevel(string label, int centsStep)
        {
            this.label = label;
            this.centsStep = centsStep;
        }
    }
}
