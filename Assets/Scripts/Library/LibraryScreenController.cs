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
    /// 音図鑑画面。ホーム基調＋サムネイル→詳細（きく／とめるトグル）。
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
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private ErrorPresenter errorPresenter;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text titleText;
        [SerializeField] private Sprite placeholderSprite;

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
        private string _playingId;
        private bool _suppressFilterEvents;

        protected override void OnShow()
        {
            EnsureWired();
            ApplyTheme();
            StopPlayback();
            Reload();
        }

        protected override void Update()
        {
            base.Update();
            if (_state == LibraryState.Playing && (_audio == null || !_audio.IsPlaying))
                StopPlayback();
        }

        private void EnsureWired()
        {
            if (_wired) return;

            _content = ServiceRegistry.Resolve<IContentService>();
            _progression = ServiceRegistry.Resolve<IProgressionService>();
            _audio = EnsureAudio();
            _nav = ServiceRegistry.Resolve<INavigationService>();

            LibraryBootstrap.EnsureCatalogs(curatedCatalog, unlockRules, timbreTagCatalog);

            if (listView != null)
                listView.ItemSelected += OnItemSelected;
            if (detailPanel != null)
                detailPanel.SetPlayHandler(OnPlayToggleRequested);
            if (backButton != null) backButton.onClick.AddListener(NavigateHome);
            if (categoryDropdown != null)
                categoryDropdown.onValueChanged.AddListener(OnCategoryChanged);
            if (timbreDropdown != null)
                timbreDropdown.onValueChanged.AddListener(OnTimbreChanged);

            _wired = true;
        }

        private void OnDestroy()
        {
            if (listView != null)
                listView.ItemSelected -= OnItemSelected;
        }

        private void ApplyTheme()
        {
            if (backgroundImage != null)
                HomeUiImageUtil.ApplySolidFill(backgroundImage, HomeUiTheme.Background);

            var cam = Camera.main;
            if (cam != null)
                cam.backgroundColor = HomeUiTheme.Background;

            if (titleText != null)
            {
                UiFontResolver.ApplyTo(titleText, HomeUiTheme.ScreenTitle);
                titleText.color = HomeUiTheme.TitleOnBackground;
            }

            StylePillButton(backButton);
            StyleDropdown(categoryDropdown);
            StyleDropdown(timbreDropdown);
        }

        private static void StylePillButton(Button button)
        {
            if (button == null) return;
            var image = button.GetComponent<Image>();
            if (image != null)
                HomeUiImageUtil.ApplyPillFill(image, HomeUiTheme.PanelFill);
            var label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                UiFontResolver.ApplyTo(label, HomeUiTheme.ActionButtonLabel);
                label.color = HomeUiTheme.MenuText;
                label.fontStyle = FontStyle.Bold;
            }
        }

        private static void StyleDropdown(Dropdown dropdown)
        {
            if (dropdown == null) return;
            var image = dropdown.GetComponent<Image>();
            if (image != null) HomeUiImageUtil.ApplySolidFill(image, HomeUiTheme.InputFill);
            if (dropdown.captionText != null)
            {
                UiFontResolver.ApplyTo(dropdown.captionText, HomeUiTheme.Body);
                dropdown.captionText.color = HomeUiTheme.MenuText;
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
            if (_state == LibraryState.Playing)
                StopPlayback();
            _selectedId = item.id;
            RefreshDetail();
        }

        private void RefreshDetail()
        {
            if (detailPanel == null) return;
            if (LibraryFilterOptions.TryGetItem(_items, _selectedId, out var item))
            {
                bool playing = _state == LibraryState.Playing && _playingId == item.id;
                detailPanel.Show(item, placeholderSprite, playing);
            }
            else
            {
                detailPanel.Clear();
            }
        }

        private void OnPlayToggleRequested(LibraryItemView item)
        {
            if (!string.IsNullOrEmpty(item.id) &&
                LibraryFilterOptions.TryGetItem(_items, item.id, out var fresh))
                item = fresh;

            if (_state == LibraryState.Playing)
            {
                StopPlayback();
                return;
            }

            if (!item.isUnlocked || item.clip == null)
            {
                ShowError("まだ きけない おとだよ");
                return;
            }

            _audio = EnsureAudio();
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

            _playingId = item.id;
            SetState(LibraryState.Playing);
            detailPanel?.SetPlaying(true);
        }

        private void StopPlayback()
        {
            _audio?.Stop();
            _playingId = null;
            if (_state == LibraryState.Playing)
                SetState(LibraryState.Ready);
            detailPanel?.SetPlaying(false);
        }

        private void NavigateHome()
        {
            StopPlayback();
            if (_nav != null) _nav.GoTo(SceneId.Home);
            else OnBackPressed();
        }

        public override void OnBackPressed()
        {
            StopPlayback();
            base.OnBackPressed();
            _nav?.GoBack();
        }

        private static IAudioService EnsureAudio()
        {
            var audio = ServiceRegistry.Resolve<IAudioService>();
            if (audio != null) return audio;
            audio = new AudioService();
            ServiceRegistry.Register<IAudioService>(audio);
            return audio;
        }

        /// <summary>
        /// Edit Mode 用プレビュー。カタログを初期解除状態でグリッド／詳細に載せる（遷移・試聴はしない）。
        /// </summary>
        public void BuildEditModePreview()
        {
            if (Application.isPlaying)
            {
                Reload();
                return;
            }

            ApplyTheme();
            LibraryBootstrap.EnsureCatalogs(curatedCatalog, unlockRules, timbreTagCatalog);

            _validDefs = curatedCatalog != null
                ? curatedCatalog.ValidItems()
                : new List<CuratedSoundDefinition>();
            var unlock = UnlockEvaluator.ApplyInitialUnlocks(UnlockState.Empty(), _validDefs);
            _items = UnlockEvaluator.Project(_validDefs, unlock, timbreTagCatalog);

            if (listView != null) listView.SetItems(_items);

            if (categoryDropdown != null)
            {
                categoryDropdown.ClearOptions();
                categoryDropdown.AddOptions(LibraryFilterOptions.CategoryLabels(_validDefs));
                categoryDropdown.value = 0;
                categoryDropdown.RefreshShownValue();
            }

            if (timbreDropdown != null)
            {
                timbreDropdown.ClearOptions();
                timbreDropdown.AddOptions(LibraryFilterOptions.TimbreLabels(timbreTagCatalog));
                timbreDropdown.value = 0;
                timbreDropdown.RefreshShownValue();
            }

            if (detailPanel != null)
            {
                if (_items.Count > 0)
                {
                    _selectedId = _items[0].id;
                    detailPanel.Show(_items[0], placeholderSprite);
                }
                else
                {
                    _selectedId = null;
                    detailPanel.Clear();
                }
            }

            SetState(_items.Count == 0 ? LibraryState.Empty : LibraryState.Ready);
        }

        /// <summary>Edit Mode プレビュー用。グリッドと詳細を空にする。</summary>
        public void ClearEditModePreview()
        {
            if (Application.isPlaying) return;
            _items = new List<LibraryItemView>();
            _selectedId = null;
            listView?.ClearPreview();
            detailPanel?.Clear();
        }

        private void SetState(LibraryState state) => _state = state;

        private void ShowError(string message)
        {
            if (errorPresenter != null) errorPresenter.ShowError(message);
        }
    }
}
