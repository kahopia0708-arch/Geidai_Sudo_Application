using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Geidai.Common.Game;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.UI;
using Geidai.Services;
using Geidai.Services.Audio;
using Geidai.Services.Navigation;
using Geidai.Services.Storage;

namespace Geidai.Game1
{
    /// <summary>
    /// ①音合わせ。右睡蓮へおたまを置くと答え合わせ（カエル→おたまの順再生）→判定。
    /// 正解後は次の問題／メニューを選ぶ。不正解はメッセージ後におたまが池へアニメで戻る。
    /// 音再生中もタップ・ドラッグはブロックしない（操作は並行）。
    /// </summary>
    public class SoundMatchGameController : ScreenRootBase
    {
        private enum State { Loading, Empty, Playing, AwaitingProgressChoice, Result }

        [Header("設定")]
        [SerializeField] private SoundMatchConfig config;
        [SerializeField] private int difficultyIndex = 1; // 既定=ふつう
        [Tooltip("0 のとき起動時刻からシードを作る（毎回変化）。固定値でデバッグ再現。")]
        [SerializeField] private int seed = 0;

        [Header("UI 参照")]
        [SerializeField] private FrogTargetView frog;
        [SerializeField] private SoundMatchAnswerPadView answerPad;
        [SerializeField] private List<ChoiceItemView> choiceViews = new List<ChoiceItemView>();
        [SerializeField] private ResultEffectController resultEffect;
        [SerializeField] private ErrorPresenter errorPresenter;
        [SerializeField] private GameObject emptyState;
        [SerializeField] private Button backButton;

        [Header("答え合わせ")]
        [Tooltip("カエルの音のあと、おたまの音を鳴らすまでの間（秒）。")]
        [SerializeField] private float gapBetweenMatchSoundsSeconds = 0.12f;

        [Tooltip("正解後、選択パネルを出すまでの待ち（秒）。成長カエルを見せる余白。")]
        [SerializeField] private float delayBeforeCorrectChoiceSeconds = 1.35f;

        [Tooltip("不正解メッセージを見せる時間（秒）。")]
        [SerializeField] private float delayAfterWrongSeconds = 0.85f;

        [Tooltip("不正解後、池へ戻るアニメの長さ（秒）。")]
        [SerializeField] private float wrongReturnHomeDurationSeconds = 0.55f;

        [Tooltip("タップ確認時、再生終了の少し手前で口を閉じる（秒）。0 で音終了まで開けたまま。")]
        [SerializeField] private float previewMouthCloseEarlySeconds = 0.15f;

        private IStorageService _storage;
        private IPitchVariationService _pitch;
        private INavigationService _nav;
        private Canvas _canvas;
        private RectTransform _playArea;

        private GameSession _session;
        private AudioBuffer _baseBuffer;
        private string _baseSoundId = string.Empty;
        private Sprite _choiceDisplaySprite;
        private Sprite _choiceOpenSprite;
        private Sprite _targetDisplaySprite;
        private Sprite _targetOpenSprite;
        private Sprite _evolvedChoiceSprite;
        private State _state = State.Loading;
        private Coroutine _matchCoroutine;
        private Coroutine _frogDemoCoroutine;
        private readonly Dictionary<ChoiceItemView, Coroutine> _choiceDemoCoroutines = new();
        private Coroutine _wrongAnswerCoroutine;

        internal bool HasAnswerPad => answerPad != null;

        internal SoundMatchConfig Config => config;

        internal void ApplyStoryboardReferences(
            FrogTargetView demoFrog,
            SoundMatchAnswerPadView pad,
            RectTransform playArea)
        {
            frog = demoFrog;
            answerPad = pad;
            _playArea = playArea;
        }

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
            StopMatchCoroutine();
            StopAllPreviewDemos();
        }

        protected override void OnShow()
        {
            EnsureWired();
            EnsureInstrumentShadows();
            if (resultEffect != null && _playArea != null)
                resultEffect.EnsureOverlayRoot(_playArea);
            StartGame();
        }

        private void EnsureInstrumentShadows()
        {
            var displaySettings = InstrumentDisplaySettings.LoadRuntime();
            if (choiceViews != null)
            {
                for (int i = 0; i < choiceViews.Count; i++)
                {
                    var view = choiceViews[i];
                    if (view == null) continue;
                    InstrumentFloorShadow.Ensure(
                        view.transform as RectTransform,
                        displaySettings);
                }
            }
        }

        private void EnsureWired()
        {
            _storage = ServiceRegistry.Resolve<IStorageService>();
            _nav = ServiceRegistry.Resolve<INavigationService>();
            _pitch = Game1Bootstrap.EnsurePitchVariationService();
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
            if (_playArea == null && choiceViews != null && choiceViews.Count > 0 && choiceViews[0] != null)
                _playArea = choiceViews[0].transform.parent as RectTransform;
        }

        private void RefreshDisplaySpritesFromConfig()
        {
            if (config == null) return;
            _targetDisplaySprite = config.PlaceholderTargetSprite;
            _targetOpenSprite = config.PlaceholderTargetOpenSprite;
            _choiceDisplaySprite = config.PlaceholderChoiceSprite;
            _choiceOpenSprite = config.PlaceholderChoiceOpenSprite;
            _evolvedChoiceSprite = config.PlaceholderTargetSprite;
        }

        /// <summary>ゲームを開始する（素材選択・フォールバック集約＋出題生成）。</summary>
        public void StartGame()
        {
            _state = State.Loading;
            int effectiveSeed = seed != 0 ? seed : System.Environment.TickCount;

            if (!TryLoadBase(effectiveSeed))
            {
                ShowEmpty();
                return;
            }

            _pitch.SetBase(_baseBuffer);

            var questions = QuestionBuilder.BuildQuestions(_baseSoundId, config, difficultyIndex, effectiveSeed);
            if (questions == null || questions.Count == 0)
            {
                ShowEmpty();
                return;
            }

            _session = new GameSession(questions);
            if (emptyState != null) emptyState.SetActive(false);
            RefreshDisplaySpritesFromConfig();
            if (resultEffect != null)
            {
                if (_playArea != null) resultEffect.EnsureOverlayRoot(_playArea);
                resultEffect.ResetGrowth(_targetDisplaySprite);
                resultEffect.HideCorrectCelebration();
            }
            PresentCurrent();
        }

        private Sprite ResolveEvolvedSprite()
        {
            return config != null ? config.PlaceholderTargetSprite : _targetDisplaySprite;
        }

        private bool TryLoadBase(int seedValue)
        {
            if (config != null &&
                SoundMatchBaseSoundSelector.TrySelectFromCatalog(
                    config.CuratedCatalog,
                    config.PlaceholderTargetSprite,
                    seedValue,
                    out var fromCatalog))
            {
                _baseBuffer = fromCatalog.Buffer;
                _baseSoundId = fromCatalog.SoundId;
                RefreshDisplaySpritesFromConfig();
                return true;
            }

            // 保存音から素材を選ぶ（seed で決定的な順序）。読込失敗は次候補へ。
            if (_storage != null)
            {
                var listResult = _storage.ListSounds();
                if (listResult.IsSuccess && listResult.Value != null && listResult.Value.Count > 0)
                {
                    var items = listResult.Value;
                    var order = new List<int>(items.Count);
                    for (int i = 0; i < items.Count; i++) order.Add(i);
                    var rng = new System.Random(seedValue);
                    for (int i = order.Count - 1; i > 0; i--)
                    {
                        int j = rng.Next(0, i + 1);
                        (order[i], order[j]) = (order[j], order[i]);
                    }

                    foreach (int idx in order)
                    {
                        var meta = items[idx].meta;
                        if (meta == null || string.IsNullOrEmpty(meta.id)) continue;
                        var bufResult = _storage.LoadSoundBuffer(meta.id);
                        if (bufResult.IsSuccess && bufResult.Value != null)
                        {
                            _baseBuffer = bufResult.Value;
                            _baseSoundId = meta.id;
                            RefreshDisplaySpritesFromConfig();
                            return true;
                        }
                    }
                }
            }

            // 保存音が無い/全滅 → fallbackClip
            if (config != null && config.FallbackClip != null)
            {
                var buf = SoundMatchBaseSoundSelector.BufferFromClip(config.FallbackClip);
                if (buf != null)
                {
                    _baseBuffer = buf;
                    _baseSoundId = string.Empty;
                    RefreshDisplaySpritesFromConfig();
                    return true;
                }
            }

            return false;
        }

        private void PresentCurrent()
        {
            var q = _session.Current;
            if (q == null)
            {
                ShowResult();
                return;
            }

            _state = State.Playing;
            StopAllPreviewDemos();
            if (resultEffect != null)
            {
                if (_playArea != null) resultEffect.EnsureOverlayRoot(_playArea);
                resultEffect.HideCorrectCelebration();
                resultEffect.HideWrongMessage();
            }
            if (frog != null)
                frog.Setup(this, _targetDisplaySprite, _targetOpenSprite);
            RefreshDisplaySpritesFromConfig();
            _evolvedChoiceSprite = ResolveEvolvedSprite();

            for (int i = 0; i < choiceViews.Count; i++)
            {
                var view = choiceViews[i];
                if (view == null) continue;
                if (i < q.choices.Count)
                {
                    view.gameObject.SetActive(true);
                    view.Setup(
                        this,
                        q.choices[i],
                        i,
                        _choiceDisplaySprite,
                        _evolvedChoiceSprite,
                        _choiceOpenSprite);
                    view.ApplyPoolHomeLayout(i);
                    view.SetAsTadpole(_choiceDisplaySprite);
                }
                else
                {
                    view.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>カエルタップ：口を開けてお手本の音。</summary>
        public void OnDemoFrogTapped(FrogTargetView view)
        {
            if (!CanPreviewSound() || _session == null || view == null) return;
            StopAllPreviewDemos();
            _frogDemoCoroutine = StartCoroutine(PlayTargetDemo(view));
        }

        /// <summary>おたまタップ：口を開けてその選択肢の音。</summary>
        public void OnChoiceTapped(ChoiceItemView view)
        {
            if (!CanPreviewSound() || view == null) return;
            StopAllPreviewDemos();
            var co = StartCoroutine(PlayChoiceDemo(view));
            _choiceDemoCoroutines[view] = co;
        }

        private bool CanPreviewSound()
        {
            return _state == State.Playing || _state == State.AwaitingProgressChoice;
        }

        private bool CanSubmitAnswer()
        {
            return _state == State.Playing && _session != null;
        }

        private void StopAllPreviewDemos()
        {
            if (_frogDemoCoroutine != null)
            {
                StopCoroutine(_frogDemoCoroutine);
                _frogDemoCoroutine = null;
            }

            foreach (var pair in _choiceDemoCoroutines)
            {
                if (pair.Key != null && pair.Value != null)
                    StopCoroutine(pair.Value);
            }

            _choiceDemoCoroutines.Clear();
            CloseAllPreviewMouths();
        }

        private void CloseAllPreviewMouths()
        {
            if (frog != null) frog.CloseMouth();
            if (choiceViews == null) return;
            for (int i = 0; i < choiceViews.Count; i++)
            {
                if (choiceViews[i] != null)
                    choiceViews[i].CloseMouth();
            }
        }

        private IEnumerator PlayTargetDemo(FrogTargetView view)
        {
            var q = _session != null ? _session.Current : null;
            if (q == null || _pitch == null) yield break;
            try
            {
                view.OpenMouth();
                _pitch.Play(q.targetCents);
                yield return WaitForPreviewPlayback();
            }
            finally
            {
                view.CloseMouth();
                _frogDemoCoroutine = null;
            }
        }

        private IEnumerator PlayChoiceDemo(ChoiceItemView view)
        {
            if (_pitch == null) yield break;
            try
            {
                view.OpenMouth();
                _pitch.Play(view.Cents);
                yield return WaitForPreviewPlayback();
            }
            finally
            {
                view.CloseMouth();
                _choiceDemoCoroutines.Remove(view);
            }
        }

        private IEnumerator WaitForPreviewPlayback()
        {
            yield return null;
            while (_pitch != null && _pitch.IsPlaying)
            {
                if (previewMouthCloseEarlySeconds > 0f &&
                    _pitch.PlaybackSecondsRemaining <= previewMouthCloseEarlySeconds)
                    break;
                yield return null;
            }
        }

        private IEnumerator PlayPitchAndWait(int cents)
        {
            _pitch.Play(cents);
            yield return WaitForPitchPlayback();
        }

        /// <summary>お手本（カエル）の確認再生（口アニメなし・内部用）。</summary>
        public void PreviewTarget()
        {
            var q = _session != null ? _session.Current : null;
            if (q == null || _pitch == null) return;
            _pitch.Play(q.targetCents);
        }

        /// <summary>選択肢の確認再生（口アニメなし・内部用）。</summary>
        public void PreviewChoice(int cents)
        {
            if (_pitch == null) return;
            _pitch.Play(cents);
        }

        /// <summary>おたまを右の睡蓮に置いたとき：答え合わせ（カエル→おたまの順再生）を開始。</summary>
        public void OnChoiceDropped(ChoiceItemView view, Vector2 screenPos)
        {
            if (view == null) return;
            if (!CanSubmitAnswer())
            {
                view.ResetPosition();
                return;
            }

            var pad = answerPad;
            if (pad == null)
            {
                view.ResetPosition();
                return;
            }

            Camera cam = ResolveEventCamera();
            if (!pad.AcceptsDrop(view.transform as RectTransform, screenPos, cam))
            {
                view.ResetPosition();
                view.SetAsTadpole(_choiceDisplaySprite);
                return;
            }

            if (_playArea == null)
                _playArea = view.transform.parent as RectTransform;

            ReturnOtherChoicesToPool(view);
            view.SnapOverAnchor(pad.SnapAnchor, _playArea, cam);
            StopMatchCoroutine();
            if (_wrongAnswerCoroutine != null)
            {
                StopCoroutine(_wrongAnswerCoroutine);
                _wrongAnswerCoroutine = null;
            }

            _matchCoroutine = StartCoroutine(RunAnswerMatch(view));
        }

        private void ReturnOtherChoicesToPool(ChoiceItemView active)
        {
            if (choiceViews == null) return;
            for (int i = 0; i < choiceViews.Count; i++)
            {
                var c = choiceViews[i];
                if (c == null || c == active) continue;
                c.ResetPosition();
                c.SetAsTadpole(_choiceDisplaySprite);
            }
        }

        private IEnumerator RunAnswerMatch(ChoiceItemView view)
        {
            var q = _session != null ? _session.Current : null;
            if (q == null || _pitch == null)
            {
                view.ResetPosition();
                _matchCoroutine = null;
                yield break;
            }

            StopAllPreviewDemos();

            // 1. お手本（左のカエル）の音
            if (frog != null)
            {
                frog.OpenMouth();
                _pitch.Play(q.targetCents);
                yield return WaitForPitchPlayback();
                frog.CloseMouth();
            }
            else
            {
                yield return PlayPitchAndWait(q.targetCents);
            }

            if (gapBetweenMatchSoundsSeconds > 0f)
                yield return new WaitForSeconds(gapBetweenMatchSoundsSeconds);

            // 2. 右睡蓮上のおたまの音
            view.OpenMouth();
            _pitch.Play(view.Cents);
            yield return WaitForPitchPlayback();
            view.CloseMouth();

            bool correct = view.Index == q.correctIndex;

            if (correct)
            {
                view.EvolveToFrog();
                _session.MarkCorrect();
                yield return view.RunCelebratePulse();

                if (delayBeforeCorrectChoiceSeconds > 0f)
                    yield return new WaitForSeconds(delayBeforeCorrectChoiceSeconds);

                if (resultEffect != null)
                {
                    resultEffect.EnsureOverlayRoot(_playArea);
                    resultEffect.ShowCorrectCelebration(
                        "せいかい！ おとが あいました",
                        "おたまが カエルに なったよ。つぎは どうする？",
                        OnCorrectNextQuestion,
                        OnCorrectReturnToMenu);
                }

                _state = State.AwaitingProgressChoice;
                _matchCoroutine = null;
                yield break;
            }

            _wrongAnswerCoroutine = StartCoroutine(HandleWrongAnswer(view));
            _matchCoroutine = null;
        }

        private IEnumerator HandleWrongAnswer(ChoiceItemView view)
        {
            view.SetAsTadpole(_choiceDisplaySprite);
            if (resultEffect != null)
            {
                resultEffect.EnsureOverlayRoot(_playArea);
                resultEffect.ShowWrongMessage("おとが ちがうよ。もういちど きいてみよう");
            }

            if (delayAfterWrongSeconds > 0f)
                yield return new WaitForSeconds(delayAfterWrongSeconds);

            if (resultEffect != null)
                resultEffect.HideWrongMessage();

            yield return view.AnimateReturnHome(wrongReturnHomeDurationSeconds);
            _wrongAnswerCoroutine = null;
        }

        private void OnCorrectNextQuestion()
        {
            if (_state != State.AwaitingProgressChoice || _session == null) return;
            if (resultEffect != null) resultEffect.HideCorrectCelebration();
            _session.Advance();
            _state = State.Playing;
            if (_session.IsFinished)
                ShowResult();
            else
                PresentCurrent();
        }

        private void OnCorrectReturnToMenu()
        {
            if (resultEffect != null) resultEffect.HideCorrectCelebration();
            OnBackPressed();
        }

        private IEnumerator WaitForPitchPlayback()
        {
            yield return null;
            while (_pitch != null && _pitch.IsPlaying)
                yield return null;
        }

        private void StopMatchCoroutine()
        {
            if (_matchCoroutine != null)
            {
                StopCoroutine(_matchCoroutine);
                _matchCoroutine = null;
            }
        }

        private Camera ResolveEventCamera()
        {
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
            if (_canvas == null) return null;
            return _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : _canvas.worldCamera;
        }

        private void ShowResult()
        {
            _state = State.Result;
            if (resultEffect != null && _session != null)
                resultEffect.ShowResult(_session.correctCount, _session.Total);
        }

        private void ShowEmpty()
        {
            _state = State.Empty;
            if (emptyState != null) emptyState.SetActive(true);
            if (errorPresenter != null)
                errorPresenter.ShowWarning("つかえる おとが ないよ。おとずかん か ろくおんを かくにんしてね");
        }

        public override void OnBackPressed()
        {
            StopMatchCoroutine();
            if (_wrongAnswerCoroutine != null)
            {
                StopCoroutine(_wrongAnswerCoroutine);
                _wrongAnswerCoroutine = null;
            }

            StopAllPreviewDemos();
            if (resultEffect != null)
            {
                resultEffect.HideCorrectCelebration();
                resultEffect.HideWrongMessage();
            }

            if (_pitch != null) _pitch.Stop();
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
