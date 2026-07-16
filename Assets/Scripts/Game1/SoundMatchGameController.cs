using System.Collections.Generic;
using UnityEngine;
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
    /// ①音合わせの統括（U6 / P3/P5）。素材選択→出題生成→タップ確認→ドラッグ解答→判定→演出→進行。
    /// 保存音は IStorageService（Collection 非依存）、発音は IPitchVariationService（再生時ピッチ・非保存）。
    /// 失敗は Result＋ErrorPresenter でクラッシュさせない。
    /// </summary>
    public class SoundMatchGameController : ScreenRootBase
    {
        private enum State { Loading, Empty, Playing, Judging, Result }

        [Header("設定")]
        [SerializeField] private SoundMatchConfig config;
        [SerializeField] private int difficultyIndex = 1; // 既定=ふつう
        [Tooltip("0 のとき起動時刻からシードを作る（毎回変化）。固定値でデバッグ再現。")]
        [SerializeField] private int seed = 0;

        [Header("UI 参照")]
        [SerializeField] private FrogTargetView frog;
        [SerializeField] private List<ChoiceItemView> choiceViews = new List<ChoiceItemView>();
        [SerializeField] private ResultEffectController resultEffect;
        [SerializeField] private ErrorPresenter errorPresenter;
        [SerializeField] private GameObject emptyState;

        private IStorageService _storage;
        private IPitchVariationService _pitch;
        private INavigationService _nav;
        private Canvas _canvas;

        private GameSession _session;
        private AudioBuffer _baseBuffer;
        private string _baseSoundId = string.Empty;
        private State _state = State.Loading;

        protected override void OnShow()
        {
            EnsureWired();
            StartGame();
        }

        private void EnsureWired()
        {
            _storage = ServiceRegistry.Resolve<IStorageService>();
            _nav = ServiceRegistry.Resolve<INavigationService>();
            _pitch = Game1Bootstrap.EnsurePitchVariationService();
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
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
            PresentCurrent();
        }

        private bool TryLoadBase(int seedValue)
        {
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
                            return true;
                        }
                    }
                }
            }

            // 保存音が無い/全滅 → fallbackClip
            if (config != null && config.FallbackClip != null)
            {
                var buf = BufferFromClip(config.FallbackClip);
                if (buf != null)
                {
                    _baseBuffer = buf;
                    _baseSoundId = string.Empty;
                    return true;
                }
            }

            return false;
        }

        private static AudioBuffer BufferFromClip(AudioClip clip)
        {
            if (clip == null || clip.samples <= 0) return null;
            var data = new float[clip.samples * clip.channels];
            if (!clip.GetData(data, 0)) return null;
            return new AudioBuffer(data);
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
            if (frog != null) frog.Setup(this);

            for (int i = 0; i < choiceViews.Count; i++)
            {
                var view = choiceViews[i];
                if (view == null) continue;
                if (i < q.choices.Count)
                {
                    view.gameObject.SetActive(true);
                    view.Setup(this, q.choices[i], i);
                }
                else
                {
                    view.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>お手本（カエル）の確認再生。</summary>
        public void PreviewTarget()
        {
            var q = _session != null ? _session.Current : null;
            if (q == null || _pitch == null) return;
            _pitch.Play(q.targetCents);
        }

        /// <summary>選択肢（おたまじゃくし）の確認再生。</summary>
        public void PreviewChoice(int cents)
        {
            if (_pitch == null) return;
            _pitch.Play(cents);
        }

        /// <summary>選択肢がお手本のドロップ領域で離されたときの処理。</summary>
        public void OnChoiceDropped(ChoiceItemView view, Vector2 screenPos)
        {
            if (view == null) return;
            Camera cam = _canvas != null ? _canvas.worldCamera : null;
            if (frog != null && frog.ContainsScreenPoint(screenPos, cam))
                SubmitAnswer(view.Index);
            else
                view.ResetPosition();
        }

        /// <summary>解答を確定して判定する（純粋比較）。</summary>
        public void SubmitAnswer(int choiceIndex)
        {
            if (_state != State.Playing || _session == null) return;
            var q = _session.Current;
            if (q == null) return;

            _state = State.Judging;
            bool correct = choiceIndex == q.correctIndex;

            if (correct)
            {
                _session.MarkCorrect();
                if (resultEffect != null) resultEffect.PlayCorrect();
                _session.Advance();
                if (_session.IsFinished) ShowResult();
                else PresentCurrent();
            }
            else
            {
                if (resultEffect != null) resultEffect.PlayRetry();
                _state = State.Playing;
            }
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
            if (errorPresenter != null) errorPresenter.ShowWarning("ろくおんした おとが ないよ。さきに ろくおんしてね");
        }

        public override void OnBackPressed()
        {
            if (_pitch != null) _pitch.Stop();
            if (_nav == null) _nav = ServiceRegistry.Resolve<INavigationService>();
            if (_nav == null)
            {
                base.OnBackPressed();
                return;
            }

            Result result = _nav.GoTo(SceneId.Home);
            if (!result.IsSuccess && errorPresenter != null) errorPresenter.ShowFromResult(result);
        }
    }
}
