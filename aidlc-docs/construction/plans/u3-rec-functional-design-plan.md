# U3 Rec — Functional Design Plan（機能設計 計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U3 Rec（録音・加工・保存）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Functional Design（Part 1: Planning）
**入力**: `../../inception/application-design/unit-of-work.md`、`unit-of-work-story-map.md`、`../../inception/requirements/requirements.md`、`../../inception/user-stories/stories.md`、U1 成果物（`../u1-foundation/*`、`Assets/Scripts/Common|Services`）、既存録音実装（`Assets/Scripts/VoiceRecordingSection.cs` ほか）
**含むストーリー**: US-REC-01, US-REC-02, US-REC-03, US-TECH-03（対応要件: FR-05/06/07/08, NFR-03/06/08, SECURITY-15）

> 本ステージは**技術非依存の業務ロジック/ドメイン/業務ルール/画面構造**を詳細化する。DSP フィルタの具体値・リアルタイム反映方式・GC 対策などの技術パラメータは **NFR Design**、実シーンへの配線・削除は **Code Generation 以降（Unity MCP）** で扱う。
> **回答方法**: 各質問の `[Answer]:` に記号（例: `A`）を記入してください。合う選択肢が無ければ「Other」。各質問に「(推奨)」案あり。完了で「done」（または「全部推奨で」）。

---

## A. 実行チェックリスト（Part 2 で実行）
- [ ] `../u3-rec/functional-design/domain-entities.md`（U3 で扱うモデル：録音状態・録音バッファ・加工設定・保存音／U1 の `AudioBuffer`・`SoundEffectSettingsData`・`SoundClipMeta`・`SavedSound` を再利用し、既存 `SoundEffectSettings` との対応関係を定義）
- [ ] `../u3-rec/functional-design/business-logic-model.md`（録音→加工プレビュー→保存のふるまいとデータフロー、マイク権限フロー、加工の再適用モデル）
- [ ] `../u3-rec/functional-design/business-rules.md`（3秒固定録音・自動停止、加工パラメータ範囲、バイパス、保存の対保存・フェイルセーフ、録音実装一本化）
- [ ] `../u3-rec/functional-design/frontend-components.md`（Rec 画面：録音/再生/加工パネル/保存プロンプトの構造・状態・操作フロー・S さんハンドオフ点）
- [ ] 要件（FR-05〜08 / NFR-03/06/08 / SECURITY-15）・ストーリー（US-REC/US-TECH-03）とのトレース整合確認

## B. スコープ（U3 で確定する対象）
- **画面コントローラ（ふるまい）**: `RecScreenController`（U1 `ScreenRootBase` 継承）/ `RecordingController`（録音制御）/ `EffectPanelController`（加工 UI）/ `SavePromptController`（保存確認）
- **サービス実装**: U1 の `IAudioService`（`StartRecording`/`StopRecording`/`Play`/`Stop`）の **本実装**（`Geidai.Rec` 側 or `Geidai.Services.Audio` 側かは Q6）
- **フロー**: 録音（3秒・自動停止）→ 加工プレビュー（リアルタイム体感）→ 保存（WAV＋設定を対で永続化）
- **業務ルール**: 3秒固定・自動停止、マイク権限フェイルセーフ、加工範囲/バイパス、保存の原子性は最小（堅牢化は U4）
- **U1 依存の利用**: `AudioBuffer`、`SoundEffectSettingsData`、`SoundClipMeta`/`SavedSound`、`WavCodec`、`PitchMath`、`IStorageService`（＋拡張、Q5）、`ErrorPresenter`、`NavigationService`
- **スコープ外**: コレクション一覧/視聴/削除（U4）、永続化の堅牢化=原子的置換/破損フォールバック本実装（U4）、お題連携（U5）、ゲーム用ピッチ加工出題（U6）

## C. 既存実装（brownfield）との関係（要判断の背景）
- **録音・加工エンジン**: `VoiceRecordingSection.cs`（Unity 標準 AudioFilter／ToggleRecording・各 Set*/Toggle*・Save）が実質的な現行実装。`maxRecordSeconds=10`・手動 Toggle・44100Hz。ピッチは `AudioSource.pitch`（再生速度）で非破壊反映。
- **重複実装**: `RecorderWithEffects.cs`（独自 DSP）、`Scean.cs` 等は US-TECH-03 で「整理（参照除去）」対象。
- **保存経路（旧）**: `MySoundCollectionStorage.SaveSoundWithSettings` → `persistentDataPath/MySoundCollection/sound_YYYYMMDD_HHMMSS.wav`＋`.json`（グローバル名前空間 `SoundEffectSettings`）。`WavUtility`/`SoundSavePaths` に依存。
- **保存経路（新・U1）**: `StorageService` は `persistentDataPath/sounds/{id}.wav`＋`{id}.meta.json`（`SavedSound` = `SoundClipMeta`＋`SoundEffectSettingsData`）。**ただし `IStorageService` に保存メソッド（SaveSound）が未定義**（現状 `LoadProfile/SaveProfile/ListSounds/LoadSound` のみ）→ Q5 で拡張方針を確認。
- **加工モデルの差**: 旧 `SoundEffectSettings`（pitchCents・tonePresetIndex 0-2・noiseReductionAmount 0-1・lowPass/highPass/reverbLevel/echo/distortion…）↔ U1 `SoundEffectSettingsData`（pitchSemitones ±12・NoiseLevel[None/Low/Medium/High]・TimbreType[Original/Soft/Hard]・reverb 0-1）。US-REC-02 の要求（リバーブ / ノイズ低減[0/弱/中/強] / ピッチ / 音色[なし/ロボット/コーラス系]）との整合を Q2 で確認。

---

## D. 設計に関する質問（Q1〜Q7）

## Question 1（録音仕様：長さ・自動停止・フォーマット）
録音の基本仕様は？（US-REC-01「3秒間録音し自動停止」／NFR-03）

A) (推奨) **3秒固定・自動停止／44100Hz・モノラル・16bit PCM**（U1 `AudioBuffer`＝132300サンプルに一致）。録音ボタンで開始→3秒経過で自動停止→自動でプレビュー可能に。録り直しは再度録音で上書き。既存 `maxRecordSeconds=10`・手動 Toggle は 3秒固定へ統一。

B) 3秒を基本とするが、**上限3秒の手動停止も許可**（早く止めたら短い録音）。フォーマットは A と同じ。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 2（加工モデル：正準データと UI マッピング）
保存・UI で扱う加工設定の「正準モデル」は？（US-REC-02／既存 `SoundEffectSettings` と U1 `SoundEffectSettingsData` の差を吸収）

A) (推奨) **U1 `SoundEffectSettingsData` を正準**とする：**ピッチ（半音 ±12）／ノイズ低減（None/Low/Medium/High）／音色（Original/Soft/Hard）／リバーブ（0〜1）**。UI の「音色[なし/ロボット/コーラス系]」は TimbreType にマッピング（なし=Original・ロボット=Hard・コーラス系=Soft 等、命名は S さん調整可）。旧 `SoundEffectSettings` の echo/distortion/lowpass/highpass は**音色プリセットの内部実装**として畳み込み、保存モデルには出さない（MVP 簡素化）。各加工に**バイパス（on/off）**を持たせる。

B) 既存 `SoundEffectSettings`（cents・echo・distortion 等を含む詳細）をそのまま正準にして保存する（表現力優先・簡素化しない）。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 3（加工の適用方式：非破壊プレビュー vs 焼き込み保存）
加工音の「保存」の意味は？（US-REC-03「加工した音を保存／加工設定が音声と対で保存」）

A) (推奨) **非破壊**：保存する WAV は**録音そのまま（生）**、加工は `SoundEffectSettingsData` として**対で保存**し、**再生時に再適用**（既存 `VoiceRecordingSection` と同じ思想）。利点＝再編集可能・軽量・要件「設定が音声と対で保存」に忠実。プレビューは AudioSource＋AudioFilter でリアルタイム体感（NFR-06）。

B) **焼き込み**：加工結果をレンダリングして WAV 自体に反映して保存（設定も一応対で保存）。利点＝どこで再生しても同じ音。欠点＝再編集不可・処理コスト。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 4（マイク権限・フェイルセーフのふるまい）
マイク権限・デバイス不在時のふるまいは？（US-REC-01 AC3／SECURITY-15）

A) (推奨) **録音開始時に権限確認**。未許可なら要求ダイアログ、**拒否／デバイス無しなら録音を行わず** `ErrorPresenter` で子ども向けに平易案内（例:「マイクが つかえないみたい」）＋録音ボタンを無効化。**いかなる場合もクラッシュしない**（`Result` で失敗を返す）。録音音声は端末外へ送信しない（NFR-04）。

B) 起動/画面表示時に先回りで権限要求してから録音可否を決める。以降は A と同じ。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 5（保存 IF：`IStorageService` 拡張 vs U3 ローカル保存）
録音物の保存経路は？（U1 `IStorageService` に保存メソッドが未定義）

A) (推奨) **`IStorageService` に保存契約を追加**（例: `Result SaveSound(SavedSound sound, AudioBuffer buffer)`）し、`StorageService` に **U3 最小実装**（`sounds/{id}.wav`＝`WavCodec` エンコード＋`{id}.meta.json`）。**原子的置換・破損フォールバックの堅牢化は U4**。旧 `MySoundCollectionStorage`/`SoundSavePaths`（MySoundCollection フォルダ）は新形式へ統一（旧データ移行は U4 or 対象外）。保存失敗は `Result(IOError)` で安全通知（US-REC-03 AC3）。

B) `IStorageService` は変更せず、U3 内に保存ヘルパーを持たせる（後で U4 が集約）。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 6（録音実装の一本化と配置：US-TECH-03）
録音・加工実装の統合方針と `IAudioService` 実装の置き場所は？

A) (推奨) **`IAudioService` の本実装を新設**（録音3秒・停止で `AudioBuffer` 返却・再生/停止）。既存 `VoiceRecordingSection` の**加工適用ロジック（AudioFilter 設定）を新コンポーネントへ移植**して一本化。`RecorderWithEffects.cs`・`Scean.cs` 等の重複/不要実装は **U3 で削除**（参照除去・ビルド影響なし）。実装は `Geidai.Rec` アセンブリ（`Geidai.Services.Audio` 実装 or `Geidai.Rec` 内実装かは NFR/コード生成で確定）。実シーン配線・旧コンポーネント差し替えは Code Generation 以降で Unity MCP。

B) 既存 `VoiceRecordingSection` を**そのまま中核**として残し、その周囲に薄いコントローラを被せて統合（新規実装を最小化）。`RecorderWithEffects` 等のみ削除。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 7（Rec 画面の構成・保存フロー・エッジケース）
Rec 画面の画面構成と保存フローは？

A) (推奨) 1画面に **録音ボタン＋残り時間表示 / 再生・停止 / 加工パネル（ピッチ・ノイズ低減・音色・リバーブ＋各バイパス）/ 保存ボタン** を配置。保存押下→`SavePromptController` で**任意タイトル入力＋確認**（未入力は日時等の既定名）。保存後は「保存できたよ」通知。**もどる/ホーム**でホームへ（未保存の録音がある場合は破棄確認）。存在しない遷移先は `Result(NotFound)` で安全処理。加工の見た目（色/アイコン/配置）は S さんハンドオフ（US-TECH-07）。

B) 録音画面と加工画面を分割（録音→次へ→加工/保存）。その他は A と同じ。

C) Other（[Answer]: の後に記述）

[Answer]:

---

## E. 完了条件
- Q1〜Q7 に回答 → 矛盾チェック（曖昧回答は追質問）→ domain-entities / business-logic-model / business-rules / frontend-components を生成 → 承認ゲート。
- 生成物は技術非依存（DSP 数値・リアルタイム方式・シーン配線は NFR Design / Code Generation で扱う）。
- 既存実装との差分（3秒固定化・加工モデル統一・保存形式統一・録音実装一本化・重複削除）が設計に反映されている。
