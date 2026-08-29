# U7a — Business Rules

**ユニット**: U7a  
**作成**: 2026-08-29

| ID | 規則 |
|---|---|
| BR-U7A-01 | 必須フィールド欠落の定義は `ValidItems` から除外する |
| BR-U7A-02 | `id` はカタログ内一意。Upsert 時に衝突すれば拒否 |
| BR-U7A-03 | `encyclopediaNumber` は ≥1 かつカタログ内一意。衝突すれば拒否 |
| BR-U7A-04 | `timbreTagId` は `TimbreTagCatalog` に存在しなければならない |
| BR-U7A-05 | 音色タグ削除は、いずれかの音が参照している場合 **不可**（Q1=A） |
| BR-U7A-06 | 音色タグ id はカタログ内一意。空・空白不可 |
| BR-U7A-07 | `basePitchMidi` 未設定（-1）は許可。図鑑・Create 利用可。ピッチ系ゲーム除外は後続（Q2=A） |
| BR-U7A-08 | `allowPitchShift` 既定 true |
| BR-U7A-09 | `LibraryQuery` 既定ソートは encyclopediaNumber 昇順。同番号は id 昇順で安定化 |
| BR-U7A-10 | フィルタ category / timbreTagId が null または空文字のときは「すべて」 |
| BR-U7A-11 | UnlockEvaluator.Project は新フィールドを投影に載せるが、解除ロジックは変更しない |
| BR-U7A-12 | サンプルカタログは新スキーマ＋既定タグで再登録する（Q4=A） |
| BR-U7A-13 | ランタイムはカタログ読取のみ。書込は Editor（U7b） |
| BR-U7A-14 | PII を属性に置かない。ログに表示名・説明全文を出さない（既存 SafeLogger 方針） |
