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
        [Header("設定")]
        [SerializeField] private MemoryMatchConfig config;
        [SerializeField] private CuratedSoundCatalog curatedCatalog;
        [SerializeField] private int seed;

        [Header("UI")]
        [SerializeField] private RectTransform cardGrid;
        [SerializeField] private MemoryMatchCardView cardPrefab;
        [SerializeField] private Text statusText;
        [SerializeField] private GameObject emptyState;
        [SerializeField] private ErrorPresenter errorPresenter;
        [SerializeField] private Button backButton;

        private readonly List<MemoryMatchCardView> _cards = new List<MemoryMatchCardView>();
        private readonly List<MemoryMatchCardSpec> _deck = new List<MemoryMatchCardSpec>();

        private INavigationService _nav;
        private AudioSource _audioSource;

        private int? _firstPickIndex;
        private bool _busy;
        private int _matchedPairs;
        private int _targetPairs;

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

        protected override void OnShow()
        {
            EnsureWired();
            StartRound();
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

            if (curatedCatalog == null)
            {
                ShowEmpty("おとの ずかんが ないよ");
                return;
            }

            int effectiveSeed = seed != 0 ? seed : System.Environment.TickCount;
            int pairs = config != null ? config.PairCount : 4;
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
            SetStatus($"ペアを そろえてね（{_targetPairs} 組）");

            SpawnCards();
        }

        private void SpawnCards()
        {
            if (cardGrid == null || cardPrefab == null) return;

            for (int i = 0; i < _deck.Count; i++)
            {
                var view = Instantiate(cardPrefab, cardGrid);
                view.Bind(i, _deck[i]);
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

        private void OnCardClicked(int index)
        {
            if (_busy || index < 0 || index >= _cards.Count) return;
            var card = _cards[index];
            if (card.IsMatched || card.IsFaceUp) return;

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
                a.SetMatched(true);
                b.SetMatched(true);
                _matchedPairs++;
                SetStatus($"あわせた！ {_matchedPairs} / {_targetPairs}");
                StartCoroutine(AfterMatchPause());
            }
            else
            {
                SetStatus("ざんねん… もういちど");
                float delay = config != null ? config.MismatchHideDelaySeconds : 0.9f;
                StartCoroutine(HideMismatch(first, index, delay));
            }
        }

        private IEnumerator AfterMatchPause()
        {
            float pause = config != null ? config.MatchPauseSeconds : 0.35f;
            if (pause > 0f) yield return new WaitForSeconds(pause);
            _busy = false;

            if (_matchedPairs >= _targetPairs)
                SetStatus("ぜんぶ あわせた！");
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
            SetStatus("ペアを そろえてね");
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip == null || _audioSource == null) return;
            _audioSource.PlayOneShot(clip);
        }

        private void ShowEmpty(string message)
        {
            if (emptyState != null) emptyState.SetActive(true);
            SetStatus(message);
            if (errorPresenter != null) errorPresenter.ShowWarning(message);
        }

        private void SetStatus(string text)
        {
            if (statusText != null) statusText.text = text ?? string.Empty;
        }

        public override void OnBackPressed()
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
