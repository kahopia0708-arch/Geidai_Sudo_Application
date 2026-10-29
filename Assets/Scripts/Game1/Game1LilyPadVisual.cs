using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Game1
{
    /// <summary>
    /// 睡蓮 UI：スプライト内の疑似影を除き、薄い半透明黒の楕円影を下に付ける。
    /// </summary>
    public static class Game1LilyPadVisual
    {
        public const string PadGraphicName = "PadGraphic";
        public const string ShadowChildName = "LilyPadShadow";

        private static Sprite _softBlackShadowSprite;

        public static void Setup(RectTransform root, Sprite padSprite)
        {
            if (root == null) return;
            MigrateLegacyRootImage(root, padSprite);
            var pad = EnsurePadGraphic(root, padSprite);
            if (pad != null)
            {
                Game1CharacterImage.Apply(pad);
                pad.raycastTarget = false;
            }

            EnsureShadow(root);
        }

        private static void MigrateLegacyRootImage(RectTransform root, Sprite padSprite)
        {
            var rootImg = root.GetComponent<Image>();
            if (rootImg == null) return;
            if (root.Find(PadGraphicName) != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(rootImg);
                else
                    Object.DestroyImmediate(rootImg);
                return;
            }

            var padGo = new GameObject(PadGraphicName, typeof(RectTransform), typeof(Image));
            padGo.transform.SetParent(root, false);
            Stretch(padGo.GetComponent<RectTransform>());
            var padImg = padGo.GetComponent<Image>();
            padImg.sprite = rootImg.sprite != null ? rootImg.sprite : padSprite;
            padImg.color = rootImg.color;

            if (Application.isPlaying)
                Object.Destroy(rootImg);
            else
                Object.DestroyImmediate(rootImg);
        }

        private static Image EnsurePadGraphic(RectTransform root, Sprite padSprite)
        {
            var padTr = root.Find(PadGraphicName) as RectTransform;
            Image padImg;
            if (padTr == null)
            {
                var go = new GameObject(PadGraphicName, typeof(RectTransform), typeof(Image));
                go.transform.SetParent(root, false);
                padTr = go.GetComponent<RectTransform>();
                Stretch(padTr);
                padImg = go.GetComponent<Image>();
            }
            else
            {
                padImg = padTr.GetComponent<Image>();
            }

            if (padSprite != null && padImg != null)
            {
                padImg.sprite = padSprite;
                padImg.color = Color.white;
                padImg.preserveAspect = true;
            }

            padTr.SetAsLastSibling();
            return padImg;
        }

        private static void EnsureShadow(RectTransform root)
        {
            var shadowTr = root.Find(ShadowChildName) as RectTransform;
            Image shadowImg;
            if (shadowTr == null)
            {
                var go = new GameObject(
                    ShadowChildName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                go.transform.SetParent(root, false);
                shadowTr = go.GetComponent<RectTransform>();
                shadowImg = go.GetComponent<Image>();
            }
            else
            {
                shadowImg = shadowTr.GetComponent<Image>();
            }

            shadowImg.sprite = GetSoftBlackShadowSprite();
            shadowImg.color = new Color(0f, 0f, 0f, Game1StoryboardLayout.LilyPadShadowAlpha);
            shadowImg.raycastTarget = false;
            shadowImg.preserveAspect = true;

            shadowTr.anchorMin = new Vector2(0.5f, 0.5f);
            shadowTr.anchorMax = new Vector2(0.5f, 0.5f);
            shadowTr.pivot = new Vector2(0.5f, 0.5f);
            shadowTr.anchoredPosition = Game1StoryboardLayout.LilyPadShadowOffset;
            shadowTr.sizeDelta = Game1StoryboardLayout.LilyPadShadowSize;
            shadowTr.SetAsFirstSibling();
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Sprite GetSoftBlackShadowSprite()
        {
            if (_softBlackShadowSprite != null) return _softBlackShadowSprite;

            const int w = 128;
            const int h = 40;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            float cx = w * 0.5f;
            float cy = h * 0.5f;
            float rx = w * 0.46f;
            float ry = h * 0.48f;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - cx) / rx;
                    float dy = (y - cy) / ry;
                    float d = dx * dx + dy * dy;
                    float a = d <= 1f ? Mathf.Clamp01(1f - d) : 0f;
                    tex.SetPixel(x, y, new Color(0f, 0f, 0f, a));
                }
            }

            tex.Apply();
            _softBlackShadowSprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, w, h),
                new Vector2(0.5f, 0.5f),
                100f);
            return _softBlackShadowSprite;
        }
    }
}
