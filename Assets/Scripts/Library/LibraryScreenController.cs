using System.Collections.Generic;
using Geidai.Common.Library;
using Geidai.Common.Models;
using Geidai.Common.UI;
using Geidai.Foundation;
using Geidai.Services;
using Geidai.Services.Audio;
using Geidai.Services.Content;
using Geidai.Services.Navigation;
using Geidai.Services.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Library
{
    /// <summary>
    /// 音図鑑画面（US-LIB-01）。フィルタ・詳細・HomeUiTheme（U7b）。
    /// </summary>
    public class LibraryScreenController : ScreenRootBase
    {
        [Header("Catalogs")]
        [SerializeField] private CuratedSoundCatalog curatedCatalog;
        [SerializeField] private UnlockRulesCatalog unlockRules;
        [SerializeField] private TimbreTagCatalog timbreTagCatalog;

        [Header("Views")]
        [SerializeField] private CuratedSoundListView listView;
        [SerializeField] private LibraryDetailPanel detailPanel;
        [SerializeField] private Dropdown categoryDropdown;
        [SerializeField] private Dropdown timbreDropdown;
        [SerializeField] private Button backButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private ErrorPresenter errorPresenter;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text titleText;

        private IContentService _content;
        private IProgressionService _progression;
        private IAudioService _audio;
        private INavigationService _nav;
        private LibraryState _state = LibraryState.Loading;
        private bool _wired;
        private List<LibraryItemView> _items = new List<LibraryItemView>();
        private List<CuratedSoundDefinition> _validDefs = new List<CuratedSoundDefinition>();
        private List<string> _categoryLabels = new List<string>();
        private List<string> _timbreIds = new List<string>();
        private string _categoryFilter;
        private string _timbreFilter;
        private string _selectedId;
        private bool _suppressFilterEvents;

        protected override void OnShow()
        {
            EnsureWired();
            ApplyTheme();
            Reload();
        }

        private void EnsureWired()
        {
            if (_wired) return;

            _content = ServiceRegistry.Resolve<IContentService>();
            _progression = ServiceRegistry.Resolve<IProgressionService>();
            _audio = ServiceRegistry.Resolve<IAudioService>();
            _nav = ServiceRegistry.Resolve<INavigationService>();

            LibraryBootstrap.EnsureCatalogs(curatedCatalog, unlockRules, timbreTagCatalog);

            if (listView != null)
            {
                listView.ItemPlayRequested += OnPlayRequested;
                listView.ItemSelected += OnItemSelected;
            }
            if (backButton != null) backButton.onClick.AddListener(NavigateHome);
            if (stopButton != null) stopButton.onClick.AddListener(StopPlayback);
            if (categoryDropdown != null)
                categoryDropdown.onValueChanged.AddListener(OnCategoryChanged);
            if (timbreDropdown != null)
                timbreDropdown.onValueChanged.AddListener(OnTimbreChanged);

            _wired = true;
        }

        private void OnDestroy()
        {
            if (listView != null)
            {
                listView.ItemPlayRequested -= OnPlayRequested;
                listView.ItemSelected -= OnItemSelected;
            }
        }

        private void ApplyTheme()
        {
            if (backgroundImage != null)
                HomeUiImageUtil.ApplySolidFill(backgroundImage, HomeUiTheme.Background);
            if (titleText != null)
            {
                UiFontResolver.ApplyTo(titleText, HomeUiTheme.ScreenTitle);
                titleText.color = HomeUiTheme.TitleOnBackground;
            }
        }

        public void Reload()
        {
            SetState(LibraryState.Loading);
            if (loadingIndicator != null) loadingIndicator.SetActive(true);

            _progression?.Reload();
            LibraryBootstrap.EnsureCatalogs(curatedCatalog, unlockRules, timbreTagCatalog);

            if (_content == null)
            {
                ShowError("おとのずかんが ないよ");
                SetState(LibraryState.Error);
                if (loadingIndicator != null) loadingIndicator.SetActive(false);
                return;
            }

            var catalogResult = _content.GetCuratedCatalog();
            if (!catalogResult.IsSuccess || catalogResult.Value == null)
            {
                ShowError(catalogResult.Message);
                SetState(LibraryState.Error);
                if (loadingIndicator != null) loadingIndicator.SetActive(false);
                return;
            }

            TimbreTagCatalog timbres = null;
            var tr = _content.GetTimbreTagCatalog();
            if (tr.IsSuccess) timbres = tr.Value;

            _validDefs = catalogResult.Value.ValidItems();
            RebuildFilterOptions(timbres);
            ApplyFiltersAndProject(timbres);

            if (loadingIndicator != null) loadingIndicator.SetActive(false);
            SetState(_items.Count == 0 ? LibraryState.Empty : LibraryState.Ready);
        }

        private void RebuildFilterOptions(TimbreTagCatalog timbres)
        {
            _suppressFilterEvents = true;
            _categoryLabels = LibraryFilterOptions.CategoryLabels(_validDefs);
            if (categoryDropdown != null)
            {
                categoryDropdown.ClearOptions();
                categoryDropdown.AddOptions(_categoryLabels);
                categoryDropdown.value = 0;
                categoryDropdown.RefreshShownValue();
            }
            _categoryFilter = null;

            _timbreIds = LibraryFilterOptions.TimbreIds(timbres);
            if (timbreDropdown != null)
            {
                timbreDropdown.ClearOptions();
                timbreDropdown.AddOptions(LibraryFilterOptions.TimbreLabels(timbres));
                timbreDropdown.value = 0;
                timbreDropdown.RefreshShownValue();
            }
            _timbreFilter = null;
            _suppressFilterEvents = false;
        }

        private void ApplyFiltersAndProject(TimbreTagCatalog timbres)
        {
            var unlock = _progression != null ? _progression.CurrentUnlockState : UnlockState.Empty();
            var filtered = LibraryQuery.SortAndFilter(_validDefs, _categoryFilter, _timbreFilter);
            _items = UnlockEvaluator.Project(filtered, unlock, timbres);
            _selectedId = LibraryFilterOptions.ResolveSelectionAfterFilter(_selectedId, _items);

            if (listView != null) listView.SetItems(_items);
            RefreshDetail();
        }

        private void OnCategoryChanged(int index)
        {
            if (_suppressFilterEvents) return;
            _categoryFilter = LibraryFilterOptions.CategoryValueAt(_categoryLabels, index);
            ApplyFiltersAndProject(ResolveTimbres());
            SetState(_items.Count == 0 ? LibraryState.Empty : LibraryState.Ready);
        }

        private void OnTimbreChanged(int index)
        {
            if (_suppressFilterEvents) return;
            _timbreFilter = LibraryFilterOptions.TimbreValueAt(_timbreIds, index);
            ApplyFiltersAndProject(ResolveTimbres());
            SetState(_items.Count == 0 ? LibraryState.Empty : LibraryState.Ready);
        }

        private TimbreTagCatalog ResolveTimbres()
        {
            if (_content == null) return null;
            var tr = _content.GetTimbreTagCatalog();
            return tr.IsSuccess ? tr.Value : null;
        }

        private void OnItemSelected(LibraryItemView item)
        {
            _selectedId = item.id;
            RefreshDetail();
        }

        private void RefreshDetail()
        {
            if (detailPanel == null) return;
            if (LibraryFilterOptions.TryGetItem(_items, _selectedId, out var item))
                detailPanel.Show(item);
            else
                detailPanel.Clear();
        }

        private void OnPlayRequested(LibraryItemView item)
        {
            if (!item.isUnlocked || item.clip == null)
            {
                ShowError("まだ きけない おとだよ");
                return;
            }

            if (_audio == null)
            {
                ShowError("さいせいできなかったよ");
                return;
            }

            var result = _audio.PlayCuratedClip(item.clip);
            if (!result.IsSuccess)
            {
                ShowError(result.Message);
                return;
            }
            SetState(LibraryState.Playing);
        }

        private void StopPlayback()
        {
            _audio?.Stop();
            if (_state == LibraryState.Playing) SetState(LibraryState.Ready);
        }

        private void NavigateHome()
        {
            _audio?.Stop();
            if (_nav != null) _nav.GoTo(SceneId.Home);
            else OnBackPressed();
        }

        public override void OnBackPressed()
        {
            _audio?.Stop();
            base.OnBackPressed();
            _nav?.GoBack();
        }

        private void SetState(LibraryState state) => _state = state;

        private void ShowError(string message)
        {
            if (errorPresenter != null) errorPresenter.ShowError(message);
        }
    }
}
