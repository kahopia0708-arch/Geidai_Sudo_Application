# U6 Game①音合わせ — Code Summary（コード生成サマリ）

**ユニット**: U6 Game①音合わせ（最終ユニット）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / Code Generation（Part 2 完了）
**検証**: 公式 Unity AI Assistant（Unity MCP Server / `user-unity-mcp`）

---

## 1. 概要
保存済みの録音音をピッチ加工して出題する「①音合わせ」ゲームを実装。お手本（カエル）に合う音程の選択肢（おたまじゃくし）を、タップで確認・ドラッグ＆ドロップで解答する。出題は純粋関数 `QuestionBuilder`（音は作らずピッチのメタのみ／決定的）で生成し、発音は再生時ピッチ（`AudioSource.pitch = PitchMath.CentsToRatio`）を適用する `PitchVariationService`（加工音は非生成・非保存・低GC）が担う。パラメータ（出題数/選択肢数/難易度＝セント段階/フォールバック素材）は `SoundMatchConfig`（ScriptableObject）で Sさん が調整可能。UI は新アセンブリ `Geidai.Game1` に配置し、一方向依存 `Geidai.Game1 → Geidai.Services → Geidai.Common` を維持（Collection/Rec 非依存）。

## 2. 新規ファイル
| パス | 役割 | アセンブリ/名前空間 |
|---|---|---|
| `Assets/Scripts/Common/Game/ChoiceSpec.cs` | 選択肢1件のピッチ指定（cents/isCorrect） | `Geidai.Common.Game` |
| `Assets/Scripts/Common/Game/Question.cs` | 1問（baseSoundId/targetCents/choices/correctIndex） | `Geidai.Common.Game` |
| `Assets/Scripts/Common/Game/DifficultyLevel.cs` | 難易度1段階（label/centsStep＝選択肢間の最小間隔） | `Geidai.Common.Game` |
| `Assets/Scripts/Common/Game/SoundMatchConfig.cs` | 出題パラメータ SO（クランプアクセサ・`GetDifficulty`・`[CreateAssetMenu]`） | `Geidai.Common.Game` |
| `Assets/Scripts/Common/Game/QuestionBuilder.cs` | 出題生成の純粋関数（`Build`/`BuildQuestions`・決定的・正解1つ・距離条件） | `Geidai.Common.Game` |
| `Assets/Scripts/Common/Game/GameSession.cs` | 進行状態 POCO（非永続・`Current`/`IsFinished`/`MarkCorrect`/`Advance`） | `Geidai.Common.Game` |
| `Assets/Scripts/Services/Audio/IPitchVariationService.cs` | 出題用ピッチ再生の抽象（`SetBase`/`Play(cents)`/`Stop`/`IsPlaying`） | `Geidai.Services.Audio` |
| `Assets/Scripts/Services/Audio/PitchVariationService.cs` | 専用リグ＋基準 AudioClip キャッシュ＋再生時 pitch。非保存・低GC | `Geidai.Services.Audio` |
| `Assets/Scripts/Game1/Geidai.Game1.asmdef` | 新アセンブリ（Common/Services/UnityEngine.UI・**Collection/Rec 非依存**） | `Geidai.Game1` |
| `Assets/Scripts/Game1/Game1Bootstrap.cs` | `IPitchVariationService` の確保/登録（Rec/Theme と同パターン） | `Geidai.Game1` |
| `Assets/Scripts/Game1/SoundMatchGameController.cs` | 統括（`ScreenRootBase`・素材選択/フォールバック集約・出題・判定・進行・戻る） | `Geidai.Game1` |
| `Assets/Scripts/Game1/ChoiceItemView.cs` | 選択肢（タップ確認＋uGUI ドラッグ＆ドロップ・領域外は復帰） | `Geidai.Game1` |
| `Assets/Scripts/Game1/FrogTargetView.cs` | お手本＋ドロップ領域（タップ確認・当たり判定） | `Geidai.Game1` |
| `Assets/Scripts/Game1/ResultEffectController.cs` | 正解演出（カエル進化）/やり直し/結果サマリ（演出のみ） | `Geidai.Game1` |
| `Assets/Scripts/Tests/EditMode/QuestionBuilderTests.cs` | `QuestionBuilder` の PBT＋例示 | `Geidai.Tests` |
| `Assets/Scripts/Tests/EditMode/SoundMatchConfigTests.cs` | `SoundMatchConfig` のクランプ/フォールバック単体 | `Geidai.Tests` |
| `Assets/Settings/SoundMatchConfig.asset` | 既定出題パラメータ（かんたん200/ふつう100/むずかしい50/とても難しい20・MCP 生成） | アセット |

## 3. 変更ファイル
| パス | 変更内容 |
|---|---|
| （なし） | 既存コードへの変更なし。全て追加のみ（既存資産非破壊）。素材取得は既存 `IStorageService.ListSounds`/`LoadSoundBuffer`、換算は既存 `PitchMath.CentsToRatio` を再利用。 |

> 注: 当初計画では `Geidai.Tests.asmdef` に `Geidai.Game1` 参照追加を想定したが、テスト対象（`QuestionBuilder`/`SoundMatchConfig`/`DifficultyLevel`）は全て `Geidai.Common.Game` にあり既存参照で足りるため、参照追加は不要と判断した。

## 4. 依存構造（循環なし）
```
Geidai.Game1 (UI: SoundMatchGameController/ChoiceItemView/FrogTargetView/ResultEffectController/Game1Bootstrap)
   └─> Geidai.Services (IPitchVariationService/PitchVariationService/IStorageService/INavigationService/ServiceRegistry)
          └─> Geidai.Common (QuestionBuilder/SoundMatchConfig/Question/ChoiceSpec/DifficultyLevel/GameSession/PitchMath/AudioBuffer/Result/SceneId/ScreenRootBase/ErrorPresenter)
```
- Assembly-CSharp（旧ゲーム選択 UI）への参照なし。Collection/Rec への依存なし。導線は `INavigationService`（`SceneId.Game1`）経由。

## 5. NFR / 設計トレース
- **P1 リアルタイムピッチ再生**（NFR-U6-01/02）: `PitchVariationService`＝専用リグ（`DontDestroyOnLoad`）＋基準 `AudioClip` を一度キャッシュ、再生時 `AudioSource.pitch` 適用。加工音は非生成・非保存・低GC。
- **P2 純粋出題生成**（NFR-U6-03/04）: `QuestionBuilder.Build`＝`System.Random(seed)` で決定的。正解ちょうど1つ・不正解は `centsStep` 以上離す・選択肢重複なし。PBT 対象。
- **P3 素材選択/フォールバック集約**（NFR-U6-03）: `SoundMatchGameController.TryLoadBase`＝保存音を seed 順で試行→失敗は次候補→0件/全滅は `fallbackClip`→`Empty`（`ErrorPresenter` 警告）。
- **P4 配置・データ型・設定**（NFR-U6-05）: UI は新 `Geidai.Game1`／純粋・SO 型は `Common.Game`／`PitchVariationService` は `Services.Audio`。素材は `IStorageService`（Collection 非依存）。
- **P5 インタラクション/演出/既存 UI 接続**（NFR-U6-01）: `ChoiceItemView`（タップ確認＋ドラッグ）／`FrogTargetView`（タップ確認＋ドロップ判定）／`ResultEffectController`（正解=進化・不正解=無ペナルティやり直し・結果サマリ）。既存ゲーム選択 UI は Navigation で接続（フォローアップ）。
- **プライバシー**（NFR-U6-06）: 素材は端末内の保存音のみ・加工音/進行状態は非永続・外部送信なし・PII をログに出さない。

## 6. MCP 検証結果（`user-unity-mcp`）
- ベースライン: Error 0 / Warning 0。
- `AssetDatabase.Refresh`（ドメインリロードで一時切断→再接続）後: **コンパイル Error 0 / Warning 0**。
- スモーク（`Unity_RunCommand`・`Geidai.Common.Game`）: `QuestionBuilder.Build(seed=42, choices=4, step=100)` →
  `choices=4 / correctCount=1 / correctIndex=3 / distinct=True / distanceOK=True / deterministic=True`。全不変条件を確認。
- 既定 `SoundMatchConfig.asset` を `Assets/Settings/` に生成（q=5・choices=3・難易度4段階）。
- EditMode テスト（`QuestionBuilderTests`/`SoundMatchConfigTests`）は Unity Test Runner で実行（MCP はプロジェクトアセンブリの Test 実行を直接行わないため・U3/U4/U5 と同方針）。純粋ロジックの健全性は上記スモークでも確認済み。
- `PitchVariationService` は `AudioSource` を伴うため EditMode 単体対象外。セント→pitch 換算は既存 `PitchMath`（PBT 済）に委譲し Service は薄く保つ（実発音はシーン/手動確認）。

## 7. UI ハンドオフ点（Sさん / US-TECH-07）
- カエル/おたまじゃくしのイラスト・成長段階スプライト（`ResultEffectController.growthStages`）。
- 正解/やり直し/結果サマリの演出・文言・配色（`UITheme` 準拠）。
- 選択肢の並び・ドラッグの当たり判定・ドロップ領域（`FrogTargetView.dropArea`）。
- `emptyState`（保存音なし）の見た目。
- 難易度・出題数・選択肢数（`SoundMatchConfig`）の調整。

## 8. 残タスク（MCP フォローアップ）
- Game1 シーン作成/配線：`SoundMatchGameController`＋`FrogTargetView`＋`ChoiceItemView`（プレハブ複数）＋`ResultEffectController`、`SoundMatchConfig`（既定アセット）注入、レスポンシブ/SafeArea 参照設定。
- 既存ゲーム選択 UI（`GameListUI`/`StartGameButton` 等）から `NavigationService.GoTo(Game1)` 接続（＋`ModuleId.Game1`/`SceneId.Game1` のシーン割当確認）。
- 演出アニメ/イラスト・効果音の実装。
- Build Settings に Game1 シーン登録（未登録なら）。
- （任意）`AppManager` 起動時に `SoundMatchConfig` を配線 or 各画面で `Game1Bootstrap` に委譲。
- 音色/強弱の本格 DSP、②〜⑧ゲーム（今回スコープ外）。
