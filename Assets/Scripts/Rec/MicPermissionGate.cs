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
    /// </summary>
    public static class MicPermissionGate
    {
        /// <summary>現在の権限状態を確認する（要求はしない）。</summary>
        public static MicPermissionStatus Check()
        {
            if (Microphone.devices == null || Microphone.devices.Length == 0)
                return MicPermissionStatus.NoDevice;

#if UNITY_ANDROID && !UNITY_EDITOR
            return Permission.HasUserAuthorizedPermission(Permission.Microphone)
                ? MicPermissionStatus.Granted
                : MicPermissionStatus.Denied;
#elif UNITY_IOS && !UNITY_EDITOR
            return Application.HasUserAuthorization(UserAuthorization.Microphone)
                ? MicPermissionStatus.Granted
                : MicPermissionStatus.Denied;
#else
            // エディタ/デスクトップ: デバイスがあれば許可扱い。
            return MicPermissionStatus.Granted;
#endif
        }

        /// <summary>
        /// 権限を要求して結果を返すコルーチン（呼び出し側 MonoBehaviour が StartCoroutine する）。
        /// デバイス無しは即 NoDevice。既に許可済みは即 Granted。
        /// </summary>
        public static IEnumerator RequestRoutine(Action<MicPermissionStatus> onResult)
        {
            if (Microphone.devices == null || Microphone.devices.Length == 0)
            {
                onResult?.Invoke(MicPermissionStatus.NoDevice);
                yield break;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                onResult?.Invoke(MicPermissionStatus.Granted);
                yield break;
            }

            bool done = false;
            MicPermissionStatus resolved = MicPermissionStatus.Denied;
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += _ => { resolved = MicPermissionStatus.Granted; done = true; };
            callbacks.PermissionDenied += _ => { resolved = MicPermissionStatus.Denied; done = true; };
            callbacks.PermissionDeniedAndDontAskAgain += _ => { resolved = MicPermissionStatus.Denied; done = true; };
            Permission.RequestUserPermission(Permission.Microphone, callbacks);

            while (!done) yield return null;
            onResult?.Invoke(resolved);
#elif UNITY_IOS && !UNITY_EDITOR
            if (Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                onResult?.Invoke(MicPermissionStatus.Granted);
                yield break;
            }

            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            onResult?.Invoke(
                Application.HasUserAuthorization(UserAuthorization.Microphone)
                    ? MicPermissionStatus.Granted
                    : MicPermissionStatus.Denied);
#else
            onResult?.Invoke(MicPermissionStatus.Granted);
            yield break;
#endif
        }
    }
}
