using UnityEngine;

namespace Geidai.Common.UI
{
    /// <summary>
    /// ゲーム画面の楽器下に置く共通の影（1 種類 / 須藤さん要望）。
    /// Sさんは Sprite の差し替えと色・サイズ調整が可能。
    /// </summary>
    [CreateAssetMenu(
        fileName = "InstrumentDisplaySettings",
        menuName = "Geidai/Instrument Display Settings",
        order = 1)]
    public class InstrumentDisplaySettings : ScriptableObject
    {
        public const string ResourcesPath = "Geidai/InstrumentDisplaySettings";

        [Tooltip("全楽器共通の影スプライト（差し替え可）")]
        public Sprite shadowSprite;

        public Color shadowColor = new Color(0.75f, 0.92f, 0.35f, 0.9f);

        public Vector2 shadowSize = new Vector2(110f, 26f);

        [Tooltip("親 Rect の下端からのオフセット")]
        public float shadowBottomInset = 10f;

        private static InstrumentDisplaySettings _cached;
        private static Sprite _proceduralShadowSprite;

        public static InstrumentDisplaySettings LoadRuntime()
        {
            if (_cached != null) return _cached;
            _cached = Resources.Load<InstrumentDisplaySettings>(ResourcesPath);
            return _cached;
        }

        /// <summary>設定の Sprite、なければ共通フォールバック。</summary>
        public Sprite ResolveShadowSprite()
        {
            if (shadowSprite != null) return shadowSprite;
            return GetProceduralShadowSprite();
        }

        public static Sprite GetProceduralShadowSprite()
        {
            if (_proceduralShadowSprite != null) return _proceduralShadowSprite;

            const int w = 128;
            const int h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var fill = new Color(0.75f, 0.92f, 0.35f, 0.85f);
            var clear = new Color(0f, 0f, 0f, 0f);
            float cx = w * 0.5f;
            float cy = h * 0.5f;
            float rx = w * 0.45f;
            float ry = h * 0.42f;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - cx) / rx;
                    float dy = (y - cy) / ry;
                    tex.SetPixel(x, y, dx * dx + dy * dy <= 1f ? fill : clear);
                }
            }
            tex.Apply();
            _proceduralShadowSprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, w, h),
                new Vector2(0.5f, 0.5f),
                100f);
            return _proceduralShadowSprite;
        }
    }
}
