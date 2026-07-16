# U6 Game①音合わせ — Logical Components（論理コンポーネント）

**ユニット**: U6 Game①音合わせ（最終ユニット）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / NFR Design（Part 2）
**方針**: NFR Design Patterns（P1〜P5）を支える論理部品を定義。物理実装（C# 型/シグネチャ）は Code Generation で確定。一方向依存 `Geidai.Game1 → Geidai.Services → Geidai.Common`。

---

## 1. コンポーネント一覧と配置

| # | コンポーネント | 種別 | 配置（アセンブリ） | 対応パターン |
|---|---|---|---|---|
| 1 | `ChoiceSpec` | 値オブジェクト | `Geidai.Common.Game` | P2 |
| 2 | `Question` | 値オブジェクト | `Geidai.Common.Game` | P2 |
| 3 | `DifficultyLevel` | 値オブジェクト（Serializable） | `Geidai.Common.Game` | P2/P4 |
| 4 | `SoundMatchConfig` | ScriptableObject | `Geidai.Common.Game` | P4 |
| 5 | `QuestionBuilder` | 静的純粋関数 | `Geidai.Common.Game` | P2 |
| 6 | `IPitchVariationService` / `PitchVariationService` | IF＋実装 | `Geidai.Services.Audio` | P1 |
| 7 | `GameSession` | 実行時状態（POCO・非永続） | `Geidai.Game1`（or `Common.Game`） | P2/P3 |
| 8 | `SoundMatchGameController` | `ScreenRootBase` サブクラス | `Geidai.Game1` | P3/P5 |
| 9 | `ChoiceItemView` | MonoBehaviour | `Geidai.Game1` | P5 |
| 10 | `FrogTargetView` | MonoBehaviour | `Geidai.Game1` | P5 |
| 11 | `ResultEffectController` | MonoBehaviour | `Geidai.Game1` | P5 |
| 12 | `Game1Bootstrap`（任意） | 静的初期化 | `Geidai.Game1` | P1/P4 |

---

## 2. 各コンポーネントの責務

### 2.1 `ChoiceSpec` / `Question`（Geidai.Common.Game）
- `ChoiceSpec{ int cents; bool isCorrect }`。
- `Question{ string baseSoundId; int targetCents; List<ChoiceSpec> choices; int correctIndex }`。

### 2.2 `DifficultyLevel` / `SoundMatchConfig`（Geidai.Common.Game）
- `DifficultyLevel{ string label; int centsStep }`。
- `SoundMatchConfig : ScriptableObject`：`questionCount`/`choiceCount`/`difficulties`/`fallbackClip`＋クランプ用アクセサ（`ClampedChoiceCount` 等）。`[CreateAssetMenu]`。

### 2.3 `QuestionBuilder`（Geidai.Common.Game・static 純粋）
- `Question Build(string baseSoundId, SoundMatchConfig config, DifficultyLevel diff, int seed)`（P2）。
- 副作用なし・決定的・PBT 対象。

### 2.4 `IPitchVariationService` / `PitchVariationService`（Geidai.Services.Audio）
- `Result Play(AudioBuffer baseBuffer, int cents)`（再生時 pitch・非保存 / P1）。
- `Result Stop()`／`bool IsPlaying`。
- 専用リグ（`AudioSource`）を自前保持し、基準 `AudioClip` をキャッシュ。`ServiceRegistry` に登録。

### 2.5 `GameSession`（実行時・非永続）
- `List<Question> questions`／`int currentIndex`／`int correctCount`／`bool IsFinished`。
- 進行の保持のみ（判定・演出は Controller）。

### 2.6 `SoundMatchGameController : ScreenRootBase`（Geidai.Game1）
- 依存: `IStorageService`（素材）、`IPitchVariationService`（発音）、`SoundMatchConfig`（注入 or `IContentService`）、`INavigationService`（戻る）、`ErrorPresenter`。`ServiceRegistry` 解決。
- 責務: `StartGame()`（素材選択・フォールバック集約 P3・出題生成 P2）、`NextQuestion()`、`OnAnswer(index)`（純粋判定→`ResultEffectController`）、`OnBackPressed()`→ホーム。
- 状態: Loading/Empty/Playing/Judging/Result。

### 2.7 `ChoiceItemView` / `FrogTargetView`（Geidai.Game1）
- `ChoiceItemView`：`ChoiceSpec`＋index 保持、タップ確認再生、uGUI ドラッグ、領域外復帰。
- `FrogTargetView`：お手本タップ再生、ドロップ領域判定→`OnAnswer`。

### 2.8 `ResultEffectController`（Geidai.Game1）
- `PlayCorrect()`（カエル進化演出）／`PlayRetry()`（やさしい再挑戦）／`ShowResult(correct, total)`。演出のみ（進行は Controller）。

### 2.9 `Game1Bootstrap`（任意）
- `IPitchVariationService` を解決/未登録なら登録、`SoundMatchConfig` を確保（`RecBootstrap`/`ThemeBootstrap` と同パターン）。

---

## 3. 連携（テキスト表現）
- `SoundMatchGameController.StartGame` → `IStorageService.ListSounds`/`LoadSoundBuffer`（素材）→ `QuestionBuilder.Build`（各問）→ `GameSession`。
- お手本/選択肢の発音 → `IPitchVariationService.Play(baseBuffer, cents)`。
- 解答（ドロップ）→ `OnAnswer(index)`（`index==correctIndex`）→ `GameSession.correctCount` 更新 → `ResultEffectController`。
- 戻る → `INavigationService.GoTo(Home)`。
- 起動 → `Game1Bootstrap`（任意）→ `IPitchVariationService`/`SoundMatchConfig` 確保。

## 4. 依存方向（循環なし）
```
Geidai.Game1 (UI: SoundMatchGameController/ChoiceItemView/FrogTargetView/ResultEffectController/GameSession)
   └─> Geidai.Services (IStorageService/IPitchVariationService/IAudioService/INavigationService/ServiceRegistry)
          └─> Geidai.Common (QuestionBuilder/SoundMatchConfig/Question/ChoiceSpec/DifficultyLevel/PitchMath/Result/SavedSound/AudioBuffer/ScreenRootBase)
```
- `Geidai.Collection`/`Geidai.Rec` へは非依存。Assembly-CSharp（旧ゲーム選択 UI）へは非依存（導線は Navigation 経由）。

## 5. テスト対応（NFR-U6-04）
- `QuestionBuilder.Build`：PBT（正解1つ・距離条件・選択肢数・決定的）。
- `SoundMatchConfig`：クランプ単体。
- `PitchVariationService`：cents→pitch 換算が `PitchMath.CentsToRatio` と一致（単体・軽量）。
- 配置: `Geidai.Tests`（EditMode）に `Geidai.Game1` 参照を追加。

## 6. トレース
P1→2.4/2.9 ／ P2→2.1/2.2/2.3/2.5 ／ P3→2.6 ／ P4→2.1〜2.5・§4 ／ P5→2.6/2.7/2.8。NFR-U6-01〜06・Functional Design（domain-entities/business-logic-model/business-rules/frontend-components）に整合。
