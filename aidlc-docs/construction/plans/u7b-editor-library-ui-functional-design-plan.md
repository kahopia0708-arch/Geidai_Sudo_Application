# U7b Functional Design Plan — Editor & Library UI

**ユニット**: U7b  
**作成**: 2026-08-29  
**Stories**: US-LIB-04（Editor）、US-LIB-01 更新（プレイヤー画面）  
**前提**: U7a 完了（スキーマ／Validation／LibraryQuery／TimbreTagCatalog）  
**削除ポリシー**: U7a Q1=A（参照中タグは削除不可）を継承

## チェックリスト
- [x] 質問回答（Q1〜Q6 = A）
- [x] domain-entities / business-rules / business-logic-model / frontend-components 生成
- [ ] 承認ゲート

## スコープ（確認）
- Editor: WAV→Clip インポート、属性フォーム、カタログ追加／更新、音色語彙 CRUD
- Player: 図鑑ナンバー順、カテゴリ／音色フィルタ、画像プレースホルダー、試聴、HomeUiTheme
- 非対象: ゲーム出題、Collection 混在、Unlock 条件変更

---

## Question 1 — Editor ウィンドウの編集モード

カタログ登録フローの基本形は？

A) **推奨**: 一覧＋選択編集。既存音を選んで属性を直し、新規は「追加」で空フォーム

B) 新規追加専用フォームのみ（既存の修正は Inspector / 別手段）

C) ID を手入力し、既存なら上書き・無ければ追加（単一フォーム）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 2 — WAV インポート先

A) **推奨**: `Assets/Audio/Library/` に `{id}.wav`（または元拡張子）としてコピー／移動し AudioClip 化

B) `Assets/Audio/Library/{category}/` 配下に置く

C) ユーザーが指定したプロジェクト内パスをそのまま使う（規約フォルダなし）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 3 — プレイヤー画面の詳細表示

A) **推奨**: 一覧行に ナンバー・名前・画像・ロック・試聴。タップ／選択で説明を下（または横）の詳細パネルに表示

B) 一覧行に説明まで全部載せる（詳細パネルなし）

C) 行タップで別画面（Detail）へ遷移

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 4 — カテゴリ絞り込みの選択肢

A) **推奨**: カタログ内の有効音からユニークな `category` を集めてドロップダウン（先頭に「すべて」）

B) カテゴリも Timbre 同様の制御語彙 SO（別カタログ）を用意

C) 自由入力テキストフィルタのみ

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 5 — 画像未設定時のプレースホルダー

A) **推奨**: 共有 Sprite（例: `Assets/Art/Library/Icons/placeholder.png`）を画面側で差し替え表示

B) 画像枠を非表示（テキストのみ）

C) Home と同様の単色スプライト生成ユーティリティで仮画像

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 6 — ロック中の試聴 UI

A) **推奨**: 既存どおり再生ボタン非活性＋ロック表示（押下不可）

B) ボタンは押せるがエラーメッセージ（既存 `Reload` のガード文言）

C) ロック中は再生ボタン自体を隠す

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

記入後 **done** と送ってください。
