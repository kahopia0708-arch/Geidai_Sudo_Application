# U7a — Business Logic Model

**ユニット**: U7a  
**作成**: 2026-08-29

## 1. カタログ読取フロー

```text
Bootstrap / Scene
  → ContentService.SetCuratedCatalog / SetTimbreTagCatalog
  → GetCuratedCatalog → ValidItems
  → UnlockEvaluator.Project(unlockState)
  → LibraryQuery.SortByEncyclopediaNumber
  → (任意) LibraryQuery.Filter(category, timbreTagId)
  → LibraryItemView リスト
```

## 2. 検証フロー（Editor が呼ぶ想定の純粋／Catalog API）

```text
Candidate Def
  → IsValid?
  → TimbreTagCatalog.ContainsId(timbreTagId)?
  → !ContainsId(id) or same-row update?
  → !ContainsNumber(encyclopediaNumber) or same-row update?
  → Accept / Reject(Result)
```

## 3. 音色タグ削除

```text
Remove(tagId)
  → any CuratedSound.timbreTagId == tagId ?
      Yes → Fail（削除不可）
      No  → Remove from TimbreTagCatalog
```

## 4. LibraryQuery

| 操作 | 論理 |
|---|---|
| Sort | OrderBy encyclopediaNumber, ThenBy id |
| Filter | category 一致（指定時）AND timbreTagId 一致（指定時） |

## 5. ContentService 拡張

| メソッド | 挙動 |
|---|---|
| SetTimbreTagCatalog | 注入 |
| GetTimbreTagCatalog | 未設定 → Fail(NotFound) |

既存 GetCuratedCatalog / UnlockRules は不変。

## 6. テスト観点（U7a）
- IsValid 境界（欠落・番号≤0）
- 重複 id / number
- LibraryQuery ソート・フィルタ決定性（PBT 可）
- TimbreTag CanRemove 参照あり／なし
- UnlockEvaluator 既存テストが新必須フィールドでも通るよう Def ヘルパ更新
