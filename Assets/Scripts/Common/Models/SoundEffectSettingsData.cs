using System;
using UnityEngine;

namespace Geidai.Common.Models
{
    /// <summary>ノイズリダクション量（4段階）。</summary>
    public enum NoiseLevel
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }

    /// <summary>音色プリセット（3種）。</summary>
    public enum TimbreType
    {
        Original = 0,
        Soft = 1,
        Hard = 2
    }

    /// <summary>
    /// 加工設定のデータモデル（Functional Design domain-entities）。
    /// pitchSemitones は ±12（半音）、reverb は 0〜1。JsonUtility 対応の素直な構造。
    /// ※ 既存 <c>SoundEffectSettings</c>（グローバル名前空間）とは別物。U3 で録音実装統合時に対応付ける。
    /// </summary>
    [Serializable]
    public class SoundEffectSettingsData
    {
        [Range(-12, 12)] public int pitchSemitones;
        public NoiseLevel noiseLevel;
        public TimbreType timbre;
        [Range(0f, 1f)] public float reverb;

        public SoundEffectSettingsData()
        {
            pitchSemitones = 0;
            noiseLevel = NoiseLevel.None;
            timbre = TimbreType.Original;
            reverb = 0f;
        }
    }
}
