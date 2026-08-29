using System.Collections.Generic;
using Geidai.Common.Library;
using Geidai.Common.Models;
using Geidai.Common.UI;
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
    /// 音図鑑画面（US-LIB-01〜03）。ロック投影・解除済み試聴のみ。
    /// </summary>
    public class LibraryScreenController : ScreenRootBase
    {
        [Header("Catalogs")]
        [SerializeField] private CuratedSoundCatalog curatedCatalog;
        [SerializeField] private UnlockRulesCatalog unlockRules;
        [SerializeField] private TimbreTagCatalog timbreTagCatalog;

        [Header("Views")]
        [SerializeField] private CuratedSoundListView listView;
        [SerializeField] private Button backButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private ErrorPresenter errorPresenter;

        private IContentService _content;
        private IProgressionService _progression;
        private IAudioService _audio;
        private INavigationService _nav;
        private LibraryState _state = LibraryState.Loading;
        private bool _wired;
        private List<LibraryItemView> _items = new List<LibraryItemView>();

        protected override void OnShow()
        {
            EnsureWired();
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

            if (listView != null) listView.ItemPlayRequested += OnPlayRequested;
            if (backButton != null) backButton.onClick.AddListener(NavigateHome);
            if (stopButton != null) stopButton.onClick.AddListener(StopPlayback);

            _wired = true;
        }

        private void OnDestroy()
        {
            if (listView != null) listView.ItemPlayRequested -= OnPlayRequested;
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

            var unlock = _progression != null ? _progression.CurrentUnlockState : UnlockState.Empty();
            TimbreTagCatalog timbres = null;
            if (_content != null)
            {
                var tr = _content.GetTimbreTagCatalog();
                if (tr.IsSuccess) timbres = tr.Value;
            }

            var valid = catalogResult.Value.ValidItems();
            valid = LibraryQuery.SortByEncyclopediaNumber(valid);
            _items = UnlockEvaluator.Project(valid, unlock, timbres);

            if (listView != null) listView.SetItems(_items);
            if (loadingIndicator != null) loadingIndicator.SetActive(false);

            SetState(_items.Count == 0 ? LibraryState.Empty : LibraryState.Ready);
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
