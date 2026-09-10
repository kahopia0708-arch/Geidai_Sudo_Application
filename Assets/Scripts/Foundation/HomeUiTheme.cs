using UnityEngine;

namespace Geidai.Foundation
{
    /// <summary>
    /// ホーム基調 UI の共通カラー・タイポグラフィ（アプリ全体で統一）。
    /// </summary>
    public static class HomeUiTheme
    {
        public static readonly Color Background = new Color(0.478f, 0.580f, 0.722f, 1f);
        public static readonly Color MenuText = new Color(0.22f, 0.32f, 0.45f, 1f);
        public static readonly Color FieldLabelOnBackground = new Color(1f, 1f, 1f, 0.92f);
        public static readonly Color TitleOnBackground = Color.white;
        public static readonly Color InputFill = Color.white;
        public static readonly Color PanelFill = Color.white;

        /// <summary>画面上部タイトル（せってい / おとあそび 等）。</summary>
        public const int ScreenTitle = 44;

        /// <summary>ホームメニューボタンラベル。</summary>
        public const int MenuButtonLabel = 48;

        /// <summary>プロフィールパネル見出し。</summary>
        public const int PanelTitle = 40;

        /// <summary>本文・入力値・パネル統計行。</summary>
        public const int Body = 32;

        /// <summary>フィールドラベル・バッジニックネーム。</summary>
        public const int FieldLabel = 28;

        /// <summary>プレースホルダー文字。</summary>
        public const int Placeholder = 28;

        /// <summary>確定／戻る等のアクションボタン。</summary>
        public const int ActionButtonLabel = 36;

        public static Color PlaceholderText =>
            new Color(MenuText.r, MenuText.g, MenuText.b, 0.55f);
    }
}
