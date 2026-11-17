using Geidai.Common.Models;
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
    /// �B���Ȃ�ׂ̓����B
    /// </summary>
    public class Game3Controller : ScreenRootBase
    {
        [Header("�ݒ�")]
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

        private bool _isPlayingHint;

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
        private Sprite _baseImage;

        [Header("Hint UI")]


        [Header("Result UI")]
        [SerializeField] private Text resultText;

        [SerializeField] private float resultDisplaySeconds = 0.8f;

        [SerializeField] private float clearDisplaySeconds = 1.2f;
        private bool _isResolvingAnswer;
        protected override void OnShow()
        {
            EnsureWired();
            EnsureInstrumentShadows();

            difficultySelectPanel.SetActive(true);
            gamePanel.SetActive(false);
        }

        private void EnsureInstrumentShadows()
        {
            var displaySettings = InstrumentDisplaySettings.LoadRuntime();
            if (pitchItems == null) return;
            for (int i = 0; i < pitchItems.Count; i++)
            {
                var item = pitchItems[i];
                if (item == null) continue;
                InstrumentFloorShadow.Ensure(item.transform as RectTransform, displaySettings);
            }
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
                Debug.LogError("Game3: Game3Config���ݒ肳��Ă��܂���");
                return;
            }

            if (!TryLoadBaseSound())
            {
                Debug.LogWarning("Game3: �g�p�ł��鉹�f�ނ�����܂���");
                return;
            }

            var result = _pitch.SetBase(_baseBuffer);

            if (!result.IsSuccess)
            {
                Debug.LogWarning("Game3: ����̐ݒ�Ɏ��s���܂���");
                return;
            }

            Debug.Log("Game3: ����̏������ł��܂���");

            // �Q�[���J�n����1���
            _questionIndex = 0;

            StartQuestion();
        }
        private void StartQuestion()
        {
            int questionCount =
                config.GetQuestionCount(difficultyIndex);

            // �S��I��
            if (_questionIndex >= questionCount)
            {
                Debug.Log("Game3: �S��N���A�I");
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

            // �g�p����_�����\��
            for (int i = 0; i < cloudSlots.Count; i++)
            {
                if (cloudSlots[i] != null)
                {
                    cloudSlots[i].gameObject.SetActive(i < count);
                }
            }

            // ���� �� �E��ɋϓ��z�u
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
                        $"Game3: {slotRect.name} �� CloudSlotView ������܂���"
                    );

                    return false;
                }

                // �܂������u����Ă��Ȃ��_������
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

            // �S�Ẳ_�����܂�܂ł͔��肵�Ȃ�
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
            _baseImage = null;
            // 1. ���}�ӂ���A�s�b�`�ύX�\�ȉ���T��
            if (config != null && config.CuratedCatalog != null)
            {
                var items = config.CuratedCatalog.ValidItems();

                var candidates = new List<Geidai.Common.Library.CuratedSoundDefinition>();

                foreach (var item in items)
                {
                    if (item == null)
                        continue;

                    // ������������f�ނ����g��
                    if (!item.allowPitchShift)
                        continue;

                    if (!item.HasBasePitch)
                        continue;

                    if (item.clipRef == null)
                        continue;
                    if (item.imageRef == null)
                        continue;

                    candidates.Add(item);
                }

                if (candidates.Count > 0)
                {
                    var selected =
                        candidates[UnityEngine.Random.Range(0, candidates.Count)];

                    var buffer = BufferFromClip(selected.clipRef);

                    if (buffer != null)
                    {
                        _baseBuffer = buffer;
                        _baseImage = selected.imageRef;                       
                        return true;
                    }
                    
                }
            }

            // 2. ���}�ӂɎg���鉹��������΃��[�U�[�^��
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
                                $"Game3: ���[�U�[�����g�p���܂� ({item.meta.id})"
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
            // ����������������������������������������������������������
            // 1. �J�^���O��clip��buffer�ɕϊ�
            // ����������������������������������������������������������
            if (clip == null || clip.samples <= 0)
                return null;

            var data = new float[clip.samples * clip.channels];

            if (!clip.GetData(data, 0))
                return null;

            return new AudioBuffer(data);
        }
        public void PlayAnswerHint()
        {
            if (_currentQuestion == null)
                return;

            if (_isResolvingAnswer || _isPlayingHint)
                return;

            StartCoroutine(PlayAnswerHintCoroutine());
        }

        private IEnumerator PlayAnswerHintCoroutine()
        {
            _isPlayingHint = true;

            // ���̖��Ŏg���Ă��鉹���𐳉����ɕ��ׂ�
            List<int> answer =
                new List<int>(_currentQuestion.InitialOrder);

            answer.Sort(); // �Ⴂ �� ����

            foreach (int cents in answer)
            {
                _pitch.Play(cents);

                yield return new WaitForSecondsRealtime(1.5f);
            }

            _pitch.Stop();
            _isPlayingHint = false;
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
                        _currentQuestion.InitialOrder[i],
                        _baseImage
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

            Debug.Log("Game3: �����I");

            ShowResult("�����I");

            yield return new WaitForSecondsRealtime(
                resultDisplaySeconds
            );

            // �J�[�h�����i�֖߂�
            ResetPitchItemsForNextQuestion();

            // ���̖��ԍ���
            _questionIndex++;

            int questionCount =
                config.GetQuestionCount(difficultyIndex);

            // -----------------------------
            // �S��I��
            // -----------------------------
            if (_questionIndex >= questionCount)
            {
                Debug.Log("Game3: �S��N���A�I");

                ShowResult("�S��N���A�I");

                yield return new WaitForSecondsRealtime(
                    clearDisplaySeconds
                );

                HideResult();

                // �Q�[����Ԃ����Z�b�g
                _currentQuestion = null;
                _questionIndex = 0;

                // �Q�[����ʂ����
                if (gamePanel != null)
                {
                    gamePanel.SetActive(false);
                }

                // ��Փx�I����ʂ֖߂�
                if (difficultySelectPanel != null)
                {
                    difficultySelectPanel.SetActive(true);
                }

                _isResolvingAnswer = false;

                yield break;
            }

            // -----------------------------
            // �܂����̖�肪����
            // -----------------------------
            HideResult();

            StartQuestion();

            _isResolvingAnswer = false;
        }
        private IEnumerator HandleWrongAnswer()
        {
            _isResolvingAnswer = true;

            ShowResult("�s����");

            // ��������u�^���ɗ�����v�A�j���[�V����
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
            // ����\���Ȃǂ̏������~�߂�
            StopAllCoroutines();
            _isResolvingAnswer = false;
            _isPlayingHint = false;
            HideResult();

            // �Đ����̉����~�߂�
            if (_pitch != null)
            {
                _pitch.Stop();
            }

            // PitchItem��S�����̉��i�֖߂�
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

            // ���݂̖������Z�b�g
            _currentQuestion = null;
            _questionIndex = 0;

            // �Q�[����� �� ��Փx�I�����
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
            // �Q�[�����Ȃ�A�܂���Փx�I���֖߂�
            if (gamePanel != null && gamePanel.activeSelf)
            {
                BackToDifficultySelection();
                return;
            }

            // ��Փx�I����ʂȂ�A�Q�[���I����ʂ֖߂�
            if (_pitch != null)
            {
                _pitch.Stop();
            }

            if (_nav == null)
            {
                _nav = ServiceRegistry.Resolve<INavigationService>();
            }

            if (_nav == null)
            {
                base.OnBackPressed();
                return;
            }

            _nav.GoTo(SceneId.GameSelect);
        }
    }
}