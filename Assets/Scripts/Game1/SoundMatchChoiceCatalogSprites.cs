using System.Collections.Generic;
using Geidai.Common.Library;
using UnityEngine;

namespace Geidai.Game1
{
    /// <summary>
    /// 音合わせの選択肢 UI：図鑑の imageRef と clip（soundId）をセットで割り当てる。
    /// </summary>
    public static class SoundMatchChoiceCatalogSprites
    {
        public static SoundMatchChoicePresentation[] BuildForQuestion(
            CuratedSoundCatalog catalog,
            Sprite targetDisplaySprite,
            string targetSoundId,
            int choiceCount,
            int correctIndex,
            int seed)
        {
            var result = new SoundMatchChoicePresentation[Mathf.Max(0, choiceCount)];
            var pool = CollectPool(catalog, targetSoundId, targetDisplaySprite);
            var rng = new System.Random(seed);

            for (int i = 0; i < result.Length; i++)
            {
                if (i == correctIndex)
                {
                    result[i] = new SoundMatchChoicePresentation(
                        targetDisplaySprite,
                        targetSoundId ?? string.Empty);
                    continue;
                }

                var pick = PickDistinct(pool, targetSoundId, targetDisplaySprite, rng);
                result[i] = pick != null
                    ? new SoundMatchChoicePresentation(
                        pick.imageRef,
                        pick.id)
                    : new SoundMatchChoicePresentation(targetDisplaySprite, targetSoundId ?? string.Empty);
            }

            return result;
        }

        private static List<CuratedSoundDefinition> CollectPool(
            CuratedSoundCatalog catalog,
            string excludeSoundId,
            Sprite excludeSprite)
        {
            var pool = new List<CuratedSoundDefinition>();
            if (catalog == null) return pool;

            foreach (var item in catalog.ValidItems())
            {
                if (item == null) continue;
                if (item.clipRef == null) continue;
                if (item.imageRef == null) continue;
                if (!string.IsNullOrEmpty(excludeSoundId) && item.id == excludeSoundId) continue;
                if (excludeSprite != null && item.imageRef == excludeSprite) continue;
                if (pool.Exists(p => p.id == item.id)) continue;
                pool.Add(item);
            }

            return pool;
        }

        private static CuratedSoundDefinition PickDistinct(
            List<CuratedSoundDefinition> pool,
            string avoidSoundId,
            Sprite avoidSprite,
            System.Random rng)
        {
            if (pool == null || pool.Count == 0) return null;

            for (int attempt = 0; attempt < 12; attempt++)
            {
                var pick = pool[rng.Next(0, pool.Count)];
                if (pick == null) continue;
                if (!string.IsNullOrEmpty(avoidSoundId) && pick.id == avoidSoundId) continue;
                if (avoidSprite != null && pick.imageRef == avoidSprite) continue;
                return pick;
            }

            return pool[0];
        }
    }
}
