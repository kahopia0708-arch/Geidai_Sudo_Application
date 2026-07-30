using System;
using System.Collections.Generic;
using Geidai.Common.Create;
using Geidai.Common.Library;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Create
{
    /// <summary>スロット A/B の解除済み素材ピッカー（Dropdown）。</summary>
    public class RecipeLayerPicker : MonoBehaviour
    {
        [SerializeField] private Dropdown slotA;
        [SerializeField] private Dropdown slotB;

        private readonly List<string> _ids = new List<string>();

        public event Action SelectionChanged;

        public string SelectedIdA => IndexToId(slotA);
        public string SelectedIdB => IndexToId(slotB);

        public void SetOptions(IReadOnlyList<LibraryItemView> unlockedItems)
        {
            string previousA = SelectedIdA;
            string previousB = SelectedIdB;
            _ids.Clear();
            var options = new List<Dropdown.OptionData> { new Dropdown.OptionData("（なし）") };

            if (unlockedItems != null)
            {
                for (int i = 0; i < unlockedItems.Count; i++)
                {
                    var item = unlockedItems[i];
                    if (!item.isUnlocked) continue;
                    _ids.Add(item.id);
                    options.Add(new Dropdown.OptionData(item.displayName));
                }
            }

            Wire(slotA, options);
            Wire(slotB, options);

            // 既存選択を維持し、初回は異なる2音を自動選択してすぐ試聴できるようにする。
            string nextA = _ids.Contains(previousA)
                ? previousA
                : (_ids.Count > 0 ? _ids[0] : null);
            string nextB = _ids.Contains(previousB)
                ? previousB
                : (_ids.Count > 1 ? _ids[1] : null);
            SetSelection(nextA, nextB);
        }

        public void ApplyToRecipe(SoundRecipe recipe)
        {
            if (recipe == null) return;
            recipe.layerA = MakeLayer(SelectedIdA, recipe.layerA);
            recipe.layerB = MakeLayer(SelectedIdB, recipe.layerB);
        }

        /// <summary>保存レシピを開いたとき、素材IDに対応する選択状態を復元する。</summary>
        public void SetSelection(string idA, string idB)
        {
            SetDropdownValue(slotA, idA);
            SetDropdownValue(slotB, idB);
        }

        private static SoundRecipeLayer MakeLayer(string id, SoundRecipeLayer existing)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var layer = existing?.Clone() ?? new SoundRecipeLayer();
            layer.curatedSoundId = id;
            if (existing == null || string.IsNullOrEmpty(existing.curatedSoundId))
            {
                layer.volume = 1f;
                layer.pitchSemitones = 0;
                layer.reverb = 0f;
                layer.timbre = RecipeTimbreKind.None;
            }
            return layer;
        }

        private void Wire(Dropdown dropdown, List<Dropdown.OptionData> options)
        {
            if (dropdown == null) return;
            dropdown.onValueChanged.RemoveListener(OnChanged);
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.value = 0;
            dropdown.onValueChanged.AddListener(OnChanged);
        }

        private void OnChanged(int _) => SelectionChanged?.Invoke();

        private void SetDropdownValue(Dropdown dropdown, string id)
        {
            if (dropdown == null) return;
            int index = string.IsNullOrEmpty(id) ? -1 : _ids.IndexOf(id);
            dropdown.SetValueWithoutNotify(index + 1); // 0 = なし
            dropdown.RefreshShownValue();
        }

        private string IndexToId(Dropdown dropdown)
        {
            if (dropdown == null) return null;
            int idx = dropdown.value - 1; // 0 = なし
            if (idx < 0 || idx >= _ids.Count) return null;
            return _ids[idx];
        }
    }
}
