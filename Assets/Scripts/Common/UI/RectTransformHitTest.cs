using UnityEngine;

namespace Geidai.Common.UI
{
    /// <summary>uGUI の RectTransform 同士のスクリーン空間での当たり判定。</summary>
    public static class RectTransformHitTest
    {
        public static Rect GetScreenRect(RectTransform rectTransform, Camera eventCamera)
        {
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            float xMin = corners[0].x;
            float xMax = corners[0].x;
            float yMin = corners[0].y;
            float yMax = corners[0].y;
            for (int i = 1; i < 4; i++)
            {
                xMin = Mathf.Min(xMin, corners[i].x);
                xMax = Mathf.Max(xMax, corners[i].x);
                yMin = Mathf.Min(yMin, corners[i].y);
                yMax = Mathf.Max(yMax, corners[i].y);
            }

            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public static bool Overlaps(RectTransform a, RectTransform b, Camera eventCamera, float minOverlapArea)
        {
            if (a == null || b == null) return false;
            var ra = GetScreenRect(a, eventCamera);
            var rb = GetScreenRect(b, eventCamera);
            if (!ra.Overlaps(rb, true)) return false;

            float xMin = Mathf.Max(ra.xMin, rb.xMin);
            float xMax = Mathf.Min(ra.xMax, rb.xMax);
            float yMin = Mathf.Max(ra.yMin, rb.yMin);
            float yMax = Mathf.Min(ra.yMax, rb.yMax);
            float overlap = Mathf.Max(0f, xMax - xMin) * Mathf.Max(0f, yMax - yMin);
            return overlap >= minOverlapArea;
        }

        public static bool ContainsScreenPoint(
            RectTransform rectTransform,
            Vector2 screenPoint,
            Camera eventCamera,
            float paddingPixels)
        {
            if (rectTransform == null) return false;
            var rect = GetScreenRect(rectTransform, eventCamera);
            if (paddingPixels > 0f)
            {
                rect.xMin -= paddingPixels;
                rect.yMin -= paddingPixels;
                rect.xMax += paddingPixels;
                rect.yMax += paddingPixels;
            }

            return rect.Contains(screenPoint);
        }

        public static Vector2 GetScreenCenter(RectTransform rectTransform, Camera eventCamera)
        {
            if (rectTransform == null) return Vector2.zero;
            return RectTransformUtility.WorldToScreenPoint(eventCamera, rectTransform.TransformPoint(rectTransform.rect.center));
        }
    }
}
