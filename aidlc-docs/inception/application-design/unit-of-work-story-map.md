# Unit of Work Story Map（ストーリー→ユニット割当）

**プロジェクト**: 藝大 音響教育アプリ
**作成**: 2026-07-15 / AI-DLC Units Generation（Part 2）
**更新**: 2026-07-30 / フェーズC 割当追加
**入力**: `../user-stories/stories.md`、`unit-of-work.md`

---

## 1. ストーリー割当表

| ユニット | フェーズ | ストーリー | 対応要件 |
|---|:---:|---|---|
| U1 基盤 | A/C | US-TECH-01, 02, 04, 05, 07, 09（共通IF） | NFR-05/10/11/12/15/17, §7 |
| U2 Foundation | A | US-NAV-01, 02, US-REG-01, 02 | FR-01〜04 |
| U3 Rec | A | US-REC-01〜03, US-TECH-03 | FR-05〜08 |
| U4 Persistence/Collection | A/C | US-COL-01〜04, US-TECH-06 | FR-09〜12, NFR-07/14 |
| U5 weekly theme | B | US-THEME-01〜03 | FR-13/14 |
| U6 Game①音合わせ | B | US-GAME1-01〜05 | FR-15〜19 |
| U7 Sound Library | C | US-LIB-01〜03 | FR-20〜24, NFR-13/14 |
| U8 Sound Create | C | US-CREATE-01〜04 | FR-25〜29, NFR-06/07/14 |
| 横断（Build） | C | US-TECH-08 | NFR-16/17 |
| 将来 | 将来 | FUT-01〜05 | §4 スコープ外 |

## 2. 網羅性チェック

- **NAV / REG / REC / COL / THEME / GAME1**: 既存どおり U2〜U6 ✔
- **LIBRARY**: US-LIB-01〜03 → U7 ✔
- **CREATE**: US-CREATE-01〜04 → U8 ✔
- **TECH**: 01/02/04/05/07/09 → U1、03 → U3、06 → U4、08 → Build横断 ✔
- **FUTURE**: FUT-01〜05 → 将来 ✔

**結果**: フェーズA/B/C の US-* はすべて割当済み。未割当なし。

## 3. フェーズ別サマリ
- **A**: U1〜U4（基盤〜保存）— 実装済み
- **B**: U5〜U6（お題・①音合わせ）— 実装済み
- **C**: U7〜U8＋共通IF拡張＋展示試用ビルド — 計画中
- **将来**: FUT-01〜05

## 4. 補足
- アンロック進行は U1 ProgressionService に集約し、U3/U6 はイベント通知のみ。
- UI詳細調整は各ユニット完了時に企画・デザインへハンドオフ（US-TECH-07）。
- 追加ミニゲームは将来の縦割りユニットとして FUT-01 から展開する。
