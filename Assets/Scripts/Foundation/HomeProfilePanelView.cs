using System;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Foundation
{
    /// <summary>
    /// ホームのプロフィールオーバーレイ（ラベル＋プレースホルダー値）。
    /// </summary>
    public class HomeProfilePanelView : MonoBehaviour
    {
        private const string PlaceholderValue = "—";
        private static readonly Color PanelFillColor = HomeUiTheme.PanelFill;
        private static readonly Color MenuTextColor = HomeUiTheme.MenuText;

        [SerializeField] private Image panelBackground;
        [SerializeField] private Image settingsButtonBackground;
        [SerializeField] private Image closeButtonBackground;
        [SerializeField] private GameObject root;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text soundsCollectedValueText;
        [SerializeField] private Text pointsCollectedValueText;
        [SerializeField] private Text untilNewSoundValueText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button backdropButton;

        public event Action SettingsRequested;
        public event Action CloseRequested;

        private void Awake()
        {
            EnsureWired();
            var fontRoot = contentRoot != null ? contentRoot : transform;
            UiFontResolver.ApplyToChildren(fontRoot, HomeUiTheme.Body);
            if (titleText != null) UiFontResolver.ApplyTo(titleText, HomeUiTheme.PanelTitle);

            if (closeButton != null) closeButton.onClick.AddListener(() => CloseRequested?.Invoke());
            if (settingsButton != null) settingsButton.onClick.AddListener(() => SettingsRequested?.Invoke());
            if (backdropButton != null) backdropButton.onClick.AddListener(() => CloseRequested?.Invoke());
        }

        private void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveAllListeners();
            if (settingsButton != null) settingsButton.onClick.RemoveAllListeners();
            if (backdropButton != null) backdropButton.onClick.RemoveAllListeners();
        }

        public void Initialize(HomeMenuIconCatalog catalog)
        {
            EnsureWired();
            ApplyPanelChrome(catalog);
            ApplyLayout();
        }

        public void Show(string nickname)
        {
            EnsureWired();
            ApplyPanelFill();
            ApplyLayout();

            if (root != null)
            {
                root.SetActive(true);
                root.transform.SetAsLastSibling();
            }

            if (titleText != null)
            {
                UiFontResolver.ApplyTo(titleText, HomeUiTheme.PanelTitle);
                titleText.text = string.IsNullOrEmpty(nickname) ? "プロフィール" : $"{nickname} のプロフィール";
            }

            SetPlaceholderValues();
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        public bool IsOpen => root != null && root.activeSelf;

        private void EnsureWired()
        {
            if (root == null) root = gameObject;

            if (contentRoot == null)
            {
                var panel = transform.Find("Panel");
                if (panel != null) contentRoot = panel;
            }

            if (titleText == null && contentRoot != null)
                titleText = contentRoot.Find("Title")?.GetComponent<Text>();

            if (panelBackground == null && contentRoot != null)
                panelBackground = contentRoot.GetComponent<Image>();

            if (settingsButton == null && contentRoot != null)
                settingsButton = contentRoot.Find("SettingsButton")?.GetComponent<Button>();

            if (closeButton == null && contentRoot != null)
                closeButton = contentRoot.Find("CloseButton")?.GetComponent<Button>();

            if (settingsButton != null && settingsButtonBackground == null)
                settingsButtonBackground = settingsButton.GetComponent<Image>();

            if (closeButton != null && closeButtonBackground == null)
                closeButtonBackground = closeButton.GetComponent<Image>();

            if (soundsCollectedValueText == null && contentRoot != null)
                soundsCollectedValueText = contentRoot.Find("StatSounds/Value")?.GetComponent<Text>();
            if (pointsCollectedValueText == null && contentRoot != null)
                pointsCollectedValueText = contentRoot.Find("StatPoints/Value")?.GetComponent<Text>();
            if (untilNewSoundValueText == null && contentRoot != null)
                untilNewSoundValueText = contentRoot.Find("StatUntil/Value")?.GetComponent<Text>();
        }

        private void ApplyPanelChrome(HomeMenuIconCatalog catalog)
        {
            ApplyPanelFill();
            if (catalog == null) return;

            var pill = catalog.Resolve("pill");
            var gear = catalog.Resolve("settings");
            ApplySettingsIcon(gear);
            HomeUiImageUtil.ApplyBackground(closeButtonBackground, pill, Color.white);
        }

        private void ApplySettingsIcon(Sprite gear)
        {
            if (settingsButton == null) return;

            var label = settingsButton.GetComponentInChildren<Text>(true);
            if (label != null) label.gameObject.SetActive(false);

            var img = settingsButtonBackground != null
                ? settingsButtonBackground
                : settingsButton.GetComponent<Image>();
            if (img == null) return;

            if (gear != null)
            {
                img.sprite = gear;
                img.type = Image.Type.Simple;
                img.preserveAspect = true;
                img.color = MenuTextColor;
            }
            else
            {
                HomeUiImageUtil.ApplySolidFill(img, Color.white);
            }
        }

        private void ApplyPanelFill()
        {
            HomeUiImageUtil.ApplySolidFill(panelBackground, PanelFillColor);
        }

        /// <summary>タイトル直下に統計行を詰め、設定はタイトル右の歯車アイコン。</summary>
        private void ApplyLayout()
        {
            if (contentRoot == null) return;

            ApplyRect(titleText != null ? titleText.rectTransform : contentRoot.Find("Title") as RectTransform,
                0.06f, 0.84f, 0.76f, 0.94f);
            if (titleText != null) titleText.alignment = TextAnchor.MiddleLeft;

            ApplyStatRow(contentRoot, "StatSounds", 0.72f, 0.09f);
            ApplyStatRow(contentRoot, "StatPoints", 0.60f, 0.09f);
            ApplyStatRow(contentRoot, "StatUntil", 0.48f, 0.09f);

            if (settingsButton != null)
            {
                var rt = settingsButton.GetComponent<RectTransform>();
                rt.SetParent(contentRoot, false);
                ApplyRect(rt, 0.80f, 0.84f, 0.94f, 0.94f);
            }

            if (closeButton != null)
            {
                var rt = closeButton.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.05f);
                rt.anchorMax = new Vector2(0.5f, 0.05f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(280f, 72f);
                rt.anchoredPosition = Vector2.zero;
            }
        }

        private static void ApplyStatRow(Transform parent, string rowName, float top, float height)
        {
            var row = parent.Find(rowName) as RectTransform;
            if (row == null) return;
            ApplyRect(row, 0.08f, top - height, 0.92f, top);
        }

        private static void ApplyRect(RectTransform rt, float minX, float minY, float maxX, float maxY)
        {
            if (rt == null) return;
            rt.anchorMin = new Vector2(minX, minY);
            rt.anchorMax = new Vector2(maxX, maxY);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void SetPlaceholderValues()
        {
            if (soundsCollectedValueText != null) soundsCollectedValueText.text = PlaceholderValue;
            if (pointsCollectedValueText != null) pointsCollectedValueText.text = PlaceholderValue;
            if (untilNewSoundValueText != null) untilNewSoundValueText.text = PlaceholderValue;
        }
    }
}
