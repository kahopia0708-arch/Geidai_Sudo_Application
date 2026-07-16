using System;
using System.Collections.Generic;
using NUnit.Framework;
using FsCheck;
using UnityEngine;
using Geidai.Common.Game;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// QuestionBuilder（純粋出題生成）の性質ベーステスト（U6 / P2 / NFR-U6-04）。
    /// 不変条件：正解ちょうど1つ・correctIndex 一致・選択肢重複なし・不正解は centsStep 以上離れる・
    /// 選択肢数一致・同一 seed で決定的。
    /// </summary>
    public class QuestionBuilderTests
    {
        private static SoundMatchConfig MakeConfig(int choiceCount)
        {
            var config = ScriptableObject.CreateInstance<SoundMatchConfig>();
            config.SetValues(5, choiceCount,
                new List<DifficultyLevel> { new DifficultyLevel("t", 100) }, null);
            return config;
        }

        [Test]
        public void Correct_Is_Exactly_One_And_Index_Matches()
        {
            Prop.ForAll<int, int, int>((seed, rawChoice, rawStep) =>
            {
                int choiceCount = 2 + (Math.Abs(rawChoice) % 7); // 2..8
                int step = 1 + (Math.Abs(rawStep) % 300);        // 1..300
                var config = MakeConfig(choiceCount);
                var diff = new DifficultyLevel("t", step);

                var q = QuestionBuilder.Build("base", config, diff, seed);

                int correctCount = 0;
                for (int i = 0; i < q.choices.Count; i++)
                    if (q.choices[i].isCorrect) correctCount++;

                bool indexInRange = q.correctIndex >= 0 && q.correctIndex < q.choices.Count;
                bool indexIsCorrect = indexInRange && q.choices[q.correctIndex].isCorrect;
                bool correctMatchesTarget = indexInRange && q.choices[q.correctIndex].cents == q.targetCents;

                return correctCount == 1 && indexIsCorrect && correctMatchesTarget;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void Choices_Are_Distinct_And_ChoiceCount_Matches()
        {
            Prop.ForAll<int, int, int>((seed, rawChoice, rawStep) =>
            {
                int choiceCount = 2 + (Math.Abs(rawChoice) % 7);
                int step = 1 + (Math.Abs(rawStep) % 300);
                var config = MakeConfig(choiceCount);
                var diff = new DifficultyLevel("t", step);

                var q = QuestionBuilder.Build("base", config, diff, seed);

                var set = new HashSet<int>();
                for (int i = 0; i < q.choices.Count; i++)
                    if (!set.Add(q.choices[i].cents)) return false;

                return q.choices.Count == choiceCount;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void Distractors_Are_At_Least_CentsStep_Away()
        {
            Prop.ForAll<int, int, int>((seed, rawChoice, rawStep) =>
            {
                int choiceCount = 2 + (Math.Abs(rawChoice) % 7);
                int step = 1 + (Math.Abs(rawStep) % 300);
                var config = MakeConfig(choiceCount);
                var diff = new DifficultyLevel("t", step);

                var q = QuestionBuilder.Build("base", config, diff, seed);

                for (int i = 0; i < q.choices.Count; i++)
                {
                    if (q.choices[i].isCorrect) continue;
                    if (Math.Abs(q.choices[i].cents - q.targetCents) < step) return false;
                }
                return true;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void Is_Deterministic_For_Same_Seed()
        {
            Prop.ForAll<int, int, int>((seed, rawChoice, rawStep) =>
            {
                int choiceCount = 2 + (Math.Abs(rawChoice) % 7);
                int step = 1 + (Math.Abs(rawStep) % 300);
                var config = MakeConfig(choiceCount);
                var diff = new DifficultyLevel("t", step);

                var a = QuestionBuilder.Build("base", config, diff, seed);
                var b = QuestionBuilder.Build("base", config, diff, seed);

                if (a.targetCents != b.targetCents) return false;
                if (a.correctIndex != b.correctIndex) return false;
                if (a.choices.Count != b.choices.Count) return false;
                for (int i = 0; i < a.choices.Count; i++)
                {
                    if (a.choices[i].cents != b.choices[i].cents) return false;
                    if (a.choices[i].isCorrect != b.choices[i].isCorrect) return false;
                }
                return true;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void BuildQuestions_Returns_QuestionCount_Items()
        {
            var config = ScriptableObject.CreateInstance<SoundMatchConfig>();
            config.SetValues(4, 3,
                new List<DifficultyLevel> { new DifficultyLevel("ふつう", 100) }, null);

            var list = QuestionBuilder.BuildQuestions("base", config, 0, 123);
            Assert.AreEqual(4, list.Count);
            foreach (var q in list)
            {
                Assert.AreEqual(3, q.choices.Count);
                Assert.IsTrue(q.correctIndex >= 0 && q.correctIndex < 3);
            }
        }

        [Test]
        public void BuildQuestions_NullConfig_Returns_Empty()
        {
            var list = QuestionBuilder.BuildQuestions("base", null, 0, 1);
            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);
        }
    }
}
