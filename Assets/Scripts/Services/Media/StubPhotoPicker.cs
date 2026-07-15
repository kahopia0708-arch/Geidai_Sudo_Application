using System;
using Geidai.Common.Results;
using Geidai.Common.Utils;

namespace Geidai.Services.Media
{
    /// <summary>
    /// <see cref="IPhotoPicker"/> の U4 スタブ実装（実機ピッカー未実装 / nfr-design §5）。
    /// フロー（選択→一時パス→原子的コピー→meta 反映）を成立させるための足場。
    /// 既定ではテスト用の固定パスが無いため NotImplemented を返す（クラッシュさせない）。
    /// テスト等で固定パスを与えたい場合は <see cref="FixedTempPath"/> を設定する。
    /// クラウド送信は一切行わない（NFR-COL-Priv1）。
    /// </summary>
    public class StubPhotoPicker : IPhotoPicker
    {
        /// <summary>テスト/エディタで返す固定の一時パス（未設定なら NotImplemented）。</summary>
        public string FixedTempPath { get; set; }

        public void Pick(Action<Result<string>> onResult)
        {
            if (onResult == null) return;

            if (!string.IsNullOrEmpty(FixedTempPath))
            {
                onResult(Result<string>.Ok(FixedTempPath));
                return;
            }

            SafeLogger.Log("[PhotoPicker] stub: 実機ピッカーは未実装（フォローアップ）。");
            onResult(Result<string>.Fail(ResultCode.NotImplemented, "しゃしんの せんたくは まだ つかえないよ"));
        }
    }
}
