# U8 Sound Create — Functional Design Plan

**ユニット**: U8 Sound Create（音を作る）
**作成**: 2026-07-30
**前提**: 要件回答 B（レシピ＋任意WAVE）、U7 UnlockState 依存。承認済み方針から確定。

## スコープ
- アンロック済み2音の選択・重ね試聴
- 音量・ピッチ・リバーブ・音色の調整
- レシピ保存・再編集
- 必要時 WAVE 書き出し
- 欠損素材の安全処理

## 確定方針
| 項目 | 決定 |
|---|---|
| レイヤー数 | 2（MVP） |
| 保存 | SoundRecipe（素材ID＋パラメータ） |
| 書き出し | 任意 WAVE 16bit PCM |
| 元音複製 | しない |
| UI | CreateScreen＋ピッカー＋エフェクト＋レシピ一覧 |

## Part 2 成果物
- [x] domain-entities.md
- [x] business-logic-model.md
- [x] business-rules.md
- [x] frontend-components.md
