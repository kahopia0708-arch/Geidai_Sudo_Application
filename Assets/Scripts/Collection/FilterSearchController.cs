using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Geidai.Common.Collection;

namespace Geidai.Collection
{
    /// <summary>
    /// 絞込（月別）＋検索（キーワード）UI（US-COL-03/04 / frontend-components §4）。
    /// UI から <see cref="CollectionQuery"/> を組み立て、変更時に <see cref="QueryChanged"/> を発火する。
    /// 実際の絞込は純粋関数 <see cref="CollectionFilter.Filter"/>（画面統括が実行）。見た目は S さん調整。
    /// </summary>
    public class FilterSearchController : MonoBehaviour
    {
        [SerializeField] private Dropdown monthDropdown;   // index 0 = すべて
        [SerializeField] private InputField keywordInput;
        [SerializeField] private Button clearButton;

        private readonly List<string> _months = new List<string>(); // index0=""(すべて) に対応
        private bool _hooked;

        public event Action<CollectionQuery> QueryChanged;

        private void Awake() => HookOnce();

        private void HookOnce()
        {
            if (_hooked) return;
            if (monthDropdown != null) monthDropdown.onValueChanged.AddListener(_ => RaiseChanged());
            if (keywordInput != null) keywordInput.onValueChanged.AddListener(_ => RaiseChanged());
            if (clearButton != null) clearButton.onClick.AddListener(Clear);
            _hooked = true;
        }

        /// <summary>一覧から得た利用可能な月（"YYYY-MM"）でドロップダウンを再構成する。</summary>
        public void SetAvailableMonths(IEnumerable<string> months)
        {
            HookOnce();
            _months.Clear();
            _months.Add(string.Empty); // すべて

            var seen = new HashSet<string>();
            if (months != null)
            {
                foreach (var m in months)
                {
                    if (string.IsNullOrEmpty(m) || seen.Contains(m)) continue;
                    seen.Add(m);
                    _months.Add(m);
                }
            }
            _months.Sort((a, b) => string.CompareOrdinal(b, a)); // 新しい月を上に（"" は最上位維持のため後で先頭へ）
            _months.Remove(string.Empty);
            _months.Insert(0, string.Empty);

            if (monthDropdown != null)
            {
                var options = new List<string> { "すべて" };
                for (int i = 1; i < _months.Count; i++) options.Add(_months[i]);
                monthDropdown.ClearOptions();
                monthDropdown.AddOptions(options);
                monthDropdown.SetValueWithoutNotify(0);
            }
        }

        /// <summary>現在の UI から検索クエリを生成する。</summary>
        public CollectionQuery BuildQuery()
        {
            string month = string.Empty;
            if (monthDropdown != null)
            {
                int idx = monthDropdown.value;
                if (idx >= 0 && idx < _months.Count) month = _months[idx];
            }
            string keyword = keywordInput != null ? keywordInput.text : string.Empty;
            return new CollectionQuery(month, keyword);
        }

        public void Clear()
        {
            if (monthDropdown != null) monthDropdown.SetValueWithoutNotify(0);
            if (keywordInput != null) keywordInput.SetTextWithoutNotify(string.Empty);
            RaiseChanged();
        }

        private void RaiseChanged() => QueryChanged?.Invoke(BuildQuery());
    }
}
