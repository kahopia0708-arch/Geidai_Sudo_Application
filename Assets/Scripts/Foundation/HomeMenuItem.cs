using System;

namespace Geidai.Foundation
{
    /// <summary>
    /// ホームに並ぶ 1 導線を表す値（domain-entities.md / US-NAV-02 / US-TECH-07）。
    /// label / iconKey / order は Sさん がアセット編集で調整できる（コード非依存）。
    /// Place・テストは項目に含めない（BR-11）。
    /// </summary>
    [Serializable]
    public class HomeMenuItem
    {
        public ModuleId moduleId;
        public string label;
        public string iconKey;
        public bool visible = true;
        public bool enabled = true;
        public int order;
    }
}
