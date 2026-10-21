using System;
using Geidai.Common.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Game2
{
    /// <summary>神経衰弱のカード1枚（タップでめくる）。</summary>
    [RequireComponent(typeof(Button))]
    public class MemoryMatchCardView : MonoBehaviour
    {
        [SerializeField] private Button cardButton;
        [SerializeField] private Image backImage;
        [SerializeField] private Image frontImage;
        [SerializeField] private Text labelText;

        private MemoryMatchCardSpec _spec;
        private int _index = -1;
        private bool _faceUp;
        private bool _matched;

        public int Index => _index;
        public bool IsFaceUp => _faceUp;
        public bool IsMatched => _matched;
        public MemoryMatchCardSpec Spec => _spec;

        public event Action<int> Clicked;

        private void Awake()
        {
            if (cardButton == null) cardButton = GetComponent<Button>();
            if (cardButton != null)
            {
                cardButton.onClick.RemoveListener(OnClick);
                cardButton.onClick.AddListener(OnClick);
            }
        }

        private void OnDestroy()
        {
            if (cardButton != null) cardButton.onClick.RemoveListener(OnClick);
        }

        public void Bind(int index, MemoryMatchCardSpec spec)
        {
            _index = index;
            _spec = spec;
            _faceUp = false;
            _matched = false;
            RefreshVisual();
            SetInteractable(true);
        }

        public void SetFaceUp(bool faceUp)
        {
            _faceUp = faceUp;
            RefreshVisual();
        }

        public void SetMatched(bool matched)
        {
            _matched = matched;
            if (_matched) _faceUp = true;
            RefreshVisual();
            SetInteractable(!_matched);
        }

        public void SetInteractable(bool interactable)
        {
            if (cardButton != null) cardButton.interactable = interactable && !_matched;
        }

        private void RefreshVisual()
        {
            bool showFront = _faceUp || _matched;
            if (backImage != null) backImage.enabled = !showFront;
            if (frontImage != null)
            {
                frontImage.enabled = showFront;
                if (showFront && _spec.Image != null) frontImage.sprite = _spec.Image;
            }
            if (labelText != null)
            {
                labelText.enabled = showFront;
                labelText.text = _spec.DisplayName ?? string.Empty;
            }
        }

        private void OnClick()
        {
            if (_matched) return;
            Clicked?.Invoke(_index);
        }
    }
}
