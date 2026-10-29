using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Game1
{
    /// <summary>
    /// 実行時生成 UI 用の日本語フォント（Game1 専用・Foundation 非依存）。
    /// </summary>
    public static class Game1UiFont
    {
        private static Font _cached;

        public static Font Resolve(int fontSize = 28)
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
                "Arial Unicode MS",
                "Arial"
            }, fontSize);

            if (_cached == null)
            {
                _cached = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                          ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return _cached;
        }

        public static void Apply(Text text, int fontSize = 28)
        {
            if (text == null) return;
            text.font = Resolve(fontSize);
            if (text.fontSize < fontSize) text.fontSize = fontSize;
        }

        public static void ApplyToChildren(Transform root, int fontSize = 28)
        {
            if (root == null) return;
            foreach (var text in root.GetComponentsInChildren<Text>(true))
                Apply(text, fontSize);
        }
    }
}
