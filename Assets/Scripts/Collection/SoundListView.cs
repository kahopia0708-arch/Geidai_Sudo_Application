using System;
using System.Collections.Generic;
using UnityEngine;

namespace Geidai.Collection
{
    /// <summary>
    /// 一覧ビュー（frontend-components §3 / NFR-COL-P1）。
    /// ViewModel 群を受けて <see cref="SoundListItemView"/> を並べる。項目はプールして
    /// 再利用し、GC/生成コストを抑える（将来の仮想化にも耐える構造）。
    /// サムネは遅延ローダ（id→Sprite）で必要時のみ読み込む。空時は空状態を表示。
    /// </summary>
    public class SoundListView : MonoBehaviour
    {
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private SoundListItemView itemPrefab;
        [SerializeField] private GameObject emptyState;

        private readonly List<SoundListItemView> _pool = new List<SoundListItemView>();

        /// <summary>id→サムネ Sprite を返す遅延ローダ（写真無し/失敗は null）。</summary>
        public Func<string, Sprite> ThumbnailLoader { get; set; }

        public event Action<string> ItemOpenRequested;
        public event Action<string> ItemPlayRequested;

        public void SetItems(IReadOnlyList<SoundItemViewModel> items)
        {
            int count = items != null ? items.Count : 0;

            if (emptyState != null) emptyState.SetActive(count == 0);

            EnsurePool(count);

            for (int i = 0; i < _pool.Count; i++)
            {
                var view = _pool[i];
                if (i < count)
                {
                    var vm = items[i];
                    view.gameObject.SetActive(true);
                    view.Bind(vm, OnOpen, OnPlay);
                    ApplyThumbnail(view, vm);
                }
                else
                {
                    view.gameObject.SetActive(false);
                }
            }
        }

        private void ApplyThumbnail(SoundListItemView view, SoundItemViewModel vm)
        {
            if (vm == null || !vm.hasPhoto || ThumbnailLoader == null)
            {
                view.SetThumbnail(null);
                return;
            }
            view.SetThumbnail(ThumbnailLoader(vm.id));
        }

        private void EnsurePool(int count)
        {
            if (itemPrefab == null || contentRoot == null) return;
            while (_pool.Count < count)
            {
                var view = Instantiate(itemPrefab, contentRoot);
                _pool.Add(view);
            }
        }

        private void OnOpen(string id) => ItemOpenRequested?.Invoke(id);
        private void OnPlay(string id) => ItemPlayRequested?.Invoke(id);
    }
}
