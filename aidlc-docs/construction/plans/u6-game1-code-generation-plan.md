# U6 Game①音合わせ — Code Generation Plan（Part 1: 詳細計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U6 Game①音合わせ（最終ユニット）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / Code Generation（Part 1: Planning）
**入力**: `../u6-game1/functional-design/*`, `../u6-game1/nfr-requirements/*`, `../u6-game1/nfr-design/*`（P1〜P5・logical-components）
**検証**: 公式 Unity AI Assistant（Unity MCP Server / `user-unity-mcp`）でコンパイル/スモーク（US-TECH-05）

> 本計画は Part 2（生成）で実行する手順。各ステップ完了で `[x]` に更新する。
> **承認方法**: 計画に問題なければ「**Continue**」（または「done」）。修正点があれば指摘してください。

---

## 0. 方針・前提（確定済み設計の反映）
- **一方向依存**: `Geidai.Game1（UI）→ Geidai.Services → Geidai.Common`（＋`UnityEngine.UI`）。`Geidai.Collection`/`Geidai.Rec` 非依存。Assembly-CSharp（旧ゲーム選択 UI）へは非参照（導線は Navigation 経由）。
- **データ/純粋は Common.Game**: `ChoiceSpec`・`Question`・`DifficultyLevel`・`SoundMatchConfig`(SO)・`QuestionBuilder`(純粋)・`GameSession`。
- **音声は Services.Audio**: `IPitchVariationService`＋`PitchVariationService`（再生時 pitch＝`AudioSource.pitch = PitchMath.CentsToRatio`・非保存）。既存 `AudioService` の AudioClip 生成パターン（`AudioClip.Create`＋`SetData`）を踏襲。
- **素材取得**: `IStorageService.ListSounds()`／`LoadSoundBuffer(id)`。0 件は `fallbackClip`→`Empty` フォールバック。
- **既存資産の再利用**: `Result`/`ResultCode`、`SceneId.Game1`、`INavigationService`、`ScreenRootBase`、`ErrorPresenter`、`UITheme`、`PitchMath`、`ServiceRegistry`。

---

## 1. 実装ステップ（Part 2 で実行）

### コード生成
- [x] **Step0** MCP 接続確認・コンソール ベースライン（Error/Warning 現況）
- [x] **Step1** `Assets/Scripts/Common/Game/ChoiceSpec.cs`／`Question.cs`／`DifficultyLevel.cs`（`Geidai.Common.Game`・Serializable 値オブジェクト）
- [x] **Step2** `Assets/Scripts/Common/Game/SoundMatchConfig.cs`（`ScriptableObject`・`questionCount`/`choiceCount`/`difficulties`/`fallbackClip`＋クランプアクセサ。`[CreateAssetMenu]`）
- [x] **Step3** `Assets/Scripts/Common/Game/QuestionBuilder.cs`（static 純粋：`Build(baseSoundId, config, diff, seed)`＝`System.Random(seed)` で target/不正解セント決定・正解1つ・距離条件・シャッフル・`correctIndex`。加えて `BuildQuestions(...)` で1ゲーム分）
- [x] **Step4** `Assets/Scripts/Common/Game/GameSession.cs`（`Geidai.Common.Game`・実行時 POCO：`questions`/`currentIndex`/`correctCount`/`IsFinished`・非永続）
- [x] **Step5** `Assets/Scripts/Services/Audio/IPitchVariationService.cs`＋`PitchVariationService.cs`（`Geidai.Services.Audio`：`Play(AudioBuffer, cents)`＝専用リグ[AudioSource]＋基準 AudioClip キャッシュ＋`pitch=CentsToRatio`／`Stop`／`IsPlaying`／`SetBase(AudioBuffer)`。非保存・低GC）
- [x] **Step6** `Assets/Scripts/Game1/Geidai.Game1.asmdef`（参照＝`Geidai.Common`/`Geidai.Services`/`UnityEngine.UI`・**Collection/Rec 非依存**）
- [x] **Step7** `Assets/Scripts/Game1/Game1Bootstrap.cs`（static：`IPitchVariationService` を解決/未登録なら登録。`RecBootstrap`/`ThemeBootstrap` と同パターン）
- [x] **Step8** `Assets/Scripts/Game1/SoundMatchGameController.cs`（`ScreenRootBase`：`StartGame`＝素材選択/フォールバック集約＋出題生成、`PresentCurrent`、`SubmitAnswer(index)`＝純粋判定→`ResultEffectController`、`OnBackPressed`→ホーム。状態 Loading/Empty/Playing/Judging/Result）
- [x] **Step9** `Assets/Scripts/Game1/ChoiceItemView.cs`＋`FrogTargetView.cs`（タップ確認＝`PitchVariationService.Play`／uGUI ドラッグ＋ドロップ領域判定→`OnChoiceDropped`→`SubmitAnswer`。領域外は元位置復帰）
- [x] **Step10** `Assets/Scripts/Game1/ResultEffectController.cs`（`PlayCorrect`＝カエル進化フック／`PlayRetry`／`ShowResult(correct,total)`。演出のみ・進行は Controller）

### テスト
- [x] **Step11** EditMode テスト（テストは `Geidai.Common.Game` のみ参照＝`Geidai.Tests` は既存参照で足りるため `Geidai.Game1` 追加不要）
  - `QuestionBuilderTests.cs`（PBT：正解ちょうど1つ／不正解は `centsStep` 以上／選択肢数=config／同一 seed 決定的／`BuildQuestions` 件数）
  - `SoundMatchConfigTests.cs`（クランプ：choiceCount≥2・questionCount≥1・centsStep≥1・difficulty index クランプ・空リスト既定）

### 検証・記録
- [x] **Step12** MCP 検証：`AssetDatabase.Refresh`→コンパイル Error 0 確認、`QuestionBuilder`（純粋）スモーク合格（choices=4・正解1つ・distinct・距離OK・決定的）。既定 `SoundMatchConfig.asset`（かんたん200/ふつう100/むずかしい50/とても難しい20）を `Assets/Settings` に生成
- [x] **Step13** `../u6-game1/code/code-summary.md` 生成＋`stories.md`（US-GAME1-01〜05）実装状況追記＋commit

---

## 2. 生成物一覧（想定パス）
| 種別 | パス |
|---|---|
| 型（Common.Game） | `Assets/Scripts/Common/Game/ChoiceSpec.cs`／`Question.cs`／`DifficultyLevel.cs`／`GameSession.cs` |
| データ（SO） | `Assets/Scripts/Common/Game/SoundMatchConfig.cs` |
| 純粋（Common.Game） | `Assets/Scripts/Common/Game/QuestionBuilder.cs` |
| 音声（Services.Audio） | `Assets/Scripts/Services/Audio/IPitchVariationService.cs`／`PitchVariationService.cs` |
| asmdef（Game1） | `Assets/Scripts/Game1/Geidai.Game1.asmdef` |
| 初期化（Game1） | `Assets/Scripts/Game1/Game1Bootstrap.cs` |
| UI（Game1） | `SoundMatchGameController.cs`／`ChoiceItemView.cs`／`FrogTargetView.cs`／`ResultEffectController.cs` |
| テスト（EditMode） | `Assets/Scripts/Tests/EditMode/QuestionBuilderTests.cs`／`SoundMatchConfigTests.cs` |
| データアセット | `Assets/Settings/SoundMatchConfig.asset`（MCP 生成） |
| ドキュメント | `aidlc-docs/construction/u6-game1/code/code-summary.md` |

## 3. スコープ外（フォローアップ）
- Game1 シーン作成/配線（`SoundMatchGameController`＋`FrogTargetView`＋`ChoiceItemView`（プレハブ）＋`ResultEffectController`・サービス解決）、既存ゲーム選択 UI（`GameListUI`/`StartGameButton`）から `NavigationService.GoTo(Game1)` 接続、演出アニメ/イラスト、Build Settings 登録 → **MCP フォローアップ**（code-summary に明記）。
- 音色/強弱の本格 DSP、②〜⑧ゲーム。

## 4. リスク・緩和
- **MCP でプロジェクトアセンブリ参照実行が制限される**（U3/U4 既知）→ ロジック検証は EditMode Test Runner に集約。MCP は純粋関数の軽スモーク＋コンパイル確認中心。
- **AudioSource を伴う PitchVariationService は EditMode 単体が難しい** → セント→pitch 換算は `PitchMath`（既存 PBT）に委譲し、Service は薄く保つ（発音自体はシーン/手動確認）。
- **保存音 0 件** → `fallbackClip`→`Empty` フォールバック（BR-GAME1-02）。

## 5. 完了条件
- Step0〜13 完了、コンパイル Error 0、EditMode テスト整備、code-summary/stories 更新・commit。
- 一方向依存（`Game1→Services→Common`）維持・既存資産非破壊。
- これで U6 の per-unit ループ（Functional/NFR/Code）完了 → 全 6 ユニット完了 → Build and Test へ。
