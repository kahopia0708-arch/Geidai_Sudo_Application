using System;
using Geidai.Common.Library;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Geidai.Library
{
    /// <summary>音図鑑の1行。ロック中は試聴不可。行タップで選択。</summary>
    public class CuratedSoundItemView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Text numberLabel;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text categoryLabel;
        [SerializeField] private Text lockLabel;
        [SerializeField] private Button playButton;
        [SerializeField] private Image lockIcon;
        [SerializeField] private Image iconImage;
        [SerializeField] private Sprite placeholderSprite;

        private LibraryItemView _item;
        private Action<LibraryItemView> _onPlay;
        private Action<LibraryItemView> _onSelect;

        public void Bind(
            LibraryItemView item,
            Action<LibraryItemView> onPlay,
            Action<LibraryItemView> onSelect = null,
            Sprite placeholder = null)
        {
            _item = item;
            _onPlay = onPlay;
            _onSelect = onSelect;
            if (placeholder != null) placeholderSprite = placeholder;

            if (numberLabel != null) numberLabel.text = $"#{item.encyclopediaNumber}";
            if (nameLabel != null) nameLabel.text = item.displayName ?? string.Empty;
            if (categoryLabel != null) categoryLabel.text = item.category ?? string.Empty;

            if (iconImage != null)
            {
                iconImage.sprite = item.image != null ? item.image : placeholderSprite;
                iconImage.enabled = iconImage.sprite != null;
            }

            bool locked = !item.isUnlocked;
            if (lockLabel != null)
            {
                lockLabel.gameObject.SetActive(locked);
                lockLabel.text = locked ? "ロック" : string.Empty;
            }
            if (lockIcon != null) lockIcon.enabled = locked;
            if (playButton != null)
            {
                playButton.interactable = !locked;
                playButton.onClick.RemoveAllListeners();
                if (!locked) playButton.onClick.AddListener(OnPlayClicked);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData != null && eventData.button != PointerEventData.InputButton.Left) return;
            _onSelect?.Invoke(_item);
        }

        private void OnPlayClicked() => _onPlay?.Invoke(_item);
    }
}
