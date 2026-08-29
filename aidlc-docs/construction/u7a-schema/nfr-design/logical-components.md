# U7a — Logical Components

**作成**: 2026-08-29

| 論理コンポーネント | 実装候補 | 役割 |
|---|---|---|
| Sound Definition | `CuratedSoundDefinition` | 新スキーマ1音 |
| Timbre Vocabulary | `TimbreTagDefinition` / `TimbreTagCatalog` | 音色語彙 |
| Sound Catalog | `CuratedSoundCatalog` | 一覧・Find・ValidItems |
| Validation | `CuratedSoundValidation` | IsValid／重複／タグ参照／CanRemoveTag |
| Query | `LibraryQuery` | Sort／Filter |
| Projection | `LibraryItemView` | 一覧行 DTO |
| Content Facade | `IContentService` | Get/Set Curated＋Timbre |
| Unlock (既存) | `UnlockEvaluator` / `UnlockState` | 変更最小 |

## 依存

```text
Validation / Query  (純粋)
        ▲
Catalog / TimbreTagCatalog (SO)
        ▲
IContentService
        ▲
Library UI / Create / Progression  (読取)
```

## N/A
Circuit breaker, queue, cache cluster, cloud failover — 本アプリ非該当。
