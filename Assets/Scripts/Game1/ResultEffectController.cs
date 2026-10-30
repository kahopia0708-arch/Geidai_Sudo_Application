using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Geidai.Common.UI;

namespace Geidai.Game1
{
    /// <summary>
    /// 正解・不正解・結果の見せ方。ロジックは SoundMatchGameController が担当。
    /// </summary>
    public class ResultEffectController : MonoBehaviour
    {
        private const string DefaultCorrectBanner = "せいかい！ おとが あいました";
        private const string DefaultCorrectBody = "おとが ぴったり！ つぎは どうする？";
        private const string DefaultWrongMessage = "おとが ちがうよ。もういちど きいてみよう";

        /// <summary>正解ポップアップ（メッセージ＋ボタン一体）。睡蓮を隠さないよう下寄せ。</summary>
        private static readonly Vector2 CorrectPanelAnchorMin = new Vector2(0.08f, 0.05f);
        private static readonly Vector2 CorrectPanelAnchorMax = new Vector2(0.92f, 0.28f);

        [Header("正解演出（カエル成長）")]
        [Tooltip("OFF = 魔法使い UI（師匠の楽器 Reveal のみ）。ON = 旧カエル／おたま成長。")]
        [SerializeField] private bool legacyGrowthEnabled;
        [SerializeField] private Image frogImage;
        [SerializeField] private Sprite[] growthStages;

        [Header("フィードバック（未配線なら Play 時に自動生成）")]
        [SerializeField] private GameObject correctFeedback;
        [SerializeField] private Text correctBannerText;
        [SerializeField] private GameObject correctChoicePanel;
        [SerializeField] private Text correctChoiceMessageText;
        [SerializeField] private Button nextQuestionButton;
        [SerializeField] private Button menuAfterCorrectButton;

        [SerializeField] private GameObject retryFeedback;
        [SerializeField] private Text retryMessageText;

        [Header("結果サマリ")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Text resultText;

        private RectTransform _overlayRoot;
        private Action _onNextQuestion;
        private Action _onMenu;
        private bool _uiBuilt;
        private Coroutine _feedbackMotion;

        public void EnsureOverlayRoot(RectTransform playArea)
        {
            if (playArea == null) return;
            _overlayRoot = playArea;
            if (!_uiBuilt) BuildFeedbackUiIfNeeded();
        }

        public void ResetGrowth(Sprite targetSprite)
        {
            if (frogImage == null) return;
            if (!ShouldUseLegacyGrowth())
            {
                HideGrowthImage();
                return;
            }

            frogImage.enabled = true;
            if (growthStages != null && growthStages.Length > 0)
                frogImage.sprite = growthStages[0];
            else if (targetSprite != null)
            {
                frogImage.sprite = targetSprite;
                frogImage.preserveAspect = true;
            }
        }

        /// <summary>正解：1 枚のポップアップ（メッセージ＋次へ／メニュー）。</summary>
        public void ShowCorrectCelebration(
            string bannerMessage,
            string bodyMessage,
            Action onNextQuestion,
            Action onMenu)
        {
            if (_overlayRoot == null)
            {
                var fitter = GetComponentInParent<Geidai.Common.UI.SafeAreaFitter>();
                if (fitter != null) EnsureOverlayRoot(fitter.transform as RectTransform);
            }

            BuildFeedbackUiIfNeeded();
            HideTransient();
            ApplyCorrectPanelLayout();

            _onNextQuestion = onNextQuestion;
            _onMenu = onMenu;

            if (correctFeedback != null)
                correctFeedback.SetActive(false);

            if (correctChoicePanel != null)
            {
                if (correctChoiceMessageText != null)
                    correctChoiceMessageText.text = FormatCorrectPopupText(bannerMessage, bodyMessage);
                correctChoicePanel.SetActive(true);
            }

            AdvanceGrowthStage();
            BringFeedbackToFront();
            PlayFeedbackMotion(correctChoicePanel != null ? correctChoicePanel.transform as RectTransform : null, true);
        }

        public void SetLegacyGrowthEnabled(bool enabled)
        {
            legacyGrowthEnabled = enabled;
            if (!enabled)
                HideGrowthImage();
        }

        public void HideCorrectCelebration()
        {
            StopFeedbackMotion();
            if (correctFeedback != null) correctFeedback.SetActive(false);
            if (correctChoicePanel != null) correctChoicePanel.SetActive(false);
            _onNextQuestion = null;
            _onMenu = null;
        }

        public void ShowWrongMessage(string message)
        {
            if (_overlayRoot == null)
            {
                var fitter = GetComponentInParent<Geidai.Common.UI.SafeAreaFitter>();
                if (fitter != null) EnsureOverlayRoot(fitter.transform as RectTransform);
            }

            BuildFeedbackUiIfNeeded();
            if (retryFeedback != null)
            {
                if (retryMessageText != null)
                    retryMessageText.text = string.IsNullOrEmpty(message) ? DefaultWrongMessage : message;
                retryFeedback.SetActive(true);
            }

            BringFeedbackToFront();
            PlayFeedbackMotion(retryFeedback != null ? retryFeedback.transform as RectTransform : null, false);
        }

        public void HideWrongMessage()
        {
            StopFeedbackMotion();
            if (retryFeedback != null) retryFeedback.SetActive(false);
        }

        public void ShowResult(int correctCount, int total)
        {
            HideTransient();
            HideCorrectCelebration();
            if (resultPanel != null) resultPanel.SetActive(true);
            if (resultText != null) resultText.text = $"{total} もん ちゅう {correctCount} もん せいかい！";
        }

        public void HideResultSummary()
        {
            if (resultPanel != null) resultPanel.SetActive(false);
        }

        private void AdvanceGrowthStage()
        {
            if (!ShouldUseLegacyGrowth()) return;
            if (frogImage == null || growthStages == null || growthStages.Length == 0) return;
            int next = Mathf.Min(growthStages.Length - 1, GetCurrentGrowthIndex() + 1);
            frogImage.sprite = growthStages[next];
            frogImage.preserveAspect = true;
        }

        private int GetCurrentGrowthIndex()
        {
            if (frogImage == null || growthStages == null) return 0;
            for (int i = 0; i < growthStages.Length; i++)
            {
                if (growthStages[i] == frogImage.sprite) return i;
            }

            return 0;
        }

        private bool ShouldUseLegacyGrowth()
        {
            if (!legacyGrowthEnabled || frogImage == null) return false;
            var parent = frogImage.transform.parent;
            if (parent != null && parent.name == "Frog") return false;
            return true;
        }

        private void HideGrowthImage()
        {
            if (frogImage == null) return;
            frogImage.sprite = null;
            frogImage.enabled = false;
        }

        private void HideTransient()
        {
            HideWrongMessage();
        }

        private void BuildFeedbackUiIfNeeded()
        {
            if (_uiBuilt || _overlayRoot == null) return;
            _uiBuilt = true;

            if (retryFeedback == null)
            {
                retryFeedback = CreateBannerPanel(
                    "WrongBanner",
                    new Vector2(0.08f, 0.72f),
                    new Vector2(0.92f, 0.82f),
                    new Color(1f, 0.92f, 0.85f, 0.95f),
                    out retryMessageText);
                retryFeedback.transform.SetParent(_overlayRoot, false);
            }

            if (correctChoicePanel == null)
            {
                correctChoicePanel = CreateCorrectChoicePanel(_overlayRoot);
            }

            if (correctFeedback != null)
                correctFeedback.SetActive(false);

            WireCorrectChoiceButtons();
            Game1UiFont.ApplyToChildren(_overlayRoot, 28);
        }

        private void BringFeedbackToFront()
        {
            if (_overlayRoot == null) return;
            if (retryFeedback != null) retryFeedback.transform.SetAsLastSibling();
            if (correctChoicePanel != null) correctChoicePanel.transform.SetAsLastSibling();
        }

        private void WireCorrectChoiceButtons()
        {
            if (nextQuestionButton != null)
            {
                nextQuestionButton.onClick.RemoveListener(OnNextQuestionClicked);
                nextQuestionButton.onClick.AddListener(OnNextQuestionClicked);
            }

            if (menuAfterCorrectButton != null)
            {
                menuAfterCorrectButton.onClick.RemoveListener(OnMenuClicked);
                menuAfterCorrectButton.onClick.AddListener(OnMenuClicked);
            }
        }

        private void OnNextQuestionClicked() => _onNextQuestion?.Invoke();

        private void OnMenuClicked() => _onMenu?.Invoke();

        private static GameObject CreateBannerPanel(
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color bg,
            out Text messageText)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = bg;
            img.raycastTarget = false;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(12f, 8f);
            trt.offsetMax = new Vector2(-12f, -8f);
            messageText = textGo.GetComponent<Text>();
            messageText.alignment = TextAnchor.MiddleCenter;
            messageText.fontSize = 26;
            messageText.color = new Color(0.15f, 0.15f, 0.2f);
            messageText.text = string.Empty;
            Game1UiFont.Apply(messageText, 26);
            go.SetActive(false);
            return go;
        }

        private GameObject CreateCorrectChoicePanel(RectTransform root)
        {
            var panelGo = new GameObject("CorrectChoicePanel", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(root, false);
            var panelRt = panelGo.GetComponent<RectTransform>();
            panelRt.anchorMin = CorrectPanelAnchorMin;
            panelRt.anchorMax = CorrectPanelAnchorMax;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            var panelImg = panelGo.GetComponent<Image>();
            panelImg.color = new Color(0.88f, 0.98f, 0.9f, 0.96f);
            panelImg.raycastTarget = true;

            var msgGo = new GameObject("Message", typeof(RectTransform), typeof(Text));
            msgGo.transform.SetParent(panelGo.transform, false);
            var msgRt = msgGo.GetComponent<RectTransform>();
            msgRt.anchorMin = new Vector2(0.06f, 0.4f);
            msgRt.anchorMax = new Vector2(0.94f, 0.94f);
            msgRt.offsetMin = Vector2.zero;
            msgRt.offsetMax = Vector2.zero;
            correctChoiceMessageText = msgGo.GetComponent<Text>();
            correctChoiceMessageText.alignment = TextAnchor.MiddleCenter;
            correctChoiceMessageText.fontSize = 26;
            correctChoiceMessageText.lineSpacing = 1.05f;
            correctChoiceMessageText.color = new Color(0.1f, 0.28f, 0.16f);
            Game1UiFont.Apply(correctChoiceMessageText, 26);

            nextQuestionButton = CreatePanelButton(panelGo.transform, "NextButton", "つぎの もんだい", new Vector2(0.06f, 0.06f), new Vector2(0.48f, 0.34f));
            menuAfterCorrectButton = CreatePanelButton(panelGo.transform, "MenuButton", "メニューに もどる", new Vector2(0.52f, 0.06f), new Vector2(0.94f, 0.34f));

            panelGo.SetActive(false);
            return panelGo;
        }

        private static Button CreatePanelButton(
            Transform parent,
            string name,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0.92f, 0.96f, 1f, 1f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            var text = textGo.GetComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 24;
            text.color = new Color(0.12f, 0.12f, 0.18f);
            Game1UiFont.Apply(text, 24);
            return go.GetComponent<Button>();
        }

        private static string FormatCorrectPopupText(string bannerMessage, string bodyMessage)
        {
            var banner = string.IsNullOrEmpty(bannerMessage) ? DefaultCorrectBanner : bannerMessage;
            var body = string.IsNullOrEmpty(bodyMessage) ? DefaultCorrectBody : bodyMessage;
            return $"{banner}\n{body}";
        }

        private void ApplyCorrectPanelLayout()
        {
            if (correctChoicePanel == null) return;
            var rt = correctChoicePanel.GetComponent<RectTransform>();
            rt.anchorMin = CorrectPanelAnchorMin;
            rt.anchorMax = CorrectPanelAnchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void PlayFeedbackMotion(RectTransform target, bool successPop)
        {
            StopFeedbackMotion();
            if (target == null) return;
            _feedbackMotion = StartCoroutine(successPop
                ? GeidaiUiMotion.ScalePop(target, 1.06f, 0.3f)
                : GeidaiUiMotion.HorizontalWiggle(target, 18f, 0.42f, 3));
        }

        private void StopFeedbackMotion()
        {
            if (_feedbackMotion == null) return;
            StopCoroutine(_feedbackMotion);
            _feedbackMotion = null;
        }
    }
}
