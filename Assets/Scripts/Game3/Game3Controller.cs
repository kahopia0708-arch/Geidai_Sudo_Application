using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.UI;
using Geidai.Services;
using Geidai.Services.Audio;
using Geidai.Services.Navigation;
using Geidai.Services.Storage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Geidai.Game3
{
    /// <summary>
    /// ③音ならべの統括。
    /// </summary>
    public class Game3Controller : ScreenRootBase
    {
        [Header("設定")]
        [SerializeField] private Game3Config config;

        [Header("Screen UI")]
        [SerializeField] private GameObject difficultySelectPanel;
        [SerializeField] private GameObject gamePanel;

        [SerializeField] private int difficultyIndex = 1;
        private IStorageService _storage;
        private IPitchVariationService _pitch;
        private INavigationService _nav;

        private AudioBuffer _baseBuffer;

        private PitchOrderQuestion _currentQuestion;
        private int _questionIndex = 0;

        [Header("Cloud UI")]
        [SerializeField]
        private RectTransform cloudArea;
        [SerializeField]
        private List<RectTransform> cloudSlots = new List<RectTransform>();
        [SerializeField]
        private Vector2 cloudStartPosition = new Vector2(-150f, -150f);
        [SerializeField]
        private Vector2 cloudEndPosition = new Vector2(150f, 150f);

        [Header("PitchItem UI")]
        [SerializeField]
        private List<PitchItemView> pitchItems = new List<PitchItemView>();

        [Header("Result UI")]
        [SerializeField] private Text resultText;

        [SerializeField] private float resultDisplaySeconds = 0.8f;

        [SerializeField] private float clearDisplaySeconds = 1.2f;
        private bool _isResolvingAnswer;
        protected override void OnShow()
        {
            EnsureWired();

            difficultySelectPanel.SetActive(true);
            gamePanel.SetActive(false);
        }

        private void EnsureWired()
        {
            _storage = ServiceRegistry.Resolve<IStorageService>();
            _nav = ServiceRegistry.Resolve<INavigationService>();
            _pitch = Game3Bootstrap.EnsurePitchVariationService();
        }
        public void SelectDifficulty(int index)
        {
            difficultyIndex = index;

            difficultySelectPanel.SetActive(false);
            gamePanel.SetActive(true);

            StartGame();
        }
        public void StartGame()
        {
            if (config == null)
            {
                Debug.LogError("Game3: Game3Configが設定されていません");
                return;
            }

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

            // ゲーム開始時は1問目
            _questionIndex = 0;

            StartQuestion();
        }
        private void StartQuestion()
        {
            int questionCount =
                config.GetQuestionCount(difficultyIndex);

            // 全問終了
            if (_questionIndex >= questionCount)
            {
                Debug.Log("Game3: 全問クリア！");
                return;
            }

            int choiceCount =
                config.GetChoiceCount(difficultyIndex);

            int centsStep =
                config.GetCentsStep(
                    difficultyIndex,
                    _questionIndex
                );

            ConfigureCloudSlots(choiceCount);

            _currentQuestion =
                PitchOrderQuestionBuilder.Build(
                    choiceCount,
                    centsStep,
                    System.Environment.TickCount
                );

      /*  Debug.Log(
                $"Game3 Question: " +
                $"Question={_questionIndex + 1}/{questionCount}, " +
                $"ChoiceCount={choiceCount}, " +
                $"CentsStep={centsStep}, " +
                $"InitialOrder=[{string.Join(", ", _currentQuestion.InitialOrder)}]"
            );*/

            PresentQuestion();
        }
        private void ConfigureCloudSlots(int count)
        {
            count = Mathf.Clamp(count, 2, 4);

            // 使用する雲だけ表示
            for (int i = 0; i < cloudSlots.Count; i++)
            {
                if (cloudSlots[i] != null)
                {
                    cloudSlots[i].gameObject.SetActive(i < count);
                }
            }

            // 左下 → 右上に均等配置
            for (int i = 0; i < count; i++)
            {
                if (cloudSlots[i] == null)
                    continue;

                float t = (float)i / (count - 1);

                cloudSlots[i].anchoredPosition =
                    Vector2.Lerp(
                        cloudStartPosition,
                        cloudEndPosition,
                        t
                    );
            }
        }
        private bool TryGetCurrentCloudOrder(out List<int> order)
        {
            order = new List<int>();

            for (int i = 0; i < cloudSlots.Count; i++)
            {
                RectTransform slotRect = cloudSlots[i];

                if (slotRect == null ||
                    !slotRect.gameObject.activeInHierarchy)
                {
                    continue;
                }

                CloudSlotView slot =
                    slotRect.GetComponent<CloudSlotView>();

                if (slot == null)
                {
                    Debug.LogWarning(
                        $"Game3: {slotRect.name} に CloudSlotView がありません"
                    );

                    return false;
                }

                // まだ何も置かれていない雲がある
                if (slot.Occupant == null)
                {
                    return false;
                }

                order.Add(slot.Occupant.Cents);
            }

            return order.Count >= 2;
        }
        public void CheckCurrentOrder()
        {
            if (_currentQuestion == null)
                return;

            // 全ての雲が埋まるまでは判定しない
            if (!TryGetCurrentCloudOrder(
                out List<int> currentOrder))
            {
                return;
            }

            bool correct =
                PitchOrderJudge.IsAscending(currentOrder);

            if (correct)
            {
               
                StartCoroutine(  HandleCorrectAnswer()   );
            }
            else
            {
               
                StartCoroutine( HandleWrongAnswer()  );
            }
        }
        private void ReturnAllPitchItemsHome()
        {
            foreach (PitchItemView item in pitchItems)
            {
                if (item == null ||
                    !item.gameObject.activeInHierarchy)
                {
                    continue;
                }

                PitchItemDragHandler dragHandler =
                    item.GetComponent<PitchItemDragHandler>();

                if (dragHandler != null)
                {
                    dragHandler.FallAndSendHome();
                }
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
        private IEnumerator HandleCorrectAnswer()
        {
            _isResolvingAnswer = true;

            Debug.Log("Game3: 正解！");

            ShowResult("正解！");

            yield return new WaitForSecondsRealtime(
                resultDisplaySeconds
            );

            // カードを下段へ戻す
            ResetPitchItemsForNextQuestion();

            // 次の問題番号へ
            _questionIndex++;

            int questionCount =
                config.GetQuestionCount(difficultyIndex);

            // -----------------------------
            // 全問終了
            // -----------------------------
            if (_questionIndex >= questionCount)
            {
                Debug.Log("Game3: 全問クリア！");

                ShowResult("全問クリア！");

                yield return new WaitForSecondsRealtime(
                    clearDisplaySeconds
                );

                HideResult();

                // ゲーム状態をリセット
                _currentQuestion = null;
                _questionIndex = 0;

                // ゲーム画面を閉じる
                if (gamePanel != null)
                {
                    gamePanel.SetActive(false);
                }

                // 難易度選択画面へ戻る
                if (difficultySelectPanel != null)
                {
                    difficultySelectPanel.SetActive(true);
                }

                _isResolvingAnswer = false;

                yield break;
            }

            // -----------------------------
            // まだ次の問題がある
            // -----------------------------
            HideResult();

            StartQuestion();

            _isResolvingAnswer = false;
        }
        private IEnumerator HandleWrongAnswer()
        {
            _isResolvingAnswer = true;

            ShowResult("不正解");

            // 今作った「真下に落ちる」アニメーション
            ReturnAllPitchItemsHome();

            yield return new WaitForSecondsRealtime(
                resultDisplaySeconds
            );

            HideResult();

            _isResolvingAnswer = false;
        }
        private void ShowResult(string message)
        {
            if (resultText == null)
                return;

            resultText.text = message;
            resultText.gameObject.SetActive(true);
        }

        private void HideResult()
        {
            if (resultText == null)
                return;

            resultText.gameObject.SetActive(false);
        }
        private void ResetPitchItemsForNextQuestion()
        {
            foreach (PitchItemView item in pitchItems)
            {
                if (item == null)
                    continue;

                PitchItemDragHandler drag =
                    item.GetComponent<PitchItemDragHandler>();

                if (drag != null)
                {
                    drag.SendHome();
                }
            }
        }

        public void BackToDifficultySelection()
        {
            // 再生中の音を止める
            if (_pitch != null)
            {
                _pitch.Stop();
            }

            // PitchItemを全部元の下段へ戻す
            foreach (PitchItemView item in pitchItems)
            {
                if (item == null)
                    continue;

                PitchItemDragHandler dragHandler =
                    item.GetComponent<PitchItemDragHandler>();

                if (dragHandler != null)
                {
                    dragHandler.SendHome();
                }
            }

            // 現在の問題をリセット
            _currentQuestion = null;
            _questionIndex = 0;

            // ゲーム画面 → 難易度選択画面
            if (gamePanel != null)
            {
                gamePanel.SetActive(false);
            }

            if (difficultySelectPanel != null)
            {
                difficultySelectPanel.SetActive(true);
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