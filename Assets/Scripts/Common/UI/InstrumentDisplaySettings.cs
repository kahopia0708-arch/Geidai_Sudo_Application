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

        public static InstrumentDisplaySettings LoadRuntime()
        {
            if (_cached != null) return _cached;
            _cached = Resources.Load<InstrumentDisplaySettings>(ResourcesPath);
            return _cached;
        }
    }
}
