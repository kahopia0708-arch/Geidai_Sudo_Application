using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Foundation
{
    /// <summary>
    /// ホーム UI の Image 背景を安全に適用する（9-slice border 未設定時は Simple にフォールバック）。
    /// </summary>
    public static class HomeUiImageUtil
    {
        private static Sprite _unityWhiteSprite;

        /// <summary>Unity 組込みの白 UI スプライト（塗りつぶしの最終フォールバック）。</summary>
        public static Sprite UnityWhiteSprite
        {
            get
            {
                if (_unityWhiteSprite == null)
                    _unityWhiteSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                return _unityWhiteSprite;
            }
        }

        public static void ApplyBackground(Image image, Sprite sprite, Color color)
        {
            if (image == null) return;

            image.color = color;
            if (sprite == null)
            {
                ApplySolidFill(image, color);
                return;
            }

            image.sprite = sprite;
            var border = sprite.border;
            bool hasBorder = border.x > 0.01f || border.y > 0.01f || border.z > 0.01f || border.w > 0.01f;
            if (!hasBorder)
            {
                ApplySolidFill(image, color);
                return;
            }

            image.type = Image.Type.Sliced;
        }

        /// <summary>角丸スプライトに依存せず矩形を確実に塗る。</summary>
        public static void ApplySolidFill(Image image, Color color)
        {
            if (image == null) return;
            image.sprite = UnityWhiteSprite;
            image.type = Image.Type.Simple;
            image.color = color;
            image.enabled = true;
        }
    }
}
