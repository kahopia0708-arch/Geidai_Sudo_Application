# U7b — Frontend Components

**ユニット**: U7b  
**作成**: 2026-08-29  
**回答**: Q1〜Q6 = A

## 1. Editor（Unity Editor Window）

### `CuratedSoundCatalogEditorWindow`
| 項目 | 内容 |
|---|---|
| メニュー | 例: `Geidai/Library/Curated Sound Catalog` |
| 左ペイン | 登録音一覧（id / number / name）。選択でフォーム、[追加] で新規 |
| 中央 | 属性フォーム（U7a スキーマ全フィールド）。WAV ドロップ／ファイル選択 |
| 右または下 | 音色タグ CRUD リスト |
| 状態 | 対象 SO 参照、選択 id、エラー文字列 |
| 保存 | Validation 通過後のみ Dirty + SaveAssets |

Editor は uGUI ではなく `EditorGUILayout` / IMGUI（または UI Toolkit）。ランタイム UI とは分離。

## 2. Player — 画面階層

```text
LibraryScreenController (ScreenRootBase)
├── Background (HomeUiTheme.Background)
├── Title
├── FilterBar
│   ├── CategoryDropdown（すべて + ユニーク category）
│   └── TimbreDropdown（すべて + ValidTags）
├── CuratedSoundListView (Scroll)
│   └── CuratedSoundItemView[]（行）
├── DetailPanel（選択時: 説明・読み・カテゴリ・音色名）
├── Stop / Back
└── ErrorPresenter / Loading / Empty
```

## 3. コンポーネント責務

| コンポーネント | Props / 状態 | 操作 |
|---|---|---|
| `LibraryScreenController` | catalogs, filters, selectedId, items | Reload, OnCategoryChanged, OnTimbreChanged, OnItemSelected, OnPlay, NavigateHome |
| `CuratedSoundListView` | items | SetItems; ItemPlayRequested; ItemSelected（追加） |
| `CuratedSoundItemView` | LibraryItemView + placeholderSprite | Bind: number, name, image, lock, play（ロック時非活性） |
| `LibraryDetailPanel`（新規可） | selected item or null | 説明等表示。未選択は空 |
| `LibraryFilterBar`（新規可） | options + current | ドロップダウン変更イベント |

## 4. テーマ

- 背景・タイトル・本文・ボタンラベルに `HomeUiTheme` + `UiFontResolver`
- 設定画面と同程度のホーム基調（カード乱立は避け、一覧＋詳細の一構成）

## 5. シーン／プレースホルダー

- `GeidaiLibrary` シーンに FilterBar・DetailPanel・TimbreTagCatalog 参照を配線
- `Assets/Art/Library/Icons/placeholder` を同梱（簡易画像で可。後で差し替え）
- 既存 List/Item プレハブを拡張（ナンバー・Image スロット追加）

## 6. インタラクション要約

| 操作 | 結果 |
|---|---|
| カテゴリ／音色変更 | フィルタ再適用。選択解除または選択維持（id が結果に無ければ解除） |
| 行タップ | 詳細更新。試聴は再生ボタンのみ |
| 再生（解除済） | AudioService 再生 |
| 再生（ロック） | ボタン非活性のため不可 |
| もどる | Home |
