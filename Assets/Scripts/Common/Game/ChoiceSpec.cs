using System;

namespace Geidai.Common.Game
{
    /// <summary>
    /// 選択肢（おたまじゃくし）1件のピッチ指定（U6 / domain-entities）。
    /// 音そのものではなく「基準音からのセント差」で表す（再生時に適用・非保存 / FR-19）。
    /// </summary>
    [Serializable]
    public struct ChoiceSpec
    {
        /// <summary>基準音からのピッチオフセット（セント）。</summary>
        public int cents;

        /// <summary>お手本と一致する正解か。</summary>
        public bool isCorrect;

        public ChoiceSpec(int cents, bool isCorrect)
        {
            this.cents = cents;
            this.isCorrect = isCorrect;
        }
    }
}
