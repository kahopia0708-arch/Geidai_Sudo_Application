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
        private Sprite _cardFaceSprite;
        private Sprite _cardBackSprite;
        private bool _showLabels = true;

        private static readonly Color DefaultBackColor = new Color(0.25f, 0.4f, 0.65f);
        private static readonly Color DefaultFrontFallbackColor = new Color(0.92f, 0.94f, 0.98f);

        public int Index => _index;
        public bool IsFaceUp => _faceUp;
        public bool IsMatched => _matched;
        public MemoryMatchCardSpec Spec => _spec;

        public event Action<int> Clicked;

        private Image _rootImage;

        private void Awake()
        {
            if (cardButton == null) cardButton = GetComponent<Button>();
            _rootImage = GetComponent<Image>();
            if (cardButton != null)
            {
                cardButton.onClick.RemoveListener(OnClick);
                cardButton.onClick.AddListener(OnClick);
            }

            if (frontImage != null && frontImage.gameObject != gameObject)
                frontImage.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (cardButton != null) cardButton.onClick.RemoveListener(OnClick);
        }

        public void Bind(int index, MemoryMatchCardSpec spec, Sprite cardFaceSprite, Sprite cardBackSprite, bool showLabels)
        {
            _index = index;
            _spec = spec;
            _cardFaceSprite = cardFaceSprite;
            _cardBackSprite = cardBackSprite;
            _showLabels = showLabels;
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

            if (_rootImage != null)
                _rootImage.enabled = false;

            if (backImage != null)
            {
                bool showBack = !showFront;
                if (backImage.gameObject != gameObject)
                    backImage.gameObject.SetActive(showBack);
                backImage.enabled = showBack;
                if (showBack)
                {
                    if (_cardBackSprite != null)
                    {
                        backImage.sprite = _cardBackSprite;
                        backImage.color = Color.white;
                        backImage.preserveAspect = true;
                    }
                    else
                    {
                        backImage.sprite = null;
                        backImage.color = DefaultBackColor;
                    }
                }
            }

            if (frontImage != null)
            {
                if (frontImage.gameObject != gameObject)
                    frontImage.gameObject.SetActive(showFront);
                frontImage.enabled = showFront;
                if (showFront)
                {
                    if (_cardFaceSprite != null)
                    {
                        frontImage.sprite = _cardFaceSprite;
                        frontImage.color = Color.white;
                        frontImage.preserveAspect = true;
                    }
                    else
                    {
                        frontImage.sprite = null;
                        frontImage.color = DefaultFrontFallbackColor;
                    }
                }
            }

            if (labelText != null)
            {
                bool showLabel = showFront && _showLabels;
                labelText.enabled = showLabel;
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
