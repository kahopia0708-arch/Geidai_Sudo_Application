using System;
using System.Collections;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Geidai.Rec
{
    /// <summary>
    /// マイク権限の確認/要求を集約する（nfr-design §3 / SECURITY-15）。
    /// プラットフォーム分岐（iOS/Android/デバイス有無）を内包し、外部へは
    /// <see cref="MicPermissionStatus"/> のみを返す。常時録音はしない（録音時のみ使用）。
    /// <para>
    /// iOS 注意: 権限未許可のあいだ <c>Microphone.devices</c> は空になりやすい。
    /// デバイス有無判定は <b>許可後</b> に行う（未許可を NoDevice と誤判定しない）。
    /// </para>
    /// </summary>
    public static class MicPermissionGate
    {
        /// <summary>現在の権限状態を確認する（要求はしない）。</summary>
        public static MicPermissionStatus Check()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
                return MicPermissionStatus.Denied;
            return HasMicrophoneDevice() ? MicPermissionStatus.Granted : MicPermissionStatus.NoDevice;
#elif UNITY_IOS && !UNITY_EDITOR
            // 未要求/拒否時はデバイス列挙が空でも Denied（NoDevice ではない）。
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
                return MicPermissionStatus.Denied;
            return HasMicrophoneDevice() ? MicPermissionStatus.Granted : MicPermissionStatus.NoDevice;
#else
            // エディタ/デスクトップ: デバイスがあれば許可扱い。
            return HasMicrophoneDevice() ? MicPermissionStatus.Granted : MicPermissionStatus.NoDevice;
#endif
        }

        /// <summary>
        /// 権限を要求して結果を返すコルーチン（呼び出し側 MonoBehaviour が StartCoroutine する）。
        /// iOS/Android は先に権限ダイアログを出し、許可後にデバイス有無を確認する。
        /// </summary>
        public static IEnumerator RequestRoutine(Action<MicPermissionStatus> onResult)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                bool done = false;
                MicPermissionStatus resolved = MicPermissionStatus.Denied;
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionGranted += _ => { resolved = MicPermissionStatus.Granted; done = true; };
                callbacks.PermissionDenied += _ => { resolved = MicPermissionStatus.Denied; done = true; };
#pragma warning disable CS0618 // 旧 API。拒否確定のフォールバックとして残す。
                callbacks.PermissionDeniedAndDontAskAgain += _ => { resolved = MicPermissionStatus.Denied; done = true; };
#pragma warning restore CS0618
                Permission.RequestUserPermission(Permission.Microphone, callbacks);
                while (!done) yield return null;

                if (resolved != MicPermissionStatus.Granted)
                {
                    onResult?.Invoke(MicPermissionStatus.Denied);
                    yield break;
                }
            }

            onResult?.Invoke(HasMicrophoneDevice() ? MicPermissionStatus.Granted : MicPermissionStatus.NoDevice);
#elif UNITY_IOS && !UNITY_EDITOR
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
                yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);

            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                onResult?.Invoke(MicPermissionStatus.Denied);
                yield break;
            }

            // 許可直後にデバイス列挙が遅延することがあるため短く待つ。
            for (int i = 0; i < 10 && !HasMicrophoneDevice(); i++)
                yield return null;

            onResult?.Invoke(HasMicrophoneDevice() ? MicPermissionStatus.Granted : MicPermissionStatus.NoDevice);
#else
            onResult?.Invoke(HasMicrophoneDevice() ? MicPermissionStatus.Granted : MicPermissionStatus.NoDevice);
            yield break;
#endif
        }

        private static bool HasMicrophoneDevice()
        {
            return Microphone.devices != null && Microphone.devices.Length > 0;
        }
    }
}
