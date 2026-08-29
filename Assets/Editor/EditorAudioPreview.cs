using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Geidai.EditorTools
{
    /// <summary>Editor 内の AudioClip 試聴（Unity 内部 AudioUtil）。</summary>
    public static class EditorAudioPreview
    {
        private static MethodInfo _play;
        private static MethodInfo _stop;

        public static void Play(AudioClip clip)
        {
            if (clip == null) return;
            EnsureMethods();
            Stop();
            if (_play == null) return;

            // Unity バージョン差: (AudioClip, int) or (AudioClip, int, bool)
            var parameters = _play.GetParameters();
            if (parameters.Length == 3)
                _play.Invoke(null, new object[] { clip, 0, false });
            else if (parameters.Length == 2)
                _play.Invoke(null, new object[] { clip, 0 });
            else if (parameters.Length == 1)
                _play.Invoke(null, new object[] { clip });
        }

        public static void Stop()
        {
            EnsureMethods();
            _stop?.Invoke(null, null);
        }

        private static void EnsureMethods()
        {
            if (_play != null || _stop != null) return;

            var audioUtil = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
            if (audioUtil == null) return;

            _play = audioUtil.GetMethod(
                "PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null);
            if (_play == null)
            {
                _play = audioUtil.GetMethod(
                    "PlayPreviewClip",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            }

            _stop = audioUtil.GetMethod(
                "StopAllPreviewClips",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (_stop == null)
            {
                _stop = audioUtil.GetMethod(
                    "StopAllClips",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            }
        }
    }
}
