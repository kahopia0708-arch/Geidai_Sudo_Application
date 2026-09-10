using System.Collections.Generic;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.UI;
using Geidai.Services;
using Geidai.Services.Audio;
using Geidai.Services.Navigation;
using Geidai.Services.Storage;
using UnityEngine;


namespace Geidai.Game3
{
    /// <summary>
    /// ③音ならべの統括。
    /// </summary>
    public class Game3Controller : ScreenRootBase
    {
        [Header("設定")]
        [SerializeField] private Game3Config config;

        [SerializeField] private int difficultyIndex = 1;
        private IStorageService _storage;
        private IPitchVariationService _pitch;
        private INavigationService _nav;

        private AudioBuffer _baseBuffer;

        private PitchOrderQuestion _currentQuestion;

        [Header("UI")]
        [SerializeField]
        private List<PitchItemView> pitchItems = new List<PitchItemView>();

        protected override void OnShow()
        {
            EnsureWired();
            StartGame();
        }

        private void EnsureWired()
        {
            _storage = ServiceRegistry.Resolve<IStorageService>();
            _nav = ServiceRegistry.Resolve<INavigationService>();
            _pitch = Game3Bootstrap.EnsurePitchVariationService();
        }

        public void StartGame()
        {
            if (!TryLoadBaseSound())
            {
                Debug.LogWarning("Game3: 使用できる音素材がありません");
                return;
            }

            var result = _pitch.SetBase(_baseBuffer);

            if (!result.IsSuccess)
            {
                Debug.LogWarning("Game3: 基準音の設定に失敗しました");
                return;
            }

            Debug.Log("Game3: 基準音の準備ができました");

            var difficulty = config.GetDifficulty(difficultyIndex);

            _currentQuestion = PitchOrderQuestionBuilder.Build(
                config.ChoiceCount,
                difficulty.centsStep,
                System.Environment.TickCount
            );

            Debug.Log(
                $"Game3 Question: " +
                $"Direction={_currentQuestion.Direction}, " +
                $"InitialOrder=[{string.Join(", ", _currentQuestion.InitialOrder)}]"
            );

            PresentQuestion();
        }
        private List<int> GetCurrentPitchOrder()
        {
            var order = new List<int>();

            if (pitchItems == null || pitchItems.Count == 0)
                return order;

            Transform container = pitchItems[0].transform.parent;

            for (int i = 0; i < container.childCount; i++)
            {
                PitchItemView item =
                    container.GetChild(i).GetComponent<PitchItemView>();

                if (item != null)
                {
                    order.Add(item.Cents);
                }
            }

            return order;
        }
        public void CheckCurrentOrder()
        {
            if (_currentQuestion == null)
                return;

            List<int> currentOrder = GetCurrentPitchOrder();

            bool correct;

            if (_currentQuestion.Direction == PitchOrderDirection.Ascending)
            {
                correct = PitchOrderJudge.IsAscending(currentOrder);
            }
            else
            {
                correct = PitchOrderJudge.IsDescending(currentOrder);
            }

            Debug.Log(
                $"Game3 Judge: " +
                $"Direction={_currentQuestion.Direction}, " +
                $"CurrentOrder=[{string.Join(", ", currentOrder)}], " +
                $"Correct={correct}"
            );

            if (correct)
            {
                Debug.Log("Game3: 正解！");
            }
            else
            {
                Debug.Log("Game3: まだ違います");
            }
        }
        private bool TryLoadBaseSound()
        {
            // 1. Game3Configに設定されたカタログ音を優先
            if (config != null &&
                config.CatalogClips != null &&
                config.CatalogClips.Count > 0)
            {
                foreach (var clip in config.CatalogClips)
                {
                    if (clip == null)
                        continue;

                    var buffer = BufferFromClip(clip);

                    if (buffer != null)
                    {
                        _baseBuffer = buffer;

                        Debug.Log(
                            $"Game3: カタログ音を使用します ({clip.name})"
                        );

                        return true;
                    }
                }
            }

            // 2. カタログ音が無ければユーザー録音
            if (_storage != null)
            {
                var listResult = _storage.ListSounds();

                if (listResult.IsSuccess &&
                    listResult.Value != null &&
                    listResult.Value.Count > 0)
                {
                    foreach (var item in listResult.Value)
                    {
                        if (item.meta == null ||
                            string.IsNullOrEmpty(item.meta.id))
                        {
                            continue;
                        }

                        var bufferResult =
                            _storage.LoadSoundBuffer(item.meta.id);

                        if (bufferResult.IsSuccess &&
                            bufferResult.Value != null)
                        {
                            _baseBuffer = bufferResult.Value;

                            Debug.Log(
                                $"Game3: ユーザー音を使用します ({item.meta.id})"
                            );

                            return true;
                        }
                    }
                }
            }

            return false;
        }
        private static AudioBuffer BufferFromClip(AudioClip clip)
        {
            // ─────────────────────────────
            // 1. カタログ音clipをbufferに変換
            // ─────────────────────────────
            if (clip == null || clip.samples <= 0)
                return null;

            var data = new float[clip.samples * clip.channels];

            if (!clip.GetData(data, 0))
                return null;

            return new AudioBuffer(data);
        }

        public void PreviewPitch(int cents)
        {
            if (_pitch == null)
                return;

            _pitch.Play(cents);
        }
        private void PresentQuestion()
        {
            if (_currentQuestion == null)
                return;

            for (int i = 0; i < pitchItems.Count; i++)
            {
                var view = pitchItems[i];

                if (view == null)
                    continue;

                if (i < _currentQuestion.InitialOrder.Count)
                {
                    view.gameObject.SetActive(true);

                    view.Setup(
                        this,
                        _currentQuestion.InitialOrder[i]
                    );
                }
                else
                {
                    view.gameObject.SetActive(false);
                }
            }
        }
        public override void OnBackPressed()
        {
            if (_pitch != null)
                _pitch.Stop();

            if (_nav == null)
                _nav = ServiceRegistry.Resolve<INavigationService>();

            if (_nav == null)
            {
                base.OnBackPressed();
                return;
            }

            Result result = _nav.GoTo(SceneId.Home);

            if (!result.IsSuccess)
            {
                UnityEngine.Debug.LogWarning(
                    $"Game3: Homeへの遷移に失敗しました: {result}"
                );
            }
        }
    }
}