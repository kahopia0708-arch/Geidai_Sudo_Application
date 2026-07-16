using Geidai.Common.Models;
using Geidai.Common.Results;

namespace Geidai.Services.Audio
{
    /// <summary>
    /// 録音/再生サービスの契約。U4 で共有実装 <see cref="AudioService"/>（Services 層）へ集約し、
    /// Rec（録音・プレビュー）と Collection（保存音の視聴）の双方が利用する（Q4=A / NFR-COL-M4）。
    /// 保存エフェクトの再適用（<see cref="Play(AudioBuffer, SoundEffectSettingsData)"/>）で
    /// 録音時と同じ聴こえを再現する。既存シグネチャ（U1/U3）は不変。
    /// </summary>
    public interface IAudioService
    {
        Result StartRecording();
        Result<AudioBuffer> StopRecording();

        /// <summary>素の再生（エフェクト中立 / 後方互換・U3）。</summary>
        Result Play(AudioBuffer buffer);

        /// <summary>
        /// 保存エフェクト（<see cref="SoundEffectSettingsData"/>）を全 on で再適用して再生する
        /// （U4 / US-COL-01・コレクション視聴）。非破壊（バッファは変更しない）。
        /// </summary>
        Result Play(AudioBuffer buffer, SoundEffectSettingsData settings);

        /// <summary>
        /// ライブプレビュー用に加工をリグへ反映する（U3 Rec の加工プレビュー / 非破壊）。
        /// 各バイパス（全体一括／ピッチ／ノイズ／音色／リバーブ）の on/off を指定する。
        /// </summary>
        Result ApplyEffects(SoundEffectSettingsData settings, bool allOn, bool pitchOn, bool noiseOn, bool timbreOn, bool reverbOn);

        Result Stop();

        /// <summary>現在再生中かどうか（再生完了検知に用いる / Rec・Collection 共用）。</summary>
        bool IsPlaying { get; }
    }
}
