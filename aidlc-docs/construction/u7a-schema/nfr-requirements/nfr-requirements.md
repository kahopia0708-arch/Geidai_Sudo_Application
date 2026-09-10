# U7a — NFR Requirements

**ユニット**: U7a Schema & Catalog API  
**作成**: 2026-08-29  
**回答**: Q1〜Q3 = 全 A  
**Infrastructure Design**: SKIP（オフライン）

| ID | 内容 |
|---|---|
| NFR-U7A-01 | カタログ 100 件以下で `LibraryQuery` Sort+Filter は EditMode 計測 16ms 未満を目安 |
| NFR-U7A-02 | 完全オフライン。端末外送信なし |
| NFR-U7A-03 | 属性・カタログに PII を置かない。ログに表示名／説明全文を出さない |
| NFR-U7A-04 | UnlockState は既存 AtomicFile＋破損時空フォールバックを維持（本ユニットで変更しない） |
| NFR-U7A-05 | IsValid／重複／CanRemove／LibraryQuery を EditMode で検証。Query は決定的＋軽量 PBT |
| NFR-U7A-06 | 既存 UnlockEvaluator テストは新必須フィールド付き Def ヘルパで追随（回帰ゼロ） |
| NFR-U7A-07 | `Geidai.Common.Library` の Query／語彙ロジックは UI／Editor 非依存 |
| NFR-U7A-08 | サンプル再登録後も Create が `GetCuratedCatalog` で読める（コンパイル・実行互換） |

## Extension
| Extension | 適用 |
|---|---|
| Security | NFR-U7A-02/03 |
| Resiliency | NFR-U7A-04（既存） |
| PBT | NFR-U7A-05 |
