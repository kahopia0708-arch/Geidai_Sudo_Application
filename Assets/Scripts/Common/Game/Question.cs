using System;
using System.Collections.Generic;

namespace Geidai.Common.Game
{
    /// <summary>
    /// ①音合わせの1問（U6 / FR-15/19）。お手本（カエル）のピッチと選択肢（おたまじゃくし）のピッチ配列を保持。
    /// 音バッファは持たない（発音は再生時に PitchVariationService が適用 / 非保存）。
    /// </summary>
    [Serializable]
    public class Question
    {
        /// <summary>素材にした保存音の ID（fallback 素材時は空）。</summary>
        public string baseSoundId;

        /// <summary>お手本（カエル）のピッチ（基準音からのセント差）。</summary>
        public int targetCents;

        /// <summary>選択肢（おたまじゃくし）のピッチ指定。</summary>
        public List<ChoiceSpec> choices;

        /// <summary>choices 内の正解 index。</summary>
        public int correctIndex;

        public Question()
        {
            baseSoundId = string.Empty;
            targetCents = 0;
            choices = new List<ChoiceSpec>();
            correctIndex = -1;
        }

        public Question(string baseSoundId, int targetCents, List<ChoiceSpec> choices, int correctIndex)
        {
            this.baseSoundId = baseSoundId ?? string.Empty;
            this.targetCents = targetCents;
            this.choices = choices ?? new List<ChoiceSpec>();
            this.correctIndex = correctIndex;
        }
    }
}
