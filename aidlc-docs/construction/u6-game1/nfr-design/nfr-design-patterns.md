# U6 Game①音合わせ — NFR Design Patterns（実現パターン）

**ユニット**: U6 Game①音合わせ（最終ユニット）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / NFR Design（Part 2）
**決定**: Q1〜Q5＝すべて A（推奨）
**前提**: U1〜U5 の設計パターン（`Result<T>`／`ScreenRootBase`＋レスポンシブ/SafeArea／`ErrorPresenter`／`ServiceRegistry` DI／`SafeLogger`／純粋関数化＋PBT／`PitchMath`）を踏襲。本書は **U6 固有の実現パターン** を定義する。

> 数値目標は NFR Requirements（NFR-U6-01〜06）で確定済み。ここでは「どう実現するか」を定める。

---

## P1. リアルタイムピッチ再生パターン（NFR-U6-01/02 / Q1=A）
- **パターン**: 加工済み音声を作らず、再生器のピッチパラメータで擬似的にバリエーションを出す（非保存・低GC）。
- **実装方針**:
  - `PitchVariationService`（`Geidai.Services.Audio`・IF＋実装・`ServiceRegistry` 登録／`AudioService` とは別サービス）。
  - `Play(AudioBuffer baseBuffer, int cents)`：専用リグ（`GameObject`＋`AudioSource`）へ、基準 `AudioClip`（開始時に `baseBuffer` の PCM から `AudioClip.SetData` で一度生成しキャッシュ）を割り当て、`AudioSource.pitch = (float)PitchMath.CentsToRatio(cents)` を設定して再生。
  - 連続タップは `Stop()`→再生で差し替え（発音重複回避）。`IsPlaying` で完了検知。
  - 基準 `AudioClip` はゲーム中のみ保持、終了時に破棄。
- **受入**: 発音開始 体感即時（< 0.1s）・加工音ファイル非生成・ゲーム中アロケーション最小。

## P2. 純粋出題生成パターン（NFR-U6-03/04 / Q2=A）
- **パターン**: 出題を「メタ（セント）」として決定的に生成し、音生成/UI と分離。
- **実装方針**:
  - `QuestionBuilder`（`Geidai.Common.Game`・static 純粋）：`Build(string baseSoundId, SoundMatchConfig config, DifficultyLevel diff, int seed)`。
  - `System.Random(seed)` で `targetCents` と不正解セント（`targetCents ± k*diff.centsStep`・重複なし）を決定→正解を含めシャッフル→`Question{ baseSoundId, targetCents, choices, correctIndex }`。
  - 副作用なし・O(choiceCount)。
- **受入（PBT）**: 正解ちょうど1つ／不正解は `centsStep` 以上離れる／選択肢数=`config.choiceCount`／同一 seed で決定的。

## P3. 素材選択・フォールバック集約パターン（NFR-U6-03 / Q3=A）
- **パターン**: 素材選択と失敗時フォールバックを開始処理の単一窓口へ集約。
- **実装方針**:
  - `SoundMatchGameController.StartGame()`：`IStorageService.ListSounds()`→有効素材を選択（seed 可）。`LoadSoundBuffer(id)` 失敗は次候補へ。
  - 有効素材が無ければ `SoundMatchConfig.fallbackClip`（`AudioClip`→基準バッファ）。それも無ければ **`Empty` 状態**（フォールバック表示：ろくおんしてね）→ ホーム誘導。
  - 全失敗は `Result`＋`ErrorPresenter`（クラッシュしない）。
- **受入**: 0件/読込失敗でも `Empty`/フォールバックで継続（クラッシュなし）。

## P4. 配置・データ型・Config パターン（NFR-U6-05 / Q4=A）
- **パターン**: 純粋ロジック/データ型/SO を横断層（`Common.Game`）へ、サービスを `Services` へ、UI を専用アセンブリへ分離した一方向依存。
- **実装方針**:
  - 新規 `Geidai.Game1`（UI）：`Geidai.Common`/`Geidai.Services`/`UnityEngine.UI` 一方向参照（**Collection/Rec 非依存**）。
  - `QuestionBuilder`（純粋）・`SoundMatchConfig`(SO)・`DifficultyLevel`/`ChoiceSpec`/`Question` は `Geidai.Common.Game`。
  - `PitchVariationService`（IF＋実装）は `Geidai.Services.Audio`。
  - `GameSession` は実行時状態（非永続）。保存音は `IStorageService`。
  - `SoundMatchConfig` はインスペクタ注入 or `ContentService` 経由取得（既定アセットは MCP 生成）。
- **受入**: 一方向依存で循環なし・既存資産（Assembly-CSharp）非破壊。

## P5. 操作・演出・既存 UI 接続パターン（NFR-U6-05・NFR-05 / Q5=A）
- **パターン**: タップ確認／ドラッグ解答／純粋判定／演出を分離し、意匠は Sさん が調整可能に。
- **実装方針**:
  - `ChoiceItemView`：タップ＝確認再生、uGUI ドラッグ（`IBeginDragHandler`/`IDragHandler`/`IEndDragHandler`）。
  - `FrogTargetView`：タップ＝お手本再生、ドロップ領域（`RectTransform` 重なり判定）。
  - ドロップ→`SoundMatchGameController.OnAnswer(index)`（純粋判定 `index==correctIndex`）→`ResultEffectController`（正解=カエル進化／不正解=やさしい再挑戦／全問終了=結果）。領域外は元位置復帰。
  - 意匠/演出/文言は `UITheme` 準拠で Sさん 調整（US-TECH-07）。レスポンシブ/SafeArea 配下。
  - 既存 brownfield ゲーム選択 UI（Assembly-CSharp）は残置し、①音合わせへは `NavigationService.GoTo(Game1)`／`ModuleRouter` 接続（実配線は MCP フォローアップ）。
- **受入**: タップ/ドラッグ/判定/演出が分離・大きく分かりやすい・一方向依存維持。

## セキュリティ/プライバシー（NFR-U6-06・継続）
- 端末内保存音のみ読取・加工音/進行（`GameSession`）は非保存・外部送信なし（NFR-02）。`SafeLogger` で PII 非ログ。

---

## トレース（パターン → NFR/機能）
| パターン | NFR | Functional/BR |
|---|---|---|
| P1 リアルタイムピッチ再生 | NFR-U6-01/02 | business-logic-model §3・BR-GAME1-21〜23 |
| P2 純粋出題生成 | NFR-U6-03/04 | UC-2・BR-GAME1-11〜15 |
| P3 素材フォールバック集約 | NFR-U6-03 | UC-1・BR-GAME1-01〜03 |
| P4 配置・Config | NFR-U6-05 | BR-GAME1-31〜33/61/62 |
| P5 操作・演出・接続 | NFR-U6-05/NFR-05 | frontend-components・BR-GAME1-41〜53/63 |
