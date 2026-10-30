using Geidai.Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Game2
{
    /// <summary>神経衰弱 UI のランタイム生成結果。</summary>
    public sealed class MemoryMatchUiHandles
    {
        public ConfirmDialog ConfirmDialog { get; set; }
        public GameObject PairCountPopupRoot { get; set; }
        public Button PopupConfirmButton { get; set; }
        public Text PopupConfirmLabel { get; set; }
        public Button ChangePairCountButton { get; set; }
        public Text ConfirmYesLabel { get; set; }
        public Text ConfirmNoLabel { get; set; }
        public Button[] PairPresetButtons { get; set; }
        public int[] PairPresetValues { get; set; }
    }
}
