using Geidai.Common.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

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

        [Test]
        public void ApplyFittedToArea_PortraitNarrow_KeepsPresetColumnsForEighteenCards()
        {
            var areaGo = new GameObject("area");
            var area = areaGo.AddComponent<RectTransform>();
            area.sizeDelta = new Vector2(420f, 900f);

            var gridGo = new GameObject("grid");
            var grid = gridGo.AddComponent<GridLayoutGroup>();
            grid.spacing = new Vector2(10f, 10f);

            MemoryMatchGridLayout.ApplyFittedToArea(grid, area, 18);

            Assert.AreEqual(6, grid.constraintCount);
            Assert.LessOrEqual(grid.cellSize.x * 6 + grid.spacing.x * 5, area.rect.width + 0.5f);

            Object.DestroyImmediate(gridGo);
            Object.DestroyImmediate(areaGo);
        }

        [Test]
        public void ApplyFittedToArea_LandscapeWide_KeepsPresetColumnsForTwelveCards()
        {
            var areaGo = new GameObject("area");
            var area = areaGo.AddComponent<RectTransform>();
            area.sizeDelta = new Vector2(900f, 420f);

            var gridGo = new GameObject("grid");
            var grid = gridGo.AddComponent<GridLayoutGroup>();

            MemoryMatchGridLayout.ApplyFittedToArea(grid, area, 12);

            Assert.AreEqual(4, grid.constraintCount);

            Object.DestroyImmediate(gridGo);
            Object.DestroyImmediate(areaGo);
        }
    }
}
