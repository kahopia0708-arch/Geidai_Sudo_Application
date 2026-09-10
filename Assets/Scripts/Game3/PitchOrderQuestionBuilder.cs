using System;
using System.Collections.Generic;

namespace Geidai.Game3
{
    public static class PitchOrderQuestionBuilder
    {
        public static PitchOrderQuestion Build(
            int choiceCount,
            int centsStep,
            int seed)
        {
            // Game3仕様：2〜4音
            choiceCount = Math.Max(2, Math.Min(4, choiceCount));

            // 0以下は困るので最低1cent
            centsStep = Math.Max(1, centsStep);

            var rng = new Random(seed);

            // ──────────────────────
            // 1. 音高を作る
            // ──────────────────────

            var cents = new List<int>();

            double center = (choiceCount - 1) / 2.0;

            for (int i = 0; i < choiceCount; i++)
            {
                int value =
                    (int)Math.Round((i - center) * centsStep);

                cents.Add(value);
            }

            // ──────────────────────
            // 2. 高い順 / 低い順をランダム決定
            // ──────────────────────

            PitchOrderDirection direction =
                rng.Next(0, 2) == 0
                    ? PitchOrderDirection.Ascending
                    : PitchOrderDirection.Descending;

            // 3. 最初の並びをシャッフル
            for (int i = cents.Count - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);

                (cents[i], cents[j]) =
                    (cents[j], cents[i]);
            }

            // 4. 最初から正解になっていたら崩す
            bool alreadyCorrect =
                direction == PitchOrderDirection.Ascending
                    ? PitchOrderJudge.IsAscending(cents)
                    : PitchOrderJudge.IsDescending(cents);

            if (alreadyCorrect)
            {
                (cents[0], cents[1]) =
                    (cents[1], cents[0]);
            }

            return new PitchOrderQuestion(
                direction,
                cents
            );
        }
    }
}