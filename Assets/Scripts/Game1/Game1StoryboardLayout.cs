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
        public static readonly Vector2 ApprenticeCharacterSize = new Vector2(520f, 520f);
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
            heldInstrument.sizeDelta = HeldInstrumentSize;
            heldInstrument.anchoredPosition = HeldInstrumentOffset;
        }

        public static readonly Vector2 ChoiceSize = new Vector2(280f, 280f);
        /// <summary>弟子の手元（AnswerPad 内・正規化座標。中央寄りの上げた手）。</summary>
        public static readonly Vector2 AnswerSnapAnchor = new Vector2(0.20f, 0.57f);
        public static readonly Vector2 AnswerSnapSize = new Vector2(200f, 200f);

        /// <summary>互換。ApplyAnswerSnapLayout を使う。</summary>
        public static readonly Vector2 AnswerSnapAnchored = Vector2.zero;

        public static void ApplyAnswerSnapLayout(RectTransform snap)
        {
            if (snap == null) return;
            snap.anchorMin = AnswerSnapAnchor;
            snap.anchorMax = AnswerSnapAnchor;
            snap.pivot = new Vector2(0.5f, 0.5f);
            snap.anchoredPosition = Vector2.zero;
            snap.sizeDelta = AnswerSnapSize;
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
