using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Common.Game
{
    /// <summary>カード枚数に応じたグリッド列数・セルサイズ。</summary>
    public static class MemoryMatchGridLayout
    {
        public static int GetColumnCount(int totalCards)
        {
            totalCards = Mathf.Max(2, totalCards);
            switch (totalCards)
            {
                case 6: return 3;
                case 8: return 4;
                case 10: return 5;
                case 12: return 4;
                case 18: return 6;
                default:
                    if (totalCards <= 8) return 4;
                    if (totalCards <= 12) return 4;
                    return 6;
            }
        }

        public static Vector2 GetCellSize(int totalCards)
        {
            totalCards = Mathf.Max(2, totalCards);
            if (totalCards >= 18) return new Vector2(118, 148);
            if (totalCards >= 12) return new Vector2(132, 162);
            if (totalCards >= 10) return new Vector2(138, 168);
            return new Vector2(150, 180);
        }

        public static void Apply(GridLayoutGroup grid, int totalCards)
        {
            if (grid == null) return;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = GetColumnCount(totalCards);
            grid.cellSize = GetCellSize(totalCards);
            grid.childAlignment = TextAnchor.UpperCenter;
        }

        /// <summary>表示領域に合わせてセルだけ拡大縮小（列数は <see cref="GetColumnCount"/> 固定）。</summary>
        public static void ApplyFittedToArea(GridLayoutGroup grid, RectTransform area, int totalCards)
        {
            if (grid == null || area == null)
            {
                Apply(grid, totalCards);
                return;
            }

            float padW = grid.padding.left + grid.padding.right;
            float padH = grid.padding.top + grid.padding.bottom;
            float availW = area.rect.width - padW;
            float availH = area.rect.height - padH;
            if (availW < 40f || availH < 40f)
            {
                Apply(grid, totalCards);
                return;
            }

            int cols = GetColumnCount(totalCards);
            int rows = Mathf.Max(1, (totalCards + cols - 1) / cols);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = cols;
            grid.childAlignment = TextAnchor.UpperCenter;
            if (grid.spacing.x < 1f) grid.spacing = new Vector2(10f, 10f);

            float gapX = grid.spacing.x * (cols - 1);
            float gapY = grid.spacing.y * (rows - 1);
            float maxCellW = (availW - gapX) / cols;
            float maxCellH = (availH - gapY) / rows;
            float cellW = maxCellW;
            float cellH = maxCellH;
            const float aspect = 5f / 6f;
            if (cellW / aspect <= cellH)
                cellH = cellW / aspect;
            else
                cellW = cellH * aspect;

            cellW = Mathf.Min(cellW, maxCellW);
            cellH = Mathf.Min(cellH, maxCellH);
            if (cellW / aspect <= cellH)
                cellH = cellW / aspect;
            else
                cellW = cellH * aspect;

            cellW = Mathf.Max(1f, Mathf.Min(cellW, maxCellW));
            cellH = Mathf.Max(1f, Mathf.Min(cellH, maxCellH));

            grid.cellSize = new Vector2(cellW, cellH);
        }
    }
}
