# U7a NFR Requirements Plan

**ユニット**: U7a Schema & Catalog API  
**作成**: 2026-08-29  
**前提**: U7 NFR・本ワークストリーム Extensions（Security/Resiliency/PBT）継続

## チェックリスト
- [x] 質問回答
- [x] nfr-requirements / tech-stack-decisions 生成
- [ ] 承認ゲート

---

## Question 1 — 性能目標（Catalog / Query）

A) **推奨**: 100 件以下で Sort+Filter は EditMode で 16ms 未満を目安。ランタイム一覧は U7b で体感即時

B) 性能数値は定めず、正しい結果のみ保証

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 2 — テスト範囲（U7a）

A) **推奨**: EditMode — IsValid／重複／LibraryQuery（決定的＋軽量 PBT）／TimbreTag CanRemove／既存 Unlock テストを新 Def に追随

B) 単体テスト最小（IsValid＋Query のみ）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 3 — Tech stack

A) **推奨**: 既存踏襲 — ScriptableObject カタログ、Common.Library 純粋ロジック、FsCheck/NUnit、UnityEngine.UI 非依存の Query

B) 別ライブラリ導入（回答に記載）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

記入後 **done** と送ってください。
