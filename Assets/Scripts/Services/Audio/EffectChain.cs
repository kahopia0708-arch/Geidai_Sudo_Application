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
        private const float LowPassMin = 1500f;
        private const float HighPassMin = 10f;

        public AudioSource Source => audioSource;

        private void Awake()
        {
            EnsureComponents();
        }

        /// <summary>再生系コンポーネントを確保・キャッシュする。</summary>
        public void EnsureComponents()
        {
            // GetComponent は未アタッチ時に「偽 null」を返すため ?? は使えない（AddComponent が呼ばれない）。
            audioSource = Ensure(audioSource);
            lowPassFilter = Ensure(lowPassFilter);
            highPassFilter = Ensure(highPassFilter);
            reverbFilter = Ensure(reverbFilter);
            distortionFilter = Ensure(distortionFilter);
        }

        private T Ensure<T>(T current) where T : Component
        {
            if (current != null) return current;
            var found = GetComponent<T>();
            return found != null ? found : gameObject.AddComponent<T>();
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
                    // 聴き分けられる差を優先し、Soft はこもった音、Hard は細く歪んだ音に振る。
                    case TimbreType.Soft:
                        lp = 2200f; hp = 10f; dist = 0f;
                        break;
                    case TimbreType.Hard:
                        lp = 3500f; hp = 900f; dist = 0.6f;
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

            lp = Mathf.Clamp(lp, LowPassMin, LowPassMax);
            hp = Mathf.Clamp(hp, 10f, 5000f);

            if (lowPassFilter != null) lowPassFilter.cutoffFrequency = lp;
            if (highPassFilter != null) highPassFilter.cutoffFrequency = hp;
            if (distortionFilter != null) distortionFilter.distortionLevel = dist;

            // --- Reverb ---
            if (reverbFilter != null)
            {
                float amount = (allOn && reverbOn) ? s.reverb : 0f;
                // プリセットを User に固定してから各値を書かないと、プリセット側の値で上書きされる。
                reverbFilter.reverbPreset = AudioReverbPreset.User;
                reverbFilter.dryLevel = 0f;
                reverbFilter.room = SoundEffectMapper.ReverbToRoomMilliBel(amount);
                reverbFilter.decayTime = SoundEffectMapper.ReverbToDecaySeconds(amount);
                reverbFilter.reverbLevel = SoundEffectMapper.ReverbToLevelMilliBel(amount);
            }
        }
    }
}
