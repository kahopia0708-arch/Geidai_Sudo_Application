using System;
using Geidai.Common.Library;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Library
{
    /// <summary>音図鑑の1行。ロック中は試聴不可。</summary>
    public class CuratedSoundItemView : MonoBehaviour
    {
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text categoryLabel;
        [SerializeField] private Text lockLabel;
        [SerializeField] private Button playButton;
        [SerializeField] private Image lockIcon;

        private LibraryItemView _item;
        private Action<LibraryItemView> _onPlay;

        public void Bind(LibraryItemView item, Action<LibraryItemView> onPlay)
        {
            _item = item;
            _onPlay = onPlay;

            if (nameLabel != null) nameLabel.text = item.displayName ?? string.Empty;
            if (categoryLabel != null) categoryLabel.text = item.category ?? string.Empty;

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

        private void OnPlayClicked() => _onPlay?.Invoke(_item);
    }
}
