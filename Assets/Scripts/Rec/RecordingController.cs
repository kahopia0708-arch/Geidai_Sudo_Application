using System;
using UnityEngine;
using UnityEngine.UI;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Services.Audio;

namespace Geidai.Rec
{
    /// <summary>
    /// 録音制御（US-REC-01 / nfr-design §2,§3）。
    /// マイク権限ゲート→録音開始→<see cref="RecordingClock"/> で 3秒自動停止→<see cref="AudioBuffer"/> 通知。
    /// 失敗/権限不可はイベントで上位（RecScreenController）へ通知し、クラッシュさせない。
    /// </summary>
    public class RecordingController : MonoBehaviour
    {
        [SerializeField] private Text remainingText;

        private IAudioService _audio;
        private readonly RecordingClock _clock = new RecordingClock();
        private bool _recording;

        /// <summary>録音が実際に開始された。</summary>
        public event Action RecordingStarted;
        /// <summary>録音が完了し、バッファが得られた。</summary>
        public event Action<AudioBuffer> RecordingCompleted;
        /// <summary>権限不可（Denied/NoDevice）で録音できなかった。</summary>
        public event Action<MicPermissionStatus> PermissionBlocked;
        /// <summary>録音開始/停止の失敗。</summary>
        public event Action<Result> RecordingFailed;
        /// <summary>残り秒の更新（表示用）。</summary>
        public event Action<float> RemainingChanged;

        public bool IsRecording => _recording;

        public void Init(IAudioService audio)
        {
            _audio = audio;
        }

        /// <summary>録音開始要求。権限確認の後、許可されていれば録音を始める。</summary>
        public void BeginRecording()
        {
            if (_recording) return;
            StartCoroutine(MicPermissionGate.RequestRoutine(OnPermissionResolved));
        }

        private void OnPermissionResolved(MicPermissionStatus status)
        {
            if (status != MicPermissionStatus.Granted)
            {
                PermissionBlocked?.Invoke(status);
                return;
            }

            if (_audio == null)
            {
                RecordingFailed?.Invoke(Result.Fail(ResultCode.Unknown, "ろくおんの じゅんびが できてないよ"));
                return;
            }

            var started = _audio.StartRecording();
            if (!started.IsSuccess)
            {
                RecordingFailed?.Invoke(started);
                return;
            }

            _clock.Start();
            _recording = true;
            RemainingChanged?.Invoke(_clock.RemainingSeconds);
            UpdateRemainingText(_clock.RemainingSeconds);
            RecordingStarted?.Invoke();
        }

        private void Update()
        {
            if (!_recording) return;

            bool done = _clock.Tick(Time.deltaTime);
            float remaining = _clock.RemainingSeconds;
            RemainingChanged?.Invoke(remaining);
            UpdateRemainingText(remaining);

            if (done)
                CompleteRecording();
        }

        private void CompleteRecording()
        {
            _recording = false;
            _clock.Reset();

            var result = _audio.StopRecording();
            if (!result.IsSuccess)
            {
                RecordingFailed?.Invoke(Result.Fail(result.Code, result.Message));
                return;
            }

            RecordingCompleted?.Invoke(result.Value);
        }

        private void UpdateRemainingText(float remaining)
        {
            if (remainingText != null)
                remainingText.text = Mathf.CeilToInt(remaining).ToString();
        }
    }
}
