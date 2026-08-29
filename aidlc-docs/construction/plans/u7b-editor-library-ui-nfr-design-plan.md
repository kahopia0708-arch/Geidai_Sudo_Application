# U7b NFR Design Plan — Editor & Library UI

**ユニット**: U7b  
**作成**: 2026-08-29  
**注**: クラウド／キュー等は N/A。Infrastructure Design SKIP。U7a パターン（純粋 Validation／Query）を Editor・UI から呼ぶ。

## チェックリスト
- [ ] 質問回答
- [ ] nfr-design-patterns / logical-components 生成
- [ ] 承認ゲート

---

## Question 1 — フィルタ選択肢ヘルパの配置

A) **推奨**: `LibraryFilterOptions`（または同等）を Common.Library の純粋静的 API に置き、カテゴリ／音色の「すべて＋一覧」生成を集約。Screen／テストが共有

B) `LibraryScreenController` 内の private メソッドのみ（共通化しない）

X) Other (please describe after [Answer]: tag below)

[Answer]: 

---

## Question 2 — Editor のアセット操作境界

A) **推奨**: `CuratedSoundCatalogEditorWindow` は UI。WAV コピー／Import／SaveAssets は `CuratedSoundCatalogEditorOps`（Editor 静的ヘルパ）に分離し、Validation は U7a API のみ呼ぶ

B) すべてを EditorWindow 1 クラスに集約

X) Other (please describe after [Answer]: tag below)

[Answer]: 

---

## Question 3 — フィルタ再適用時の選択維持

A) **推奨**: フィルタ後も `selectedId` が結果に残れば詳細維持。無ければ選択解除（詳細クリア）

B) フィルタ変更のたびに常に選択解除

X) Other (please describe after [Answer]: tag below)

[Answer]: 

---

記入後 **done** と送ってください。
