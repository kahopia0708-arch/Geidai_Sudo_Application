using Geidai.Common.Game;
using Geidai.Common.Library;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Game1
{
    /// <summary>
    /// 旧カエル／おたまプレースホルダーを魔法使い UI から除外する。
    /// </summary>
    public static class Game1LegacyTadpoleMotif
    {
        public static bool IsLegacyTadpole(SoundMatchConfig config, Sprite sprite)
        {
            if (sprite == null || config == null) return false;
            return sprite == config.PlaceholderChoiceSprite
                   || sprite == config.PlaceholderChoiceOpenSprite;
        }

        /// <summary>図鑑楽器として使えるスプライト（旧おたまを除外）。</summary>
        public static Sprite ResolveInstrumentSprite(SoundMatchConfig config, Sprite preferred, Sprite secondary = null)
        {
            if (preferred != null && !IsLegacyTadpole(config, preferred)) return preferred;
            if (secondary != null && !IsLegacyTadpole(config, secondary)) return secondary;
            return FirstCatalogInstrumentSprite(config);
        }

        public static Sprite FirstCatalogInstrumentSprite(SoundMatchConfig config)
        {
            if (config?.CuratedCatalog == null) return null;
            foreach (var item in config.CuratedCatalog.ValidItems())
            {
                if (item?.imageRef == null) continue;
                if (IsLegacyTadpole(config, item.imageRef)) continue;
                return item.imageRef;
            }

            return null;
        }

        public static void ClearFromImage(SoundMatchConfig config, Image image)
        {
            if (image == null) return;
            if (!IsLegacyTadpole(config, image.sprite)) return;
            image.sprite = null;
        }

        public static void ClearFromChoiceViews(RectTransform playArea, SoundMatchConfig config)
        {
            if (playArea == null || config == null) return;
            var choices = playArea.GetComponentsInChildren<ChoiceItemView>(true);
            for (int i = 0; i < choices.Length; i++)
            {
                if (choices[i] == null) continue;
                ClearFromImage(config, choices[i].ChoiceImage);
            }
        }
    }
}
