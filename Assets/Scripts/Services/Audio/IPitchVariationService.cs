using Geidai.Common.Models;
using Geidai.Common.Results;

namespace Geidai.Services.Audio
{
    /// <summary>
    /// ゲーム出題用のリアルタイムピッチ加工・再生（U6 / FR-19 / P1）。
    /// 「基準バッファ＋セント」を再生時ピッチ（AudioSource.pitch）で発音する軽量方式。
    /// 加工済み音声は生成・保存しない（非保存・低GC）。AudioService とは別サービス。
    /// </summary>
    public interface IPitchVariationService
    {
        /// <summary>出題の基準となる音バッファを設定する（ゲーム開始時に一度・AudioClip をキャッシュ）。</summary>
        Result SetBase(AudioBuffer baseBuffer);

        /// <summary>基準音を指定セントのピッチで再生する（非破壊・非保存）。</summary>
        Result Play(int cents);

        /// <summary>基準音の設定と同時に再生する（利便用）。</summary>
        Result Play(AudioBuffer baseBuffer, int cents);

        /// <summary>再生を停止する。</summary>
        Result Stop();

        /// <summary>現在再生中か。</summary>
        bool IsPlaying { get; }
    }
}
