using Geidai.Common.Content;

namespace Geidai.Services.Content
{
    /// <summary>
    /// お題→Rec のお題受け渡し用の軽量セッション状態（U5 / P3 / Q3=A）。
    /// 実行時のみ保持し、永続化しない・保存メタ（SoundClipMeta）にも記録しない（BR-THEME-32）。
    /// Rec 画面は Current を任意参照（未設定でも通常録音 / US-THEME-02）。
    /// </summary>
    public class ThemeContext
    {
        /// <summary>直近に選択/表示中のお題（未設定は null）。</summary>
        public ThemeItem Current { get; private set; }

        /// <summary>お題が設定済みか。</summary>
        public bool HasValue => Current != null;

        /// <summary>お題を設定する（お題タップ時）。</summary>
        public void Set(ThemeItem item)
        {
            Current = item;
        }

        /// <summary>お題をクリアする。</summary>
        public void Clear()
        {
            Current = null;
        }
    }
}
