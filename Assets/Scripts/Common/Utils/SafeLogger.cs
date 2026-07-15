using UnityEngine;

namespace Geidai.Common.Utils
{
    /// <summary>
    /// PII マスク付きログラッパ（Security / NFR-04）。
    /// PII（生年/ニックネーム等）はログに出さない。詳細ログは開発ビルドのみ。
    /// </summary>
    public static class SafeLogger
    {
        private const string Mask = "***";

        /// <summary>通常ログ（開発ビルドのみ詳細出力）。</summary>
        public static void Log(string message)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.Log(message);
#endif
        }

        public static void Warn(string message)
        {
            Debug.LogWarning(message);
        }

        public static void Error(string message)
        {
            Debug.LogError(message);
        }

        /// <summary>PII をマスクして返す（ログ出力前に必ず通す）。</summary>
        public static string MaskPii(string value)
        {
            if (string.IsNullOrEmpty(value)) return Mask;
            return Mask;
        }
    }
}
