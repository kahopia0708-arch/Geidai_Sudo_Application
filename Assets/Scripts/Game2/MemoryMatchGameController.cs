using System.Collections;
using System.Collections.Generic;
using Geidai.Common.Game;
using Geidai.Common.Library;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.UI;
using Geidai.Services;
using Geidai.Services.Navigation;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Game2
{
    /// <summary>②神経衰弱（同じ音の2枚をそろえる / 候補は pairKey 付き図鑑音）。</summary>
    public class MemoryMatchGameController : ScreenRootBase
    {
        private enum Phase
        {
            Setup,
            Playing,
            Cleared
        }

        [Header("設定")]
        [SerializeField] private MemoryMatchConfig config;
        [SerializeField] private CuratedSoundCatalog curatedCatalog;
        [SerializeField] private int seed;

        [Header("UI")]
        [SerializeField] private RectTransform cardGrid;
        [SerializeField] private MemoryMatchCardView cardPrefab;
        [SerializeField] private Text statusText;
        [SerializeField] private Text titleText;
        [SerializeField] private GameObject emptyState;
        [SerializeField] private ErrorPresenter errorPresenter;
        [SerializeField] private Button backButton;

        private readonly List<MemoryMatchCardView> _cards = new List<MemoryMatchCardView>();
        private readonly List<MemoryMatchCardSpec> _deck = new List<MemoryMatchCardSpec>();

        private INavigationService _nav;
        private AudioSource _audioSource;
        private MemoryMatchUiHandles _ui;
        private Phase _phase = Phase.Setup;
        private bool _hasStartedOnce;

        private int? _firstPickIndex;
        private bool _busy;
        private int _matchedPairs;
        private int _targetPairs;
        private int _selectedPairCount = 4;
        private int _activePairCount;
        private Sprite _cardFaceSprite;
        private Sprite _cardBackSprite;
        private bool _showCardLabels;
        private GridLayoutGroup _gridLayout;
        private Vector2Int _lastScreenSize;

        private void Awake()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBackPressed);
                backButton.onClick.AddListener(OnBackPressed);
            }
        }

        private void OnDestroy()
        {
            if (backButton != null) backButton.onClick.RemoveListener(OnBackPressed);
            ClearCards();
        }

        private void Update()
        {
            var size = new Vector2Int(Screen.width, Screen.height);
            if (size == _lastScreenSize) return;
            _lastScreenSize = size;

            if (_deck.Count == 0 || cardGrid == null) return;
            MemoryMatchUiBootstrap.ApplyPlayAreaLayout(cardGrid);
            ApplyGridForDeck();
        }

        protected override void OnShow()
        {
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            EnsureWired();
            _hasStartedOnce = false;
            _phase = Phase.Setup;
            _selectedPairCount = config != null ? config.PairCount : 4;
            _selectedPairCount = MemoryMatchPairPresets.PairCountForIndex(
                MemoryMatchPairPresets.IndexForPairCount(_selectedPairCount));
            _cardFaceSprite = Game2CardArt.ResolveCardFace(config);
            _cardBackSprite = Game2CardArt.ResolveCardBack(config);
            _showCardLabels = config != null && config.ShowCardLabels;

            _ui = MemoryMatchUiBootstrap.EnsureChrome(
                ResolveContentRoot(),
                this,
                statusText != null ? statusText.font : null,
                backButton,
                ResolveTitleText());

            MemoryMatchUiBootstrap.UpdatePairSelection(_ui, _selectedPairCount);
            EnterSetupPhase(showPairPopup: true);
        }

        public void OnPairCountPresetClicked(int pairCount)
        {
            _selectedPairCount = Mathf.Clamp(pairCount, 1, 9);
            MemoryMatchUiBootstrap.UpdatePairSelection(_ui, _selectedPairCount);
        }

        public void OpenPairCountPopup()
        {
            MemoryMatchUiBootstrap.SetPopupConfirmLabel(
                _ui,
                _hasStartedOnce ? "この枚数で" : "はじめる");
            MemoryMatchUiBootstrap.UpdatePairSelection(_ui, _selectedPairCount);
            MemoryMatchUiBootstrap.SetPairCountPopupVisible(_ui, true);
            if (_ui?.PairCountPopupRoot != null)
                _ui.PairCountPopupRoot.transform.SetAsLastSibling();
        }

        public void ConfirmPairCountPopup()
        {
            _activePairCount = _selectedPairCount;
            MemoryMatchUiBootstrap.SetPairCountPopupVisible(_ui, false);
            _hasStartedOnce = true;
            MemoryMatchUiBootstrap.SetChangePairCountVisible(_ui?.ChangePairCountButton, true);
            MemoryMatchUiBootstrap.SetCardGridVisible(cardGrid, true);
            if (emptyState != null) emptyState.SetActive(false);
            StartRound();
        }

        private void EnterSetupPhase(bool showPairPopup)
        {
            _phase = Phase.Setup;
            ClearCards();
            _firstPickIndex = null;
            _busy = false;
            _matchedPairs = 0;
            DismissClearDialogIfOpen();
            MemoryMatchUiBootstrap.SetCardGridVisible(cardGrid, false);
            MemoryMatchUiBootstrap.SetChangePairCountVisible(_ui?.ChangePairCountButton, false);
            SetStatus(string.Empty);
            if (emptyState != null) emptyState.SetActive(false);
            if (showPairPopup)
                OpenPairCountPopup();
            else
                MemoryMatchUiBootstrap.SetPairCountPopupVisible(_ui, false);
        }

        private RectTransform ResolveContentRoot()
        {
            if (cardGrid != null) return cardGrid.parent as RectTransform;
            return transform as RectTransform;
        }

        private Text ResolveTitleText()
        {
            if (titleText != null) return titleText;
            var root = ResolveContentRoot();
            if (root == null) return null;
            return root.Find("Title")?.GetComponent<Text>();
        }

        private void EnsureWired()
        {
            _nav = ServiceRegistry.Resolve<INavigationService>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.GetComponent<AudioSource>();
                if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }
        }

        public void StartRound()
        {
            ClearCards();
            _firstPickIndex = null;
            _busy = false;
            _matchedPairs = 0;
            _phase = Phase.Playing;
            DismissClearDialogIfOpen();
            MemoryMatchUiBootstrap.SetPairCountPopupVisible(_ui, false);
            MemoryMatchUiBootstrap.SetChangePairCountVisible(_ui?.ChangePairCountButton, true);
            MemoryMatchUiBootstrap.SetCardGridVisible(cardGrid, true);
            MemoryMatchUiBootstrap.ApplyPlayAreaLayout(cardGrid);

            if (curatedCatalog == null)
            {
                ShowEmpty("おとの ずかんが ないよ");
                return;
            }

            int effectiveSeed = seed != 0 ? seed : System.Environment.TickCount;
            int pairs = _activePairCount > 0 ? _activePairCount : _selectedPairCount;
            var build = MemoryMatchDeckBuilder.Build(curatedCatalog.ValidItems(), pairs, effectiveSeed);
            if (!build.IsSuccess)
            {
                ShowEmpty(build.Message);
                return;
            }

            _deck.Clear();
            _deck.AddRange(build.Cards);
            _targetPairs = _deck.Count / 2;

            if (emptyState != null) emptyState.SetActive(false);
            SetStatus(string.Empty);

            ApplyGridForDeck();
            SpawnCards();
            StartCoroutine(FitCardGridNextFrame());
        }

        private IEnumerator FitCardGridNextFrame()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            if (_gridLayout != null && cardGrid != null)
                MemoryMatchGridLayout.ApplyFittedToArea(_gridLayout, cardGrid, _deck.Count);
        }

        private void ApplyGridForDeck()
        {
            if (cardGrid == null) return;
            if (_gridLayout == null) _gridLayout = cardGrid.GetComponent<GridLayoutGroup>();
            if (_gridLayout == null) _gridLayout = cardGrid.gameObject.AddComponent<GridLayoutGroup>();
            if (cardGrid.GetComponent<RectMask2D>() == null)
                cardGrid.gameObject.AddComponent<RectMask2D>();
            MemoryMatchGridLayout.ApplyFittedToArea(_gridLayout, cardGrid, _deck.Count);
        }

        private void SpawnCards()
        {
            if (cardGrid == null || cardPrefab == null) return;

            for (int i = 0; i < _deck.Count; i++)
            {
                var view = Instantiate(cardPrefab, cardGrid);
                view.Bind(i, _deck[i], _cardFaceSprite, _cardBackSprite, _showCardLabels);
                view.Clicked += OnCardClicked;
                _cards.Add(view);
            }
        }

        private void ClearCards()
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                if (_cards[i] != null)
                {
                    _cards[i].Clicked -= OnCardClicked;
                    Destroy(_cards[i].gameObject);
                }
            }
            _cards.Clear();
            _deck.Clear();
        }

        private bool IsPairPopupOpen =>
            _ui?.PairCountPopupRoot != null && _ui.PairCountPopupRoot.activeSelf;

        private void OnCardClicked(int index)
        {
            if (_phase != Phase.Playing || _busy) return;
            if (IsPairPopupOpen) return;
            if (_ui?.ConfirmDialog != null && _ui.ConfirmDialog.IsOpen) return;

            if (index < 0 || index >= _cards.Count) return;
            var card = _cards[index];
            if (card.IsEmptySlot || card.IsMatched || card.IsFaceUp) return;

            card.SetFaceUp(true);
            PlayClip(card.Spec.Clip);

            if (!_firstPickIndex.HasValue)
            {
                _firstPickIndex = index;
                return;
            }

            int first = _firstPickIndex.Value;
            _firstPickIndex = null;
            _busy = true;

            var a = _cards[first];
            var b = card;
            bool match = a.Spec.SoundId == b.Spec.SoundId;

            if (match)
            {
                _matchedPairs++;
                StartCoroutine(ClearMatchedPairSlots(first, index));
            }
            else
            {
                float delay = config != null ? config.MismatchHideDelaySeconds : 0.9f;
                StartCoroutine(HideMismatch(first, index, delay));
            }
        }

        private IEnumerator ClearMatchedPairSlots(int firstIndex, int secondIndex)
        {
            float pause = config != null ? config.MatchPauseSeconds : 0.35f;
            if (pause > 0f) yield return new WaitForSeconds(pause);

            if (firstIndex >= 0 && firstIndex < _cards.Count)
                _cards[firstIndex].SetAsEmptySlot();
            if (secondIndex >= 0 && secondIndex < _cards.Count)
                _cards[secondIndex].SetAsEmptySlot();

            _busy = false;

            if (_matchedPairs >= _targetPairs)
                ShowClearDialog();
        }

        private void ShowClearDialog()
        {
            _phase = Phase.Cleared;
            if (_ui?.ConfirmDialog == null)
                return;

            if (_ui.ConfirmYesLabel != null) _ui.ConfirmYesLabel.text = "もういちど";
            if (_ui.ConfirmNoLabel != null) _ui.ConfirmNoLabel.text = "メニュー";

            _ui.ConfirmDialog.Show(
                string.Empty,
                string.Empty,
                onYes: () =>
                {
                    _phase = Phase.Playing;
                    StartRound();
                },
                onNo: GoToGameSelectMenu);
        }

        private void DismissClearDialogIfOpen()
        {
            if (_ui?.ConfirmDialog != null && _ui.ConfirmDialog.IsOpen)
                _ui.ConfirmDialog.Hide();
        }

        private IEnumerator HideMismatch(int firstIndex, int secondIndex, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (firstIndex >= 0 && firstIndex < _cards.Count)
            {
                var c = _cards[firstIndex];
                if (!c.IsMatched) c.SetFaceUp(false);
            }
            if (secondIndex >= 0 && secondIndex < _cards.Count)
            {
                var c = _cards[secondIndex];
                if (!c.IsMatched) c.SetFaceUp(false);
            }
            _busy = false;
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip == null || _audioSource == null) return;
            _audioSource.PlayOneShot(clip);
        }

        private void ShowEmpty(string message)
        {
            _phase = Phase.Setup;
            MemoryMatchUiBootstrap.SetCardGridVisible(cardGrid, false);
            MemoryMatchUiBootstrap.SetChangePairCountVisible(_ui?.ChangePairCountButton, false);
            if (emptyState != null) emptyState.SetActive(true);
            SetStatus(message);
            if (errorPresenter != null) errorPresenter.ShowWarning(message);
            OpenPairCountPopup();
        }

        private void SetStatus(string text)
        {
            if (statusText != null) statusText.text = text ?? string.Empty;
        }

        public override void OnBackPressed()
        {
            if (IsPairPopupOpen && !_hasStartedOnce)
            {
                GoToGameSelectMenu();
                return;
            }

            if (IsPairPopupOpen)
            {
                MemoryMatchUiBootstrap.SetPairCountPopupVisible(_ui, false);
                return;
            }

            if (_ui?.ConfirmDialog != null && _ui.ConfirmDialog.IsOpen)
                _ui.ConfirmDialog.Hide();
            GoToGameSelectMenu();
        }

        private void GoToGameSelectMenu()
        {
            if (_audioSource != null) _audioSource.Stop();
            if (_nav == null) _nav = ServiceRegistry.Resolve<INavigationService>();
            if (_nav == null)
            {
                base.OnBackPressed();
                return;
            }

            Result result = _nav.GoTo(SceneId.GameSelect);
            if (!result.IsSuccess && errorPresenter != null) errorPresenter.ShowFromResult(result);
        }
    }
}
