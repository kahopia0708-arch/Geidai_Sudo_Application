# Components — サウンドライブラリ属性

**作成**: 2026-08-29  
**回答**: Q1=A＋Editorで語彙CRUD / Q2=A / Q3=A / Q4=A / Q5=A

## 1. コンポーネント一覧

| コンポーネント | 層 | 責務 |
|---|---|---|
| `CuratedSoundDefinition` | Common.Library | 1音の新スキーマ定義（同一型フィールド拡張） |
| `TimbreTagDefinition` / `TimbreTagCatalog` | Common.Library | 音色タグ語彙（SO）。Editor から追加・変更・削除 |
| `CuratedSoundCatalog` | Common.Library | 音一覧 SO。ValidItems / FindById / 重複検証ヘルパ |
| `LibraryQuery` | Common.Library | 図鑑ナンバー順ソート・カテゴリ／音色フィルタ（純粋） |
| `LibraryItemView` | Common.Library | 一覧投影（画像・ナンバー・タグ含む） |
| `IContentService` / `ContentService` | Services | カタログ読取（既存拡張）。語彙カタログ参照可 |
| `CuratedSoundCatalogEditorWindow` | Editor | WAV→Clip→属性フォーム→カタログ追加。タグ語彙CRUD |
| `LibraryScreenController` | Library | 画面制御・HomeUiTheme・絞り込み・試聴 |
| `CuratedSoundListView` / `ItemView` | Library | 一覧／行 UI |
| `UnlockEvaluator` | Common.Library | 既存維持（Project に新フィールド投影） |

## 2. 設計方針（Q1 補足）

音色は **C# enum 固定ではなく、`TimbreTagCatalog` SO の制御語彙**とする。  
- 各音は `timbreTagId`（1つ・必須）で語彙を参照  
- Editor ウィンドウから語彙の追加・変更・削除が可能（コード変更不要）  
- 削除時は参照中の音があれば拒否、または「Other」へ付け替え（FD で確定）

## 3. 画像（Q2）

- 配置: `Assets/Art/Library/Icons/`  
- 定義: `Sprite imageRef`（任意）。未設定時プレースホルダー

## 4. スキーマ（Q3）

- 型名は `CuratedSoundDefinition` のままフィールド拡張  
- 旧サンプルは再登録。最低限の新スキーマサンプルを同梱
