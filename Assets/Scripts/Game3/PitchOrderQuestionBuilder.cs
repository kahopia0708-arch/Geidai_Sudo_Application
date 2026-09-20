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
            choiceCount = Math.Clamp(choiceCount, 2, 4);
            centsStep = Math.Max(1, centsStep);

            List<int> cents = BuildPitchValues(
                choiceCount,
                centsStep
            );

            Shuffle(cents, seed);

            // 最初から正解状態になるのを防ぐ
            if (PitchOrderJudge.IsAscending(cents))
            {
                (cents[0], cents[1]) =
                    (cents[1], cents[0]);
            }

            // このゲームでは常に低い → 高い順に並べる
            return new PitchOrderQuestion(
                PitchOrderDirection.Ascending,
                cents
            );
        }

        private static List<int> BuildPitchValues(
            int choiceCount,
            int step)
        {
            switch (choiceCount)
            {
                case 2:
                    return new List<int>
                    {
                        0,
                        step
                    };

                case 3:
                    return new List<int>
                    {
                        -step,
                        0,
                        step
                    };

                case 4:
                    return new List<int>
                    {
                        -step,
                        0,
                        step,
                        step * 2
                    };

                default:
                    return new List<int>
                    {
                        0,
                        step
                    };
            }
        }

        private static void Shuffle(
            List<int> values,
            int seed)
        {
            Random random = new Random(seed);

            for (int i = values.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);

                (values[i], values[j]) =
                    (values[j], values[i]);
            }
        }
    }
}