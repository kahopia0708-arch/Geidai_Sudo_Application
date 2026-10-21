using System.Collections.Generic;
using Geidai.Common.Results;
using UnityEngine;

namespace Geidai.Common.Library
{
    /// <summary>
    /// 制作側音図鑑カタログ（差し替え可能 / FR-20 / US-TECH-07）。
    /// 企画・デザイン担当がインスペクタで編集する ScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = "CuratedSoundCatalog", menuName = "Geidai/Curated Sound Catalog", order = 10)]
    public class CuratedSoundCatalog : ScriptableObject
    {
        [SerializeField] private List<CuratedSoundDefinition> items = new List<CuratedSoundDefinition>();

        public IReadOnlyList<CuratedSoundDefinition> Items => items;

        public List<CuratedSoundDefinition> ValidItems()
        {
            var result = new List<CuratedSoundDefinition>();
            if (items == null) return result;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item != null && item.IsValid) result.Add(item);
            }
            return result;
        }

        public CuratedSoundDefinition FindById(string id)
        {
            if (string.IsNullOrEmpty(id) || items == null) return null;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item != null && item.id == id) return item;
            }
            return null;
        }

        public bool ContainsId(string id) => FindById(id) != null;

        public bool ContainsEncyclopediaNumber(int number, string exceptId = null)
        {
            if (number < 1 || items == null) return false;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null || !item.IsValid) continue;
                if (!string.IsNullOrEmpty(exceptId) && item.id == exceptId) continue;
                if (item.encyclopediaNumber == number) return true;
            }
            return false;
        }

        public void SetItems(IEnumerable<CuratedSoundDefinition> newItems)
        {
            items = newItems != null ? new List<CuratedSoundDefinition>(newItems) : new List<CuratedSoundDefinition>();
        }

        /// <summary>追加または id 一致の置換（Editor Upsert）。</summary>
        public void Upsert(CuratedSoundDefinition definition, string replaceId = null)
        {
            if (definition == null) return;
            if (items == null) items = new List<CuratedSoundDefinition>();

            string key = !string.IsNullOrEmpty(replaceId) ? replaceId : definition.id;
            for (int i = 0; i < items.Count; i++)
            {
                var existing = items[i];
                if (existing != null && existing.id == key)
                {
                    items[i] = definition;
                    return;
                }
            }
            items.Add(definition);
        }

        /// <summary>id 一致の行を削除。見つからなければ false。</summary>
        public bool RemoveById(string id)
        {
            if (string.IsNullOrEmpty(id) || items == null) return false;
            for (int i = 0; i < items.Count; i++)
            {
                var existing = items[i];
                if (existing != null && existing.id == id)
                {
                    items.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 図鑑ナンバー昇順の並びで、隣接する音と encyclopediaNumber を入れ替える。
        /// direction: -1=上（番号が小さい側へ）、+1=下。
        /// </summary>
        public Result SwapEncyclopediaNumberWithSortedNeighbor(string id, int direction)
        {
            if (string.IsNullOrEmpty(id))
                return Result.Fail(ResultCode.ValidationError, "ID が ないよ");
            if (direction != -1 && direction != 1)
                return Result.Fail(ResultCode.ValidationError, "並べ替え方向が 正しくないよ");

            var sorted = LibraryQuery.SortByEncyclopediaNumber(items);
            int index = -1;
            for (int i = 0; i < sorted.Count; i++)
            {
                if (sorted[i].id == id)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
                return Result.Fail(ResultCode.NotFound, "音が みつからないよ");

            int otherIndex = index + direction;
            if (otherIndex < 0 || otherIndex >= sorted.Count)
                return Result.Fail(ResultCode.ValidationError, "これ以上 は 動かせないよ");

            int a = sorted[index].encyclopediaNumber;
            int b = sorted[otherIndex].encyclopediaNumber;
            sorted[index].encyclopediaNumber = b;
            sorted[otherIndex].encyclopediaNumber = a;
            return Result.Ok();
        }
    }
}
