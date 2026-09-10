using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Foundation
{
    /// <summary>
    /// 実行時に OS 日本語フォントを解決する（Editor 生成 UI の m_Font 未保存対策）。
    /// </summary>
    public static class UiFontResolver
    {
        private static Font _cached;

        public static Font Resolve(int fontSize = 32)
        {
            if (_cached != null) return _cached;

            _cached = Font.CreateDynamicFontFromOSFont(new[]
            {
                "Hiragino Sans",
                "Hiragino Kaku Gothic ProN",
                "Yu Gothic UI",
                "Yu Gothic",
                "Meiryo",
                "Noto Sans CJK JP",
                "Apple SD Gothic Neo",
                "Arial Unicode MS",
                "Arial"
            }, fontSize);

            if (_cached != null) return _cached;

            _cached = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                      ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            return _cached;
        }

        public static void ApplyTo(Text text, int fontSize)
        {
            if (text == null) return;
            text.font = Resolve(fontSize);
            if (text.fontSize < fontSize) text.fontSize = fontSize;
        }

        public static void ApplyToChildren(Transform root, int fontSize)
        {
            if (root == null) return;
            foreach (var text in root.GetComponentsInChildren<Text>(true))
                ApplyTo(text, fontSize);
        }
    }
}
