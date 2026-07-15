# Code Structure（概要把握レベル）

## Build System
- **Type**: Unity（Assembly-CSharp、`Assembly-CSharp.csproj` 自動生成）
- **Editor Version**: 6000.4.2f1（Unity 6）
- **Render Pipeline**: URP 17.4.0
- **Assembly Definitions**: なし（全スクリプトが既定 Assembly-CSharp に含まれる）

## Existing Files Inventory（既存スクリプト＝brownfieldの改修候補）

### 録音・加工
- `Assets/Scripts/RecorderWithEffects.cs` — マイク録音＋カスタムDSP加工（pitch 0.5–2.0 / noise cutoff+gate / timbre: Original,Robot,Bitcrush / reverb）。`OnAudioFilterRead` でリアルタイム、`ProcessSamplesOffline` で保存用レンダリング。maxRecordSeconds=300。
- `Assets/Scripts/VoiceRecordingSection.cs` — マイク録音＋Unity標準AudioFilter加工（LowPass/HighPass/Reverb/Echo/Distortion、pitch(cents)、音色プリセット3種、ノイズリダクション、バイパストグル）。maxRecordSeconds=10。保存は `MySoundCollectionStorage` 経由。
- `Assets/Scripts/SoundEffectSettings.cs` — 加工設定のデータモデル（displayName, wavFileName, pitchCents, tonePresetIndex, noiseReductionAmount, lowPass/highPass, reverbLevel, echoDelay/Decay, distortionLevel）。`[Serializable]`。

### 保存・ユーティリティ
- `Assets/Scripts/WavUtility.cs` — 16bit PCM WAV の Save/Load（float[]↔WAV変換、data チャンク探索対応）。
- `Assets/Scripts/SoundSavePaths.cs` — 保存先 `persistentDataPath/MySoundCollection`、ファイル名 `sound_yyyyMMdd_HHmmss.wav` 生成、一覧取得。
- `Assets/Scripts/MySoundCollectionStorage.cs` — WAV＋設定JSONを対で保存/読込、設定ファイル一覧取得。

### ゲーム選択UI
- `Assets/Scripts/GameListUI.cs`（class `GameCardListUI`）— カテゴリ変更でカード一覧を再生成。
- `Assets/Scripts/CategorySelectorUI.cs` — カテゴリ切替（kikiwake/narabekae/action/kumiawase）、`OnCategoryChanged` イベント発火。
- `Assets/Scripts/GameCardData.cs` — カードデータ（title/category/difficulty/questionCount/selectSoundCount/selectSoundType/thumbnail/description）。
- `Assets/Scripts/GameCardUI.cs` — カード1枚の表示（title/description/thumbnail）。
- `Assets/ScrollRectSnapLoopController.cs`（class `ScrollRectSnapLoop`）— 横スクロールのスワイプ＆スナップ。

### 画面遷移（Navigation）
- `Assets/Scripts/SceneSwitcher.cs` — 汎用シーン遷移（名前/インデックス、ホーム復帰）。
- `Assets/Scripts/GoToRec.cs`（class `GoToRecButton`）— "Rec" へ。
- `Assets/Scripts/GoToPlace.cs`（class `GoToPlaceButton`）— "place" へ（**注意: シーン名は Place.unity。大文字小文字不一致の懸念**）。
- `Assets/Scripts/GoToSoundCollection.cs`（class `GoToMySoundLibraryButton`）— "MySoundCollection" へ。
- `Assets/Scripts/ReturnHomeButton.cs` — "Home" へ復帰。
- `Assets/Scripts/StartGameButton.cs` — 指定シーンへ（ゲーム開始）。

### その他
- `Assets/Scripts/WeeklyTextController.cs` — 週替わりテキスト（オノマトペ13種）表示。
- `Assets/Scripts/Scean.cs` — **空クラス**（"Scene" のtypo と思われる未使用ファイル）。

## Scenes
- `Assets/Main画面.unity`, `Home.unity`, `Rec.unity`, `MySoundCollection.unity`, `Place.unity`, `game_Home.unity`, `Game01.unity`
- `Assets/Scenes/SampleScene.unity`（Unity既定サンプル、未使用想定）

## Design Patterns（観察されたもの）
- **静的ユーティリティ**: `WavUtility` / `SoundSavePaths` / `MySoundCollectionStorage`（static クラスでファイルI/Oを集約）。
- **イベント駆動UI**: `CategorySelectorUI.OnCategoryChanged` → `GameCardListUI` が購読して再描画。
- **MonoBehaviour + Inspector 参照注入**: UI要素を `[SerializeField]` で紐付け。

## Technical Debt / 注意点
- 録音機能が2実装（`RecorderWithEffects` と `VoiceRecordingSection`）で**重複**。どちらを正とするか未決。
- `GoToPlaceButton` の遷移先 "place" とシーンファイル `Place.unity` の**大文字小文字不一致**（ビルド環境によりロード失敗の恐れ）。
- `Scean.cs` は空クラス（削除候補）。
- **テストなし**（`com.unity.test-framework` は導入済みだがテストコード無し）。
- Assembly Definition 未整備（全コードが単一アセンブリ）。
- 日本語シーン名 `Main画面.unity`（ビルド/参照時の文字コード注意）。
