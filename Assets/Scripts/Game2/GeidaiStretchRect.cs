using UnityEngine;

namespace Geidai.Game2
{
    internal static class GeidaiStretchRect
    {
        public static void Full(RectTransform rt)
        {
            if (rt == null) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
