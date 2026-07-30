using Geidai.Common.Create;
using Geidai.Common.Models;
using Geidai.Common.Results;
using UnityEngine;

namespace Geidai.Services.Audio
{
    /// <summary>
    /// 録音/再生サービスの契約。U4 で共有実装。U7/U8 で同梱試聴・レイヤー再生・書き出しを後方互換追加。
    /// </summary>
    public interface IAudioService
    {
        Result StartRecording();
        Result<AudioBuffer> StopRecording();
        Result Play(AudioBuffer buffer);
        Result Play(AudioBuffer buffer, SoundEffectSettingsData settings);
        Result ApplyEffects(SoundEffectSettingsData settings, bool allOn, bool pitchOn, bool noiseOn, bool timbreOn, bool reverbOn);
        Result Stop();
        bool IsPlaying { get; }

        /// <summary>同梱 AudioClip を素で試聴する（U7 / 読み取り専用）。</summary>
        Result PlayCuratedClip(AudioClip clip);

        /// <summary>最大2レイヤーを同時再生（再生時加工・非破壊 / U8）。null レイヤーはスキップ。</summary>
        Result PlayLayers(AudioClip clipA, SoundRecipeLayer layerA, AudioClip clipB, SoundRecipeLayer layerB);

        /// <summary>2レイヤーをオフラインミックスして WAVE バイト列を返す（U8 / 明示書き出し）。</summary>
        Result<byte[]> RenderRecipeToWav(AudioClip clipA, SoundRecipeLayer layerA, AudioClip clipB, SoundRecipeLayer layerB);
    }
}
