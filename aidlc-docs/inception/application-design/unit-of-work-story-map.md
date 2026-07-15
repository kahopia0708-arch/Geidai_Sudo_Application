# Unit of Work Story Map（ストーリー→ユニット割当）

**プロジェクト**: 藝大 須藤さんアプリ
**作成**: 2026-07-15 / AI-DLC Units Generation（Part 2）
**入力**: `../user-stories/stories.md`、`unit-of-work.md`

---

## 1. ストーリー割当表

| ユニット | フェーズ | ストーリー | 対応要件 |
|---|:---:|---|---|
| U1 基盤 | A | US-TECH-01, US-TECH-02, US-TECH-04, US-TECH-05, US-TECH-07 | FR-02, NFR-05/10/11/12, §7 UI開発フロー |
| U2 Foundation | A | US-NAV-01, US-NAV-02, US-REG-01, US-REG-02 | FR-01/02/03/04, SEC-05 |
| U3 Rec | A | US-REC-01, US-REC-02, US-REC-03, US-TECH-03 | FR-05/06/07/08, NFR-03/06/08 |
| U4 Persistence/Collection | A | US-COL-01, US-COL-02, US-COL-03, US-COL-04, US-TECH-06 | FR-09/10/11/12, NFR-07 |
| U5 weekly theme | B | US-THEME-01, US-THEME-02, US-THEME-03 | FR-13/14 |
| U6 Game①音合わせ | B | US-GAME1-01, US-GAME1-02, US-GAME1-03, US-GAME1-04, US-GAME1-05 | FR-15〜19, NFR-03/06 |
| 将来（未着手） | 将来 | FUT-01, FUT-02, FUT-03, FUT-04 | §4 スコープ外 |

## 2. 網羅性チェック（全ストーリーが割当済み）

- **NAV**: US-NAV-01→U2, US-NAV-02→U2 ✔
- **REG**: US-REG-01→U2, US-REG-02→U2 ✔
- **REC**: US-REC-01→U3, US-REC-02→U3, US-REC-03→U3 ✔
- **COL**: US-COL-01→U4, US-COL-02→U4, US-COL-03→U4, US-COL-04→U4 ✔
- **THEME**: US-THEME-01→U5, US-THEME-02→U5, US-THEME-03→U5 ✔
- **GAME1**: US-GAME1-01→U6, 02→U6, 03→U6, 04→U6, 05→U6 ✔
- **TECH**: US-TECH-01→U1, 02→U1, 03→U3, 04→U1, 05→U1, 06→U4, 07→U1 ✔
- **FUTURE**: FUT-01〜04→将来（MVP外） ✔

**結果**: MVP 対象の全ストーリー（US-*＝24）が U1〜U6 に割当済み。将来スタブ（FUT-*）は将来ユニット。**未割当なし**。

## 3. フェーズ別サマリ
- **フェーズA（基盤〜保存）**: U1, U2, U3, U4 — 中間報告（2026-11）に向けた主軸。
- **フェーズB（最初のゲーム）**: U5, U6 — お題連携と①音合わせ。
- **将来**: FUT-01〜04。

## 4. 補足
- 技術イネーブラー（US-TECH-*）は主に U1 に集約し、録音一本化（US-TECH-03）は Rec 実装と不可分のため U3、データ堅牢性（US-TECH-06）は永続化本実装の U4 に配置。
- 各ユニット完了時に UI 詳細調整を Sさん へハンドオフ（US-TECH-07 は U1 で仕組みを用意し、全ユニットで運用）。
