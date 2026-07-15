# U3 Rec — NFR Design Patterns（NFRの実現パターン）

**ユニット**: U3 Rec（録音・加工・保存）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Design（Part 2）
**決定**: Q1〜Q6＝すべて A（推奨）
**入力**: `../nfr-requirements/nfr-requirements.md`, `tech-stack-decisions.md`, `../functional-design/*`, U1/U2 NFR Design 成果物

> U3 の各 NFR を「どう実現するか」を設計パターンで定義。数値は NFR Requirements で確定済み。U1/U2 パターンを踏襲し U3 固有の差分を示す。

---

## 1. Performance — リアルタイム加工チェーン（Q1=A / NFR-06）
- **`EffectChain`**（AudioSource＋各 AudioFilter を束ねる論理部品）を導入。
- `EffectPanelController` は UI 操作を `SoundEffectSettingsData` に反映し、**`EffectChain.Apply(settings)` で一括再構成**する:
  - ピッチ → `AudioSource.pitch = PitchMath.SemitonesToRatio(settings.pitchSemitones)`。
  - 音色（TimbreType）→ lowpass/highpass/distortion のプリセット値。
  - リバーブ（0〜1）→ `AudioReverbFilter.reverbLevel` へ換算。
  - ノイズ低減（4段）→ フィルタ組合せ。
  - **バイパス**: 該当 EffectKind off → そのフィルタを中立値へ（＝無効化）。全体一括 off も同様。
- **再生中もライブ反映**: パラメータ変更時に `Apply` を呼ぶ（体感即時 < 0.1s）。
- **GC/性能**: フィルタ参照は初期化時にキャッシュ（毎フレーム `GetComponent` 禁止）。`Apply` は値代入のみでアロケーションを避ける。

## 2. Performance/Resilience — 録音クロックとバッファ（Q2=A / NFR-03・NFR-06・NFR-07）
- **`RecordingClock`**（コルーチン or `Update` 経過計測）で **3.0s 到達時に `StopRecording()` を駆動**（カウントダウン値も供給）。
- 録音は `Microphone.Start`（3秒・44100・mono）。停止時にサンプルを **再利用可能な固定長 `AudioBuffer`（132300）へコピー**。
- **GC 削減**: `AudioBuffer.Samples` は使い回し（録り直しは同配列へ上書き）。毎回の大量確保を避ける（U1 §2 パターン踏襲）。
- **信頼性**: クロックで確実に自動停止（`Microphone` 依存の誤差を吸収）。異常時も `Result` で表現。

## 3. Resilience — マイク権限ゲート（Q3=A / NFR-07・SECURITY-15）
- **`MicPermissionGate`** に権限確認/要求を集約（プラットフォーム分岐を閉じ込め）:
  - iOS: `Application.RequestUserAuthorization(UserAuthorization.Microphone)`。
  - Android: `Permission.RequestUserPermission(Permission.Microphone)`。
  - デバイス有無: `Microphone.devices`。
- 返り値 **`MicPermissionStatus`**（Granted/Denied/NoDevice/Unknown）。
- `RecordingController` は録音前に Gate を通し、`Denied`/`NoDevice` は **`ErrorPresenter` 案内＋録音無効**（`Result` で表現・クラッシュ禁止）。
- **受入**: 権限拒否/デバイス無しを注入してもクラッシュせず案内が出る。

## 4. Resilience — SaveSound の最小原子性（Q4=A / NFR-07・SECURITY-15）
- `IStorageService.SaveSound(SavedSound, AudioBuffer)` の U3 実装パターン:
  1. `sounds/` 作成（無ければ）。
  2. `WavCodec.Encode(buffer)` → `sounds/{id}.wav` 書き込み。
  3. `JsonUtility.ToJson(savedSound)` → `sounds/{id}.meta.json` 書き込み。
  4. **meta 書き込み失敗時は書いた wav を削除**（中途半端な対を残さない＝ベストエフォート原子性）。
- 成功は **wav＋meta 両立時のみ** `Result.Ok`。失敗は `Result(IOError)`（録音は保持し再試行可）。例外は捕捉して `Result` に変換。
- **完全な原子置換（temp→rename）・破損復旧は U4**（本パターンは最小）。

## 5. Testability — 換算 Mapper の純粋関数化（Q5=A / NFR-09・PBT）
- **`SoundEffectMapper`（静的・副作用なし）** に数値換算を集約:
  - 旧 `SoundEffectSettings` ↔ 新 `SoundEffectSettingsData`。
  - `cents → pitchSemitones`（100=1半音・最寄り丸め）。
  - ノイズ連続(0〜1) → `NoiseLevel`（4段離散化）。
  - `reverbLevel(mB) → reverb(0〜1)` 正規化。
- **PBT 対象**（境界・丸め・往復の一貫性）。`SoundEffectSettingsData → 具体フィルタ値` の技術寄り写像は `EffectChain` 側に置き、Mapper は数値換算に限定（テスト容易性の分離）。

## 6. Security/Privacy（NFR-04・SECURITY-15）
- 録音音声・WAV・設定は **端末内（`persistentDataPath/sounds`）のみ**。ネットワーク送信なし（NFR-02）。
- ログは `SafeLogger` 経由（PII/内容を出さない）。本番ビルドで詳細エラー非表示（SECURITY-09）。
- マイクは録音時のみ使用（`MicPermissionGate` 経由・常時録音しない）。

## 7. DI / Logical Components / 一本化（Q6=A / NFR-08・US-TECH-03）
- `IAudioService` 本実装を **`ServiceRegistry` に登録**（`AppManager` 起動時 or Rec シーン初期化時）。
- `RecScreenController`（`ScreenRootBase` 継承）が **`RecordingController`/`EffectPanelController`/`SavePromptController`** を調停。
- ロジックは POCO/静的へ寄せ MonoBehaviour 依存を最小化（`SoundEffectMapper`・`StartupRouter` 的分離）。
- 再利用: `ConfirmDialog`（離脱破棄確認）・`ErrorPresenter`（失敗通知）・`NavigationService`（遷移）。
- 重複 `RecorderWithEffects`/`Scean` 等は削除（参照除去・ビルド影響なし）。
- 新規アセンブリ `Geidai.Rec`（`Geidai.Common`/`Geidai.Services`/`UnityEngine.UI` 一方向依存・循環回避）。

## 8. Responsive / SafeArea（NFR-11/12）
- Rec 画面ルートは `ScreenRootBase` テンプレート（`ResponsiveCanvasConfigurator`＋`SafeAreaFitter`）を継承し、表示時/向き変更で再適用（U1 §4 踏襲）。

## 9. Scalability
- **N/A**（単一端末・1 録音単位・オフライン）。

## トレース
NFR-06→§1,§2 / NFR-03→§2 / NFR-07→§2,§3,§4 / SECURITY-15→§3,§4,§6 / NFR-09→§5 / NFR-04→§6 / NFR-08→§7 / NFR-11・NFR-12→§8 / NFR-02→§6,§9。
US-REC-01→§2,§3 / US-REC-02→§1,§5 / US-REC-03→§4 / US-TECH-03→§5,§7。
