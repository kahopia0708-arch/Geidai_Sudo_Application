using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Game1
{
    /// <summary>
    /// 口を開けたスプライトと閉じたスプライトを切り替える（タップ確認・答え合わせ再生用）。
    /// </summary>
    public static class Game1MouthVisual
    {
        public static void SetClosed(Image image, Sprite closedSprite)
        {
            if (image == null || closedSprite == null) return;
            image.sprite = closedSprite;
            image.enabled = true;
            Game1CharacterImage.Apply(image);
        }

        public static void SetOpen(Image image, Sprite openSprite, Sprite closedFallback)
        {
            if (image == null) return;
            var sprite = openSprite != null ? openSprite : closedFallback;
            if (sprite == null) return;
            image.sprite = sprite;
            image.enabled = true;
            Game1CharacterImage.Apply(image);
        }

        public static IEnumerator HoldOpenWhile(
            Image image,
            Sprite closedSprite,
            Sprite openSprite,
            IEnumerator whilePlaying)
        {
            SetOpen(image, openSprite, closedSprite);
            if (whilePlaying != null)
            {
                while (whilePlaying.MoveNext())
                    yield return whilePlaying.Current;
            }

            SetClosed(image, closedSprite);
        }
    }
}
