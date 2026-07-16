using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Common.UI
{
    /// <summary>
    /// CanvasScaler を端末横断・縦横両対応の統一設定にする（NFR-11 / US-TECH-01）。
    /// 参照解像度 1080x1920 / Match=0.5（NFR Requirements 確定値）。
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class ResponsiveCanvasConfigurator : MonoBehaviour
    {
        [SerializeField] private Vector2 referenceResolution = new Vector2(1080f, 1920f);
        [SerializeField, Range(0f, 1f)] private float matchWidthOrHeight = 0.5f;

        private void Awake()
        {
            Configure();
        }

        public void Configure()
        {
            var scaler = GetComponent<CanvasScaler>();
            if (scaler == null) return;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = matchWidthOrHeight;
        }
    }
}
