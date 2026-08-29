# U7a Code Generation Plan — Schema & Catalog API

**ユニット**: U7a  
**ブランチ**: `feature/sound-library-attributes`  
**作成**: 2026-08-29  
**Stories**: US-LIB-05（主）、US-LIB-01 投影準備、Unlock テスト追随

## コンテキスト
- 変更先: `Assets/Scripts/Common/Library/*`, `Assets/Scripts/Services/Content/*`, `Assets/Scripts/Tests/EditMode/*`, `Assets/Settings/*`
- Editor / Library UI は **U7b**（本計画に含めない）
- Infrastructure: SKIP

## 実行ステップ

- [ ] **Step 0** — 既存 `CuratedSoundDefinition` / Catalog / Unlock テスト／サンプル SO を確認
- [ ] **Step 1** — `LoudnessBand` / `DurationBand` 列挙を追加
- [ ] **Step 2** — `TimbreTagDefinition` / `TimbreTagCatalog`（SO・CreateAssetMenu）を追加
- [ ] **Step 3** — `CuratedSoundDefinition` を新スキーマへ拡張（`UnsetPitchMidi = -1`）。`IsValid` 更新
- [ ] **Step 4** — `CuratedSoundValidation`（純粋）: 重複 id/number、タグ存在、`CanRemoveTag`
- [ ] **Step 5** — `CuratedSoundCatalog` に `ContainsId` / `ContainsEncyclopediaNumber` / Upsert 用ヘルパ（必要なら）
- [ ] **Step 6** — `LibraryQuery`（SortByEncyclopediaNumber / Filter）を追加
- [ ] **Step 7** — `LibraryItemView` を拡張（number / timbre / image）
- [ ] **Step 8** — `UnlockEvaluator.Project` が新フィールドを投影（解除ロジック不変）
- [ ] **Step 9** — `IContentService` / `ContentService` に TimbreTagCatalog Get/Set
- [ ] **Step 10** — 既定 `TimbreTagCatalog_Default.asset` 生成（bell/drum/environment/voice/other）
- [ ] **Step 11** — `CuratedSoundCatalog_Default` を新スキーマで再登録（既存ベル／ドラム等）
- [ ] **Step 12** — EditMode: Validation / LibraryQuery（＋軽量 PBT）/ CanRemove / Unlock ヘルパ更新
- [ ] **Step 13** — Unity コンパイル確認（MCP 可）。`code-summary.md` 作成
- [ ] **Step 14** — 本ユニット完了後にコミット（例: `feat(library): サウンド属性スキーマとカタログ API を追加する`）

## 非対象（U7b）
Editor Window、Library 画面の絞り込み UI、HomeUiTheme 適用

---

**Part 1 承認**: この計画で Part 2（コード生成）を実行してよいですか？  
問題なければ **OK** / **Continue** と返信してください。
