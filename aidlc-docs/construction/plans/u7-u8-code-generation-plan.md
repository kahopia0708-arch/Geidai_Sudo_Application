# U7 Sound Library / U8 Sound Create — Code Generation Plan（Part 1）

**プロジェクト**: 藝大 須藤さんアプリ  
**ユニット**: U7 Sound Library → U8 Sound Create（共通IFを先に）  
**作成**: 2026-07-30 / AI-DLC CONSTRUCTION / Code Generation（Part 1: Planning）  
**ブランチ**: `feature/sound-library-planning`  
**入力**: `../u7-library/*`, `../u8-create/*`, Application Design（services/components）, Requirements FR-20〜29  
**検証**: 公式 Unity MCP（`user-unity-mcp`）でコンパイル／純ロジックスモーク（US-TECH-05）

> 本計画は Part 2（生成）の**唯一の正**。各ステップ完了時に `[x]` を付ける。  
> **承認方法**: 問題なければ「**Continue**」（または「OK」／「done」）。修正点があれば指摘してください。

---

## 0. 方針・前提（確定設計の反映）

- **一方向依存**:
  - `Geidai.Library` → `Geidai.Services` → `Geidai.Common`（＋`UnityEngine.UI`）
  - `Geidai.Create` → `Geidai.Services` → `Geidai.Common`（＋`UnityEngine.UI`）
  - **Library / Create は `Geidai.Rec` / `Geidai.Collection` に依存しない**
  - Rec / Game1 は後続で `IProgressionService` を呼ぶだけ（本計画ではフック用の薄い呼び出し点を任意で追加可。必須は IF＋実装）
- **通貨・XP・ライフなし**（BR-UNLOCK-05）
- **同梱音は読み取り専用**。ユーザー領域へ複製しない（BR-LIB-02 / BR-CREATE-03）
- **解除条件は SO データ駆動**（コード埋め込み禁止 / BR-UNLOCK-01）
- **永続化は AtomicFile**（UnlockState / Recipe / Export）
- **既存シグネチャは後方互換拡張のみ**（末尾追加）
- **`.meta` は Unity 生成**。シーン配線は MCP フォローアップ（本計画の必須完了条件外）
- **UI は uGUI 枠組み**。意匠は企画・デザイン担当ハンドオフ

### 生成先フォルダ構成（新規／追記）

```
Assets/Scripts/
├── Common/
│   ├── Models/SceneId.cs                    (修正: Library, Create を末尾追加)
│   ├── Library/                             ★新規（Geidai.Common.Library）
│   │   ├── CuratedSoundDefinition.cs
│   │   ├── CuratedSoundCatalog.cs           (SO)
│   │   ├── UnlockConditionKind.cs
│   │   ├── UnlockRule.cs
│   │   ├── UnlockRulesCatalog.cs            (SO)
│   │   ├── UnlockState.cs
│   │   ├── ProgressionEvent.cs / ProgressionEventType.cs
│   │   ├── UnlockEvaluator.cs               (静的純粋・PBT)
│   │   └── LibraryItemView.cs               (表示投影・struct/POCO)
│   └── Create/                              ★新規（Geidai.Common.Create）
│       ├── SoundRecipeLayer.cs
│       ├── SoundRecipe.cs
│       ├── RecipeClamp.cs                   (定数: volume 0..1, pitch -12..12, reverb 0..1)
│       ├── RecipeValidator.cs               (静的純粋・クランプ・PBT)
│       └── TimbreKind.cs                    (none/robot/chorus)
├── Services/
│   ├── Content/IContentService.cs           (修正: カタログ／解除表 Get/Set)
│   ├── Content/ContentService.cs            (修正: 実装)
│   ├── Storage/IStorageService.cs           (修正: UnlockState / Recipe CRUD / ExportPath)
│   ├── Storage/StorageService.cs            (修正: AtomicFile 実装)
│   ├── Audio/IAudioService.cs               (修正: PlayCuratedClip / PlayLayers / RenderRecipeToWav)
│   ├── Audio/AudioService.cs                (修正: 実装)
│   ├── Progression/                         ★新規
│   │   ├── IProgressionService.cs
│   │   └── ProgressionService.cs
│   └── AppManager.cs                        (修正: ProgressionService 登録)
├── Library/                                 ★新規 asmdef Geidai.Library
│   ├── Geidai.Library.asmdef
│   ├── LibraryBootstrap.cs
│   ├── LibraryState.cs
│   ├── LibraryScreenController.cs
│   ├── CuratedSoundListView.cs
│   └── CuratedSoundItemView.cs
├── Create/                                  ★新規 asmdef Geidai.Create
│   ├── Geidai.Create.asmdef
│   ├── CreateBootstrap.cs
│   ├── CreateState.cs
│   ├── CreateScreenController.cs
│   ├── RecipeLayerPicker.cs
│   ├── RecipeEffectPanel.cs
│   ├── RecipeListController.cs
│   └── RecipeExportController.cs
└── Tests/EditMode/
    ├── Geidai.Tests.asmdef                  (修正: Library/Create 参照追加は任意・純ロジックは Common のみ可)
    ├── UnlockEvaluatorTests.cs
    ├── UnlockStateJsonTests.cs
    ├── RecipeValidatorTests.cs
    └── SoundRecipeJsonTests.cs

Assets/Settings/
├── CuratedSoundCatalog_Default.asset        (MCP: 少数サンプル clip 付き／実素材は後続)
└── UnlockRulesCatalog_Default.asset         (MCP: サンプルルール)
```

---

## 1. 対象ストーリー

| Story | 実装の主担当 |
|---|---|
| US-LIB-01 音図鑑一覧（ロック投影） | LibraryScreen + UnlockState + Catalog |
| US-LIB-02 解除済み試聴 | Library + IAudioService.PlayCuratedClip |
| US-LIB-03 進行による解除 | ProgressionService + UnlockEvaluator |
| US-CREATE-01 解除音から2音選択 | Create + UnlockState |
| US-CREATE-02 パラメータ調整・プレビュー | RecipeEffectPanel + PlayLayers |
| US-CREATE-03 レシピ保存 | SaveRecipe（原子的） |
| US-CREATE-04 任意 WAVE 書き出し | RenderRecipeToWav + 原子的 Export |
| US-TECH-08 共同実装境界（asmdef） | Geidai.Library / Geidai.Create |
| US-TECH-09 展示ビルドサイズ計測フック | code-summary に計測手順記載（実装はログ/README） |

---

## 2. 依存・IF 契約（追加分）

### IContentService（後方互換追加）
- `void SetCuratedCatalog(CuratedSoundCatalog catalog)`
- `Result<CuratedSoundCatalog> GetCuratedCatalog()`
- `void SetUnlockRules(UnlockRulesCatalog rules)`
- `Result<UnlockRulesCatalog> GetUnlockRules()`

### IStorageService（後方互換追加）
- `Result<UnlockState> LoadUnlockState()` / `Result SaveUnlockState(UnlockState state)`
- `Result SaveRecipe(SoundRecipe recipe)` / `Result DeleteRecipe(string id)` / `Result<List<SoundRecipe>> ListRecipes()` / `Result<SoundRecipe> LoadRecipe(string id)`
- `Result SaveRecipeExport(string id, byte[] wavBytes)`（`exports/{id}.wav`・原子的・失敗時不完全ファイル残さない）

### IAudioService（後方互換追加）
- `Result PlayCuratedClip(AudioClip clip)`（同梱試聴・エフェクト中立）
- `Result PlayLayers(AudioClip a, SoundRecipeLayer la, AudioClip b, SoundRecipeLayer lb)`（最大2・再生時加工・非破壊）
- `Result<byte[]> RenderRecipeToWav(AudioClip a, SoundRecipeLayer la, AudioClip b, SoundRecipeLayer lb)`（明示書き出し用 PCM）

### IProgressionService（新規）
- `Result NotifyGameCleared(string gameKey)`
- `Result NotifyRecordingChallenge(string challengeKey)`
- `Result ApplyInitialUnlocks()`（`initiallyUnlocked` を起動時反映）
- `UnlockState CurrentUnlockState { get; }`（キャッシュ可）

パス:
- UnlockState: `persistentDataPath/progression/unlock-state.json`
- Recipes: `persistentDataPath/recipes/{id}.json`
- Exports: `persistentDataPath/exports/{id}.wav`

---

## 実行ステップ（Part 2）

### A. 共通基盤

- [ ] **Step0** MCP 接続確認・コンソール ベースライン（Error/Warning）
- [ ] **Step1** `SceneId` に `Library`, `Create` を末尾追加。Navigation マップは既存パターンに合わせて定数／辞書側を追記（未登録シーンはフォローアップで可）
- [ ] **Step2** Common.Library モデル一式（Definition / Unlock* / ProgressionEvent / LibraryItemView）
- [ ] **Step3** `CuratedSoundCatalog` / `UnlockRulesCatalog`（SO・`CreateAssetMenu`）
- [ ] **Step4** `UnlockEvaluator`（静的純粋: `Apply(state, rules, catalog, event)`／`Project(items, state)`／冪等）
- [ ] **Step5** Common.Create モデル一式（TimbreKind / Layer / Recipe / RecipeClamp / RecipeValidator）
- [ ] **Step6** `IContentService`／`ContentService` 拡張（カタログ・解除表）
- [ ] **Step7** `IStorageService`／`StorageService` 拡張（UnlockState・Recipe・Export・AtomicFile・破損時空フォールバック）
- [ ] **Step8** `IAudioService`／`AudioService` 拡張（PlayCuratedClip / PlayLayers / RenderRecipeToWav）。PlayLayers は2ソース同時再生（2 AudioSource）＋レイヤー別 volume/pitch/reverb/timbre を簡易適用。Render はオフラインミックス→WavCodec（既存）
- [ ] **Step9** `IProgressionService`／`ProgressionService`＋`AppManager` 登録。起動時 `ApplyInitialUnlocks`

### B. U7 Library UI

- [ ] **Step10** `Geidai.Library.asmdef`（refs: Common, Services, UnityEngine.UI）
- [ ] **Step11** `LibraryBootstrap` / `LibraryState` / `LibraryScreenController` / `CuratedSoundListView` / `CuratedSoundItemView`（locked は試聴不可・unlocked のみ Play）

### C. U8 Create UI

- [ ] **Step12** `Geidai.Create.asmdef`（refs: Common, Services, UnityEngine.UI・**Library 非依存**＝UnlockState は Storage/Progression 経由）
- [ ] **Step13** `CreateBootstrap` / `CreateState` / Controllers（Picker / EffectPanel / List / Export）。選択は解除済み ID のみ。保存はレシピのみ。書き出しは ConfirmDialog 経由

### D. テスト・検証・記録

- [ ] **Step14** EditMode テスト4種（UnlockEvaluator PBT／UnlockState JSON／RecipeValidator／SoundRecipe JSON）
- [ ] **Step15** MCP: Refresh→コンパイル Error 0、UnlockEvaluator／RecipeValidator スモーク、既定カタログ／ルール asset を少数サンプルで生成（実50〜100音は後続アセット投入）
- [ ] **Step16** `u7-library/code/code-summary.md` ＋ `u8-create/code/code-summary.md`、`stories.md` 実装状況追記、progress／aidlc-state 更新、commit

---

## 3. スコープ外（フォローアップ）

- 実シーン作成・Home メニュー導線・Build Settings 登録（MCP）
- 制作側の本カタログ 50〜100 音・解除表の本番データ投入
- Rec 保存成功／Game1 クリアから ProgressionService への本番配線（薄いフックは任意で本計画内）
- 展示ビルドの実測サイズ報告（手順のみ code-summary）
- 意匠・イラスト・ロック表現の最終 UI

## 4. リスク・緩和

| リスク | 緩和 |
|---|---|
| 同時2音再生の AudioSource 管理 | AudioService 内に Create 用デュアルリグを遅延生成・Stop で解放 |
| RenderRecipe の DSP 精度 | MVP は単純ミックス＋既存 EffectChain/Pitch 近似。品質は後続 |
| カタログ clip 未割当 | 無効定義除外（BR-LIB-01）＋EmptyState |
| 大きなカタログのビルドサイズ | 初期少数サンプル＋NFR-13 計測手順をドキュメント化 |

## 5. 完了条件

- Step0〜16 全 `[x]`、コンパイル Error 0、EditMode テスト整備、code-summary 作成・commit
- 一方向依存維持・既存 U1〜U6 非破壊
- シーン未配線でもモジュール単体として成立

## 6. Extension コンプライアンス（生成時）

| Extension | 適用 |
|---|---|
| Security | 端末内のみ・PII非ログ・共有なし。ログに個人情報を出さない |
| Resiliency | AtomicFile・破損フォールバック。クラウド DR は N/A |
| PBT | Unlock 冪等・JSON 往復・クランプを EditMode で実装 |
