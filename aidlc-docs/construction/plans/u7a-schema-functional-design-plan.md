# U7a Functional Design Plan — Schema & Catalog API

**ユニット**: U7a  
**作成**: 2026-08-29

## チェックリスト
- [x] 質問回答
- [x] domain-entities / business-rules / business-logic-model 生成
- [ ] 承認ゲート

---

## Question 1 — 音色タグ削除時（参照あり）

A) **推奨**: 参照中の音があるタグは削除不可（エラー表示）

B) 削除時に参照音の `timbreTagId` を「other」相当の既定タグへ付け替え

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 2 — `basePitchMidi` 未設定の音

A) **推奨**: カタログには残す。ピッチ系ゲームは後続で除外。図鑑・Create では利用可

B) 必須にする（Editor 保存時に未入力不可）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 3 — 強弱帯・長さ帯の値

A) **推奨**: Soft/Mid/Loud と Short/Mid/Long の固定列挙（未設定 None 可）

B) 数値（dB 相当・秒）で持つ

C) 本ユニットではフィールドのみ（値の意味は後続）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 4 — サンプル再登録

A) **推奨**: 既存ベル／ドラム等を新スキーマ＋既定 TimbreTag（bell/drum/…）で差し替え同梱

B) 空カタログ＋語彙だけ同梱（音はコンテンツ担当が Editor で追加）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

記入後 **done** と送ってください。
