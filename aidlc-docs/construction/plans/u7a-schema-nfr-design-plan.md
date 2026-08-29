# U7a NFR Design Plan

**ユニット**: U7a  
**作成**: 2026-08-29  
**注**: クラウド／キュー等は N/A（オフライン）。Infrastructure Design SKIP。

## チェックリスト
- [x] 質問回答
- [x] nfr-design-patterns / logical-components 生成
- [ ] 承認ゲート

---

## Question 1 — 検証・Query の配置

A) **推奨**: `LibraryQuery` / `CuratedSoundValidation`（または Catalog メソッド）を Common.Library の静的／純粋 API に集約。UI・Editor はこれを呼ぶ

B) 検証ロジックを Catalog SO インスタンスメソッドのみに置く

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 2 — 失敗時の返し方（ランタイム読取）

A) **推奨**: 既存どおり `Result` / Fail(NotFound)。不正定義は ValidItems で黙って除外（ログは開発時のみ・PIIなし）

B) 不正定義が1件でもあればカタログ全体 Fail

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

記入後 **done** と送ってください。
