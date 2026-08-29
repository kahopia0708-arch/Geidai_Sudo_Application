# Unit of Work — サウンドライブラリ属性

**作成**: 2026-08-29  
**分割**: Q5=A（U7a → U7b）

## U7a — Schema & Catalog API

| 項目 | 内容 |
|---|---|
| 目的 | 新スキーマ・音色語彙 SO・Catalog／ContentService 拡張・純粋クエリ・テスト |
| 成果物 | `CuratedSoundDefinition` 拡張、`TimbreTagCatalog`、`LibraryQuery`、`LibraryItemView` 拡張、`IContentService`、サンプル再登録、EditMode テスト |
| 非対象 | Editor Window、Library 画面 UI 刷新 |

## U7b — Editor & Library UI

| 項目 | 内容 |
|---|---|
| 目的 | コンテンツ登録ウィンドウ＋プレイヤー図鑑（ソート／絞込／画像／HomeUiTheme） |
| 成果物 | `CuratedSoundCatalogEditorWindow`、語彙CRUD UI、`LibraryScreenController` 拡張、List/Item View、シーン／プレースホルダー |
| 依存 | U7a 完了後 |

## コード配置
- Runtime: 既存 `Assets/Scripts/Common/Library` / `Services` / `Library`
- Editor: `Assets/Editor/`（新規）
- Art: `Assets/Art/Library/Icons/`、Audio は `Assets/Audio/Library/`（FD でパス確定可）
