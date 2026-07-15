namespace Geidai.Rec
{
    /// <summary>
    /// 加工種別（domain-entities §2.4）。バイパス（on/off）と UI パネルの単位。
    /// </summary>
    public enum EffectKind
    {
        Pitch,           // ピッチ（pitchSemitones）
        NoiseReduction,  // ノイズ低減（noiseLevel）
        Timbre,          // 音色（timbre）
        Reverb           // リバーブ（reverb）
    }
}
