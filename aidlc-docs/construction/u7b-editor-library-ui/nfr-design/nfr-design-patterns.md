# U7b — NFR Design Patterns

**作成**: 2026-08-29  
**回答**: Q1〜Q3 = 全 A  
**Infrastructure**: SKIP（N/A — オフライン・キュー／キャッシュ基盤なし）

## P1. U7a 検証・クエリの再利用
Editor／プレイヤー UI は `CuratedSoundValidation` / `LibraryQuery` / `CanRemoveTag` のみを使い、独自の重複・必須判定を持たない。

## P2. フィルタ選択肢の純粋 API
`LibraryFilterOptions`（Common.Library）がカテゴリ／音色の「すべて＋選択肢」を生成（Q1=A）。Screen と EditMode が共有。

## P3. Editor UI と Ops の分離
`CuratedSoundCatalogEditorWindow` = IMGUI。`CuratedSoundCatalogEditorOps` = WAV 配置・Import・SO Dirty/Save（Q2=A）。失敗時は非 Dirty＋メッセージ（NFR-U7B-05）。

## P4. 寛容な一覧＋選択維持
不正定義は ValidItems で除外。フィルタ後は `selectedId` が結果に残れば詳細維持、無ければ解除（Q3=A）。

## P5. 性能（シンプル再構築）
100 件以下想定。フィルタ変更で SortAndFilter → List 再 Bind。仮想化なし。

## P6. セキュリティ／プライバシー
端末内のみ。ログに説明全文・PII なし。ランタイム書込なし。

## P7. テーマ一貫性
プレイヤー画面は `HomeUiTheme` + `UiFontResolver`。Editor は標準 Editor スキン（ランタイムテーマ非適用）。

## P8. テスト容易性
FilterOptions / placeholder 解決 / Ops のうち純粋部分を EditMode。Window 本体は手動。
