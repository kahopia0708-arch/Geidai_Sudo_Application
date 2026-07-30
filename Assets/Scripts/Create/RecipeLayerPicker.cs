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
        }

        public void ApplyToRecipe(SoundRecipe recipe)
        {
            if (recipe == null) return;
            recipe.layerA = MakeLayer(SelectedIdA, recipe.layerA);
            recipe.layerB = MakeLayer(SelectedIdB, recipe.layerB);
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

        private string IndexToId(Dropdown dropdown)
        {
            if (dropdown == null) return null;
            int idx = dropdown.value - 1; // 0 = なし
            if (idx < 0 || idx >= _ids.Count) return null;
            return _ids[idx];
        }
    }
}
