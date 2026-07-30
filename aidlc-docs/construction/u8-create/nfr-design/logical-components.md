# U8 Sound Create — Logical Components

**作成**: 2026-07-30

| # | コンポーネント | 配置 | 責務 |
|---|---|---|---|
| 1 | SoundRecipe / Layer | Common | レシピモデル |
| 2 | RecipeValidator | Common | 純粋検証・クランプ |
| 3 | IAudioService 拡張 | Services | PlayLayers / RenderRecipe |
| 4 | IStorageService 拡張 | Services | Recipe CRUD / Export |
| 5 | CreateScreenController 等 | Geidai.Create | UI |
| 6 | CreateBootstrap | Geidai.Create | 配線 |

**依存**: Create → Services → Common  
**テスト**: Recipe JSON PBT、Validator、2音再生スモーク
