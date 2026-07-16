using System.Collections.Generic;

namespace Geidai.Common.Game
{
    /// <summary>
    /// ①音合わせの出題生成（U6 / P2 / NFR-U6-03/04）。純粋関数（副作用なし・決定的）。
    /// 音は作らず「ピッチのメタ」を返す（発音は再生時に PitchVariationService が適用 / 非保存）。
    /// 不変条件：正解ちょうど1つ・不正解は centsStep 以上離れる・選択肢は重複なし・同一 seed で決定的。
    /// </summary>
    public static class QuestionBuilder
    {
        /// <summary>1 問を生成する。</summary>
        public static Question Build(string baseSoundId, SoundMatchConfig config, DifficultyLevel diff, int seed)
        {
            int choiceCount = config != null ? config.ChoiceCount : 3;
            int centsStep = diff.centsStep >= 1 ? diff.centsStep : 1;

            var rng = new System.Random(seed);

            // お手本ピッチ（基準音からのオフセット）。-2..+2 段のいずれか（0 含む）。
            int targetCents = (rng.Next(0, 5) - 2) * centsStep;

            // 選択肢セント（正解＝targetCents ＋ target±k*step の相異なる不正解）。
            var used = new HashSet<int> { targetCents };
            var cents = new List<int> { targetCents };
            int k = 1;
            while (cents.Count < choiceCount)
            {
                int plus = targetCents + k * centsStep;
                if (used.Add(plus)) cents.Add(plus);
                if (cents.Count >= choiceCount) break;

                int minus = targetCents - k * centsStep;
                if (used.Add(minus)) cents.Add(minus);
                k++;
            }

            // Fisher-Yates シャッフル（seed 依存・決定的）。
            for (int i = cents.Count - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                int tmp = cents[i];
                cents[i] = cents[j];
                cents[j] = tmp;
            }

            var choices = new List<ChoiceSpec>(cents.Count);
            int correctIndex = -1;
            for (int i = 0; i < cents.Count; i++)
            {
                bool isCorrect = cents[i] == targetCents;
                if (isCorrect) correctIndex = i;
                choices.Add(new ChoiceSpec(cents[i], isCorrect));
            }

            return new Question(baseSoundId, targetCents, choices, correctIndex);
        }

        /// <summary>
        /// 1 ゲーム分の問題列を生成する（questionCount 問）。各問は seed+i で決定的。
        /// </summary>
        public static List<Question> BuildQuestions(string baseSoundId, SoundMatchConfig config, int difficultyIndex, int seed)
        {
            var list = new List<Question>();
            if (config == null) return list;

            DifficultyLevel diff = config.GetDifficulty(difficultyIndex);
            int count = config.QuestionCount;
            for (int i = 0; i < count; i++)
                list.Add(Build(baseSoundId, config, diff, unchecked(seed + i)));
            return list;
        }
    }
}
