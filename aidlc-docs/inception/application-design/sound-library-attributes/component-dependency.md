# Component Dependency — サウンドライブラリ属性

**作成**: 2026-08-29

```text
EditorWindow
    │ writes
    ▼
TimbreTagCatalog (SO)  ◀──refs──  CuratedSoundDefinition.timbreTagId
CuratedSoundCatalog (SO)
    │
    ▼
IContentService ──▶ LibraryScreenController ──▶ ListView
                 └─▶ CreateScreenController (読取のみ)
                 └─▶ ProgressionService (UnlockEvaluator)

LibraryQuery / UnlockEvaluator  … Common 純粋（UI・Services から利用）
```

## 依存ルール
- `Geidai.Library` → Services → Common（既存）
- Editor アセンブリは runtime を参照。runtime は Editor を参照しない
- Create / Game は新フィールドを無視してコンパイル可能（必要な属性は後続タスクで利用）

## 結合度
- 語彙削除と音参照は Catalog 側で検査し、密結合を Editor 検証に閉じる
