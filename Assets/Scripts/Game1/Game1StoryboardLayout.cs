using UnityEngine;

namespace Geidai.Game1
{
    /// <summary>
    /// ①音合わせ UI の配置サイズ（Bootstrap / Editor メニュー共通）。
    /// </summary>
    public static class Game1StoryboardLayout
    {
        /// <summary>池（水面）の上端。画面下からの比率（≈上から33%が水面）。</summary>
        public const float WaterSurfaceAnchorY = 0.67f;

        public static readonly Vector2 LilyPadSize = new Vector2(680f, 560f);
        public static readonly Vector2 DemoFrogSize = new Vector2(480f, 480f);
        public static readonly Vector2 ChoiceSize = new Vector2(420f, 420f);
        public static readonly Vector2 AnswerSnapSize = new Vector2(400f, 400f);

        /// <summary>睡蓮（カエル・答え場）— 水面付近。</summary>
        public static readonly Vector2 LilyAnchorLeft = new Vector2(0.28f, 0.64f);
        public static readonly Vector2 LilyAnchorAnswer = new Vector2(0.72f, 0.64f);

        public static readonly float[] ChoicePoolX = { -600f, 0f, 600f };

        /// <summary>池内のおたま列（戻るボタンと重ならない高さ）。</summary>
        public const float ChoicePoolAnchorY = 0.34f;

        public const float BackButtonWidth = 200f;
        public const float BackButtonHeight = 72f;
        public const float BackButtonMarginLeft = 24f;
        public const float BackButtonMarginTop = 24f;

        public static readonly Vector2 LilyPadShadowSize = new Vector2(460f, 56f);
        public static readonly Vector2 LilyPadShadowOffset = new Vector2(0f, -118f);
        public const float LilyPadShadowAlpha = 0.32f;

        public static void ApplyBackButtonLayout(RectTransform backButton)
        {
            if (backButton == null) return;
            backButton.anchorMin = new Vector2(0f, 1f);
            backButton.anchorMax = new Vector2(0f, 1f);
            backButton.pivot = new Vector2(0f, 1f);
            backButton.sizeDelta = new Vector2(BackButtonWidth, BackButtonHeight);
            backButton.anchoredPosition = new Vector2(BackButtonMarginLeft, -BackButtonMarginTop);
            backButton.SetAsLastSibling();
        }
    }
}
