namespace Geidai.Common.Game
{
    /// <summary>企画どおりの枚数（6 / 8 / 10 / 12 / 18）。</summary>
    public static class MemoryMatchPairPresets
    {
        public static readonly int[] PairCounts = { 3, 4, 5, 6, 9 };

        public static readonly int[] CardCounts = { 6, 8, 10, 12, 18 };

        public static int PairCountForIndex(int presetIndex)
        {
            if (presetIndex < 0 || presetIndex >= PairCounts.Length) return PairCounts[1];
            return PairCounts[presetIndex];
        }

        public static int CardCountForIndex(int presetIndex) => PairCountForIndex(presetIndex) * 2;

        public static int IndexForPairCount(int pairCount)
        {
            for (int i = 0; i < PairCounts.Length; i++)
            {
                if (PairCounts[i] == pairCount) return i;
            }

            return 1;
        }
    }
}
