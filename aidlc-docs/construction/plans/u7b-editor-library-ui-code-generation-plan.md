# U7b Code Generation Plan — Editor & Library UI

**ユニット**: U7b  
**ブランチ**: `feature/sound-library-attributes`  
**作成**: 2026-08-29  
**Stories**: US-LIB-04（Editor）、US-LIB-01 更新（プレイヤー）  
**前提**: U7a 完了／FD・NFR Design 承認／Infrastructure SKIP

## コンテキスト
| 領域 | パス |
|---|---|
| Common | `Assets/Scripts/Common/Library/`（`LibraryFilterOptions` 新規） |
| Library UI | `Assets/Scripts/Library/` |
| Editor | `Assets/Editor/`（Window + Ops） |
| Art | `Assets/Art/Library/Icons/`（placeholder） |
| Audio | `Assets/Audio/Library/`（空フォルダ＋.gitkeep 可） |
| Settings | 既存 Catalog / Timbre SO |
| Tests | `Assets/Scripts/Tests/EditMode/` |
| Docs | `aidlc-docs/construction/u7b-editor-library-ui/code/` |

## 実行ステップ

- [x] **Step 0** — 既存 Library Screen / List / Item / GeidaiLibrary シーン／Editor 配置を確認
- [x] **Step 1** — `LibraryFilterOptions`（Common）: カテゴリ／音色の「すべて」＋選択肢生成
- [x] **Step 2** — `CuratedSoundCatalogEditorOps`（Editor）: WAV→`Assets/Audio/Library/{id}`、Import、Catalog/Timbre Save、Validation 呼び出し
- [x] **Step 3** — `CuratedSoundCatalogEditorWindow`（IMGUI）: 一覧＋選択編集／追加、属性フォーム、語彙 CRUD、エラー表示
- [x] **Step 4** — `LibraryDetailPanel`（または Screen 内）: 選択時に説明・読み・カテゴリ・音色名
- [x] **Step 5** — `CuratedSoundItemView` 拡張: encyclopediaNumber・Image（placeholder）・ロック時再生非活性維持
- [x] **Step 6** — `CuratedSoundListView` 拡張: `ItemSelected` イベント
- [x] **Step 7** — `LibraryScreenController` 拡張: フィルタ Dropdown、FilterOptions、選択維持（NFR Q3=A）、HomeUiTheme、Timbre 配線
- [x] **Step 8** — placeholder Sprite 資産（`Assets/Art/Library/Icons/`）＋ Audio/Library フォルダ
- [x] **Step 9** — シーン／ブートストラップ更新: Filter・Detail・TimbreTagCatalog・placeholder 参照（`GeidaiSceneBootstrap` または専用 Builder／手動配線手順を code-summary に記載）
- [x] **Step 10** — EditMode: `LibraryFilterOptions`／選択維持ヘルパ。既存 Validation 回帰
- [x] **Step 11** — Unity コンパイル確認（MCP）。`code-summary.md` 作成
- [x] **Step 12** — コミット（例: `feat(library): Editor登録と図鑑UIの絞り込みを追加する`）

## 非対象
- ゲーム出題／ピッチシフト本体
- Collection 混在
- Unlock 条件変更
- UI Toolkit Editor
- 仮想化リスト

---

**Part 1 承認**: この計画で Part 2（コード生成）を実行してよいですか？  
問題なければ **OK** / **Continue** と返信してください。
