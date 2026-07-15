using System;

namespace Geidai.Common.Content
{
    /// <summary>
    /// 「今週のお題」1 件（U5 / domain-entities）。オノマトペ等の制作側コンテンツで PII を含まない。
    /// `text` 空の項目は無効（選択対象外 / BR-THEME-11）。
    /// </summary>
    [Serializable]
    public class ThemeItem
    {
        public string id;
        public string text;
        public string reading;
        public string hint;

        public ThemeItem()
        {
            id = string.Empty;
            text = string.Empty;
            reading = string.Empty;
            hint = string.Empty;
        }

        public ThemeItem(string text, string reading = "", string hint = "", string id = "")
        {
            this.id = id ?? string.Empty;
            this.text = text ?? string.Empty;
            this.reading = reading ?? string.Empty;
            this.hint = hint ?? string.Empty;
        }

        /// <summary>本文が非空なら有効（BR-THEME-11）。</summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(text);
    }
}
