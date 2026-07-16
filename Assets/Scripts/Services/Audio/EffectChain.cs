using UnityEngine;
using Geidai.Common.Audio;
using Geidai.Common.Models;

namespace Geidai.Services.Audio
{
    /// <summary>
    /// 再生系（AudioSource＋各 AudioFilter）を束ね、加工設定を一括反映する（nfr-design §4）。
    /// U4 で Rec から Services へ移設し、Rec（プレビュー）と Collection（視聴）で共有する（Q4=A）。
    /// 非破壊：録音/保存バッファは変更せず、再生時のパラメータのみを更新する（US-REC-02 / US-COL-01）。
    /// 具体フィルタ値はここに閉じ込め、数値換算は <see cref="SoundEffectMapper"/> / <see cref="PitchMath"/> を用いる。
    /// フィルタ参照は初期化時にキャッシュし、毎フレームの GetComponent を避ける（GC/性能）。
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class EffectChain : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioLowPassFilter lowPassFilter;
        [SerializeField] private AudioHighPassFilter highPassFilter;
        [SerializeField] private AudioReverbFilter reverbFilter;
        [SerializeField] private AudioDistortionFilter distortionFilter;

        // 音色プリセットの基準値（Original/Soft/Hard）
        private const float LowPassMax = 22000f;
        private const float HighPassMin = 10f;

        public AudioSource Source => audioSource;

        private void Awake()
        {
            EnsureComponents();
        }

        /// <summary>再生系コンポーネントを確保・キャッシュする。</summary>
        public void EnsureComponents()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            if (lowPassFilter == null) lowPassFilter = GetComponent<AudioLowPassFilter>() ?? gameObject.AddComponent<AudioLowPassFilter>();
            if (highPassFilter == null) highPassFilter = GetComponent<AudioHighPassFilter>() ?? gameObject.AddComponent<AudioHighPassFilter>();
            if (reverbFilter == null) reverbFilter = GetComponent<AudioReverbFilter>() ?? gameObject.AddComponent<AudioReverbFilter>();
            if (distortionFilter == null) distortionFilter = GetComponent<AudioDistortionFilter>() ?? gameObject.AddComponent<AudioDistortionFilter>();
        }

        /// <summary>
        /// 加工設定と各バイパス（EffectKind ごとの on/off・全体一括）を再生系へ反映する。
        /// off の加工はフィルタを中立化して有無の比較を可能にする（US-REC-02 AC3）。
        /// </summary>
        public void Apply(SoundEffectSettingsData s, bool allOn, bool pitchOn, bool noiseOn, bool timbreOn, bool reverbOn)
        {
            if (s == null) return;
            EnsureComponents();

            // --- Pitch ---
            audioSource.pitch = (allOn && pitchOn)
                ? (float)PitchMath.SemitonesToRatio(s.pitchSemitones)
                : 1f;

            // --- Timbre（音色プリセット） ---
            float lp = LowPassMax;
            float hp = HighPassMin;
            float dist = 0f;
            if (allOn && timbreOn)
            {
                switch (s.timbre)
                {
                    case TimbreType.Soft:
                        lp = 8000f; hp = 10f; dist = 0f;
                        break;
                    case TimbreType.Hard:
                        lp = 22000f; hp = 800f; dist = 0.35f;
                        break;
                    case TimbreType.Original:
                    default:
                        lp = LowPassMax; hp = HighPassMin; dist = 0f;
                        break;
                }
            }

            // --- Noise Reduction（高域/低域を調整） ---
            if (allOn && noiseOn)
            {
                float amt = SoundEffectMapper.NoiseLevelToContinuous(s.noiseLevel);
                hp += Mathf.Lerp(0f, 700f, amt);
                lp -= Mathf.Lerp(0f, 5000f, amt);
            }

            lp = Mathf.Clamp(lp, 3000f, 22000f);
            hp = Mathf.Clamp(hp, 10f, 5000f);

            if (lowPassFilter != null) lowPassFilter.cutoffFrequency = lp;
            if (highPassFilter != null) highPassFilter.cutoffFrequency = hp;
            if (distortionFilter != null) distortionFilter.distortionLevel = dist;

            // --- Reverb ---
            if (reverbFilter != null)
            {
                reverbFilter.reverbLevel = (allOn && reverbOn)
                    ? SoundEffectMapper.DenormalizeReverb(s.reverb)
                    : -10000f;
            }
        }
    }
}
