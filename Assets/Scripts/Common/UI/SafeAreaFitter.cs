using UnityEngine;

namespace Geidai.Common.UI
{
    /// <summary>
    /// RectTransform を Screen.safeArea に追従させる（NFR-12 / US-TECH-02）。
    /// 表示時＋解像度/向き変更時に再適用。差分検知で過剰更新を間引く（nfr-design §4）。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;
        private ScreenOrientation _lastOrientation;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            Apply();
        }

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            if (_lastSafeArea != Screen.safeArea
                || _lastScreenSize.x != Screen.width
                || _lastScreenSize.y != Screen.height
                || _lastOrientation != Screen.orientation)
            {
                Apply();
            }
        }

        public void Apply()
        {
            if (_rect == null) _rect = GetComponent<RectTransform>();
            if (_rect == null) return;
            if (Screen.width <= 0 || Screen.height <= 0) return;

            Rect safeArea = Screen.safeArea;
            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            _lastOrientation = Screen.orientation;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            if (anchorMin.x >= 0f && anchorMin.y >= 0f && anchorMax.x <= 1f && anchorMax.y <= 1f)
            {
                _rect.anchorMin = anchorMin;
                _rect.anchorMax = anchorMax;
            }
        }
    }
}
