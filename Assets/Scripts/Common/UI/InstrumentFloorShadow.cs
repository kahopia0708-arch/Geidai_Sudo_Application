using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Common.UI
{
    /// <summary>
    /// 楽器 UI の直下に共通影を表示する（Game1 選択肢 / Game3 ピッチカード等）。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class InstrumentFloorShadow : MonoBehaviour
    {
        public const string ShadowChildName = "InstrumentShadow";

        [SerializeField] private InstrumentDisplaySettings settings;
        [SerializeField] private RectTransform shadowRect;
        [SerializeField] private Image shadowImage;

        private void Awake() => Apply();

        public static void Ensure(RectTransform host, InstrumentDisplaySettings displaySettings = null)
        {
            if (host == null) return;

            var component = host.GetComponent<InstrumentFloorShadow>();
            if (component == null)
                component = host.gameObject.AddComponent<InstrumentFloorShadow>();

            if (displaySettings != null)
                component.settings = displaySettings;

            component.Apply();
        }

        public void Apply()
        {
            var resolved = settings != null ? settings : InstrumentDisplaySettings.LoadRuntime();
            var sprite = resolved != null
                ? resolved.ResolveShadowSprite()
                : InstrumentDisplaySettings.GetProceduralShadowSprite();
            if (sprite == null)
                return;

            EnsureShadowObjects();
            shadowImage.sprite = sprite;
            shadowImage.color = resolved != null
                ? resolved.shadowColor
                : new Color(0.75f, 0.92f, 0.35f, 0.9f);
            shadowImage.raycastTarget = false;
            shadowImage.preserveAspect = true;

            shadowRect.SetAsFirstSibling();
            var size = resolved != null ? resolved.shadowSize : new Vector2(110f, 26f);
            var inset = resolved != null ? resolved.shadowBottomInset : 10f;
            // 親の下端から少し下にはみ出す（床の影）
            shadowRect.anchorMin = new Vector2(0.5f, 0f);
            shadowRect.anchorMax = new Vector2(0.5f, 0f);
            shadowRect.pivot = new Vector2(0.5f, 1f);
            shadowRect.anchoredPosition = new Vector2(0f, -inset);
            shadowRect.sizeDelta = size;
        }

        private void EnsureShadowObjects()
        {
            if (shadowImage != null && shadowRect != null) return;

            Transform existing = transform.Find(ShadowChildName);
            if (existing != null)
            {
                shadowRect = existing as RectTransform;
                shadowImage = existing.GetComponent<Image>();
                return;
            }

            var go = new GameObject(
                ShadowChildName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            go.transform.SetParent(transform, false);
            shadowRect = go.GetComponent<RectTransform>();
            shadowImage = go.GetComponent<Image>();
        }
    }
}
