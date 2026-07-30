using System;
using System.Collections.Generic;
using Geidai.Common.Create;
using Geidai.Common.Library;
using Geidai.Common.Models;
using Geidai.Common.UI;
using Geidai.Services;
using Geidai.Services.Audio;
using Geidai.Services.Content;
using Geidai.Services.Navigation;
using Geidai.Services.Progression;
using Geidai.Services.Storage;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Create
{
    /// <summary>
    /// 音づくり画面（US-CREATE-01〜04）。解除済み2音＋レシピ保存＋任意 WAVE 書き出し。
    /// Library asmdef 非依存（UnlockState は Progression/Storage 経由）。
    /// </summary>
    public class CreateScreenController : ScreenRootBase
    {
        [Header("Catalogs")]
        [SerializeField] private CuratedSoundCatalog curatedCatalog;
        [SerializeField] private UnlockRulesCatalog unlockRules;

        [Header("Views")]
        [SerializeField] private RecipeLayerPicker layerPicker;
        [SerializeField] private RecipeEffectPanel effectPanel;
        [SerializeField] private RecipeListController recipeList;
        [SerializeField] private RecipeExportController exportController;
        [SerializeField] private InputField titleField;
        [SerializeField] private Button previewButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Button backButton;
        [SerializeField] private ErrorPresenter errorPresenter;
        [SerializeField] private ConfirmDialog confirmDialog;

        private IContentService _content;
        private IProgressionService _progression;
        private IStorageService _storage;
        private IAudioService _audio;
        private INavigationService _nav;

        private SoundRecipe _draft = new SoundRecipe();
        private CreateState _state = CreateState.Idle;
        private bool _wired;
        private Dictionary<string, CuratedSoundDefinition> _defsById = new Dictionary<string, CuratedSoundDefinition>();

        protected override void OnShow()
        {
            EnsureWired();
            RefreshPickers();
            RefreshRecipeList();
            EnsureDraftId();
            effectPanel?.Bind(_draft);
            SetState(CreateState.Ready);
        }

        private void EnsureWired()
        {
            if (_wired) return;

            _content = ServiceRegistry.Resolve<IContentService>();
            _progression = ServiceRegistry.Resolve<IProgressionService>();
            _storage = ServiceRegistry.Resolve<IStorageService>();
            _audio = ServiceRegistry.Resolve<IAudioService>();
            _nav = ServiceRegistry.Resolve<INavigationService>();

            CreateBootstrap.EnsureCatalogs(curatedCatalog, unlockRules);

            if (layerPicker != null) layerPicker.SelectionChanged += OnSelectionChanged;
            if (effectPanel != null) effectPanel.Changed += () => { };
            if (recipeList != null)
            {
                recipeList.OpenRequested += OnOpenRecipe;
                recipeList.DeleteRequested += OnDeleteRecipe;
            }
            if (exportController != null) exportController.ExportConfirmed += OnExport;
            if (previewButton != null) previewButton.onClick.AddListener(OnPreview);
            if (saveButton != null) saveButton.onClick.AddListener(OnSave);
            if (stopButton != null) stopButton.onClick.AddListener(() => _audio?.Stop());
            if (backButton != null) backButton.onClick.AddListener(NavigateHome);

            _wired = true;
        }

        private void OnDestroy()
        {
            if (layerPicker != null) layerPicker.SelectionChanged -= OnSelectionChanged;
            if (recipeList != null)
            {
                recipeList.OpenRequested -= OnOpenRecipe;
                recipeList.DeleteRequested -= OnDeleteRecipe;
            }
            if (exportController != null) exportController.ExportConfirmed -= OnExport;
        }

        private void RefreshPickers()
        {
            _progression?.Reload();
            CreateBootstrap.EnsureCatalogs(curatedCatalog, unlockRules);

            _defsById.Clear();
            List<LibraryItemView> unlocked = new List<LibraryItemView>();

            if (_content != null)
            {
                var catalogResult = _content.GetCuratedCatalog();
                if (catalogResult.IsSuccess && catalogResult.Value != null)
                {
                    var unlock = _progression != null ? _progression.CurrentUnlockState : UnlockState.Empty();
                    var projected = UnlockEvaluator.Project(catalogResult.Value.ValidItems(), unlock);
                    for (int i = 0; i < projected.Count; i++)
                    {
                        var item = projected[i];
                        var def = catalogResult.Value.FindById(item.id);
                        if (def != null) _defsById[item.id] = def;
                        if (item.isUnlocked) unlocked.Add(item);
                    }
                }
            }

            layerPicker?.SetOptions(unlocked);
        }

        private void RefreshRecipeList()
        {
            if (_storage == null)
            {
                recipeList?.SetRecipes(new List<SoundRecipe>());
                return;
            }

            var list = _storage.ListRecipes();
            recipeList?.SetRecipes(list.IsSuccess && list.Value != null ? list.Value : new List<SoundRecipe>());
        }

        private void OnSelectionChanged()
        {
            layerPicker?.ApplyToRecipe(_draft);
            effectPanel?.Bind(_draft);
            SetState(CreateState.Picking);
        }

        private void OnPreview()
        {
            layerPicker?.ApplyToRecipe(_draft);
            ResolveClips(out var clipA, out var clipB, out var missing);
            if (missing)
            {
                ShowError("おとが たりないよ");
                return;
            }

            if (_audio == null)
            {
                ShowError("さいせいできなかったよ");
                return;
            }

            var result = _audio.PlayLayers(clipA, _draft.layerA, clipB, _draft.layerB);
            if (!result.IsSuccess)
            {
                ShowError(result.Message);
                return;
            }
            SetState(CreateState.Previewing);
        }

        private void OnSave()
        {
            layerPicker?.ApplyToRecipe(_draft);
            EnsureDraftId();
            if (titleField != null) _draft.title = titleField.text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(_draft.createdAtIso))
                _draft.createdAtIso = DateTime.UtcNow.ToString("o");

            var unlock = _progression != null ? _progression.CurrentUnlockState : UnlockState.Empty();
            var unlockSet = new HashSet<string>(unlock.unlockedIds ?? Array.Empty<string>());
            if (!RecipeValidator.CanSave(_draft, unlockSet, out var reason))
            {
                ShowError(reason);
                return;
            }

            if (_storage == null)
            {
                ShowError("ほぞんに しっぱいしたよ");
                return;
            }

            SetState(CreateState.Saving);
            var clamped = RecipeValidator.Clamp(_draft);
            var save = _storage.SaveRecipe(clamped);
            if (!save.IsSuccess)
            {
                ShowError(save.Message);
                SetState(CreateState.Error);
                return;
            }

            _draft = clamped;
            RefreshRecipeList();
            SetState(CreateState.Ready);
        }

        private void OnExport()
        {
            layerPicker?.ApplyToRecipe(_draft);
            EnsureDraftId();
            ResolveClips(out var clipA, out var clipB, out var missing);
            if (missing)
            {
                ShowError("おとが たりないよ");
                return;
            }

            if (_audio == null || _storage == null)
            {
                ShowError("かきだしに しっぱいしたよ");
                return;
            }

            SetState(CreateState.Exporting);
            var render = _audio.RenderRecipeToWav(clipA, _draft.layerA, clipB, _draft.layerB);
            if (!render.IsSuccess)
            {
                ShowError(render.Message);
                SetState(CreateState.Error);
                return;
            }

            var save = _storage.SaveRecipeExport(_draft.id, render.Value);
            if (!save.IsSuccess)
            {
                ShowError(save.Message);
                SetState(CreateState.Error);
                return;
            }
            SetState(CreateState.Ready);
        }

        private void OnOpenRecipe(SoundRecipe recipe)
        {
            if (recipe == null) return;
            _draft = recipe.Clone();
            if (titleField != null) titleField.text = _draft.title ?? string.Empty;
            layerPicker?.SetSelection(
                _draft.layerA != null ? _draft.layerA.curatedSoundId : null,
                _draft.layerB != null ? _draft.layerB.curatedSoundId : null);
            effectPanel?.Bind(_draft);
            SetState(CreateState.Editing);
        }

        private void OnDeleteRecipe(SoundRecipe recipe)
        {
            if (recipe == null) return;
            void DoDelete()
            {
                _storage?.DeleteRecipe(recipe.id);
                RefreshRecipeList();
            }

            if (confirmDialog != null)
                confirmDialog.Show("さくじょ", "このレシピを けす？", DoDelete);
            else
                DoDelete();
        }

        private void ResolveClips(out AudioClip clipA, out AudioClip clipB, out bool missing)
        {
            clipA = null;
            clipB = null;
            missing = false;

            if (_draft.layerA != null && !string.IsNullOrEmpty(_draft.layerA.curatedSoundId))
            {
                if (_defsById.TryGetValue(_draft.layerA.curatedSoundId, out var defA) && defA.clipRef != null)
                    clipA = defA.clipRef;
                else
                    missing = true;
            }

            if (_draft.layerB != null && !string.IsNullOrEmpty(_draft.layerB.curatedSoundId))
            {
                if (_defsById.TryGetValue(_draft.layerB.curatedSoundId, out var defB) && defB.clipRef != null)
                    clipB = defB.clipRef;
                else
                    missing = true;
            }

            if (_draft.LayerCount == 0) missing = true;
        }

        private void EnsureDraftId()
        {
            if (string.IsNullOrWhiteSpace(_draft.id))
                _draft.id = Guid.NewGuid().ToString("N");
        }

        private void NavigateHome()
        {
            _audio?.Stop();
            _nav?.GoTo(SceneId.Home);
        }

        public override void OnBackPressed()
        {
            _audio?.Stop();
            base.OnBackPressed();
            _nav?.GoBack();
        }

        private void SetState(CreateState state) => _state = state;

        private void ShowError(string message)
        {
            if (errorPresenter != null) errorPresenter.ShowError(message);
        }
    }
}
