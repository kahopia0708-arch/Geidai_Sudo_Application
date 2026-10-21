using System;
using System.Collections.Generic;
using Geidai.Common.Library;

namespace Geidai.Common.Game
{
    public sealed class MemoryMatchDeckBuildResult
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; }
        public List<MemoryMatchCardSpec> Cards { get; private set; } = new List<MemoryMatchCardSpec>();

        public static MemoryMatchDeckBuildResult Fail(string message) =>
            new MemoryMatchDeckBuildResult { IsSuccess = false, Message = message ?? string.Empty };

        public static MemoryMatchDeckBuildResult Ok(List<MemoryMatchCardSpec> cards) =>
            new MemoryMatchDeckBuildResult
            {
                IsSuccess = true,
                Message = string.Empty,
                Cards = cards ?? new List<MemoryMatchCardSpec>()
            };
    }

    /// <summary>
    /// 図鑑から神経衰弱の山札を組む。
    /// 1ペア＝同じ音（同一 soundId）のカード2枚。pairKey は「この音を山札候補に含める」目印として使う。
    /// </summary>
    public static class MemoryMatchDeckBuilder
    {
        public static MemoryMatchDeckBuildResult Build(IReadOnlyList<CuratedSoundDefinition> validItems, int pairCount, int seed)
        {
            pairCount = Math.Max(1, pairCount);
            if (validItems == null || validItems.Count == 0)
                return MemoryMatchDeckBuildResult.Fail("おとの ずかんが ないよ");

            var pool = new List<CuratedSoundDefinition>();
            for (int i = 0; i < validItems.Count; i++)
            {
                var item = validItems[i];
                if (item == null || !item.IsValid) continue;
                if (PairKeyTokenizer.Tokenize(item.pairKey).Count == 0) continue;
                pool.Add(item);
            }

            if (pool.Count == 0)
                return MemoryMatchDeckBuildResult.Fail("神経衰弱用の おとが まだ ないよ（pairKey を 入れてね）");

            var rng = new Random(seed);
            Shuffle(pool, rng);

            int targetPairs = Math.Min(pairCount, pool.Count);
            var cards = new List<MemoryMatchCardSpec>(targetPairs * 2);

            for (int p = 0; p < targetPairs; p++)
            {
                var sound = pool[p];
                string groupId = sound.id;
                var spec = new MemoryMatchCardSpec(
                    sound.id,
                    sound.displayName,
                    groupId,
                    sound.clipRef,
                    sound.imageRef);
                cards.Add(spec);
                cards.Add(spec);
            }

            Shuffle(cards, rng);
            return MemoryMatchDeckBuildResult.Ok(cards);
        }

        private static void Shuffle<T>(List<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
