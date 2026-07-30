# U7 Sound Library — Logical Components

**作成**: 2026-07-30

| # | コンポーネント | 配置 | 責務 |
|---|---|---|---|
| 1 | CuratedSoundDefinition / Id | Common | 素材定義 |
| 2 | UnlockRule / UnlockState / ProgressionEvent | Common | 進行モデル |
| 3 | UnlockEvaluator | Common | 純粋解除判定 |
| 4 | CuratedSoundCatalog / UnlockRulesCatalog | Common or Settings SO | データ |
| 5 | IContentService 拡張 | Services | カタログ取得 |
| 6 | IProgressionService / ProgressionService | Services | イベント→解除 |
| 7 | IStorageService 拡張 | Services | UnlockState I/O |
| 8 | LibraryScreenController 等 | Geidai.Library | UI |
| 9 | LibraryBootstrap | Geidai.Library | 配線 |

**依存**: Library → Services → Common  
**テスト**: UnlockEvaluator PBT、UnlockState JSON PBT、Library スモーク
