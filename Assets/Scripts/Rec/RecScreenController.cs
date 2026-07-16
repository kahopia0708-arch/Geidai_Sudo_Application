using UnityEngine;
using UnityEngine.UI;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.UI;
using Geidai.Services;
using Geidai.Services.Audio;
using Geidai.Services.Navigation;
using Geidai.Services.Storage;

namespace Geidai.Rec
{
    /// <summary>
    /// Rec 画面の司令塔（frontend-components §1 / business-logic-model）。
    /// <see cref="RecordingState"/> の状態遷移を管理し、録音/加工/保存の各コントローラと
    /// UI（ボタン/状態表示）を結線する。戻る時は未保存なら破棄確認（ConfirmDialog）を挟む。
    /// Common は Services に依存しないため、NavigationService への接続はここ（Rec）で購読する。
    /// </summary>
    public class RecScreenController : ScreenRootBase
    {
        [Header("Controllers")]
        [SerializeField] private RecordingController recordingController;
        [SerializeField] private EffectPanelController effectPanel;
        [SerializeField] private SavePromptController savePrompt;

        [Header("UI")]
        [SerializeField] private Button recordButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Text statusText;
        [SerializeField] private ErrorPresenter errorPresenter;
        [SerializeField] private ConfirmDialog confirmDialog;

        [Header("State visuals (任意 / S さん調整)")]
        [SerializeField] private GameObject recordingIndicator;
        [SerializeField] private GameObject recordedPanel;
        [SerializeField] private GameObject savedIndicator;
        [SerializeField] private GameObject noMicPanel;

        private RecordingState _state = RecordingState.Idle;
        private AudioBuffer _recorded;
        private INavigationService _nav;
        private IStorageService _storage;
        private IAudioService _audio;
        private bool _wired;

        public RecordingState State => _state;

        protected override void OnShow()
        {
            EnsureWired();
            // 未許可(Denied)は Idle のまま「ろくおん」で権限ダイアログを出す。
            // NoDevice（ハード無し/許可後もデバイス無し）のみ初期から録音不可にする。
            // iOS は権限前に Microphone.devices が空になり NoDevice 誤判定しやすい（MicPermissionGate 側でも防御）。
            var status = MicPermissionGate.Check();
            RefreshState(status == MicPermissionStatus.NoDevice
                ? RecordingState.NoMic
                : RecordingState.Idle);
        }

        private void EnsureWired()
        {
            if (_wired) return;

            _nav = ServiceRegistry.Resolve<INavigationService>();
            _storage = ServiceRegistry.Resolve<IStorageService>();
            _audio = RecBootstrap.EnsureAudioService();

            if (effectPanel != null) effectPanel.Init(_audio);

            if (recordingController != null)
            {
                recordingController.Init(_audio);
                recordingController.RecordingStarted += HandleRecordingStarted;
                recordingController.RecordingCompleted += HandleRecordingCompleted;
                recordingController.PermissionBlocked += HandlePermissionBlocked;
                recordingController.RecordingFailed += HandleRecordingFailed;
            }

            if (savePrompt != null)
            {
                savePrompt.Init(_storage);
                savePrompt.Saved += HandleSaved;
                savePrompt.SaveFailed += HandleSaveFailed;
            }

            if (recordButton != null) recordButton.onClick.AddListener(OnRecordPressed);
            if (playButton != null) playButton.onClick.AddListener(OnPlayPressed);
            if (saveButton != null) saveButton.onClick.AddListener(OnSavePressed);
            if (backButton != null) backButton.onClick.AddListener(OnBackPressed);

            _wired = true;
        }

        private void OnDestroy()
        {
            if (recordingController != null)
            {
                recordingController.RecordingStarted -= HandleRecordingStarted;
                recordingController.RecordingCompleted -= HandleRecordingCompleted;
                recordingController.PermissionBlocked -= HandlePermissionBlocked;
                recordingController.RecordingFailed -= HandleRecordingFailed;
            }
            if (savePrompt != null)
            {
                savePrompt.Saved -= HandleSaved;
                savePrompt.SaveFailed -= HandleSaveFailed;
            }
        }

        // --- UI actions ---

        private void OnRecordPressed()
        {
            if (_state == RecordingState.Recording || _state == RecordingState.Saving) return;
            if (recordingController != null) recordingController.BeginRecording();
        }

        private void OnPlayPressed()
        {
            if (_recorded == null || _audio == null) return;
            var result = _audio.Play(_recorded);
            if (result.IsSuccess) RefreshState(RecordingState.Playing);
            else if (errorPresenter != null) errorPresenter.ShowFromResult(result);
        }

        private void OnSavePressed()
        {
            if (_recorded == null || savePrompt == null) return;
            RefreshState(RecordingState.Saving);
            savePrompt.Save(_recorded, effectPanel != null ? effectPanel.CurrentSettings : null);
        }

        /// <summary>戻る/システムバック：未保存録音があれば破棄確認、なければ直前画面へ（お題→録音ならお題）。</summary>
        public override void OnBackPressed()
        {
            bool hasUnsaved = _state == RecordingState.Recorded || _state == RecordingState.Playing;
            if (hasUnsaved && confirmDialog != null)
            {
                confirmDialog.Show("かくにん", "ほぞんしないで もどる？", NavigateBack);
                return;
            }
            NavigateBack();
        }

        // --- controller events ---

        private void HandleRecordingStarted() => RefreshState(RecordingState.Recording);

        private void HandleRecordingCompleted(AudioBuffer buffer)
        {
            _recorded = buffer;
            if (effectPanel != null) effectPanel.ApplyToChain();
            RefreshState(RecordingState.Recorded);
        }

        private void HandlePermissionBlocked(MicPermissionStatus status)
        {
            if (errorPresenter != null)
                errorPresenter.ShowWarning(status == MicPermissionStatus.NoDevice
                    ? "マイクが みつからないよ"
                    : "マイクを つかえないよ");
            RefreshState(RecordingState.NoMic);
        }

        private void HandleRecordingFailed(Result result)
        {
            if (errorPresenter != null) errorPresenter.ShowFromResult(result);
            RefreshState(RecordingState.Idle);
        }

        private void HandleSaved() => RefreshState(RecordingState.Saved);

        private void HandleSaveFailed(Result result) => RefreshState(RecordingState.Recorded);

        // --- state machine ---

        private void RefreshState(RecordingState next)
        {
            _state = next;

            bool canRecord = next == RecordingState.Idle || next == RecordingState.Recorded || next == RecordingState.Saved;
            bool canPlayOrSave = next == RecordingState.Recorded || next == RecordingState.Saved;

            if (recordButton != null) recordButton.interactable = canRecord;
            if (playButton != null) playButton.interactable = canPlayOrSave;
            if (saveButton != null) saveButton.interactable = next == RecordingState.Recorded;

            if (recordingIndicator != null) recordingIndicator.SetActive(next == RecordingState.Recording);
            if (recordedPanel != null) recordedPanel.SetActive(canPlayOrSave);
            if (savedIndicator != null) savedIndicator.SetActive(next == RecordingState.Saved);
            if (noMicPanel != null) noMicPanel.SetActive(next == RecordingState.NoMic);

            if (statusText != null)
            {
                statusText.text = next switch
                {
                    RecordingState.Idle => "「ろくおん」を おしてね（3びょう）",
                    RecordingState.Recording => "ろくおんちゅう…",
                    RecordingState.Recorded => "できたよ。「さいせい」か「ほぞん」してね",
                    RecordingState.Playing => "さいせいちゅう…",
                    RecordingState.Saving => "ほぞんちゅう…",
                    RecordingState.Saved => "ほぞんしたよ。もどれるよ",
                    RecordingState.NoMic => "マイクが つかえないよ（せっていで きょかを みてね）",
                    _ => statusText.text
                };
            }
        }

        /// <summary>直前画面へ戻る。履歴が無い場合のみホームへ（ホーム直遷移の録音など）。</summary>
        private void NavigateBack()
        {
            if (_nav == null)
                _nav = ServiceRegistry.Resolve<INavigationService>();
            if (_nav == null) return;

            var result = _nav.GoBack();
            if (result.IsSuccess) return;

            // 戻り先なし／失敗時はホームへフォールバック（U2 NFR: GoBack → Home）
            result = _nav.GoTo(SceneId.Home);
            if (!result.IsSuccess && errorPresenter != null)
                errorPresenter.ShowFromResult(result);
        }

        protected override void Update()
        {
            base.Update(); // システムバック（Escape）検知

            // 再生完了で Playing → Recorded に戻す
            if (_state == RecordingState.Playing && _audio != null && !_audio.IsPlaying)
                RefreshState(RecordingState.Recorded);
        }
    }
}
