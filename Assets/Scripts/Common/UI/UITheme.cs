using UnityEngine;

namespace Geidai.Common.UI
{
    /// <summary>
    /// 配色・フォント・アイコン/モチーフの一元管理（ScriptableObject / US-TECH-07）。
    /// Sさん がこのアセットを編集して見た目を調整する（コード改修不要のハンドオフ点）。
    /// </summary>
    [CreateAssetMenu(fileName = "UITheme", menuName = "Geidai/UI Theme", order = 0)]
    public class UITheme : ScriptableObject
    {
        [Header("Colors")]
        public Color primaryColor = new Color(0.36f, 0.72f, 0.36f);
        public Color secondaryColor = new Color(0.98f, 0.85f, 0.36f);
        public Color backgroundColor = new Color(0.97f, 0.97f, 0.94f);
        public Color textColor = new Color(0.13f, 0.13f, 0.13f);
        public Color warningColor = new Color(0.90f, 0.49f, 0.13f);

        [Header("Fonts")]
        public Font defaultFont;

        [Header("Icons / Motifs (カエル / おたまじゃくし / 蓮)")]
        public Sprite frogIcon;
        public Sprite tadpoleIcon;
        public Sprite lotusIcon;
    }
}
