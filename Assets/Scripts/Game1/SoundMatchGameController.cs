using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Geidai.Common.Game;
using Geidai.Common.Library;
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
    /// ①音合わせ。弟子へ図鑑楽器をドラッグすると答え合わせ（師匠→楽器の順再生）→判定。
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
        private Sprite _targetDisplaySprite;
        private Sprite _wizardMasterSprite;
        private Sprite _wizardApprenticeSprite;
        private Sprite _choiceFallbackSprite;
        private State _state = State.Loading;
        private Coroutine _matchCoroutine;
        private Coroutine _frogDemoCoroutine;
        private readonly Dictionary<ChoiceItemView, Coroutine> _choiceDemoCoroutines = new();
        private Coroutine _wrongAnswerCoroutine;
        private RectTransform _wizardStage;
        private RectTransform _apprenticeStage;
        private SoundMatchChoicePresentation[] _currentChoicePresentations = System.Array.Empty<SoundMatchChoicePresentation>();

        internal bool HasAnswerPad => answerPad != null;

        internal SoundMatchConfig Config => config;

        internal RectTransform ChoicePoolParent => _playArea;

        internal int ActiveChoiceCount => choiceViews != null ? choiceViews.Count : 0;

        internal void ApplyStoryboardReferences(
            FrogTargetView demoWizard,
            SoundMatchAnswerPadView pad,
            RectTransform playArea,
            RectTransform wizardStage,
            RectTransform apprenticeStage)
        {
            frog = demoWizard;
            answerPad = pad;
            _playArea = playArea;
            _wizardStage = wizardStage;
            _apprenticeStage = apprenticeStage;
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
            var bootstrap = GetComponent<SoundMatchStoryboardUiBootstrap>();
            if (bootstrap != null)
                bootstrap.EnsureStoryboardBuilt();
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
            _wizardMasterSprite = Game1WizardArt.ResolveMaster(config);
            _wizardApprenticeSprite = Game1WizardArt.ResolveApprentice(config);
            _choiceFallbackSprite = config.PlaceholderChoiceSprite;
            _targetDisplaySprite = SanitizeTargetDisplaySprite(_targetDisplaySprite);
            if (_targetDisplaySprite == null)
                _targetDisplaySprite = SanitizeTargetDisplaySprite(_choiceFallbackSprite);
        }

        private Sprite SanitizeTargetDisplaySprite(Sprite sprite)
        {
            return Game1LegacyTadpoleMotif.ResolveInstrumentSprite(
                config,
                sprite,
                Game1LegacyTadpoleMotif.FirstCatalogInstrumentSprite(config));
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

            var questions = QuestionBuilder.BuildQuestions(
                _baseSoundId,
                config,
                difficultyIndex,
                effectiveSeed,
                config != null ? config.CuratedCatalog : null);
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
                resultEffect.HideResultSummary();
                resultEffect.ResetGrowth(_targetDisplaySprite);
                resultEffect.HideCorrectCelebration();
            }

            ResetAllChoiceInteractionState();
            PresentCurrent();
        }

        private void ResetAllChoiceInteractionState()
        {
            if (choiceViews == null) return;
            for (int i = 0; i < choiceViews.Count; i++)
            {
                var view = choiceViews[i];
                if (view == null) continue;
                view.PrepareForPoolHome(_playArea);
            }
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
                _targetDisplaySprite = fromCatalog.DisplaySprite;
                _targetDisplaySprite = SanitizeTargetDisplaySprite(_targetDisplaySprite);
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
            ApplyQuestionBase(q);
            if (_baseBuffer != null && _pitch != null)
                _pitch.SetBase(_baseBuffer);
            if (resultEffect != null)
            {
                if (_playArea != null) resultEffect.EnsureOverlayRoot(_playArea);
                resultEffect.HideCorrectCelebration();
                resultEffect.HideWrongMessage();
            }
            if (frog != null)
            {
                RefreshDisplaySpritesFromConfig();
                frog.Setup(this, _wizardMasterSprite, null);
            }
            else
            {
                RefreshDisplaySpritesFromConfig();
            }
            if (_wizardStage != null)
                Game1WizardHeldInstrument.Hide(_wizardStage);

            var choicePresentations = BuildChoicePresentationsForCurrent(q);

            for (int i = 0; i < choiceViews.Count; i++)
            {
                var view = choiceViews[i];
                if (view == null) continue;
                view.SetInputLocked(false);
                if (i < q.choices.Count)
                {
                    view.gameObject.SetActive(true);
                    var presentation = i < choicePresentations.Length
                        ? choicePresentations[i]
                        : new SoundMatchChoicePresentation(_targetDisplaySprite, _baseSoundId);
                    var sprite = ResolveChoiceDisplaySprite(presentation);
                    view.Setup(this, q.choices[i], i, sprite, sprite, null);
                    view.ApplyPoolHomeLayout(i, q.choices.Count);
                    view.SetInstrument(sprite);
                }
                else
                {
                    view.gameObject.SetActive(false);
                }
            }
        }

        private SoundMatchChoicePresentation[] BuildChoicePresentationsForCurrent(Question q)
        {
            if (q == null || _session == null)
                return System.Array.Empty<SoundMatchChoicePresentation>();

            int effectiveSeed = seed != 0 ? seed : System.Environment.TickCount;
            effectiveSeed = unchecked(effectiveSeed + _session.currentIndex * 997);
            var presentations = SoundMatchChoiceCatalogSprites.BuildForQuestion(
                config != null ? config.CuratedCatalog : null,
                SanitizeTargetDisplaySprite(_targetDisplaySprite),
                _baseSoundId,
                q.choices.Count,
                q.correctIndex,
                effectiveSeed);
            _currentChoicePresentations = presentations;
            return presentations;
        }

        private Sprite ResolveChoiceDisplaySprite(SoundMatchChoicePresentation presentation)
        {
            var fromPresentation = Game1LegacyTadpoleMotif.ResolveInstrumentSprite(
                config,
                presentation.Sprite,
                null);
            if (fromPresentation != null) return fromPresentation;

            return SanitizeTargetDisplaySprite(_targetDisplaySprite);
        }

        /// <summary>現在の問の図鑑素材（音色・表示）を _base* に反映する。</summary>
        private void ApplyQuestionBase(Question q)
        {
            if (q == null) return;

            if (!string.IsNullOrEmpty(q.baseSoundId) &&
                config?.CuratedCatalog != null)
            {
                var def = config.CuratedCatalog.FindById(q.baseSoundId);
                if (def?.clipRef != null)
                {
                    var buf = SoundMatchBaseSoundSelector.BufferFromClip(def.clipRef);
                    if (buf != null)
                    {
                        _baseSoundId = q.baseSoundId;
                        _baseBuffer = buf;
                        _targetDisplaySprite = SanitizeTargetDisplaySprite(def.imageRef);
                    }
                }
            }
        }

        private bool TryResolveBuffer(string soundId, out AudioBuffer buffer)
        {
            buffer = null;
            if (!string.IsNullOrEmpty(soundId) && soundId == _baseSoundId && _baseBuffer != null)
            {
                buffer = _baseBuffer;
                return true;
            }

            if (config?.CuratedCatalog == null || string.IsNullOrEmpty(soundId))
                return false;

            foreach (var item in config.CuratedCatalog.ValidItems())
            {
                if (item == null || item.id != soundId) continue;
                buffer = SoundMatchBaseSoundSelector.BufferFromClip(item.clipRef);
                return buffer != null;
            }

            return false;
        }

        private void PlayTargetPitch(int cents)
        {
            if (_pitch == null) return;
            if (_baseBuffer != null)
                _pitch.Play(_baseBuffer, cents);
            else
                _pitch.Play(cents);
        }

        private void PlayChoicePitch(int choiceIndex, int cents)
        {
            if (_pitch == null) return;
            string soundId = _baseSoundId;
            if (choiceIndex >= 0 &&
                choiceIndex < _currentChoicePresentations.Length &&
                !string.IsNullOrEmpty(_currentChoicePresentations[choiceIndex].SoundId))
            {
                soundId = _currentChoicePresentations[choiceIndex].SoundId;
            }

            int playbackCents = ResolveChoicePlaybackCents(soundId, cents);
            if (TryResolveBuffer(soundId, out var buffer))
                _pitch.Play(buffer, playbackCents);
            else if (_baseBuffer != null)
                _pitch.Play(_baseBuffer, cents);
            else
                _pitch.Play(cents);
        }

        /// <summary>
        /// 図鑑 clip の基準 MIDI と今回のお手本音の差を、出題セントに加算する。
        /// </summary>
        private int ResolveChoicePlaybackCents(string soundId, int questionCents)
        {
            if (config?.CuratedCatalog == null || string.IsNullOrEmpty(soundId))
                return questionCents;

            var baseDef = config.CuratedCatalog.FindById(_baseSoundId);
            var choiceDef = config.CuratedCatalog.FindById(soundId);
            if (baseDef == null || choiceDef == null)
                return questionCents;
            if (!baseDef.HasBasePitch || !choiceDef.HasBasePitch)
                return questionCents;

            int midiDelta = choiceDef.basePitchMidi - baseDef.basePitchMidi;
            return questionCents + midiDelta * 100;
        }

        /// <summary>師匠タップ：お手本の音。</summary>
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
                PlayTargetPitch(q.targetCents);
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
                PlayChoicePitch(view.Index, view.Cents);
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
            PlayTargetPitch(cents);
            yield return WaitForPitchPlayback();
        }

        /// <summary>お手本（カエル）の確認再生（口アニメなし・内部用）。</summary>
        public void PreviewTarget()
        {
            var q = _session != null ? _session.Current : null;
            if (q == null || _pitch == null) return;
            PlayTargetPitch(q.targetCents);
        }

        /// <summary>選択肢の確認再生（口アニメなし・内部用）。</summary>
        public void PreviewChoice(int choiceIndex, int cents)
        {
            if (_pitch == null) return;
            PlayChoicePitch(choiceIndex, cents);
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
                return;
            }

            if (_playArea == null)
                _playArea = view.transform.parent as RectTransform;

            ReturnOtherChoicesToPool(view);
            StopMatchCoroutine();
            if (_wrongAnswerCoroutine != null)
            {
                StopCoroutine(_wrongAnswerCoroutine);
                _wrongAnswerCoroutine = null;
            }

            _matchCoroutine = StartCoroutine(RunAnswerMatchAfterSnap(view));
        }

        private IEnumerator RunAnswerMatchAfterSnap(ChoiceItemView view)
        {
            var pad = answerPad;
            if (pad != null && pad.SnapAnchor != null)
                yield return view.AnimateMagnetSnap(pad.SnapAnchor, 0.2f);
            yield return RunAnswerMatch(view);
        }

        private void ReturnOtherChoicesToPool(ChoiceItemView active)
        {
            if (choiceViews == null) return;
            for (int i = 0; i < choiceViews.Count; i++)
            {
                var c = choiceViews[i];
                if (c == null || c == active) continue;
                c.ResetPosition();
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

            // 1. 師匠のお手本の音
            if (frog != null)
            {
                PlayTargetPitch(q.targetCents);
                yield return WaitForPitchPlayback();
            }
            else
            {
                yield return PlayPitchAndWait(q.targetCents);
            }

            if (gapBetweenMatchSoundsSeconds > 0f)
                yield return new WaitForSeconds(gapBetweenMatchSoundsSeconds);

            // 2. 弟子の手元の楽器の音（図鑑 clip ＋ ピッチ）
            PlayChoicePitch(view.Index, view.Cents);
            yield return WaitForPitchPlayback();

            bool correct = view.Index == q.correctIndex;

            if (correct)
            {
                if (_wizardStage != null)
                    Game1WizardHeldInstrument.Reveal(_wizardStage, SanitizeTargetDisplaySprite(_targetDisplaySprite));

                view.ConfirmHeldMatch();
                _session.MarkCorrect();
                yield return view.RunCelebratePulse();

                if (delayBeforeCorrectChoiceSeconds > 0f)
                    yield return new WaitForSeconds(delayBeforeCorrectChoiceSeconds);

                if (resultEffect != null)
                {
                    resultEffect.EnsureOverlayRoot(_playArea);
                    resultEffect.ShowCorrectCelebration(
                        "せいかい！ おとが あいました",
                        "おとが ぴったり！ つぎは どうする？",
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
            ReturnAllChoicesToPoolHome();
            _session.Advance();
            _state = State.Playing;
            if (_session.IsFinished)
                ShowResult();
            else
                PresentCurrent();
        }

        private void ReturnAllChoicesToPoolHome()
        {
            if (choiceViews == null) return;
            for (int i = 0; i < choiceViews.Count; i++)
            {
                var view = choiceViews[i];
                if (view == null) continue;
                view.PrepareForPoolHome(_playArea);
            }
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
            StopMatchCoroutine();
            StopAllPreviewDemos();
            FinalizeChoicesForSessionEnd();
            if (_wizardStage != null)
                Game1WizardHeldInstrument.Hide(_wizardStage);
            if (resultEffect != null && _session != null)
                resultEffect.ShowResult(_session.correctCount, _session.Total);
        }

        private void FinalizeChoicesForSessionEnd()
        {
            ReturnAllChoicesToPoolHome();
            if (choiceViews == null) return;
            for (int i = 0; i < choiceViews.Count; i++)
            {
                var view = choiceViews[i];
                if (view == null) continue;
                view.FinalizeForSessionEnd(_playArea);
            }
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
                resultEffect.HideResultSummary();
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
