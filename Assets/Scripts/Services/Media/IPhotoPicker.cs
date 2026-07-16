using System;
using Geidai.Common.Results;

namespace Geidai.Services.Media
{
    /// <summary>
    /// 写真取得の抽象（U4 / nfr-design §5・NFR-COL-Priv1・Q5=A）。
    /// プラットフォーム依存（カメラ/ギャラリー）を隠蔽し、選択された写真の**一時パス**のみを返す。
    /// クラウドアップロードは行わない（端末内のみ）。実機ピッカーはフォローアップ（U4 はスタブ）。
    /// </summary>
    public interface IPhotoPicker
    {
        /// <summary>
        /// 写真を選択し、成功時に一時ファイルパスを <paramref name="onResult"/> に返す。
        /// キャンセル/失敗は <see cref="Result{T}"/> の失敗で表現する（クラッシュさせない）。
        /// </summary>
        void Pick(Action<Result<string>> onResult);
    }
}
