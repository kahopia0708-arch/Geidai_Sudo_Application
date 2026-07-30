# U7 Sound Library — Functional Design Plan

**ユニット**: U7 Sound Library（音図鑑・アンロック）
**作成**: 2026-07-30 / CONSTRUCTION / Functional Design
**前提**: 要件変更回答（B/C/B/B/A/A）および Application Design 承認済み。質問は承認済み方針からの確定値。

## 1. スコープ
- 制作側カタログの閲覧・試聴（US-LIB-01）
- ゲームクリア＋録音課題の複合アンロック（US-LIB-02）
- 素材IDの共通参照（US-LIB-03）
- 経験値・通貨・ライフは使わない

## 2. 確定方針（承認済み要件から）
| Q | 決定 |
|---|---|
| カタログ形式 | ScriptableObject（CuratedSoundCatalog） |
| 解除条件 | UnlockRulesCatalog（データ駆動）＋純粋 UnlockEvaluator |
| 進行更新 | ProgressionService（冪等） |
| 永続化 | unlock-state.json（原子的） |
| 初期規模 | 50〜100音目安 |
| UI | LibraryScreen＋リスト／アイテム。見た目は企画・デザイン調整 |

## 3. Part 2 成果物
- [x] domain-entities.md
- [x] business-logic-model.md
- [x] business-rules.md
- [x] frontend-components.md
