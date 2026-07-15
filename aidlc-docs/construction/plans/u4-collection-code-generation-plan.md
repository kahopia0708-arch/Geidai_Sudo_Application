# U4 Persistence/Collection — Code Generation Plan（Part 1: 計画）

**ユニット**: U4 Persistence/Collection（永続化・コレクション）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Code Generation（Part 1）
**プロジェクト種別**: Brownfield（Unity 6000.4.2f1 / URP / uGUI / C#）
**Workspace Root**: `/Users/maemoto/Documents/GitHub/Geidai_Sudo_Application`
**入力**: `../u4-collection/functional-design/*`, `../u4-collection/nfr-requirements/*`, `../u4-collection/nfr-design/*`, U1〜U3 生成コード（`Assets/Scripts/Common|Services|Foundation|Rec`）

> 本プランは Code Generation の**唯一の正**（single source of truth）。Part 2 では上から順に実行し、各ステップ完了時に `[x]` を付ける。

---

## 0. 生成方針（重要）
- **アプリコードは Workspace 直下（`Assets/`）に生成**。ドキュメントのみ `aidlc-docs/`。
- **Brownfield 非破壊 / 一方向依存**: 新規 U4 コードは **`Geidai.Collection`**（新 asmdef）に分離。依存は `Collection → Services → Common` の一方向（＋`UnityEngine.UI`）。**`Geidai.Collection` は `Geidai.Rec` に依存しない**（視聴は Services 共有 Audio 経由 / Q4=A）。
- **Common/Services の後方互換な拡張のみ**（既存シグネチャ不変）:
  - `SoundClipMeta` に `title`/`photoFileName`/`memo`/`nickname` を**追記**（`JsonUtility` は欠損既定値＝後方互換 / Q2=A）。
  - `IStorageService` に `DeleteSound`／`SaveMeta`／`SavePhoto`／`RemovePhoto` を**追加**、既存 `SaveProfile`/`SaveSound`/`ListSounds` を **`AtomicFile` 経由の原子的置換・破損スキップへ内部強化**（Q1=A・Q2=A）。
  - `IAudioService` に `Play(AudioBuffer, SoundEffectSettingsData)` と `ApplyEffects(...)` を**追加**（既存 `Play(AudioBuffer)`/録音系は不変 / Q4=A）。
- **共有 Audio を Services 層へ（Q4=A）**: `EffectChain` を `Geidai.Rec` → **`Geidai.Services.Audio` へ移設**（ロジック不変・名前空間のみ変更）。`AudioService`（IAudioService 共有実装）を Services に新設し、**録音（Microphone）は U3 `RecAudioService` から挙動不変で移設**、再生はエフェクト再適用対応。自前 AudioSource（`DontDestroyOnLoad`）でシーンまたぎ発音。`AppManager` で登録。`RecAudioService` は削除し Rec は共有実装を利用（**録音側挙動は不変**）。
- **純粋部品は `Geidai.Common`**（PBT 容易）: `CollectionQuery`／`CollectionFilter`／`LoadOutcome` を `Geidai.Common.Collection` に配置。
- **写真取得は抽象**（Q5=A）: `IPhotoPicker`（Services IF）＋U4 `StubPhotoPicker`。実機ピッカーは MCP/プラグイン フォローアップ。**クラウド送信なし・PII 非ログ**（`SafeLogger`）。
- **UI は uGUI（`UnityEngine.UI`）で枠組みを生成**（`ScrollRect`/`Dropdown`/`InputField`/`Button`/`Text`/`Image`）。TMP 差し替え・意匠は S さんハンドオフ（US-TECH-07）。`data-testid` 相当として GameObject 名を安定命名。
- **`.meta` は Unity が生成**（手動生成しない）。
- **シーン配線は公式 Unity MCP（`user-unity-mcp`）で実施**（US-TECH-05）。実 Collection シーンの GameObject 配線・旧 `MySoundCollectionStorage`/`SoundSavePaths`/`GoToSoundCollection` 差し替えは Step 18 で best-effort、未接続時は §5 フォールバック。
- **テストは生成中心**（本実行は Build & Test。純粋ロジックは MCP `Unity_RunCommand` で同期スモーク。ファイル I/O は U3 同様 MCP 承認ガードの可能性 → EditMode テストで担保）。

### 生成先フォルダ構成（新規/追記/削除）
```
Assets/Scripts/
├── Common/
│   ├── Models/SoundClipMeta.cs         (修正: title/photoFileName/memo/nickname 追加・後方互換)
│   └── Collection/                      ★新規（純粋・PBT対象・Geidai.Common）
│       ├── CollectionQuery.cs           (yearMonth, keyword)
│       ├── CollectionFilter.cs          (静的純粋: Filter(items, query))
│       └── LoadOutcome.cs               (items + skippedCount)
├── Services/
│   ├── IO/AtomicFile.cs                 ★新規（静的: WriteAllBytesAtomic/WriteAllTextAtomic）
│   ├── Storage/IStorageService.cs       (修正: DeleteSound/SaveMeta/SavePhoto/RemovePhoto 追加)
│   ├── Storage/StorageService.cs        (修正: AtomicFile 統一・破損スキップ強化・新メソッド実装)
│   ├── Audio/IAudioService.cs           (修正: Play(buffer,settings) / ApplyEffects 追加)
│   ├── Audio/EffectChain.cs             ★移設（Geidai.Rec → Geidai.Services.Audio・ロジック不変）
│   ├── Audio/AudioService.cs            ★新規（IAudioService 共有実装: 録音移設＋加工再生・自前AudioSource）
│   ├── Media/IPhotoPicker.cs            ★新規（IF）
│   └── Media/StubPhotoPicker.cs         ★新規（U4 スタブ）
│   └── AppManager.cs                    (修正: 共有 AudioService を ServiceRegistry へ登録)
├── Collection/                          ★新規 asmdef Geidai.Collection
│   ├── Geidai.Collection.asmdef         (refs: Geidai.Common, Geidai.Services, UnityEngine.UI)
│   ├── CollectionState.cs               (enum: Loading/Empty/Listing/Playing/Detail/Editing/Confirm)
│   ├── SoundItemViewModel.cs            (投影 VM: id/displayTitle/createdAtIso/hasPhoto)
│   ├── SoundListView.cs                 (MonoBehaviour: 一覧描画・仮想化可能・タップ通知)
│   ├── SoundListItemView.cs             (MonoBehaviour: 1項目・Play/Open)
│   ├── FilterSearchController.cs        (MonoBehaviour: CollectionQuery 保持・純粋 Filter 適用)
│   ├── SoundDetailController.cs         (MonoBehaviour: 詳細/編集/再生/削除)
│   ├── CollectionBootstrap.cs           (初期化: 共有サービス確保)
│   └── CollectionScreenController.cs    (ScreenRootBase: 状態統括・子調停)
├── Rec/ (修正)
│   ├── RecAudioService.cs               ★削除（共有 AudioService へ移設）
│   ├── RecBootstrap.cs                  (修正: 共有 IAudioService を解決・SetPlaybackSource 廃止)
│   ├── RecordingController.cs           (修正: 共有 IAudioService を利用)
│   └── EffectPanelController.cs         (修正: EffectChain 直接保持 → IAudioService.ApplyEffects 利用)

Assets/Scripts/Tests/EditMode/ (Geidai.Tests) ※追記（refs に Geidai.Collection 追加）
├── CollectionFilterTests.cs            (PBT: 結果⊆入力・条件空→全件・冪等・AND・月/検索)
├── SavedSoundJsonTests.cs              (PBT: メタ往復・拡張フィールド後方互換＝欠損既定値)
├── AtomicFileTests.cs                  (成功で新値・失敗で旧値維持・tmp 後始末)
└── StorageCollectionTests.cs          (ListSounds 破損/対欠損スキップ・DeleteSound 対削除・SaveMeta が settings 保持)

据置（Collection シーン再配線まで残す・MCP フォローアップで最終整理）:
└── MySoundCollectionStorage.cs / SoundSavePaths.cs / GoToSoundCollection.cs / VoiceRecordingSection.cs / WavUtility.cs / SoundEffectSettings.cs
```

---

## 1. 対象ストーリー（U4 / トレース）
- **US-COL-01** コレクション一覧・視聴・削除 → `CollectionScreenController`, `SoundListView`, `SoundDetailController`, `IAudioService.Play(buffer,settings)`, `IStorageService.DeleteSound`
- **US-COL-02** メタ編集（タイトル/写真/メモ） → `SoundDetailController`, `IStorageService.SaveMeta`/`SavePhoto`/`RemovePhoto`, `IPhotoPicker`, `SoundClipMeta` 拡張
- **US-COL-03** 月別絞込・キーワード検索 → `FilterSearchController`, `CollectionQuery`, `CollectionFilter`（純粋・PBT）
- **US-COL-04** 破損スキップ・空フォールバック → `StorageService.ListSounds`（強化）, `LoadOutcome`, `EmptyState`
- **US-TECH-06** 堅牢な永続化（原子的置換） → `AtomicFile`, `StorageService`（`SaveProfile`/`SaveSound`/`SaveMeta`/写真 を原子化）
- 併せて: NFR-COL-P（性能）, NFR-COL-R（堅牢性=主眼）, NFR-COL-Priv（PII）, NFR-COL-T（PBT）, NFR-COL-M（保守・共有再生）, NFR-COL-UI（レスポンシブ）

## 2. 依存・インターフェース
- `Geidai.Collection` → `Geidai.Services`（`IStorageService`/`IAudioService`/`INavigationService`/`IPhotoPicker`/`ServiceRegistry`）＋ `Geidai.Common`（`SavedSound`/`SoundClipMeta`/`SoundEffectSettingsData`/`AudioBuffer`/`WavCodec`/`Result`/`CollectionQuery`/`CollectionFilter`/`LoadOutcome`/`ScreenRootBase`/`ConfirmDialog`/`ErrorPresenter`/`UITheme`/`SafeLogger`）＋ `UnityEngine.UI`。
- `Geidai.Services` 内で完結: `AtomicFile`（`System.IO`）／`AudioService`＋`EffectChain`（`UnityEngine` オーディオ）／`IPhotoPicker`。**`Geidai.Services` → `Geidai.Rec` 依存は作らない**（EffectChain は Rec から Services へ移設）。
- `Geidai.Rec` → `Geidai.Services`（共有 `IAudioService`/`EffectChain`）＋ `Geidai.Common`。RecAudioService 削除後も一方向を維持。
- 外部 API/ネットワークなし（完全オフライン / NFR-02）。写真はローカルのみ（`IPhotoPicker`）。

### 共有 Audio の設計（Q4=A / M4）
- `AudioService`（Services）が **自前の再生リグ**（GameObject＝AudioSource＋`EffectChain`、`DontDestroyOnLoad`）を遅延生成し、シーンをまたいで発音可能。
- `Play(AudioBuffer)`＝素の再生（後方互換）。`Play(AudioBuffer, SoundEffectSettingsData)`＝保存エフェクトを全 on で再適用して再生（Collection 視聴）。
- `ApplyEffects(settings, allOn, pitchOn, noiseOn, timbreOn, reverbOn)`＝Rec のライブプレビュー用（同一 `EffectChain.Apply` を駆動 → 挙動不変）。
- 録音（`StartRecording`/`StopRecording`）は U3 `RecAudioService` のロジックを**そのまま移設**（Microphone・固定長 `AudioBuffer` 再利用）。

---

## 実行ステップ（Part 2 でこの順に実行）

### Step 0: MCP 接続確認・ベースライン（US-TECH-05）
- [x] `Unity_GetConsoleLogs` でベースライン取得（Error 0 を確認）
- [x] `user-unity-mcp` serverStatus=ready を確認（未接続時は §5 フォールバック）
- _実施結果: serverStatus=ready・errorCount 0（ベースライン健全）_
- _トレース: US-TECH-05 / NFR-10_

### Step 1: SoundClipMeta 後方互換拡張（Common 修正）
- [x] `Common/Models/SoundClipMeta.cs` に `title`/`photoFileName`/`memo`/`nickname`（string・既定 ""）を追加。既存 `id`/`displayName`/`createdAtIso`/`wavFileName` は不変。`CreateNew` は既定値で初期化（後方互換）。
- _トレース: US-COL-02 / FR-10 / domain-entities §2 / Q2=A_

### Step 2: 純粋コレクション部品（Common/Collection）★新規
- [x] `Common/Collection/CollectionQuery.cs`（`yearMonth`(string・空=全月)／`keyword`(string・空=無条件)。struct・`Empty`/`IsEmpty`）
- [x] `Common/Collection/LoadOutcome.cs`（`List<SavedSound> items`／`int skippedCount`）
- [x] `Common/Collection/CollectionFilter.cs`（静的純粋 `List<SavedSound> Filter(IReadOnlyList<SavedSound>, CollectionQuery)`：月導出 `createdAtIso`→`YYYY-MM`（`ToYearMonth`）・検索 `title/memo/nickname` 正規化[Trim/小文字]部分一致・AND 合成。null 安全・順序保持・副作用なし）
- _トレース: US-COL-03/04 / nfr-design §5 / NFR-COL-T1 / Q5=A_

### Step 3: AtomicFile（Services/IO）★新規
- [x] `Services/IO/AtomicFile.cs`（静的）
  - `Result WriteAllBytesAtomic(string path, byte[] data)` / `Result WriteAllTextAtomic(string path, string text)` / `Result CopyAtomic(string src, string dest)`
  - 内部: `Directory.CreateDirectory` → `{path}.tmp` へ書込＋`Flush(true)`/`Dispose` → `File.Replace`（既存）/`File.Move`（新規）で原子的置換 → 例外時 `.tmp` 破棄・`Result.Fail(IOError)`（本ファイルは不変）
- _トレース: US-TECH-06 / nfr-design §1 / NFR-COL-R1 / Q1=A_

### Step 4: 永続化 IF 拡張＋StorageService 強化（Services 修正）
- [x] `Services/Storage/IStorageService.cs` に追加（既存不変）:
  - `Result DeleteSound(string id)`（`{id}.wav`＋`{id}.meta.json`＋`{id}.photo.*` 一括削除・欠損無視）
  - `Result SaveMeta(SoundClipMeta meta)`（既存 `{id}.meta.json` を読み settings 保持のまま meta 差替→原子的置換。無ければ新規）
  - `Result<string> SavePhoto(string id, string sourceTempPath)`（`sounds/{id}.photo.<ext>` へ `AtomicFile` で原子的コピー→ `photoFileName` を返す）
  - `Result RemovePhoto(string id)`（写真ファイル削除・欠損無視）
- [x] `Services/Storage/StorageService.cs` 強化:
  - `SaveProfile`/`SaveSound`/`SaveMeta`/写真 の全書込を `AtomicFile` 経由へ（原子的置換）。`SaveSound` は wav→meta を原子的書込＋対整合（新規時 meta 失敗で wav 削除）。
  - `ListSounds`/`LoadSound` の破損・対 wav 欠損スキップを維持（`SafeLogger` 非PII）。`ListSoundsDetailed()` で `LoadOutcome` 返却（IF は `Result<List<SavedSound>>` を維持）。
  - `DeleteSound`（wav+meta+photo 一括）/`SaveMeta`（settings 保持）/`SavePhoto`（拡張子検証・原子的コピー）/`RemovePhoto` を実装（例外は `Result` 化・クラッシュ禁止）。
  - 追加読取: `LoadPhoto(id)`（サムネ/詳細用バイト列）・`LoadSoundBuffer(id)`（wav→`AudioBuffer` デコード＝視聴用）を実装（写真表示・保存音再生の完結に必要）。
- _トレース: US-COL-01/02/04・US-TECH-06 / nfr-design §1/§2/§6 / NFR-COL-R1〜R4 / Q1=A/Q2=A/Q6=A_

### Step 5: IAudioService 拡張（Services 修正）
- [x] `Services/Audio/IAudioService.cs` に追加（既存 `StartRecording`/`StopRecording`/`Play(AudioBuffer)`/`Stop` は不変）:
  - `Result Play(AudioBuffer buffer, SoundEffectSettingsData settings)`（保存エフェクト全 on 再適用再生）
  - `Result ApplyEffects(SoundEffectSettingsData settings, bool allOn, bool pitchOn, bool noiseOn, bool timbreOn, bool reverbOn)`（ライブプレビュー用・非破壊）
  - `bool IsPlaying { get; }`（再生完了検知・Rec/Collection 共用）を追加
- _トレース: US-COL-01 / nfr-design §4 / NFR-COL-M4 / Q4=A_

### Step 6: EffectChain 移設（Rec → Services.Audio）
- [x] `Assets/Scripts/Rec/EffectChain.cs`（＋`.meta`）を削除し、`Assets/Scripts/Services/Audio/EffectChain.cs` として再配置。名前空間を `Geidai.Rec` → `Geidai.Services.Audio` に変更（**ロジックは不変**：AudioSource＋各 AudioFilter・`Apply(settings, allOn, pitchOn, noiseOn, timbreOn, reverbOn)`・`SoundEffectMapper`/`PitchMath` 利用）。
- _トレース: US-TECH-03 / nfr-design §4 / NFR-COL-M4_

### Step 7: AudioService 共有実装（Services.Audio）★新規
- [x] `Services/Audio/AudioService.cs`（`IAudioService`）
  - **再生リグ**: 遅延生成の GameObject（`AudioSource`＋`EffectChain`、`DontDestroyOnLoad`）を内部所有（シーンまたぎ発音）。
  - **録音**: U3 `RecAudioService` の `StartRecording`/`StopRecording` を挙動不変で移設（Microphone・4秒確保・固定長 `AudioBuffer`(132300) 再利用・0 埋め）。
  - **再生**: `Play(AudioBuffer)`＝素再生（エフェクト中立化）／`Play(AudioBuffer, settings)`＝`EffectChain.Apply(settings, all on)`後に再生／`Stop`／`IsPlaying`。
  - `ApplyEffects(...)`＝所有 `EffectChain.Apply(...)` を駆動。`GetPlaybackSource()` も提供。
  - 例外は全て `Result` 化（SECURITY-15）。
- _トレース: US-COL-01・US-REC-01/02 / nfr-design §4 / NFR-COL-M4 / Q4=A_

### Step 8: AppManager に共有 AudioService 登録（Services 修正）
- [x] `Services/AppManager.cs` の `Bootstrap` に `if (!ServiceRegistry.IsRegistered<IAudioService>()) ServiceRegistry.Register<IAudioService>(new AudioService());` を追加（Storage/Navigation/Content と同様・後方互換）。
- _トレース: NFR-COL-M4 / logical §2.8_

### Step 9: Rec を共有実装へ切替（Rec 修正・録音側挙動不変）
- [x] `Assets/Scripts/Rec/RecAudioService.cs`（＋`.meta`）を削除（共有 `AudioService` へ移設済み）。
- [x] `Rec/RecBootstrap.cs`：`EnsureAudioService()` を「共有 `IAudioService` を `ServiceRegistry.Resolve`（無ければ `AudioService` を登録）して返す」に変更。`SetPlaybackSource` 依存を廃止（再生リグは AudioService が所有）。
- [x] `Rec/RecordingController.cs`：`IAudioService`（Start/Stop）を利用（元々 IF 依存のため変更最小）。
- [x] `Rec/EffectPanelController.cs`：`EffectChain` 直接保持を廃止し `Init(IAudioService)`＋`IAudioService.ApplyEffects(...)` でプレビュー反映（`using Geidai.Services.Audio`）。UI 値↔モデル換算は `SoundEffectMapper` を継続利用。
- [x] `Rec/RecScreenController.cs`：`EffectChain`/`RecAudioService` 参照を廃止し共有 `IAudioService` を利用（`effectPanel.Init(_audio)`・再生完了検知は `_audio.IsPlaying`）。
- _注: 録音の Microphone 挙動・加工の `EffectChain.Apply` は不変。実 Rec シーン配線更新は Step 18（MCP フォローアップ）。_
- _トレース: US-REC-01/02・US-TECH-03 / NFR-COL-M4_

### Step 10: 写真ピッカー抽象（Services/Media）★新規
- [x] `Services/Media/IPhotoPicker.cs`（`void Pick(Action<Result<string>> onResult)`＝一時パスを返す抽象）
- [x] `Services/Media/StubPhotoPicker.cs`（U4 スタブ：`FixedTempPath` 設定時は成功／未設定は `Result.Fail(NotImplemented)`。クラウド送信なし）
- _トレース: US-COL-02 / nfr-design §5 / NFR-COL-Priv1 / Q5=A_

### Step 11: Geidai.Collection asmdef とフォルダ ★新規
- [x] `Assets/Scripts/Collection/Geidai.Collection.asmdef`（references: Geidai.Common, Geidai.Services, UnityEngine.UI；autoReferenced=true；**Geidai.Rec を参照しない**）
- _トレース: NFR-COL-M1 / logical §3 / Q6=A_

### Step 12: Collection 列挙・ビューモデル（Geidai.Collection）
- [x] `Collection/CollectionState.cs`（enum: Loading/Empty/Listing/Playing/Detail/Editing/Confirm）
- [x] `Collection/SoundItemViewModel.cs`（`id`/`displayTitle`(title 空なら日付)/`createdAtIso`/`hasPhoto`。`SavedSound`→投影の静的 `From(SavedSound)`＋`FormatDate`）
- _トレース: frontend-components §2/§3 / NFR-COL-P1_

### Step 13: 一覧ビュー（Geidai.Collection）
- [x] `Collection/SoundListItemView.cs`（MonoBehaviour: Title/Date/PhotoThumb/PlayButton/OpenButton。`Bind(vm,onOpen,onPlay)`＋`SetThumbnail(sprite)` でサムネ遅延差込）
- [x] `Collection/SoundListView.cs`（MonoBehaviour: `contentRoot`＋item プール描画・`ThumbnailLoader`(id→Sprite) 遅延読み・空状態トグル・`ItemOpenRequested`/`ItemPlayRequested` 通知）
- [x] `Collection/CollectionSprites.cs`（写真バイト列→Sprite ヘルパー・端末内のみ）
- _トレース: US-COL-01 / frontend-components §1/§3.2 / NFR-COL-P1/UI1 / Q3=A_

### Step 14: 絞込・検索（Geidai.Collection）
- [x] `Collection/FilterSearchController.cs`（MonoBehaviour: `monthDropdown`/`keywordInput`/`clearButton` → `SetAvailableMonths`／`BuildQuery`→`CollectionQuery`／変更で `QueryChanged` 発火。絞込実行は画面統括が `CollectionFilter.Filter` を呼ぶ）
- _トレース: US-COL-03 / frontend-components §3.4 / NFR-COL-T1 / Q5=A_

### Step 15: 詳細・編集（Geidai.Collection）
- [x] `Collection/SoundDetailController.cs`（MonoBehaviour: 詳細表示・title/memo 編集・写真変更（`IPhotoPicker`→`SavePhoto`→`SaveMeta`）/削除（`RemovePhoto`→`SaveMeta`）・視聴（`_storage.LoadSoundBuffer`→`IAudioService.Play(buffer,settings)`）・保存（`SaveMeta`＝原子的置換）・削除起動（`ConfirmDialog`→`DeleteSound`）。失敗は `ErrorPresenter`。`MetaChanged`/`Deleted`/`Closed` を通知）
- _トレース: US-COL-01/02 / frontend-components §3.3/§5 / NFR-COL-R1/U1 / Q2=A/Q6=A_

### Step 16: 画面統括・初期化（Geidai.Collection）
- [x] `Collection/CollectionBootstrap.cs`（`IPhotoPicker` を確保。未登録なら `StubPhotoPicker` を登録。Storage/Audio は AppManager 登録前提）
- [x] `Collection/CollectionScreenController.cs`（`ScreenRootBase` 継承。`OnShow`→`ListSounds`→月抽出→`CollectionFilter`→描画、`SoundListView`/`FilterSearchController`/`SoundDetailController` を調停、サムネキャッシュ、`OnBackPressed`→Detail 中は一覧へ/一覧なら `NavigationService.GoTo(Home)`、破損スキップ・全失敗は空状態フォールバック）
- _トレース: US-COL-01/04 / frontend-components §3.1 / NFR-COL-R2/R3 / Q6=A_

### Step 17: テスト生成（EditMode）
- [x] `Tests/EditMode/Geidai.Tests.asmdef` の references に `Geidai.Collection` を追加
- [x] `Tests/EditMode/CollectionFilterTests.cs`（PBT: 結果⊆入力・条件空→全件・冪等・順序保持・AND 合成・月導出/検索の正当性）
- [x] `Tests/EditMode/SavedSoundJsonTests.cs`（PBT: `SavedSound`/`SoundClipMeta` serialize↔deserialize 往復・旧 JSON 欠損時既定値＝後方互換）
- [x] `Tests/EditMode/AtomicFileTests.cs`（成功で新値・置換失敗（無効パス）で旧値維持・`.tmp` 残さない・CopyAtomic）
- [x] `Tests/EditMode/StorageCollectionTests.cs`（`ListSounds` 破損meta/対wav欠損スキップ・`DeleteSound` で wav+meta+photo 削除・`SaveMeta` が settings を保持・`LoadSoundBuffer` デコード）
- _トレース: NFR-COL-T1/T2/T3 / nfr-requirements §5_

### Step 18: MCP 検証・スモーク（best-effort）
- [x] `Unity_RunCommand` で `AssetDatabase.Refresh()`→`Unity_GetConsoleLogs`（**Error 0** 確認）。初回 `NoiseLevel.Mid` タイポを検出→`Medium` へ修正→再コンパイルで Error 0/Warning 0（唯一の Warning は Unity AI パッケージの Account API で自コード無関係）。
- [x] `Unity_RunCommand` で `CollectionFilter`／`SavedSound` メタ往復の同期スモーク **PASS**（all=3・feb=2・neko=2・febTaro=1・json title=tori・PASS=True）。
- [x] 全アセンブリロード確認：`Geidai.Collection`（CollectionScreenController/SoundListView）・`Geidai.Services`（AudioService/StorageService/AtomicFile/StubPhotoPicker）・`Geidai.Rec`（RecScreenController/RecBootstrap/EffectPanelController）を typeof で確認。Collection は Rec 非依存。
- [x] （ファイル I/O 系＝`AtomicFile`/`StorageService` は U3 同様 MCP 承認ガードのため実行時スモークは非実施 → EditMode テスト（AtomicFileTests/StorageCollectionTests）で担保）。
- [ ] （best-effort）Collection シーンの新コンポーネント配線・旧 `GoToSoundCollection`/`MySoundCollectionStorage` 差し替えは破壊回避のため **MCP フォローアップ**（§5・code-summary に手順明記）
- _トレース: US-TECH-05 / NFR-10_

### Step 19: コード生成サマリ（ドキュメント）
- [x] `aidlc-docs/construction/u4-collection/code/code-summary.md`（生成/修正/移設/削除ファイル一覧、名前空間・依存、共有 Audio 移設の要点、MCP 検証結果、Collection シーン配線 MCP 手順、旧 collection/audio 最終整理 TODO、S さんハンドオフ点、トレース）
- _注: サマリのみ aidlc-docs 配下。コードは Assets 配下。_

### Step 20: ストーリー完了マーク
- [x] `stories.md` の US-COL-01〜04・US-TECH-06 に U4 実装分の実装状況を注記（実シーン配線・旧最終整理の残タスクを明記）
- _トレース: US-COL / US-TECH-06_

---

## 3. スコープ外（U4 では実施しない）
- 実機ネイティブ写真ピッカーの本結線（`IPhotoPicker` 実装＝フォローアップ）。
- 旧 `MySoundCollection` 形式データの移行（Q1=A・対象外）。
- お題連携（U5）、ゲーム出題（U6）。クラウド/共有（Place 除外）。
- 旧 `MySoundCollectionStorage`/`SoundSavePaths`/`GoToSoundCollection`/`VoiceRecordingSection`/`WavUtility` の物理削除は Collection/Rec シーン MCP 再配線と同時（本ユニットでは新方式を提供）。

## 4. 完了条件
- Step 0〜20 のチェックボックスが全て `[x]`。
- 新規コードが `Geidai.Collection` で生成され、`Collection→Services→Common` の一方向依存でコンパイル Error 0（`Geidai.Collection` は `Geidai.Rec` 非依存）。
- `SoundClipMeta`/`IStorageService`/`IAudioService` 拡張が後方互換（既存が壊れない）。
- `EffectChain` が Services へ移設され、`RecAudioService` 削除後も Rec の録音/プレビュー挙動が不変でコンパイル成功。
- EditMode テスト（CollectionFilter PBT / SavedSound JSON PBT / AtomicFile / StorageCollection）が生成済み、純粋系の同期スモークがグリーン。
- code-summary.md に共有 Audio 移設・Collection シーン配線 MCP 手順・旧整理 TODO・S さんハンドオフ点が明記。

## 5. MCP 未接続時のフォールバック
1. 本ツールで `.cs`/`.asmdef` を `Assets/` に直接生成（コードは完成）。
2. MCP 検証（コンパイル確認・スモーク・シーン配線）は**保留チェック**として残す。
3. Unity 起動後に MCP で一括検証し、チェックを完了。

---

## 承認のお願い
本プラン（全 21 ステップ / Step 0〜20）で U4 のコード生成を進めてよいか、ご確認ください。
- **Request Changes**: ステップ/対象ファイル/方針（特に共有 Audio 移設の範囲）の修正を指定
- **Continue（承認）**: Part 2（コード生成）を開始
