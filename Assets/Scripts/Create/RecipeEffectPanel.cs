using System;
using Geidai.Common.Create;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Create
{
    /// <summary>アクティブレイヤーの volume/pitch/reverb/timbre 調整。</summary>
    public class RecipeEffectPanel : MonoBehaviour
    {
        [SerializeField] private Toggle layerAToggle;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Slider pitchSlider;
        [SerializeField] private Slider reverbSlider;
        [SerializeField] private Dropdown timbreDropdown;
        [SerializeField] private Text pitchValueLabel;

        private SoundRecipe _recipe;
        private bool _editingA = true;
        private bool _suppress;

        public event Action Changed;

        private void Awake()
        {
            if (layerAToggle != null) layerAToggle.onValueChanged.AddListener(OnLayerToggle);
            if (volumeSlider != null) volumeSlider.onValueChanged.AddListener(_ => Push());
            if (pitchSlider != null) pitchSlider.onValueChanged.AddListener(_ => Push());
            if (reverbSlider != null) reverbSlider.onValueChanged.AddListener(_ => Push());
            if (timbreDropdown != null)
            {
                timbreDropdown.ClearOptions();
                timbreDropdown.AddOptions(new System.Collections.Generic.List<string> { "なし", "ロボット", "コーラス" });
                timbreDropdown.onValueChanged.AddListener(_ => Push());
            }
        }

        public void Bind(SoundRecipe recipe)
        {
            _recipe = recipe;
            Pull();
        }

        private void OnLayerToggle(bool isA)
        {
            _editingA = isA;
            Pull();
        }

        private void Pull()
        {
            if (_recipe == null) return;
            var layer = _editingA ? _recipe.layerA : _recipe.layerB;
            _suppress = true;
            if (volumeSlider != null) volumeSlider.value = layer != null ? layer.volume : 1f;
            if (pitchSlider != null) pitchSlider.value = layer != null ? layer.pitchSemitones : 0;
            if (reverbSlider != null) reverbSlider.value = layer != null ? layer.reverb : 0f;
            if (timbreDropdown != null) timbreDropdown.value = layer != null ? (int)layer.timbre : 0;
            if (pitchValueLabel != null)
                pitchValueLabel.text = ((int)(pitchSlider != null ? pitchSlider.value : 0)).ToString();
            _suppress = false;
        }

        private void Push()
        {
            if (_suppress || _recipe == null) return;
            var layer = _editingA ? _recipe.layerA : _recipe.layerB;
            if (layer == null || string.IsNullOrEmpty(layer.curatedSoundId)) return;

            if (volumeSlider != null) layer.volume = volumeSlider.value;
            if (pitchSlider != null) layer.pitchSemitones = Mathf.RoundToInt(pitchSlider.value);
            if (reverbSlider != null) layer.reverb = reverbSlider.value;
            if (timbreDropdown != null) layer.timbre = (RecipeTimbreKind)timbreDropdown.value;
            RecipeValidator.ClampLayer(layer);

            if (pitchValueLabel != null) pitchValueLabel.text = layer.pitchSemitones.ToString();
            Changed?.Invoke();
        }
    }
}
