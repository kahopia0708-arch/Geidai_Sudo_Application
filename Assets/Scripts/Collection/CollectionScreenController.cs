using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Geidai.Common.Collection;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.UI;
using Geidai.Services;
using Geidai.Services.Audio;
using Geidai.Services.Media;
using Geidai.Services.Storage;
using Geidai.Services.Navigation;

namespace Geidai.Collection
{
    /// <summary>
    /// コレクション画面の司令塔（US-COL-01〜04 / frontend-components §1・business-logic-model）。
    /// 一覧・絞込/検索・詳細/編集・空状態を 1 画面で統括する。読込は破損スキップ（<see cref="IStorageService"/>）、
    /// 絞込は純粋関数（<see cref="CollectionFilter"/>）、視聴は共有 <see cref="IAudioService"/>。
    /// サムネ/写真は端末内のみで表示（NFR-COL-Priv2）。見た目は S さん調整（US-TECH-07）。
    /// </summary>
    public class CollectionScreenController : ScreenRootBase
    {
        [Header("Views")]
        [SerializeField] private SoundListView listView;
        [SerializeField] private FilterSearchController filterSearch;
        [SerializeField] private SoundDetailController detail;

        [Header("UI")]
        [SerializeField] private Button backButton;
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private ErrorPresenter errorPresenter;

        private IStorageService _storage;
        private IAudioService _audio;
        private INavigationService _nav;
        private IPhotoPicker _photoPicker;

        private readonly List<SavedSound> _all = new List<SavedSound>();
        private readonly Dictionary<string, Sprite> _thumbCache = new Dictionary<string, Sprite>();
        private CollectionQuery _query = CollectionQuery.Empty;
        private bool _wired;

        protected override void OnShow()
        {
            EnsureWired();
            ReloadAll();
        }

        private void EnsureWired()
        {
            if (_wired) return;

            _storage = ServiceRegistry.Resolve<IStorageService>();
            _audio = ServiceRegistry.Resolve<IAudioService>();
            _nav = ServiceRegistry.Resolve<INavigationService>();
            _photoPicker = CollectionBootstrap.EnsurePhotoPicker();

            if (listView != null)
            {
                listView.ThumbnailLoader = LoadThumbnail;
                listView.ItemOpenRequested += OpenDetail;
                listView.ItemPlayRequested += QuickPlay;
            }
            if (filterSearch != null)
                filterSearch.QueryChanged += OnQueryChanged;

            if (detail != null)
            {
                detail.Init(_storage, _audio, _photoPicker);
                detail.MetaChanged += OnMetaChanged;
                detail.Deleted += OnDeleted;
                detail.Closed += OnDetailClosed;
                detail.gameObject.SetActive(false);
            }

            if (backButton != null) backButton.onClick.AddListener(NavigateHome);

            _wired = true;
        }

        private void OnDestroy()
        {
            if (listView != null)
            {
                listView.ItemOpenRequested -= OpenDetail;
                listView.ItemPlayRequested -= QuickPlay;
            }
            if (filterSearch != null)
                filterSearch.QueryChanged -= OnQueryChanged;
            if (detail != null)
            {
                detail.MetaChanged -= OnMetaChanged;
                detail.Deleted -= OnDeleted;
                detail.Closed -= OnDetailClosed;
            }
        }

        // --- data ---

        private void ReloadAll()
        {
            SetLoading(true);
            _all.Clear();

            if (_storage != null)
            {
                var result = _storage.ListSounds();
                if (result.IsSuccess && result.Value != null)
                    _all.AddRange(result.Value);
            }

            if (filterSearch != null)
                filterSearch.SetAvailableMonths(CollectMonths(_all));

            ApplyQuery(_query);
            SetLoading(false);
        }

        private void ApplyQuery(CollectionQuery query)
        {
            _query = query;
            var filtered = CollectionFilter.Filter(_all, query);

            var vms = new List<SoundItemViewModel>(filtered.Count);
            for (int i = 0; i < filtered.Count; i++)
                vms.Add(SoundItemViewModel.From(filtered[i]));

            if (listView != null) listView.SetItems(vms);
        }

        private static IEnumerable<string> CollectMonths(IReadOnlyList<SavedSound> items)
        {
            var months = new List<string>();
            var seen = new HashSet<string>();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null || items[i].meta == null) continue;
                string ym = CollectionFilter.ToYearMonth(items[i].meta.createdAtIso);
                if (string.IsNullOrEmpty(ym) || seen.Contains(ym)) continue;
                seen.Add(ym);
                months.Add(ym);
            }
            return months;
        }

        // --- events ---

        private void OnQueryChanged(CollectionQuery query) => ApplyQuery(query);

        private void OpenDetail(string id)
        {
            var sound = FindById(id);
            if (sound == null || detail == null) return;
            detail.Show(sound);
        }

        private void QuickPlay(string id)
        {
            if (_storage == null || _audio == null) return;
            var sound = FindById(id);
            if (sound == null) return;

            var buf = _storage.LoadSoundBuffer(id);
            if (!buf.IsSuccess)
            {
                if (errorPresenter != null) errorPresenter.ShowFromResult(Result.Fail(buf.Code, buf.Message));
                return;
            }

            var settings = sound.settings ?? new SoundEffectSettingsData();
            var played = _audio.Play(buf.Value, settings);
            if (!played.IsSuccess && errorPresenter != null)
                errorPresenter.ShowFromResult(played);
        }

        private void OnMetaChanged()
        {
            InvalidateThumbCache();
            ReloadAll();
        }

        private void OnDeleted(string id)
        {
            if (detail != null) detail.gameObject.SetActive(false);
            _thumbCache.Remove(id);
            ReloadAll();
        }

        private void OnDetailClosed()
        {
            if (detail != null) detail.gameObject.SetActive(false);
        }

        // --- back ---

        public override void OnBackPressed() => NavigateHome();

        private void NavigateHome()
        {
            if (detail != null && detail.gameObject.activeSelf)
            {
                detail.gameObject.SetActive(false);
                return;
            }

            if (_nav == null) _nav = ServiceRegistry.Resolve<INavigationService>();
            if (_nav == null) return;

            var result = _nav.GoTo(SceneId.Home);
            if (!result.IsSuccess && errorPresenter != null)
                errorPresenter.ShowFromResult(result);
        }

        // --- helpers ---

        private SavedSound FindById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < _all.Count; i++)
            {
                if (_all[i] != null && _all[i].meta != null && _all[i].meta.id == id)
                    return _all[i];
            }
            return null;
        }

        private Sprite LoadThumbnail(string id)
        {
            if (string.IsNullOrEmpty(id) || _storage == null) return null;
            if (_thumbCache.TryGetValue(id, out var cached)) return cached;

            var bytes = _storage.LoadPhoto(id);
            Sprite sprite = bytes.IsSuccess ? CollectionSprites.FromBytes(bytes.Value) : null;
            _thumbCache[id] = sprite; // null もキャッシュ（再読込を避ける）
            return sprite;
        }

        private void InvalidateThumbCache() => _thumbCache.Clear();

        private void SetLoading(bool loading)
        {
            if (loadingIndicator != null) loadingIndicator.SetActive(loading);
        }
    }
}
