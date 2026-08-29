# U7a — NFR Design Patterns

**作成**: 2026-08-29  
**回答**: Q1=A, Q2=A  
**Infrastructure**: SKIP（N/A — オフライン・キュー／キャッシュ基盤なし）

## P1. 純粋検証・クエリ（Common）
`LibraryQuery` と `CuratedSoundValidation`（静的）を `Geidai.Common.Library` に置く。UI／Editor／Services はこれを呼び、重複ロジックを持たない。

## P2. 寛容な ValidItems
不正・不完全定義は一覧から除外。カタログ全体は Fail にしない（Q2=A）。ContentService 未注入のみ NotFound。

## P3. 制御語彙 SO
音色は `TimbreTagCatalog`。参照中削除不可。語彙 CRUD は Editor（U7b）。

## P4. 既存プログレッション維持
UnlockEvaluator／AtomicFile／ProgressionService は変更最小。投影フィールドのみ拡張。

## P5. セキュリティ／プライバシー
端末内のみ。属性に PII なし。ログに表示名・説明全文を出さない。

## P6. テスト容易性
Query／Validation／CanRemove は UI 非依存のため EditMode＋軽量 PBT で担保。
