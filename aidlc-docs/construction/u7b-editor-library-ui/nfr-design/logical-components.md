# U7b — Logical Components

**作成**: 2026-08-29  
**回答**: Q1〜Q3 = 全 A

| 論理コンポーネント | 実装候補 | 層 | 役割 |
|---|---|---|---|
| Filter Options | `LibraryFilterOptions` | Common.Library | 「すべて」＋ category / timbre 選択肢（純粋） |
| Placeholder Bind | ItemView / Screen ヘルパ | Library | `imageRef` null 時に共有 Sprite |
| Library Screen | `LibraryScreenController` | Library | フィルタ状態・選択・Reload・試聴・HomeUiTheme |
| List / Item / Detail | `CuratedSoundListView` / `ItemView` / DetailPanel | Library | 行表示・選択・ロック時再生非活性 |
| Editor Window | `CuratedSoundCatalogEditorWindow` | Editor | 一覧＋フォーム＋語彙 CRUD（IMGUI） |
| Editor Ops | `CuratedSoundCatalogEditorOps` | Editor | WAV→`Audio/Library/{id}`、Import、SaveAssets |
| Validation / Query | U7a 既存 | Common.Library | Upsert・CanRemove・SortAndFilter |
| Content / Unlock | 既存 Services | Services | 読取・投影（書込なし） |

## 依存

```text
CuratedSoundValidation / LibraryQuery / LibraryFilterOptions  (純粋)
        ▲
CuratedSoundCatalog / TimbreTagCatalog (SO)
        ▲
┌───────┴────────┐
│                │
EditorOps     IContentService (読取)
│                │
EditorWindow  LibraryScreenController
                     │
              List / Item / Detail
```

## N/A
Circuit breaker, queue, distributed cache, cloud failover — 本アプリ非該当。
