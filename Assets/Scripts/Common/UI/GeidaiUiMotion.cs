using System.Collections;
using UnityEngine;

namespace Geidai.Common.UI
{
    /// <summary>UI の短い演出（SE なし・展示向け）。</summary>
    public static class GeidaiUiMotion
    {
        public static IEnumerator HorizontalWiggle(
            RectTransform target,
            float amplitudePixels = 14f,
            float durationSeconds = 0.38f,
            int waveCount = 3)
        {
            if (target == null) yield break;
            Vector2 home = target.anchoredPosition;
            float duration = Mathf.Max(0.05f, durationSeconds);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float damp = 1f - t;
                float wave = Mathf.Sin(t * waveCount * Mathf.PI * 2f);
                target.anchoredPosition = home + new Vector2(amplitudePixels * wave * damp, 0f);
                yield return null;
            }

            target.anchoredPosition = home;
        }

        public static IEnumerator ScalePop(
            RectTransform target,
            float peakScale = 1.08f,
            float durationSeconds = 0.26f)
        {
            if (target == null) yield break;
            Vector3 baseScale = target.localScale;
            float half = Mathf.Max(0.04f, durationSeconds * 0.5f);
            float elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(elapsed / half);
                target.localScale = baseScale * Mathf.Lerp(1f, peakScale, u);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(elapsed / half);
                target.localScale = baseScale * Mathf.Lerp(peakScale, 1f, u);
                yield return null;
            }

            target.localScale = baseScale;
        }

        public static IEnumerator ZShake(
            RectTransform target,
            float maxDegrees = 5f,
            float durationSeconds = 0.32f,
            int waveCount = 4)
        {
            if (target == null) yield break;
            float baseZ = target.localEulerAngles.z;
            float duration = Mathf.Max(0.05f, durationSeconds);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float damp = 1f - t;
                float wave = Mathf.Sin(t * waveCount * Mathf.PI * 2f);
                target.localEulerAngles = new Vector3(0f, 0f, baseZ + maxDegrees * wave * damp);
                yield return null;
            }

            target.localEulerAngles = new Vector3(0f, 0f, baseZ);
        }

        public static IEnumerator FlipRevealScale(
            RectTransform target,
            float durationSeconds = 0.2f)
        {
            if (target == null) yield break;
            Vector3 baseScale = target.localScale;
            float half = Mathf.Max(0.04f, durationSeconds * 0.5f);
            try
            {
                float elapsed = 0f;
                while (elapsed < half)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float u = Mathf.Clamp01(elapsed / half);
                    target.localScale = new Vector3(baseScale.x * Mathf.Lerp(1f, 0.05f, u), baseScale.y, baseScale.z);
                    yield return null;
                }

                elapsed = 0f;
                while (elapsed < half)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float u = Mathf.Clamp01(elapsed / half);
                    target.localScale = new Vector3(baseScale.x * Mathf.Lerp(0.05f, 1f, u), baseScale.y, baseScale.z);
                    yield return null;
                }
            }
            finally
            {
                if (target != null)
                    target.localScale = baseScale;
            }
        }
    }
}
