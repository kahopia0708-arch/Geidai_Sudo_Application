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
            var fontRoot = contentRoot != null ? contentRoot : transform;
            UiFontResolver.ApplyToChildren(fontRoot, 32);
            if (titleText != null) UiFontResolver.ApplyTo(titleText, 40);

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

        public void Show(string nickname)
        {
            if (root != null) root.SetActive(true);
            if (titleText != null)
            {
                UiFontResolver.ApplyTo(titleText, 40);
                titleText.text = string.IsNullOrEmpty(nickname) ? "プロフィール" : $"{nickname} のプロフィール";
            }

            SetPlaceholderValues();
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        public bool IsOpen => root != null && root.activeSelf;

        private void SetPlaceholderValues()
        {
            if (soundsCollectedValueText != null) soundsCollectedValueText.text = PlaceholderValue;
            if (pointsCollectedValueText != null) pointsCollectedValueText.text = PlaceholderValue;
            if (untilNewSoundValueText != null) untilNewSoundValueText.text = PlaceholderValue;
        }
    }
}
