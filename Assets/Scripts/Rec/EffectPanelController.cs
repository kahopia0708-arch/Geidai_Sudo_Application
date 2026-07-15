using UnityEngine;
using UnityEngine.UI;
using Geidai.Common.Audio;
using Geidai.Common.Models;
using Geidai.Services.Audio;

namespace Geidai.Rec
{
    /// <summary>
    /// 加工パネル（US-REC-02 / frontend-components §2）。
    /// UI（スライダー/ドロップダウン/トグル）を正準モデル <see cref="SoundEffectSettingsData"/> に
    /// バインドし、変更のたびに共有 <see cref="IAudioService.ApplyEffects"/> で再生系へ非破壊反映する
    /// （U4 で EffectChain を Services 共有へ移設 / Q4=A）。
    /// UI 値↔モデルの換算は <see cref="SoundEffectMapper"/> を用いる。
    /// 見た目・ラベル（なし/ロボット/コーラス系 等）は S さんが調整（US-TECH-07）。
    /// </summary>
    public class EffectPanelController : MonoBehaviour
    {
        private IAudioService _audio;

        [Header("Pitch (半音 -12..12)")]
        [SerializeField] private Slider pitchSlider;   // 0..1 → -12..12
        [SerializeField] private Toggle pitchBypass;   // on=有効

        [Header("Noise Reduction (0..1 → 4段)")]
        [SerializeField] private Slider noiseSlider;
        [SerializeField] private Toggle noiseBypass;

        [Header("Timbre (0:なし 1:ロボット 2:コーラス系)")]
        [SerializeField] private Dropdown timbreDropdown;
        [SerializeField] private Toggle timbreBypass;

        [Header("Reverb (0..1)")]
        [SerializeField] private Slider reverbSlider;
        [SerializeField] private Toggle reverbBypass;

        [Header("All")]
        [SerializeField] private Toggle allBypass;      // on=全加工有効

        private readonly SoundEffectSettingsData _settings = new SoundEffectSettingsData();
        private bool _allOn = true;
        private bool _pitchOn = true;
        private bool _noiseOn = true;
        private bool _timbreOn = true;
        private bool _reverbOn = true;

        /// <summary>保存に用いる現在の加工設定。</summary>
        public SoundEffectSettingsData CurrentSettings => _settings;

        /// <summary>共有 Audio サービスを注入する（RecScreenController から）。</summary>
        public void Init(IAudioService audio)
        {
            _audio = audio;
            ApplyToChain();
        }

        private void Awake()
        {
            HookUI();
            SyncFromUI();
            ApplyToChain();
        }

        private void HookUI()
        {
            if (pitchSlider != null) pitchSlider.onValueChanged.AddListener(_ => { SyncFromUI(); ApplyToChain(); });
            if (noiseSlider != null) noiseSlider.onValueChanged.AddListener(_ => { SyncFromUI(); ApplyToChain(); });
            if (reverbSlider != null) reverbSlider.onValueChanged.AddListener(_ => { SyncFromUI(); ApplyToChain(); });
            if (timbreDropdown != null) timbreDropdown.onValueChanged.AddListener(_ => { SyncFromUI(); ApplyToChain(); });

            if (pitchBypass != null) pitchBypass.onValueChanged.AddListener(_ => { SyncFromUI(); ApplyToChain(); });
            if (noiseBypass != null) noiseBypass.onValueChanged.AddListener(_ => { SyncFromUI(); ApplyToChain(); });
            if (timbreBypass != null) timbreBypass.onValueChanged.AddListener(_ => { SyncFromUI(); ApplyToChain(); });
            if (reverbBypass != null) reverbBypass.onValueChanged.AddListener(_ => { SyncFromUI(); ApplyToChain(); });
            if (allBypass != null) allBypass.onValueChanged.AddListener(_ => { SyncFromUI(); ApplyToChain(); });
        }

        /// <summary>UI 値を設定モデル/バイパスフラグへ取り込む。</summary>
        private void SyncFromUI()
        {
            if (pitchSlider != null)
            {
                int semis = Mathf.RoundToInt(Mathf.Lerp(SoundEffectMapper.MinSemitones, SoundEffectMapper.MaxSemitones, pitchSlider.value));
                _settings.pitchSemitones = semis;
            }
            if (noiseSlider != null)
                _settings.noiseLevel = SoundEffectMapper.ContinuousToNoiseLevel(noiseSlider.value);
            if (reverbSlider != null)
                _settings.reverb = Mathf.Clamp01(reverbSlider.value);
            if (timbreDropdown != null)
                _settings.timbre = IndexToTimbre(timbreDropdown.value);

            if (allBypass != null) _allOn = allBypass.isOn;
            if (pitchBypass != null) _pitchOn = pitchBypass.isOn;
            if (noiseBypass != null) _noiseOn = noiseBypass.isOn;
            if (timbreBypass != null) _timbreOn = timbreBypass.isOn;
            if (reverbBypass != null) _reverbOn = reverbBypass.isOn;
        }

        /// <summary>現在の設定・バイパスを共有再生系へ反映する（非破壊プレビュー）。</summary>
        public void ApplyToChain()
        {
            _audio?.ApplyEffects(_settings, _allOn, _pitchOn, _noiseOn, _timbreOn, _reverbOn);
        }

        /// <summary>UI 表記（0:なし/1:ロボット/2:コーラス系）→ TimbreType。</summary>
        private static TimbreType IndexToTimbre(int index)
        {
            switch (index)
            {
                case 1: return TimbreType.Hard;   // ロボット
                case 2: return TimbreType.Soft;   // コーラス系
                case 0:
                default: return TimbreType.Original; // なし
            }
        }
    }
}
