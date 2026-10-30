using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Common.UI
{
    /// <summary>全画面 UI のストレッチ・レターボックス塗りつぶし。</summary>
    public static class GeidaiUiLayout
    {
        public const string CanvasBackdropName = "CanvasBackdrop";

        public static void StretchFull(RectTransform rt)
        {
            if (rt == null) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        /// <summary>SafeArea 外（カメラレターボックス）を同色で埋める。Canvas 直下・SafeArea より下。</summary>
        public static void EnsureCanvasBackdrop(RectTransform anyUiRoot, Color color)
        {
            if (anyUiRoot == null) return;
            var canvas = anyUiRoot.GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var canvasRt = canvas.transform as RectTransform;
            var existing = canvasRt.Find(CanvasBackdropName) as RectTransform;
            if (existing == null)
            {
                var go = new GameObject(CanvasBackdropName, typeof(RectTransform), typeof(Image));
                go.transform.SetParent(canvasRt, false);
                go.transform.SetAsFirstSibling();
                existing = go.GetComponent<RectTransform>();
                var img = go.GetComponent<Image>();
                img.raycastTarget = false;
                img.color = color;
            }
            else
            {
                existing.SetAsFirstSibling();
                existing.GetComponent<Image>().color = color;
            }

            StretchFull(existing);
        }

        public static RectTransform ResolveCanvasOverlayRoot(RectTransform contentInSafeArea)
        {
            if (contentInSafeArea == null) return null;
            var canvas = contentInSafeArea.GetComponentInParent<Canvas>();
            return canvas != null ? canvas.transform as RectTransform : contentInSafeArea;
        }

        public static bool IsPortraitTallScreen =>
            Screen.height > Screen.width * 1.05f;
    }
}
