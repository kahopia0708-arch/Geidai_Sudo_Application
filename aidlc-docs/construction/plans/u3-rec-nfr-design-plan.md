# U3 Rec — NFR Design Plan（計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U3 Rec（録音・加工・保存）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Design（Part 1: 計画）
**入力**: `../u3-rec/nfr-requirements/nfr-requirements.md`, `tech-stack-decisions.md`, `../u3-rec/functional-design/*`, U1/U2 NFR Design 成果物（`../u1-foundation/nfr-design/*`、`../u2-foundation/nfr-design/*`）

> 目的: U3 の NFR（性能/リアルタイム加工/信頼性/プライバシー/テスト容易性/保守性）を**設計パターン**と**論理コンポーネント**へ落とし込む。数値は NFR Requirements で確定済み。ここでは「どう実現するか（パターン）」を決める。
> **回答方法**: 各質問の `[Answer]:` に記号（例: `A`）。合う選択肢が無ければ「Other」。各質問に「(推奨)」あり。完了で「done」（または「全部推奨で」）。

---

## A. 実行チェックリスト（Part 2 で実行）
- [x] `../u3-rec/nfr-design/nfr-design-patterns.md` を生成（各 NFR の実現パターン）
- [x] `../u3-rec/nfr-design/logical-components.md` を生成（論理コンポーネント・責務・連携）
- [x] NFR Requirements / Functional Design とのトレース整合を確認

> **回答**: Q1〜Q6＝すべて A（推奨）。矛盾なし。Part 2 実行済み（2026-07-15）。

## B. カテゴリ適用性（このユニットでの判断）
- **Resilience（耐障害）**: 適用（マイク権限フェイルセーフ・保存失敗の非破壊・対ファイル整合）。ネットワーク再試行系は N/A。
- **Performance（性能）**: 適用（録音応答・リアルタイム加工・保存レイテンシ・GC 削減・fps）。
- **Scalability（スケーラビリティ）**: N/A（単一端末・オフライン・1録音単位）。
- **Security（セキュリティ）**: 適用（録音音声の端末内限定・非ログ）。U1 パターン踏襲。
- **Logical Components（論理部品）**: 適用（AudioService 実装・EffectChain・RecordingClock・MicPermissionGate・SaveSound 経路・換算 Mapper・Rec コントローラ群）。

## B-2. U1/U2 から継承する設計パターン（再質問しない・前提）
- **エラー伝搬**: `Result<T>`（成功/失敗＋理由コード）。致命的でない失敗はクラッシュさせない。
- **UI 基盤**: `ScreenRootBase` ＋ `ResponsiveCanvasConfigurator` ＋ `SafeAreaFitter`（表示時/向き変更で再適用）。
- **通知**: `ErrorPresenter`（子ども向けバナー）。**確認ダイアログ**: `ConfirmDialog`（再利用・既定=いいえ）。
- **DI**: 軽量サービスロケータ（`ServiceRegistry`）＋インターフェース（`IAudioService`/`IStorageService`/`INavigationService`）。
- **性能/GC**: 同期API基本・重い処理のみ非同期/コルーチン、バッファ再利用、参照キャッシュ。
- **セキュリティ**: PII/録音音声は端末外送信なし、`SafeLogger` で非ログ、本番で詳細エラー非表示。
- **テスト**: 純粋関数化＋I/O 抽象化（インターフェース）で PBT/モック可能に。

> ※ 上記の変更が必要な場合のみ、該当質問で Other 指定してください。

---

## C. 明確化のための質問（Q1〜Q6）

### Question 1（Performance — リアルタイム加工チェーンの設計）
加工プレビュー（非破壊・体感即時）の実現パターンは？

A) (推奨) **`EffectChain`（AudioSource＋各 AudioFilter を束ねる論理部品）**を用意。`EffectPanelController` は UI 操作を `SoundEffectSettingsData` に反映し、**`EffectChain.Apply(settings)` で一括再構成**（ピッチ=`AudioSource.pitch`＝`PitchMath` 換算、音色/リバーブ/ノイズ=各フィルタ値、バイパスは該当フィルタを中立化）。再生中も `Apply` を呼べばライブ反映。フィルタ参照はキャッシュし毎フレーム `GetComponent` しない（GC/性能）。

B) 各加工を個別コンポーネントが直接フィルタ操作（束ねる部品なし・最短実装）。

C) Other（[Answer]: の後に記述）

[Answer]:A

### Question 2（Performance/Resilience — 録音クロックとバッファ）
3秒固定・自動停止と録音バッファの設計は？

A) (推奨) **`RecordingClock`（コルーチン or Update 経過計測）**で 3.0s 到達時に `IAudioService.StopRecording()` を駆動（カウントダウン表示も供給）。録音は `Microphone.Start`（3秒・44100・mono）、停止時にサンプルを **再利用可能な固定長 `AudioBuffer`（132300）へコピー**（毎回の大量確保を避け GC 削減）。録り直しは同バッファへ上書き。**受入=3秒で確実に自動停止・GC スパイクを出さない**。

B) `Microphone` の録音長のみに任せ、明示的クロックを持たない（誤差許容）。

C) Other（[Answer]: の後に記述）

[Answer]:A

### Question 3（Resilience — マイク権限ゲート）
マイク権限のプラットフォーム差異と失敗処理の設計は？

A) (推奨) **`MicPermissionGate`（権限確認/要求を抽象化する論理部品）**を用意し、iOS=`Application.RequestUserAuthorization(Microphone)`、Android=`Permission.RequestUserPermission(RECORD_AUDIO)`、デバイス有無=`Microphone.devices` を内部で分岐して **`MicPermissionStatus`** を返す。`RecordingController` は録音前に Gate を通し、`Denied`/`NoDevice` は `ErrorPresenter` 案内＋録音無効（`Result` で表現・クラッシュ禁止）。プラットフォーム分岐は Gate に閉じ込める。

B) 各コントローラが直接プラットフォーム API を叩く（Gate なし）。

C) Other（[Answer]: の後に記述）

[Answer]:A

### Question 4（Resilience — SaveSound の最小原子性）
保存（wav＋meta の対）の失敗耐性の設計は？（U3 最小・U4 で本格原子化）

A) (推奨) `IStorageService.SaveSound(SavedSound, AudioBuffer)` は **wav→meta の順に書き込み**、**meta 失敗時は書いた wav を削除**して**中途半端な対を残さない**（ベストエフォート原子性）。成功は wav＋meta 両立時のみ `Result.Ok`。失敗は `Result(IOError)`（録音は保持し再試行可）。ディレクトリ作成・例外捕捉を内包。**temp→rename の完全原子置換・破損復旧は U4**。

B) 書き込み順のみ保証し、失敗時のクリーンアップはしない（U4 に一任）。

C) Other（[Answer]: の後に記述）

[Answer]:A

### Question 5（Testability — 換算 Mapper の純粋関数化）
加工設定の換算（旧→新・離散化・正規化）の設計は？

A) (推奨) **`SoundEffectMapper`（静的・純粋関数）**に集約：旧 `SoundEffectSettings`↔新 `SoundEffectSettingsData` 変換、`cents→pitchSemitones`（100=1半音・丸め）、ノイズ連続(0〜1)→`NoiseLevel`(4段)、`reverbLevel(mB)→reverb(0〜1)` 正規化。副作用なしで **PBT 対象**（境界・丸めの一貫性）。`SoundEffectSettingsData→具体フィルタ値` の写像は `EffectChain` 側（技術寄り）に置き、Mapper は数値換算に限定。

B) 換算はコントローラ内にインラインで実装（Mapper 分離なし）。

C) Other（[Answer]: の後に記述）

[Answer]:A

### Question 6（DI/Logical Components — Rec 構成と一本化）
Rec の論理コンポーネント構成とサービス登録・一本化の設計は？

A) (推奨) `IAudioService` 本実装（例 `RecAudioService`／`AudioService`）を **`ServiceRegistry` に登録**（`AppManager` 起動時 or Rec シーン初期化時）。`RecScreenController`（`ScreenRootBase` 継承）が **`RecordingController`/`EffectPanelController`/`SavePromptController` を調停**。ロジックは POCO/静的へ寄せ MonoBehaviour 依存を最小化（テスト容易性）。離脱の破棄確認は `ConfirmDialog`、失敗通知は `ErrorPresenter`、遷移は `NavigationService` を再利用。`RecorderWithEffects`/`Scean` 等の重複は削除（US-TECH-03）。

B) `RecScreenController` に録音/加工/保存を集約（サブコンポーネント分割なし・最短実装）。

C) Other（[Answer]: の後に記述）

[Answer]:A

---

## D. 完了条件
- Q1〜Q6 に回答 → 矛盾チェック（曖昧回答は追質問）→ nfr-design-patterns.md / logical-components.md を生成 → 承認ゲート。
- U1/U2 の設計パターンを踏襲し、U3 固有の論理部品（EffectChain・RecordingClock・MicPermissionGate・SoundEffectMapper・SaveSound 経路・Rec コントローラ群）を明確化する。
