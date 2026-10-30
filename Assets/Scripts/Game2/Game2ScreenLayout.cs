using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Game2
{
    /// <summary>②神経衰弱画面のレイアウト定数。</summary>
    public static class Game2ScreenLayout
    {
        public const float BackButtonWidth = 200f;
        public const float BackButtonHeight = 72f;
        public const float BackButtonMarginLeft = 24f;
        public const float BackButtonMarginTop = 24f;

        public const float PresetButtonSize = 200f;
        public const float PresetBarSpacing = 16f;
        public const int PresetGridColumns = 3;

        public static void ApplyBackButton(RectTransform backButton)
        {
            if (backButton == null) return;
            backButton.anchorMin = new Vector2(0f, 1f);
            backButton.anchorMax = new Vector2(0f, 1f);
            backButton.pivot = new Vector2(0f, 1f);
            backButton.sizeDelta = new Vector2(BackButtonWidth, BackButtonHeight);
            backButton.anchoredPosition = new Vector2(BackButtonMarginLeft, -BackButtonMarginTop);
            backButton.SetAsLastSibling();
        }

        public static void ApplyTitle(RectTransform title, Text titleText = null)
        {
            if (title == null) return;
            title.anchorMin = new Vector2(0.05f, 1f);
            title.anchorMax = new Vector2(0.95f, 1f);
            title.pivot = new Vector2(0.5f, 1f);
            title.anchoredPosition = new Vector2(0f, -28f);
            title.sizeDelta = new Vector2(0f, 96f);
            if (titleText != null)
                titleText.alignment = TextAnchor.MiddleCenter;
        }

        public static void ApplyChangePairCountButton(RectTransform button)
        {
            if (button == null) return;
            button.anchorMin = new Vector2(1f, 1f);
            button.anchorMax = new Vector2(1f, 1f);
            button.pivot = new Vector2(1f, 1f);
            button.sizeDelta = new Vector2(200f, 72f);
            button.anchoredPosition = new Vector2(-24f, -24f);
            button.SetAsLastSibling();
        }

        /// <summary>プレイ中のトランプ置き場（タイトル下〜下端いっぱい）。</summary>
        public static void ApplyCardGridForPlay(RectTransform cardGrid)
        {
            if (cardGrid == null) return;
            cardGrid.anchorMin = new Vector2(0.02f, 0.04f);
            cardGrid.anchorMax = new Vector2(0.98f, 0.90f);
            cardGrid.offsetMin = Vector2.zero;
            cardGrid.offsetMax = Vector2.zero;
        }
    }
}
