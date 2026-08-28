using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Foundation
{
    /// <summary>
    /// ホーム UI の Image 背景を安全に適用する（9-slice border 未設定時は Simple にフォールバック）。
    /// </summary>
    public static class HomeUiImageUtil
    {
        private static Sprite _solidFillSprite;

        /// <summary>単色矩形塗り用の白スプライト（Unity 6 では UISprite.psd が使えないため生成）。</summary>
        private static Sprite SolidFillSprite
        {
            get
            {
                if (_solidFillSprite != null) return _solidFillSprite;

                var tex = Texture2D.whiteTexture;
                _solidFillSprite = Sprite.Create(
                    tex,
                    new Rect(0f, 0f, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
                return _solidFillSprite;
            }
        }

        private static Sprite _cachedPillSprite;

        /// <summary>角丸 pill スプライト（カタログ → Resources の順で解決）。</summary>
        public static Sprite ResolvePillSprite(HomeMenuIconCatalog catalog = null)
        {
            if (catalog != null)
            {
                var fromCatalog = catalog.Resolve("pill");
                if (fromCatalog != null) return fromCatalog;
            }

            if (_cachedPillSprite == null)
                _cachedPillSprite = Resources.Load<Sprite>("Geidai/menu_button_pill");

            return _cachedPillSprite;
        }

        /// <summary>入力欄は角丸にせず白矩形で塗る。</summary>
        public static void ApplyInputFill(Image image)
        {
            ApplySolidFill(image, HomeUiTheme.InputFill);
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
            image.sprite = SolidFillSprite;
            image.type = Image.Type.Simple;
            image.color = color;
            image.enabled = true;
        }
    }
}
