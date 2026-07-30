# U7 Sound Library — Code Summary

**作成**: 2026-07-30  
**ブランチ**: `feature/sound-library-planning`  
**検証**: Unity MCP — コンパイル Error 0、UnlockEvaluator スモーク PASS、`Geidai.Library` ロード確認

## 1. 生成物

### Common (`Geidai.Common.Library`)
- `CuratedSoundDefinition` / `CuratedSoundCatalog`(SO)
- `UnlockConditionKind` / `UnlockRule` / `UnlockRulesCatalog`(SO)
- `UnlockState`（unlockedIds + achievedGame/RecordingKeys）
- `ProgressionEvent` / `LibraryItemView`
- `UnlockEvaluator`（純粋・冪等・Combined requireAll）

### Services
- `IContentService` / `ContentService`：カタログ／解除表 Get/Set
- `IStorageService` / `StorageService`：`Load/SaveUnlockState`（AtomicFile）
- `IProgressionService` / `ProgressionService`：イベント→解除→永続
- `IAudioService.PlayCuratedClip`
- `SceneId.Library` + Navigation マップ `GeidaiLibrary`
- `AppManager`：ProgressionService 登録

### UI (`Geidai.Library`)
- `LibraryBootstrap` / `LibraryState`
- `LibraryScreenController` / `CuratedSoundListView` / `CuratedSoundItemView`

### データ
- `Assets/Settings/CuratedSoundCatalog_Default.asset`（サンプル2音）
- `Assets/Settings/UnlockRulesCatalog_Default.asset`

### テスト
- `UnlockEvaluatorTests` / `UnlockStateJsonTests`

## 2. 依存
`Geidai.Library` → `Geidai.Services` → `Geidai.Common`（Rec/Collection 非依存）

## 3. フォローアップ（MCP）
- `GeidaiLibrary` シーン作成・Home 導線
- 本番カタログ 50〜100 音投入
- Rec/Game1 から `IProgressionService` 通知配線
- 展示ビルドサイズ計測（NFR-13）

## 4. Extension
- Security: 端末内・PII非ログ・共有なし
- Resiliency: AtomicFile・破損時空フォールバック
- PBT: Unlock 冪等・JSON 往復
