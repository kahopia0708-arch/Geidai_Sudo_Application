# U7b — Business Rules

**ユニット**: U7b  
**作成**: 2026-08-29  
**回答**: Q1〜Q6 = A

| ID | 規則 |
|---|---|
| BR-U7B-01 | Editor は **一覧＋選択編集**。新規は空フォーム「追加」（Q1=A） |
| BR-U7B-02 | 保存前に `CuratedSoundValidation.ValidateForUpsert`（必須・重複・タグ存在）。失敗時は SO を Dirty にしない |
| BR-U7B-03 | 既存 id を選択中に保存 → 置換（replaceId）。新規 id → 追加 |
| BR-U7B-04 | WAV は `Assets/Audio/Library/{id}.{ext}` へコピー／移動後に AudioClip 参照（Q2=A）。id 変更時は再インポート方針を Editor で明示（同名衝突は上書き確認） |
| BR-U7B-05 | 音色タグ削除は `CanRemoveTag`（U7a）。参照ありは削除不可＋理由表示 |
| BR-U7B-06 | タグ追加／更新は `ValidateTagUpsert`。空 id／表示名不可 |
| BR-U7B-07 | プレイヤー一覧は `LibraryQuery.SortAndFilter` 後に Unlock 投影 |
| BR-U7B-08 | カテゴリ選択肢 = ValidItems のユニーク category（ソート）＋先頭「すべて」（Q4=A） |
| BR-U7B-09 | 音色選択肢 = TimbreTagCatalog.ValidTags ＋先頭「すべて」 |
| BR-U7B-10 | 行選択で詳細パネルに説明等を表示。未選択時は詳細空または案内文言（Q3=A） |
| BR-U7B-11 | `imageRef` が null のとき共有 placeholder Sprite を表示（枠は維持）（Q5=A） |
| BR-U7B-12 | ロック中は再生ボタン **非活性**＋ロック表示。押下不可（Q6=A）。サーバ通信なし |
| BR-U7B-13 | Collection（ユーザー録音）は同一画面に出さない |
| BR-U7B-14 | 画面は `HomeUiTheme`（背景・本文・ラベル）を適用。ホーム基調と揃える |
| BR-U7B-15 | ランタイムはカタログ書込なし（Editor のみ SaveAssets） |
| BR-U7B-16 | ログに説明全文・個人情報を出さない（既存 SafeLogger） |
