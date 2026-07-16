using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Geidai.Common.Game;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// SoundMatchConfig のクランプ・フォールバックの単体テスト（U6 / BR-GAME1-32 / NFR-U6-04）。
    /// </summary>
    public class SoundMatchConfigTests
    {
        private static SoundMatchConfig Make(int q, int c, List<DifficultyLevel> diffs)
        {
            var config = ScriptableObject.CreateInstance<SoundMatchConfig>();
            config.SetValues(q, c, diffs, null);
            return config;
        }

        [Test]
        public void QuestionCount_Is_Clamped_To_Min_One()
        {
            Assert.AreEqual(1, Make(0, 3, null).QuestionCount);
            Assert.AreEqual(1, Make(-10, 3, null).QuestionCount);
            Assert.AreEqual(7, Make(7, 3, null).QuestionCount);
        }

        [Test]
        public void ChoiceCount_Is_Clamped_To_Min_Two()
        {
            Assert.AreEqual(2, Make(5, 1, null).ChoiceCount);
            Assert.AreEqual(2, Make(5, 0, null).ChoiceCount);
            Assert.AreEqual(2, Make(5, -3, null).ChoiceCount);
            Assert.AreEqual(4, Make(5, 4, null).ChoiceCount);
        }

        [Test]
        public void GetDifficulty_Clamps_Index_Into_Range()
        {
            var diffs = new List<DifficultyLevel>
            {
                new DifficultyLevel("A", 200),
                new DifficultyLevel("B", 100),
                new DifficultyLevel("C", 50),
            };
            var config = Make(5, 3, diffs);

            Assert.AreEqual(200, config.GetDifficulty(-5).centsStep);
            Assert.AreEqual(50, config.GetDifficulty(99).centsStep);
            Assert.AreEqual(100, config.GetDifficulty(1).centsStep);
        }

        [Test]
        public void GetDifficulty_Empty_List_Returns_Default()
        {
            var config = Make(5, 3, new List<DifficultyLevel>());
            var d = config.GetDifficulty(0);
            Assert.AreEqual(100, d.centsStep);
        }

        [Test]
        public void GetDifficulty_Clamps_CentsStep_To_Min_One()
        {
            var diffs = new List<DifficultyLevel> { new DifficultyLevel("weird", 0) };
            var config = Make(5, 3, diffs);
            Assert.AreEqual(1, config.GetDifficulty(0).centsStep);
        }

        [Test]
        public void FallbackClip_Defaults_To_Null()
        {
            var config = Make(5, 3, null);
            Assert.IsNull(config.FallbackClip);
        }
    }
}
