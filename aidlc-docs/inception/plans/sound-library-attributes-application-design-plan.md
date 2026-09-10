# Application Design Plan — サウンドライブラリ属性

**作成**: 2026-08-29  
**ブランチ**: `feature/sound-library-attributes`  
**前提**: Requirements / Stories / Execution Plan 承認済み

## 計画チェックリスト

- [x] 設計方針の質問に回答を得る
- [x] `application-design/sound-library-attributes/` に components / methods / services / dependency を生成
- [x] Units 計画の回答と合わせて unit-of-work 一式を生成
- [ ] 設計完了ゲートで承認

---

## Question 1 — 音色タグの型

A) **固定列挙**（例: Bell / Drum / Environment / Voice / Other）。Editor はドロップダウン。後から列挙追加はコード更新

B) **文字列タグリスト**（複数可）。自由度高。表記ゆれは運用で管理

C) **列挙＋任意追加タグ**（列挙を主、自由タグは補助）

X) Other (please describe after [Answer]: tag below)

[Answer]: AだがEditorから属性追加、変更、削除も可能とする

---

## Question 2 — 画像アセットの置き場

A) **推奨**: `Assets/Art/Library/Icons/` に PNG。Editor 登録時に任意で指定／後から差し替え

B) 音ファイルと同じフォルダに並べる（音ごとサブフォルダ）

C) 画像は後回し（常にプレースホルダー）。フィールドのみ用意

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 3 — 新スキーマ置換の扱い（Q5=B の実装形）

A) **推奨**: 同一型 `CuratedSoundDefinition` をフィールド拡張し、旧カタログは Editor で再入力。最低限のサンプルを新フィールド付きで同梱し直す

B) 型名を変える（例: `CuratedSoundEntry`）し、旧型は削除。Create/テストを一括更新

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 4 — ライブラリ画面の絞り込み UI

A) **推奨**: カテゴリ Dropdown ＋ 音色タグ Dropdown（「すべて」あり）

B) カテゴリのみ（音色は詳細表示のみ）

C) チップ／トグルの複数選択（実装コスト高）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Units 関連（同一ファイルで回答）

## Question 5 — ユニット分割

A) **推奨（実行計画どおり）**: U7a（スキーマ＋Catalog API＋テスト）→ U7b（Editor＋Library UI）

B) **単一ユニット**（スキーマ〜UI を一気に）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

記入後、チャットで **done** と送ってください。
