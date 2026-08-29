using System;
using Geidai.Common.Library;
using Geidai.Common.UI;
using Geidai.Foundation;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Library
{
    /// <summary>
    /// 選択中の音の詳細。大きい絵または「きく／とめる」で試聴（解除済みのみ）。
    /// </summary>
    public class LibraryDetailPanel : MonoBehaviour
    {
        [SerializeField] private Image panelBackground;
        [SerializeField] private Image heroImage;
        [SerializeField] private Button heroButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text hintLabel;
        [SerializeField] private Text descriptionLabel;
        [SerializeField] private Text metaLabel;
        [SerializeField] private GameObject emptyHint;
        [SerializeField] private GameObject contentRoot;
        [SerializeField] private Sprite placeholderSprite;

        private LibraryItemView _item;
        private Action<LibraryItemView> _onPlayToggle;
        private bool _playing;

        public void SetPlayHandler(Action<LibraryItemView> onPlayToggle) => _onPlayToggle = onPlayToggle;

        public void Clear()
        {
            _item = default;
            _playing = false;
            if (contentRoot != null) contentRoot.SetActive(false);
            if (emptyHint != null) emptyHint.SetActive(true);
            if (heroImage != null) heroImage.sprite = null;
            if (titleLabel != null) titleLabel.text = string.Empty;
            if (descriptionLabel != null) descriptionLabel.text = string.Empty;
            if (metaLabel != null) metaLabel.text = string.Empty;
            if (hintLabel != null) hintLabel.text = string.Empty;
            ApplyPlayButtonVisual(false, interactable: false);
        }

        public void Show(LibraryItemView item, Sprite placeholder = null, bool isPlaying = false)
        {
            _item = item;
            if (placeholder != null) placeholderSprite = placeholder;

            if (panelBackground != null)
                HomeUiImageUtil.ApplyPillFill(panelBackground, HomeUiTheme.PanelFill);

            if (emptyHint != null) emptyHint.SetActive(false);
            if (contentRoot != null) contentRoot.SetActive(true);

            if (heroImage != null)
            {
                heroImage.sprite = item.image != null ? item.image : placeholderSprite;
                heroImage.enabled = heroImage.sprite != null;
                heroImage.preserveAspect = true;
                heroImage.raycastTarget = true;
                heroImage.color = item.isUnlocked
                    ? Color.white
                    : new Color(0.12f, 0.14f, 0.18f, 1f);
            }

            if (titleLabel != null)
            {
                titleLabel.text = $"図鑑no.{item.encyclopediaNumber:000}. {item.displayName}";
                titleLabel.color = HomeUiTheme.MenuText;
                UiFontResolver.ApplyTo(titleLabel, HomeUiTheme.PanelTitle);
            }

            if (descriptionLabel != null)
            {
                descriptionLabel.text = item.description ?? string.Empty;
                descriptionLabel.color = HomeUiTheme.MenuText;
                UiFontResolver.ApplyTo(descriptionLabel, HomeUiTheme.Body);
            }

            if (metaLabel != null)
            {
                string timbre = string.IsNullOrEmpty(item.timbreDisplayName)
                    ? item.timbreTagId
                    : item.timbreDisplayName;
                metaLabel.text = $"しゅるい: {item.category}　ねいろ: {timbre}";
                metaLabel.color = HomeUiTheme.MenuText;
                UiFontResolver.ApplyTo(metaLabel, HomeUiTheme.FieldLabel);
            }

            if (heroButton != null)
            {
                heroButton.interactable = true;
                heroButton.onClick.RemoveAllListeners();
                heroButton.onClick.AddListener(OnPlayClicked);
            }

            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(OnPlayClicked);
                var img = playButton.GetComponent<Image>();
                if (img != null)
                    HomeUiImageUtil.ApplyPillFill(img, HomeUiTheme.PanelFill);
            }

            SetPlaying(isPlaying && item.isUnlocked && item.clip != null);
        }

        /// <summary>再生中はボタンを「とめる」に切り替える。</summary>
        public void SetPlaying(bool playing)
        {
            _playing = playing;
            bool canPlay = !string.IsNullOrEmpty(_item.id) && _item.isUnlocked && _item.clip != null;
            ApplyPlayButtonVisual(playing, interactable: canPlay);

            if (hintLabel != null)
            {
                if (!_item.isUnlocked)
                    hintLabel.text = "まだ きけない おとだよ";
                else if (playing)
                    hintLabel.text = "「とめる」で おとが とまるよ";
                else
                    hintLabel.text = "え か 「きく」で おとが なるよ";
                hintLabel.color = HomeUiTheme.PlaceholderText;
                UiFontResolver.ApplyTo(hintLabel, HomeUiTheme.FieldLabel);
            }
        }

        private void ApplyPlayButtonVisual(bool playing, bool interactable)
        {
            if (playButton == null) return;
            playButton.interactable = interactable;
            var label = playButton.GetComponentInChildren<Text>();
            if (label == null) return;
            label.text = playing ? "とめる" : "きく";
            label.color = HomeUiTheme.MenuText;
            label.fontStyle = FontStyle.Bold;
            UiFontResolver.ApplyTo(label, HomeUiTheme.ActionButtonLabel);
        }

        private void OnPlayClicked()
        {
            if (string.IsNullOrEmpty(_item.id)) return;
            _onPlayToggle?.Invoke(_item);
        }
    }
}
