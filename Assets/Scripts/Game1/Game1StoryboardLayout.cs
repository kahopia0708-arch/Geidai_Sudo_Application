using UnityEngine;

namespace Geidai.Game1
{
    /// <summary>
    /// ①音合わせ UI の配置サイズ（Bootstrap / Editor メニュー共通）。
    /// </summary>
    public static class Game1StoryboardLayout
    {
        public static readonly Color StageBackground = new Color(0.55f, 0.78f, 0.95f, 1f);

        public static readonly Vector2 WizardAnchor = new Vector2(0.26f, 0.58f);
        public static readonly Vector2 ApprenticeAnchor = new Vector2(0.74f, 0.58f);
        public static readonly Vector2 WizardCharacterSize = new Vector2(520f, 520f);
        /// <summary>師匠高さの約60%（須藤さん FB 2026-10-30）。</summary>
        public static readonly Vector2 ApprenticeCharacterSize = new Vector2(312f, 312f);
        /// <summary>師匠・弟子が手元に持ったときの図鑑楽器（同一サイズ）。</summary>
        public static readonly Vector2 HeldInstrumentSize = new Vector2(200f, 200f);
        /// <summary>師匠の左手付近（WizardStage 内・正規化座標＋オフセット）。</summary>
        public static readonly Vector2 HeldInstrumentAnchor = new Vector2(0.68f, 0.55f);
        public static readonly Vector2 HeldInstrumentOffset = new Vector2(72f, 72f);

        public static void ApplyHeldInstrumentLayout(RectTransform heldInstrument)
        {
            if (heldInstrument == null) return;
            heldInstrument.anchorMin = HeldInstrumentAnchor;
            heldInstrument.anchorMax = HeldInstrumentAnchor;
            heldInstrument.pivot = new Vector2(0.5f, 0.5f);
            Game1StoryboardLayout.ApplyHeldInstrumentDisplaySize(heldInstrument);
            heldInstrument.anchoredPosition = HeldInstrumentOffset;
        }

        public static readonly Vector2 ChoiceSize = new Vector2(280f, 280f);
        /// <summary>弟子の手の上（AnswerPad 内・正規化座標）。Snap の sizeDelta は <see cref="HeldInstrumentSize"/>。</summary>
        public static readonly Vector2 AnswerSnapAnchor = new Vector2(0.22f, 0.64f);

        /// <summary>互換。ApplyAnswerSnapLayout を使う。</summary>
        public static readonly Vector2 AnswerSnapAnchored = Vector2.zero;

        public static void ApplyHeldInstrumentDisplaySize(RectTransform instrumentRect)
        {
            if (instrumentRect == null) return;
            instrumentRect.sizeDelta = HeldInstrumentSize;
        }

        public static void ApplyAnswerSnapLayout(RectTransform snap)
        {
            if (snap == null) return;
            snap.anchorMin = AnswerSnapAnchor;
            snap.anchorMax = AnswerSnapAnchor;
            snap.pivot = new Vector2(0.5f, 0.5f);
            snap.anchoredPosition = Vector2.zero;
            ApplyHeldInstrumentDisplaySize(snap);
        }

        public static readonly float[] ChoicePoolX3 = { -420f, 0f, 420f };
        public static readonly float[] ChoicePoolX4 = { -540f, -180f, 180f, 540f };
        public static readonly float[] ChoicePoolX5 = { -600f, -300f, 0f, 300f, 600f };
        public static readonly float[] ChoicePoolX6 = { -650f, -390f, -130f, 130f, 390f, 650f };

        public const float ChoicePoolAnchorY = 0.22f;

        /// <summary>旧・睡蓮レイアウト（互換）。</summary>
        public const float WaterSurfaceAnchorY = 0.67f;
        public static readonly Vector2 LilyPadSize = new Vector2(680f, 560f);
        public static readonly Vector2 DemoFrogSize = WizardCharacterSize;
        public static readonly Vector2 LilyAnchorLeft = WizardAnchor;
        public static readonly Vector2 LilyAnchorAnswer = ApprenticeAnchor;
        public static readonly float[] ChoicePoolX = ChoicePoolX3;

        public const float BackButtonWidth = 200f;
        public const float BackButtonHeight = 72f;
        public const float BackButtonMarginLeft = 24f;
        public const float BackButtonMarginTop = 24f;

        public static readonly Vector2 LilyPadShadowSize = new Vector2(460f, 56f);
        public static readonly Vector2 LilyPadShadowOffset = new Vector2(0f, -118f);
        public const float LilyPadShadowAlpha = 0.32f;

        public static float[] ChoicePoolXForCount(int count)
        {
            return count switch
            {
                <= 3 => ChoicePoolX3,
                4 => ChoicePoolX4,
                5 => ChoicePoolX5,
                _ => ChoicePoolX6
            };
        }

        public static float ChoiceX(int slotIndex, int choiceCount)
        {
            var xs = ChoicePoolXForCount(choiceCount);
            if (slotIndex < 0) return 0f;
            if (slotIndex >= xs.Length) return 0f;
            return xs[slotIndex];
        }

        /// <summary>プール幅に収まるよう X と ChoiceSize を同率スケール（縦向き iPhone 等）。</summary>
        public static float ComputeChoicePoolScale(float poolWidth, int choiceCount)
        {
            choiceCount = Mathf.Max(1, choiceCount);
            if (poolWidth < 80f) return 1f;

            var xs = ChoicePoolXForCount(choiceCount);
            if (xs == null || xs.Length == 0) return 1f;

            float half = ChoiceSize.x * 0.5f;
            float span = (xs[xs.Length - 1] + half) - (xs[0] - half);
            if (span < 1f) return 1f;

            const float horizontalMargin = 48f;
            return Mathf.Min(1f, (poolWidth - horizontalMargin) / span);
        }

        public static void ApplyChoicePoolSlot(RectTransform choice, int slotIndex, int choiceCount, float poolWidth)
        {
            if (choice == null) return;
            float scale = ComputeChoicePoolScale(poolWidth, choiceCount);
            choice.anchorMin = new Vector2(0.5f, ChoicePoolAnchorY);
            choice.anchorMax = new Vector2(0.5f, ChoicePoolAnchorY);
            choice.pivot = new Vector2(0.5f, 0.5f);
            choice.sizeDelta = ChoiceSize * scale;
            choice.anchoredPosition = new Vector2(ChoiceX(slotIndex, choiceCount) * scale, 0f);
        }

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
