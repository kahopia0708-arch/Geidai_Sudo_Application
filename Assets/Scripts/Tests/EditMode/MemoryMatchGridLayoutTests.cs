using Geidai.Common.Game;
using NUnit.Framework;
using UnityEngine;

namespace Geidai.Tests.EditMode
{
    public class MemoryMatchGridLayoutTests
    {
        [TestCase(6, 3)]
        [TestCase(8, 4)]
        [TestCase(10, 5)]
        [TestCase(12, 4)]
        [TestCase(18, 6)]
        public void GetColumnCount_MatchesExhibitionPresets(int cards, int expectedColumns)
        {
            Assert.AreEqual(expectedColumns, MemoryMatchGridLayout.GetColumnCount(cards));
        }

        [Test]
        public void PairPresets_CardCountsAreDoublePairs()
        {
            for (int i = 0; i < MemoryMatchPairPresets.PairCounts.Length; i++)
            {
                Assert.AreEqual(
                    MemoryMatchPairPresets.PairCounts[i] * 2,
                    MemoryMatchPairPresets.CardCounts[i]);
            }
        }
    }
}
