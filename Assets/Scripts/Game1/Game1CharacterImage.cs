using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Game1
{
    /// <summary>
    /// カエル・おたま UI 用 Image の共通設定（透過スプライト前提）。
    /// </summary>
    public static class Game1CharacterImage
    {
        public static void Apply(Image image)
        {
            if (image == null) return;
            image.color = Color.white;
            image.preserveAspect = true;
            image.material = null;
            image.raycastTarget = true;
        }
    }
}
