using System;
using System.Collections.Generic;
using Geidai.Common.Create;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Create
{
    /// <summary>保存レシピ一覧・開く・削除。</summary>
    public class RecipeListController : MonoBehaviour
    {
        [SerializeField] private Dropdown recipeDropdown;
        [SerializeField] private Button openButton;
        [SerializeField] private Button deleteButton;

        private readonly List<SoundRecipe> _recipes = new List<SoundRecipe>();

        public event Action<SoundRecipe> OpenRequested;
        public event Action<SoundRecipe> DeleteRequested;

        private void Awake()
        {
            if (openButton != null) openButton.onClick.AddListener(OnOpen);
            if (deleteButton != null) deleteButton.onClick.AddListener(OnDelete);
        }

        public void SetRecipes(IReadOnlyList<SoundRecipe> recipes)
        {
            _recipes.Clear();
            if (recipes != null) _recipes.AddRange(recipes);

            if (recipeDropdown == null) return;
            recipeDropdown.ClearOptions();
            var options = new List<Dropdown.OptionData>();
            for (int i = 0; i < _recipes.Count; i++)
            {
                var r = _recipes[i];
                string title = string.IsNullOrEmpty(r.title) ? r.id : r.title;
                options.Add(new Dropdown.OptionData(title));
            }
            if (options.Count == 0) options.Add(new Dropdown.OptionData("（なし）"));
            recipeDropdown.AddOptions(options);
            recipeDropdown.value = 0;
        }

        private SoundRecipe Current()
        {
            if (recipeDropdown == null || _recipes.Count == 0) return null;
            int i = recipeDropdown.value;
            if (i < 0 || i >= _recipes.Count) return null;
            return _recipes[i];
        }

        private void OnOpen()
        {
            var r = Current();
            if (r != null) OpenRequested?.Invoke(r);
        }

        private void OnDelete()
        {
            var r = Current();
            if (r != null) DeleteRequested?.Invoke(r);
        }
    }
}
