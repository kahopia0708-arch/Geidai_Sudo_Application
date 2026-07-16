# U3 Rec — Code Generation Summary（コード生成サマリ）

**ユニット**: U3 Rec（録音・加工・保存）
**生成日**: 2026-07-15 / AI-DLC CONSTRUCTION / Code Generation（Part 2）
**種別**: Brownfield（Unity 6000.4.2f1 / URP / uGUI / C#）
**MCP**: 公式 Unity AI Assistant（`user-unity-mcp`）で検証（Error 0 / 純粋ロジック スモート PASS）

> アプリコードは `Assets/` 配下。本書は要約（`aidlc-docs/`）。

---

## 1. 生成/修正/削除ファイル一覧

### 新規（`Geidai.Rec` / `Assets/Scripts/Rec/`）
- Created: `Geidai.Rec.asmdef`（refs: Geidai.Common, Geidai.Services, UnityEngine.UI／一方向 `Rec→Services→Common`）
- Created: `RecordingState.cs`（enum: Idle/NoMic/Recording/Recorded/Playing/Saving/Saved）
- Created: `MicPermissionStatus.cs`（enum: Unknown/Granted/Denied/NoDevice）
- Created: `EffectKind.cs`（enum: Pitch/NoiseReduction/Timbre/Reverb）
- Created: `RecordingClock.cs`（3秒固定計測・自動停止／POCO・テスト容易）
- Created: `MicPermissionGate.cs`（権限確認/要求・iOS/Android/デバイス有無を内包／`static`＋コルーチン）
- Created: `EffectChain.cs`（AudioSource＋各 AudioFilter を束ね `Apply` で非破壊一括反映）
- Created: `RecAudioService.cs`（`IAudioService` 本実装／Microphone 録音・AudioBuffer 再利用・再生）
- Created: `RecBootstrap.cs`（Rec 側で `IAudioService` を `ServiceRegistry` 登録／循環回避）
- Created: `RecordingController.cs`（権限ゲート→録音→3秒自動停止→バッファ通知）
- Created: `EffectPanelController.cs`（加工 UI ↔ `SoundEffectSettingsData` ↔ `EffectChain`）
- Created: `SavePromptController.cs`（`SavedSound` 構築→`SaveSound`→成否 UI）
- Created: `RecScreenController.cs`（`RecordingState` 状態機械・UI 結線・破棄確認・ホーム遷移）

### 新規（`Geidai.Common.Audio` / `Assets/Scripts/Common/Audio/`）
- Created: `SoundEffectMapper.cs`（純粋換算：半音↔セント・ノイズ4段↔連続値・リバーブ mB↔正規化／PBT 対象）

### 修正（後方互換）
- Modified: `Assets/Scripts/Services/Storage/IStorageService.cs`（`SaveSound(SavedSound, AudioBuffer)` 追加。既存 4 メソッドは不変）
- Modified: `Assets/Scripts/Services/Storage/StorageService.cs`（`SaveSound` 最小実装：wav→meta・meta 失敗時 wav 削除。`using Geidai.Common.Audio` 追加）
- Modified: `Assets/Scripts/Tests/EditMode/Geidai.Tests.asmdef`（references に `Geidai.Rec` 追加）

### 削除
- Deleted: `Assets/Scripts/RecorderWithEffects.cs`（＋`.meta`）※重複 DSP。シーン/プレハブ参照なしを確認済み
- Deleted: `Assets/Scripts/Scean.cs`（＋`.meta`）※空クラス。参照なしを確認済み

### テスト（EditMode / `Geidai.Tests`）
- Created: `SoundEffectMapperTests.cs`（PBT：半音↔セント往復・クランプ、ノイズ4段往復、リバーブ正規化往復・飽和）
- Created: `RecordingClockTests.cs`（3秒到達・境界・未起動 no-op・リセット・残り秒非負・超過丸め）
- Created: `SaveSoundTests.cs`（wav＋meta 対生成・往復・一覧反映・null 入力の非書込検証）

---

## 2. 名前空間・依存
- `Geidai.Rec` → `Geidai.Services` → `Geidai.Common`（一方向・循環なし）。
- `SoundEffectMapper` は横断純粋関数のため `Geidai.Common.Audio`。
- `IAudioService` 実装（`RecAudioService`）は **Rec 側で** `ServiceRegistry` 登録（`Services→Rec` の循環を作らない。`AppManager` は不変）。
- 外部 API/ネットワークなし・音声はローカルのみ（NFR-02 / PRIVACY）。

## 3. 主要な技術判断
- **旧グローバル `SoundEffectSettings`（Assembly-CSharp）は参照しない**（asmdef 制約）。U3 は新形式 `SoundEffectSettingsData` のみ。旧データ移行は U4/対象外。
- **非破壊加工**：録音 `AudioBuffer` は変更せず、再生時に `EffectChain` のパラメータのみ更新（有無比較が可能）。
- **3秒固定録音**：`RecordingClock` で自動停止。float 累積誤差で到達が 30/31 tick に前後するため、テストは「完了まで tick して回数 29〜31」で堅牢化（実機はフレーム継続のため問題なし。Elapsed は 3.0 に丸め）。
- **保存の原子性（最小）**：wav→meta 順で書き、meta 失敗時は中途半端な wav を削除。原子的置換・破損復旧の本実装は U4。
- **AudioBuffer 再利用**：`RecAudioService` は固定長バッファを再利用し GC を抑制（NFR-06）。

## 4. MCP 検証結果（`user-unity-mcp`）
- ベースライン `Unity_GetConsoleLogs`：Error 0 / Warning 0。
- 取り込み後コンパイル：**Error 0 / Warning 0**。`Geidai.Rec`・`Geidai.Tests` を含む全 5 アセンブリのロードを確認（`isCompilationSuccessful=true`）。
- 同期スモーク（`Unity_RunCommand`）：
  - `SoundEffectMapper`：`CentsToSemitones(9999)=12`（+クランプ）、半音往復 `=5` = **PASS**。
  - `RecordingClock`：`done=True / ticks=31 / elapsed=3 / running=False`、超過 `Tick(5f)` も `done=True / elapsed=3` = **PASS**。
- `SaveSound` 実行時スモーク：AI Assistant Run Command の**ファイル書き込み承認ガード**（"User interactions are not supported"）により MCP からは実行不可。コード不具合ではなく、EditMode `SaveSoundTests` で担保（Test Runner 実行）。

## 5. UI ハンドオフ点（前本 → Sさん / US-TECH-07）
- `EffectPanelController`：ピッチ/ノイズ低減/音色（なし・ロボット・コーラス系）/リバーブの UI（`Slider`/`Dropdown`/`Toggle`）配置・ラベル・見た目を調整可（数値換算はコードが担保）。
- `EffectChain`：音色プリセット値（lowpass/highpass/distortion）やリバーブ強度の微調整はインスペクタ/コード定数で調整可。
- `ErrorPresenter`/`ConfirmDialog`（U1/U2 再利用）：マイク不可・保存失敗・破棄確認の文言/配色トーン。
- UI 枠は uGUI（`Button`/`Slider`/`Dropdown`/`Toggle`/`InputField`/`Text`）。必要に応じ TMP へ差し替え可。

## 6. 残タスク（MCP フォローアップ：実シーン配線）
> コードは完成。以下は Unity 上での GameObject 配線で、破壊回避のため別途 MCP セッションで実施する。

1. **Rec.unity 再構築**：Canvas＋`ResponsiveCanvasConfigurator`＋`SafeAreaFitter`、`RecScreenController` を配置。
   - 再生用 GameObject に `EffectChain`（AudioSource＋LowPass/HighPass/Reverb/Distortion）を付与。
   - `RecordingController`／`EffectPanelController`／`SavePromptController` を配置し、`RecScreenController` の各参照（ボタン群・`ErrorPresenter`・`ConfirmDialog`・状態表示）を結線。
   - 録音/再生/保存/もどる `Button`、加工 `Slider`/`Dropdown`/`Toggle`、残り秒 `Text`、名前 `InputField` を結線。
2. **旧録音一式の除去**：現行 Rec シーンが参照中の `VoiceRecordingSection`/`WavUtility`/`MySoundCollectionStorage`/`SoundSavePaths`/`SoundEffectSettings` を、新方式へ差し替え後に物理削除。
3. 配線後、実機（マイク権限あり/なし）で「録音→3秒自動停止→加工プレビュー→保存→ホーム戻る（未保存は破棄確認）」を通し確認（Build & Test）。

## 7. スコープ外（U3 では未実施）
- 実シーン配線（上記 §6）。
- 旧録音スクリプト群の物理削除（シーン再配線と同時）。
- 永続化の原子的置換・破損復旧の本実装（U4）。
- コレクション画面での再生/一覧（U4）。

## 8. トレース
US-REC-01→RecordingController/RecAudioService/RecordingClock/MicPermissionGate ／ US-REC-02→EffectPanelController/EffectChain/SoundEffectMapper（非破壊プレビュー）／ US-REC-03→SavePromptController＋`IStorageService.SaveSound`/StorageService ／ US-TECH-03→`Geidai.Rec` 新設・`IAudioService` 実装統合・重複DSP削除 ／ NFR-03→RecordingClock/AudioBuffer ／ NFR-06→AudioBuffer 再利用 ／ NFR-07→全失敗 Result 化 ／ NFR-09→EditMode PBT/単体 ／ SECURITY-15→MicPermissionGate/フェイルセーフ ／ PRIVACY→ローカルのみ。
