using System;
using System.Collections.Generic;
using Geidai.Common.Library;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Library
{
    /// <summary>音図鑑サムネイルグリッド。</summary>
    public class CuratedSoundListView : MonoBehaviour
    {
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private CuratedSoundItemView itemPrefab;
        [SerializeField] private GameObject emptyState;
        [SerializeField] private Sprite placeholderSprite;
        [SerializeField] private int gridColumns = 3;

        private readonly List<CuratedSoundItemView> _pool = new List<CuratedSoundItemView>();

        public event Action<LibraryItemView> ItemPlayRequested;
        public event Action<LibraryItemView> ItemSelected;

        public void SetItems(IReadOnlyList<LibraryItemView> items)
        {
            EnsureGridLayout();
            RebuildPool(items != null ? items.Count : 0);

            int count = items != null ? items.Count : 0;
            if (emptyState != null) emptyState.SetActive(count == 0);

            for (int i = 0; i < _pool.Count; i++)
            {
                var view = _pool[i];
                if (i < count)
                {
                    view.gameObject.SetActive(true);
                    view.Bind(items[i], null, OnSelect, placeholderSprite);
                }
                else
                {
                    view.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>Edit Mode プレビュー用。グリッド子を消す。</summary>
        public void ClearPreview()
        {
            RebuildPool(0);
            if (emptyState != null) emptyState.SetActive(true);
        }

        private void EnsureGridLayout()
        {
            if (contentRoot == null) return;

            var vertical = contentRoot.GetComponent<VerticalLayoutGroup>();
            if (vertical != null)
            {
                if (Application.isPlaying) Destroy(vertical);
                else DestroyImmediate(vertical);
            }

            var grid = contentRoot.GetComponent<GridLayoutGroup>();
            if (grid == null) grid = contentRoot.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Mathf.Max(2, gridColumns);
            grid.cellSize = new Vector2(200f, 220f);
            grid.spacing = new Vector2(16f, 16f);
            grid.padding = new RectOffset(16, 16, 16, 16);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;

            var fitter = contentRoot.GetComponent<ContentSizeFitter>();
            if (fitter == null) fitter = contentRoot.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void RebuildPool(int count)
        {
            _pool.Clear();
            if (contentRoot == null) return;

            for (int i = contentRoot.childCount - 1; i >= 0; i--)
            {
                var go = contentRoot.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(go);
                else DestroyImmediate(go);
            }

            if (itemPrefab == null || count <= 0) return;

            for (int i = 0; i < count; i++)
            {
                var view = UnityEngine.Object.Instantiate(itemPrefab, contentRoot);
                if (!Application.isPlaying)
                    view.gameObject.hideFlags = HideFlags.DontSave;
                _pool.Add(view);
            }
        }

        private void OnSelect(LibraryItemView item) => ItemSelected?.Invoke(item);

        public void RaisePlay(LibraryItemView item) => ItemPlayRequested?.Invoke(item);
    }
}
