using UnityEngine;

namespace Geidai.Game1
{
    /// <summary>
    /// ①音合わせ UI の配置サイズ（Bootstrap / Editor メニュー共通）。
    /// </summary>
    public static class Game1StoryboardLayout
    {
        public static readonly Color StageBackground = new Color(0.55f, 0.78f, 0.95f, 1f);

        /// <summary>師匠・弟子の足元ライン（同一 Y）。</summary>
        public static readonly float CharacterBaselineAnchorY = 0.55f;
        public static readonly Vector2 WizardAnchor = new Vector2(0.26f, CharacterBaselineAnchorY);
        public static readonly Vector2 ApprenticeAnchor = new Vector2(0.74f, CharacterBaselineAnchorY);
        public static readonly Vector2 WizardCharacterSize = new Vector2(520f, 520f);
        public static readonly float CharacterStageHeight = 520f;
        /// <summary>師匠高さの約60%（須藤さん FB 2026-10-30）。</summary>
        public static readonly Vector2 ApprenticeCharacterSize = new Vector2(312f, 312f);
        /// <summary>師匠・弟子が手元に持ったときの図鑑楽器（同一サイズ）。</summary>
        public static readonly Vector2 HeldInstrumentSize = new Vector2(200f, 200f);
        /// <summary>足元（WizardStage 下端）から師匠手元楽器を上げる量（師匠身長の比率）。</summary>
        public const float WizardHeldRaiseFractionOfWizardHeight = 0.20f;

        private const float HeldInstrumentAnchorYNominal = 0.55f;

        /// <summary>師匠の左手付近（WizardStage 内・正規化座標）。足元基準で身長の25%だけ上げる。</summary>
        public static readonly Vector2 HeldInstrumentAnchor = new Vector2(
            0.68f,
            HeldInstrumentAnchorYNominal + WizardHeldRaiseFractionOfWizardHeight);

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

        /// <summary>足元（AnswerPad 下端＝弟子の脚）から吸着を下げる量（弟子身長の比率）。</summary>
        public const float HeldSnapLowerFractionOfApprenticeHeight = 0.25f;

        private static readonly float HeldSnapLowerPixelsFromFeet =
            ApprenticeCharacterSize.y * HeldSnapLowerFractionOfApprenticeHeight;

        private const float AnswerSnapAnchorXNominal = 0.22f;
        private const float AnswerSnapAnchorYNominal = 0.72f;
        private const float HeldInstrumentSnapLocalOffsetYNominal = 72f;

        /// <summary>弟子吸着を右へずらす量（AnswerPad 幅に対する比率）。</summary>
        public const float ApprenticeHeldSnapShiftRightFraction = 0.10f;

        /// <summary>弟子の手付近（AnswerPad 内・正規化座標）。足元基準で <see cref="HeldSnapLowerFractionOfApprenticeHeight"/> 分だけ下げる。</summary>
        public static readonly Vector2 AnswerSnapAnchor = new Vector2(
            AnswerSnapAnchorXNominal + ApprenticeHeldSnapShiftRightFraction,
            AnswerSnapAnchorYNominal - HeldSnapLowerPixelsFromFeet / CharacterStageHeight);

        /// <summary>吸着後、スナップからのローカルオフセット（足元基準で同率だけ下げる）。</summary>
        public static readonly Vector2 HeldInstrumentSnapLocalOffset = new Vector2(
            0f,
            HeldInstrumentSnapLocalOffsetYNominal - HeldSnapLowerPixelsFromFeet);

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

        public const float ChoicePreviewScalePeak = 1.14f;
        public const float ChoiceIdleBobAmplitudePixels = 7f;
        public const float ChoiceIdleBobPeriodSeconds = 3.1f;

        public static void ApplyCharacterStage(RectTransform stage, Vector2 anchor)
        {
            if (stage == null) return;
            stage.anchorMin = anchor;
            stage.anchorMax = anchor;
            stage.pivot = new Vector2(0.5f, 0f);
            stage.sizeDelta = new Vector2(CharacterStageHeight, CharacterStageHeight);
            stage.anchoredPosition = Vector2.zero;
        }

        /// <summary>ステージ内で足元を揃えたキャラ画像。</summary>
        public static void ApplyCharacterPortraitBottom(RectTransform portrait, Vector2 size)
        {
            if (portrait == null) return;
            portrait.anchorMin = new Vector2(0.5f, 0f);
            portrait.anchorMax = new Vector2(0.5f, 0f);
            portrait.pivot = new Vector2(0.5f, 0f);
            portrait.sizeDelta = size;
            portrait.anchoredPosition = Vector2.zero;
        }

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
