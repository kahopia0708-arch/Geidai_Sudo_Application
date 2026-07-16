# U3 Rec — Code Generation Plan（Part 1: 計画）

**ユニット**: U3 Rec（録音・加工・保存）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Code Generation（Part 1）
**プロジェクト種別**: Brownfield（Unity 6000.4.2f1 / URP / uGUI / C#）
**Workspace Root**: `/Users/maemoto/Documents/GitHub/Geidai_Sudo_Application`
**入力**: `../u3-rec/functional-design/*`, `../u3-rec/nfr-requirements/*`, `../u3-rec/nfr-design/*`, U1/U2 生成コード（`Assets/Scripts/Common|Services|Foundation`）

> 本プランは Code Generation の**唯一の正**（single source of truth）。Part 2 では上から順に実行し、各ステップ完了時に `[x]` を付ける。

---

## 0. 生成方針（重要）
- **アプリコードは Workspace 直下（`Assets/`）に生成**。ドキュメントのみ `aidlc-docs/`。
- **Brownfield 非破壊 / 一方向依存**: 新規 U3 コードは **`Geidai.Rec`**（新 asmdef）に分離。依存は `Rec → Services → Common` の一方向（＋`UnityEngine.UI`）。
- **Common/Services の後方互換な拡張のみ**:
  - `IStorageService` に `SaveSound(...)` を**追加**（既存メソッドは不変）＋ `StorageService` に U3 最小実装。
  - `SoundEffectMapper`（純粋関数）は `Geidai.Common.Audio` に配置（`PitchMath`/`SoundEffectSettingsData` にのみ依存・PBT 容易）。
- **`IAudioService` 実装は `Geidai.Rec` 側**に置き、`ServiceRegistry` 登録も Rec 側で行う（`Geidai.Services`→`Geidai.Rec` の循環依存を作らない。`AppManager` は変更しない）。
- **旧グローバル `SoundEffectSettings`（Assembly-CSharp）は参照しない**（asmdef 制約）。旧→新データ移行は U4/対象外。U3 は新形式（`sounds/{id}`）のみ。
- **UI は uGUI（`UnityEngine.UI`）で枠組みを生成**。TMP 差し替え・意匠は S さんハンドオフ（US-TECH-07）。
- **`.meta` は Unity が生成**（手動生成しない）。
- **シーン配線は公式 Unity MCP（`user-unity-mcp`）で実施**（US-TECH-05）。実 `Rec` シーンの GameObject 配線・旧コンポーネント差し替えは Step 15 で best-effort、未接続時は §5 フォールバック。
- **テストは生成中心**（本実行は Build & Test。純粋ロジック/保存は MCP `Unity_RunCommand` で同期スモーク）。

### 生成先フォルダ構成（新規/追記）
```
Assets/Scripts/
├── Rec/                 (Geidai.Rec.asmdef → refs Geidai.Common, Geidai.Services, UnityEngine.UI) ★新規
│   ├── RecordingState.cs           (enum: Idle,NoMic,Recording,Recorded,Playing,Saving,Saved)
│   ├── MicPermissionStatus.cs      (enum: Unknown,Granted,Denied,NoDevice)
│   ├── EffectKind.cs               (enum: Pitch,NoiseReduction,Timbre,Reverb)
│   ├── MicPermissionGate.cs        (静的: 権限確認/要求→MicPermissionStatus, iOS/Android/デバイス分岐)
│   ├── RecordingClock.cs           (POCO: 3.0s 経過計測・残り時間・IsDone)
│   ├── EffectChain.cs              (MonoBehaviour: AudioSource＋各AudioFilter束ね, Apply(settings))
│   ├── RecAudioService.cs          (IAudioService 実装: Microphone録音→AudioBuffer, Play/Stop)
│   ├── RecBootstrap.cs             (Rec初期化: ServiceRegistry に IAudioService 登録)
│   ├── RecordingController.cs      (録音制御: Gate→Start, Clock で3秒自動停止, StopRecording)
│   ├── EffectPanelController.cs    (加工UI→SoundEffectSettingsData→EffectChain.Apply, バイパス)
│   ├── SavePromptController.cs     (タイトル入力→SaveSound→結果通知)
│   └── RecScreenController.cs      (ScreenRootBase; RecordingState統括・サブ調停・離脱破棄確認)
├── Common/Audio/SoundEffectMapper.cs   (Geidai.Common) ★新規（純粋: 半音↔セント/ノイズ4段/reverb正規化）
├── Services/Storage/IStorageService.cs (修正: SaveSound 追加)
└── Services/Storage/StorageService.cs  (修正: SaveSound U3最小実装)

Assets/Scripts/Tests/EditMode/ (Geidai.Tests) ※追記（refs に Geidai.Rec 追加）
├── SoundEffectMapperTests.cs       (PBT: 半音↔セント往復/ノイズ4段境界/reverb正規化)
├── RecordingClockTests.cs          (3.0s 到達・残り時間・IsDone)
└── SaveSoundTests.cs               (SaveSound→LoadSound 対生成/失敗クリーンアップ)

削除:
├── Assets/Scripts/RecorderWithEffects.cs  (重複DSP実装 / US-TECH-03)
└── Assets/Scripts/Scean.cs                (空クラス・不要)

（据置＝Rec シーン再配線まで残す・MCP フォローアップで最終整理）:
└── VoiceRecordingSection.cs / WavUtility.cs / MySoundCollectionStorage.cs / SoundSavePaths.cs / SoundEffectSettings.cs
```

---

## 1. 対象ストーリー（U3 / トレース）
- **US-REC-01** 3秒録音・自動停止・権限フェイルセーフ → `RecordingController`, `RecordingClock`, `MicPermissionGate`, `RecAudioService`
- **US-REC-02** 加工（ピッチ/ノイズ/音色/リバーブ＋バイパス・非破壊プレビュー） → `EffectPanelController`, `EffectChain`, `SoundEffectMapper`, `PitchMath`
- **US-REC-03** 加工音の保存（生WAV＋設定の対保存・失敗時安全） → `SavePromptController`, `IStorageService.SaveSound`, `WavCodec`, `SavedSound`
- **US-TECH-03** 録音実装一本化・重複削除 → `Geidai.Rec` 集約、`RecorderWithEffects`/`Scean` 削除
- 併せて: NFR-03/06（性能・リアルタイム）, NFR-07/SECURITY-15（フェイルセーフ）, NFR-04（プライバシー）, NFR-08/09（保守・PBT）

## 2. 依存・インターフェース
- `Geidai.Rec` → `Geidai.Services`（`IAudioService`/`IStorageService`/`INavigationService`/`ServiceRegistry`）＋ `Geidai.Common`（`AudioBuffer`/`SoundEffectSettingsData`/`SavedSound`/`SoundClipMeta`/`WavCodec`/`PitchMath`/`Result`/`ScreenRootBase`/`ErrorPresenter`/`ConfirmDialog`/`UITheme`/`SafeLogger`）＋ `UnityEngine.UI`。
- `IStorageService.SaveSound` は `Geidai.Services` に追加（`WavCodec`=Common に依存・OK）。
- `SoundEffectMapper` は `Geidai.Common.Audio`（`PitchMath`/`SoundEffectSettingsData` のみ）。
- 外部 API/ネットワークなし（完全オフライン / NFR-02）。マイクは録音時のみ（`MicPermissionGate` 経由）。

### AudioSource 共有方針
- `EffectChain`（MonoBehaviour）が **AudioSource＋各 AudioFilter** を所有・公開。
- `RecAudioService.Play(buffer)` は EffectChain の AudioSource を用い、`AudioBuffer`→`AudioClip` 変換して再生。
- `RecordingController` が EffectChain / RecAudioService を同一 AudioSource で結線。

---

## 実行ステップ（Part 2 でこの順に実行）

### Step 0: MCP 接続確認・ベースライン（US-TECH-05）
- [x] `Unity_GetConsoleLogs` でベースライン取得（Error 0 を確認）
- [x] `user-unity-mcp` serverStatus=ready を確認（未接続時は §5 フォールバック）
- _トレース: US-TECH-05 / NFR-10_

### Step 1: Geidai.Rec asmdef とフォルダ
- [x] `Assets/Scripts/Rec/Geidai.Rec.asmdef`（references: Geidai.Common, Geidai.Services, UnityEngine.UI；autoReferenced=true）
- _トレース: NFR-08 / logical §3_

### Step 2: Rec 列挙（Geidai.Rec）
- [x] `Rec/RecordingState.cs`（Idle/NoMic/Recording/Recorded/Playing/Saving/Saved）
- [x] `Rec/MicPermissionStatus.cs`（Unknown/Granted/Denied/NoDevice）
- [x] `Rec/EffectKind.cs`（Pitch/NoiseReduction/Timbre/Reverb）
- _トレース: domain-entities §2 / NFR-06/07_

### Step 3: SoundEffectMapper（純粋・Common.Audio）
- [x] `Common/Audio/SoundEffectMapper.cs`（静的純粋関数）
  - `int CentsToSemitones(double cents)`（100=1半音・最寄り丸め・±12 クランプ）／`double SemitonesToCents(int)`
  - `NoiseLevel ContinuousToNoiseLevel(float v01)`（0/～0.33/～0.66/1→None/Low/Medium/High）／`float NoiseLevelToContinuous(NoiseLevel)`
  - `float NormalizeReverb(float mB)`（-10000〜0→0〜1）／`float DenormalizeReverb(float v01)`
  - ※旧グローバル `SoundEffectSettings` interop は含めない（U4/対象外）
- _トレース: nfr-design §5 / NFR-09 / US-TECH-03_

### Step 4: 保存契約拡張（Services 修正）
- [x] `Services/Storage/IStorageService.cs` に `Result SaveSound(SavedSound sound, AudioBuffer buffer)` を追加
- [x] `Services/Storage/StorageService.cs` に `SaveSound` 最小実装（`sounds/` 作成→`WavCodec.Encode`→`{id}.wav`→`JsonUtility`→`{id}.meta.json`。meta 失敗時は wav 削除＝ベストエフォート原子性。成功は両立時のみ `Result.Ok`、失敗は `Result(IOError)`。例外捕捉・`SafeLogger`）
- _トレース: US-REC-03 / BR-REC-30〜34 / nfr-design §4_

### Step 5: RecAudioService（IAudioService 実装・Geidai.Rec）
- [x] `Rec/RecAudioService.cs`（`IAudioService`：`StartRecording`＝`Microphone.Start`（3秒・44100・mono）／`StopRecording`＝データを固定長 `AudioBuffer`(132300) へコピー→`Result<AudioBuffer>`／`Play(buffer)`＝AudioBuffer→AudioClip 変換して AudioSource 再生／`Stop`。AudioSource は外部注入（EffectChain 共有）。例外は `Result` 化）
- [x] `Rec/RecBootstrap.cs`（Rec 初期化時に `ServiceRegistry.Register<IAudioService>(new RecAudioService(...))`。既登録ならスキップ）
- _トレース: US-REC-01 / NFR-03/07 / logical §1_

### Step 6: RecordingClock（POCO・Geidai.Rec）
- [x] `Rec/RecordingClock.cs`（`Start()`／`Tick(deltaTime)`→残り時間更新／`RemainingSeconds`／`Elapsed`／`IsDone`（>=3.0s）。純粋寄りでテスト可能）
- _トレース: US-REC-01 / NFR-03 / nfr-design §2_

### Step 7: MicPermissionGate（静的・Geidai.Rec）
- [x] `Rec/MicPermissionGate.cs`（`MicPermissionStatus Check()`／`RequestAsync`（コールバック）。iOS=`Application.RequestUserAuthorization(Microphone)`、Android=`Permission.RequestUserPermission`、デバイス有無=`Microphone.devices`。プラットフォーム分岐を内包）
- _トレース: US-REC-01(AC3) / SECURITY-15 / nfr-design §3_

### Step 8: EffectChain（MonoBehaviour・Geidai.Rec）
- [x] `Rec/EffectChain.cs`（AudioSource＋AudioLowPass/HighPass/Reverb/Distortion を保持・初期化キャッシュ。`Apply(SoundEffectSettingsData)`：pitch=`PitchMath.SemitonesToRatio`、音色=プリセット、reverb=換算、ノイズ=フィルタ、`EffectKind` バイパスは中立化。AudioSource を公開）
- _トレース: US-REC-02 / NFR-06 / nfr-design §1_

### Step 9: RecordingController（MonoBehaviour・Geidai.Rec）
- [x] `Rec/RecordingController.cs`（`MicPermissionGate`→権限確認、OK で `IAudioService.StartRecording`、コルーチンで `RecordingClock` を回し 3秒で `StopRecording`→`AudioBuffer` 保持、残り時間表示連携、`Denied/NoDevice` は `ErrorPresenter`＋録音無効。`Result` で失敗表現）
- _トレース: US-REC-01 / NFR-03/07 / SECURITY-15_

### Step 10: EffectPanelController（MonoBehaviour・Geidai.Rec）
- [x] `Rec/EffectPanelController.cs`（ピッチ/ノイズ/音色/リバーブ UI と各バイパス、全体一括を `SoundEffectSettingsData` にバインド、変更で `EffectChain.Apply`。UI 値↔モデルは `SoundEffectMapper` で換算）
- _トレース: US-REC-02 / NFR-06_

### Step 11: SavePromptController（MonoBehaviour・Geidai.Rec）
- [x] `Rec/SavePromptController.cs`（タイトル入力（任意・未入力は既定名）→`SoundClipMeta.CreateNew`→`SavedSound(meta,settings)`→`IStorageService.SaveSound(saved, buffer)`→成功「保存できたよ」/失敗 `ErrorPresenter(IOError)`）
- _トレース: US-REC-03 / BR-REC-30〜34 / NFR-07_

### Step 12: RecScreenController（ScreenRootBase・Geidai.Rec）
- [x] `Rec/RecScreenController.cs`（`ScreenRootBase` 継承。`RecBootstrap` でサービス登録、`RecordingState` 統括、`RecordingController`/`EffectPanelController`/`SavePromptController`/`EffectChain` を調停。状態別 UI 活性、`OnBackPressed`→未保存あれば `ConfirmDialog`（破棄確認）→`NavigationService.GoTo(Home)`）
- _トレース: US-REC-01〜03 / frontend-components / NFR-05/07_

### Step 13: 重複削除（US-TECH-03）
- [x] `Assets/Scripts/RecorderWithEffects.cs`（＋`.meta`）を削除（重複DSP・`WavUtility` 依存）
- [x] `Assets/Scripts/Scean.cs`（＋`.meta`）を削除（空クラス）
- [x] 旧録音一式（`VoiceRecordingSection`/`WavUtility`/`MySoundCollectionStorage`/`SoundSavePaths`/`SoundEffectSettings`）は **据置**（Rec シーン再配線＝MCP フォローアップで最終整理。code-summary に明記）
- _トレース: US-TECH-03 / NFR-08_

### Step 14: テスト生成（EditMode）
- [x] `Tests/EditMode/Geidai.Tests.asmdef` の references に `Geidai.Rec` を追加
- [x] `Tests/EditMode/SoundEffectMapperTests.cs`（PBT：半音↔セント往復、ノイズ4段の境界/単調性、reverb 正規化の範囲/往復）
- [x] `Tests/EditMode/RecordingClockTests.cs`（累積 Tick で 3.0s 到達・残り時間・`IsDone`）
- [x] `Tests/EditMode/SaveSoundTests.cs`（`SaveSound`→`LoadSound` で対生成・値一致、meta 失敗注入で wav が残らない、後始末）
- _トレース: NFR-09 / nfr-requirements §6_

### Step 15: MCP 検証・スモーク（best-effort）
- [x] `Unity_RunCommand` で `AssetDatabase.Refresh()`→`Unity_GetConsoleLogs`（**Error 0** 確認）
- [x] `Unity_RunCommand` で `SoundEffectMapper`/`RecordingClock` 同期スモーク（全 PASS）
- [x] `Unity_RunCommand` で `SaveSound` スモーク → **AI Assistant Run Command のファイル書込承認ガード**（"User interactions are not supported"）でブロック。コード不具合ではないため **EditMode `SaveSoundTests` で担保**（Test Runner 実行）に切替。
- [x] （best-effort）`Rec` シーンの新コンポーネント配線・旧差し替えは破壊回避のため **MCP フォローアップ**（§5・code-summary に手順明記）
- _実施結果: 取込後コンパイル Error 0（Geidai.Rec/Geidai.Tests 含む全5アセンブリのロード確認）。`SoundEffectMapper`（clamp+12=12・半音往復=5）／`RecordingClock`（done=True/ticks=31/elapsed=3・超過丸め）スモーク PASS。float 累積誤差の境界（30tick で 3.0 未達→31tick 完了）を確認し `RecordingClockTests` を堅牢化。_
- _トレース: US-TECH-05 / NFR-10_

### Step 16: コード生成サマリ（ドキュメント）
- [x] `aidlc-docs/construction/u3-rec/code/code-summary.md`（生成/修正/削除/据置ファイル一覧、名前空間・依存、MCP 検証結果、Rec シーン配線 MCP 手順、旧録音の最終整理 TODO、S さんハンドオフ点）
- _注: サマリのみ aidlc-docs 配下。コードは Assets 配下。_

### Step 17: ストーリー完了マーク
- [x] `stories.md` の US-REC-01/02/03・US-TECH-03 に U3 実装分の実装状況を注記（実シーン配線・旧最終整理の残タスクを明記）
- _トレース: US-REC / US-TECH-03_

---

## 3. スコープ外（U3 では実施しない）
- コレクション一覧/視聴/削除・永続化の原子的置換本実装（U4）。
- 旧保存データ（`MySoundCollection` 形式）の移行（U4/対象外）。
- お題連携（U5）、ゲーム用ピッチ出題（U6）。
- 旧録音一式（`VoiceRecordingSection` 等）の物理削除は Rec シーン MCP 再配線と同時（本ユニットでは新方式を提供し、`RecorderWithEffects`/`Scean` のみ削除）。

## 4. 完了条件
- Step 0〜17 のチェックボックスが全て `[x]`。
- 新規コードが `Geidai.Rec` で生成され、`Rec→Services→Common` の一方向依存でコンパイル Error 0。
- `IStorageService.SaveSound` 拡張が後方互換（既存が壊れない）。
- EditMode テスト（SoundEffectMapper PBT / RecordingClock / SaveSound）が生成済み、同期スモークがグリーン。
- `RecorderWithEffects.cs`/`Scean.cs` が削除され、ビルドに影響なし。
- code-summary.md に Rec シーン配線 MCP 手順・旧録音最終整理 TODO・S さんハンドオフ点が明記。

## 5. MCP 未接続時のフォールバック
1. 本ツールで `.cs`/`.asmdef` を `Assets/` に直接生成（コードは完成）。
2. MCP 検証（コンパイル確認・スモーク・シーン配線）は**保留チェック**として残す。
3. Unity 起動後に MCP で一括検証し、チェックを完了。

---

## 承認のお願い
本プラン（全 18 ステップ / Step 0〜17）で U3 のコード生成を進めてよいか、ご確認ください。
- **Request Changes**: ステップ/対象ファイル/方針の修正を指定
- **Continue（承認）**: Part 2（コード生成）を開始
