# U4 Persistence/Collection — Code Generation Summary（コード生成サマリ）

**ユニット**: U4 Persistence/Collection（堅牢な永続化・コレクション UI・メタ編集・絞込/検索）
**生成日**: 2026-07-15 / AI-DLC CONSTRUCTION / Code Generation（Part 2）
**種別**: Brownfield（Unity 6000.4.2f1 / URP / uGUI / C#）
**MCP**: 公式 Unity AI Assistant（`user-unity-mcp`）で検証（**Error 0 / Warning 0**・純粋ロジック スモーク PASS・全アセンブリロード確認）

> アプリコードは `Assets/` 配下。本書は要約（`aidlc-docs/`）。

---

## 1. 生成/修正/移設/削除ファイル一覧

### 新規（`Geidai.Common.Collection` / `Assets/Scripts/Common/Collection/`）
- Created: `CollectionQuery.cs`（月別 `yearMonth`＋キーワード `keyword` の値オブジェクト・`Empty`/`IsEmpty`）
- Created: `LoadOutcome.cs`（有効 `items`＋読み飛ばし `skippedCount`）
- Created: `CollectionFilter.cs`（**純粋関数** `Filter(items, query)`・月導出 `ToYearMonth`・title/memo/nickname 部分一致・AND 合成・順序保持・null 安全／PBT 対象）

### 新規（`Geidai.Services` / `Assets/Scripts/Services/`）
- Created: `IO/AtomicFile.cs`（原子的置換の共通ヘルパー：temp→`File.Replace`/`Move`・例外時 tmp 破棄・`WriteAllBytesAtomic`/`WriteAllTextAtomic`/`CopyAtomic`）
- Created: `Audio/EffectChain.cs`（**Rec から移設**・名前空間 `Geidai.Services.Audio`／ロジック不変：AudioSource＋各 AudioFilter・`Apply(settings, allOn, pitchOn, noiseOn, timbreOn, reverbOn)`）
- Created: `Audio/AudioService.cs`（**共有 `IAudioService` 実装**：録音[Microphone・固定長 `AudioBuffer` 再利用＝U3 挙動移設]／再生[素・エフェクト再適用]／`ApplyEffects`／`IsPlaying`／自前リグ `DontDestroyOnLoad` でシーンまたぎ発音）
- Created: `Media/IPhotoPicker.cs`（写真取得の抽象・一時パスを返す・端末内のみ）
- Created: `Media/StubPhotoPicker.cs`（U4 スタブ：`FixedTempPath` 設定時のみ成功／既定は `NotImplemented`・クラウド送信なし）

### 新規（`Geidai.Collection` / `Assets/Scripts/Collection/`）★新アセンブリ
- Created: `Geidai.Collection.asmdef`（refs: Geidai.Common, Geidai.Services, UnityEngine.UI／**Geidai.Rec 非依存**・一方向 `Collection→Services→Common`）
- Created: `CollectionState.cs`（enum: Loading/Empty/Listing/Playing/Detail/Editing/Confirm）
- Created: `SoundItemViewModel.cs`（`SavedSound`→一覧投影・title 空時は日付・`FormatDate`）
- Created: `CollectionSprites.cs`（写真バイト列→`Sprite`・端末内のみ・失敗時 null）
- Created: `SoundListItemView.cs`（1 項目ビュー：タイトル/日付/サムネ有無/操作・`Bind`＋`SetThumbnail` で遅延差込）
- Created: `SoundListView.cs`（一覧：item プール描画・`ThumbnailLoader`(id→Sprite) 遅延・空状態トグル・open/play 通知）
- Created: `FilterSearchController.cs`（月ドロップダウン＋キーワード→`CollectionQuery`・`QueryChanged` 発火。絞込実行は統括が純粋 `CollectionFilter` を呼ぶ）
- Created: `SoundDetailController.cs`（詳細/編集：title/memo 編集・写真変更[`IPhotoPicker`→`SavePhoto`→`SaveMeta`]/削除[`RemovePhoto`]・視聴[`LoadSoundBuffer`→`Play(buffer,settings)`]・保存[`SaveMeta`]・削除[`ConfirmDialog`→`DeleteSound`]・`MetaChanged`/`Deleted`/`Closed` 通知）
- Created: `CollectionBootstrap.cs`（`IPhotoPicker` を確保：未登録なら `StubPhotoPicker` 登録）
- Created: `CollectionScreenController.cs`（`ScreenRootBase` 継承の司令塔：`ListSounds`→月抽出→`CollectionFilter`→描画・サムネキャッシュ・破損スキップ/空フォールバック・`OnBackPressed` は Detail 中→一覧/一覧→`GoTo(Home)`）

### 修正（後方互換）
- Modified: `Common/Models/SoundClipMeta.cs`（`title`/`photoFileName`/`memo`/`nickname` を**追記**・既定 ""・既存フィールド不変・`CreateNew` 初期化）
- Modified: `Services/Storage/IStorageService.cs`（追加：`DeleteSound`/`SaveMeta`/`SavePhoto`/`RemovePhoto`/`LoadPhoto`/`LoadSoundBuffer`。既存 5 メソッドは不変）
- Modified: `Services/Storage/StorageService.cs`（**全書込を `AtomicFile` へ統一**＝原子的置換／`ListSounds` は `ListSoundsDetailed()` で破損 meta・対 wav 欠損をスキップし空フォールバック／新メソッド実装）
- Modified: `Services/Audio/IAudioService.cs`（追加：`Play(buffer,settings)`／`ApplyEffects(...)`／`IsPlaying`。既存 `StartRecording`/`StopRecording`/`Play(buffer)`/`Stop` は不変）
- Modified: `Services/AppManager.cs`（`IAudioService` = 共有 `AudioService` を `ServiceRegistry` 登録・`using Geidai.Services.Audio` 追加）
- Modified: `Rec/RecBootstrap.cs`（`EnsureAudioService()` を共有 `IAudioService` 解決[無ければ `AudioService` 登録]へ・`SetPlaybackSource` 廃止）
- Modified: `Rec/EffectPanelController.cs`（`EffectChain` 直接保持を廃止・`Init(IAudioService)`＋`IAudioService.ApplyEffects(...)` でプレビュー）
- Modified: `Rec/RecScreenController.cs`（`EffectChain`/`RecAudioService` 参照廃止・共有 `IAudioService` 利用・再生完了検知は `_audio.IsPlaying`・`effectPanel.Init(_audio)`）
- Modified: `Tests/EditMode/Geidai.Tests.asmdef`（references に `Geidai.Collection` 追加）

### 移設
- Moved: `EffectChain.cs` を `Assets/Scripts/Rec/` → `Assets/Scripts/Services/Audio/`（名前空間 `Geidai.Rec`→`Geidai.Services.Audio`・**ロジック不変**）

### 削除
- Deleted: `Assets/Scripts/Rec/RecAudioService.cs`（＋`.meta`）※録音/再生ロジックを共有 `AudioService` へ移設。録音挙動は不変。

### テスト（EditMode / `Geidai.Tests`）
- Created: `CollectionFilterTests.cs`（PBT：結果⊆入力・条件空→全件・冪等・順序保持／例示：月別・キーワード[title/memo/nickname・大小無視]・AND 合成・`ToYearMonth`）
- Created: `SavedSoundJsonTests.cs`（PBT：`SavedSound`/`SoundClipMeta` JSON 往復／旧 JSON[U4 フィールド欠損]の既定値ロード＝後方互換）
- Created: `AtomicFileTests.cs`（新値生成・原子的置換・無効パス失敗で旧値維持・`.tmp` 非残置・`CopyAtomic`）
- Created: `StorageCollectionTests.cs`（`ListSounds` 破損 meta/対 wav 欠損スキップ・`DeleteSound` で wav+meta+photo 一括削除・`SaveMeta` の settings 保持・`LoadSoundBuffer` デコード）

---

## 2. 名前空間・依存
- 一方向：`Geidai.Collection` → `Geidai.Services` → `Geidai.Common`（循環なし）。**Collection は Rec 非依存**（typeof で確認済）。
- 共有 Audio（`AudioService`/`EffectChain`）は `Geidai.Services.Audio` に集約し、Rec（録音・プレビュー）と Collection（視聴）の双方が `ServiceRegistry.Resolve<IAudioService>()` で利用。
- 純粋な絞込/検索（`CollectionFilter`）と一覧クエリ（`CollectionQuery`/`LoadOutcome`）は横断のため `Geidai.Common.Collection`。
- 原子的 I/O（`AtomicFile`）・写真抽象（`IPhotoPicker`）は `Geidai.Services`。外部 API/ネットワークなし・写真/メモ/音声はローカルのみ（PRIVACY / NFR-COL-Priv1/2）。

## 3. 主要な技術判断
- **共有 Audio へ集約（Q4=A）**：`Collection→Rec` 依存を避けるため、U3 `RecAudioService` の録音/再生を `Geidai.Services.Audio.AudioService` へ移設し、`EffectChain` も Services へ移設。**録音側の挙動は不変**（Microphone・固定長 `AudioBuffer` 再利用）。`AppManager` が起動時に登録し、Rec/Collection が共用。シーンまたぎ発音のため実装が自前 `AudioSource`（`DontDestroyOnLoad`）を所有。
- **原子的置換に統一（Q1=A）**：profile/meta/wav/写真の全書込を `AtomicFile`（temp→`File.Replace`/`Move`）へ集約。書込中断でも本ファイルは無傷。`SaveSound` は wav→meta 順で原子的書込＋対整合（新規時 meta 失敗で wav 掃除）。
- **堅牢な読込（Q2=A）**：`ListSoundsDetailed()` が meta 単位で try/catch し、破損 meta・対 wav 欠損をスキップ。ディレクトリ無し/全失敗は空リストへフォールバック（クラッシュしない）。
- **純粋な絞込/検索（Q5=A）**：`CollectionFilter.Filter` は副作用なし・決定的で PBT 可能。UI（`FilterSearchController`）はクエリ生成のみ担当し、実行は画面統括。
- **写真は端末内参照（Q5=A）**：`SavePhoto` は拡張子検証（jpg/jpeg/png）後 `AtomicFile.CopyAtomic` で `sounds/{id}.photo.<ext>` へ取込。表示は `LoadPhoto`（バイト列）→`CollectionSprites`。実機ピッカーは `IPhotoPicker`/`StubPhotoPicker` で抽象化（本結線はフォローアップ）。
- **一覧性能（Q3=A）**：`SoundListView` は item プール＋相対レイアウト前提、サムネは遅延ローダ＋キャッシュ（null もキャッシュして再読込回避）。将来の仮想化に耐える構造。
- **後方互換**：`SoundClipMeta` は追記のみ（旧 JSON は既定値でロード＝`SavedSoundJsonTests` で担保）。`IStorageService`/`IAudioService` は既存シグネチャ不変で追加のみ。

## 4. MCP 検証結果（`user-unity-mcp`）
- ベースライン `Unity_GetConsoleLogs`：Error 0。
- 取り込み後：初回コンパイルで `SavedSoundJsonTests` の `NoiseLevel.Mid` タイポを検出（CS0117）→ `Medium` へ修正 → 再コンパイルで **Error 0 / Warning 0**（唯一の Warning は Unity AI パッケージの Account API アクセスで自コード無関係）。
- 同期スモーク（`Unity_RunCommand`）：`CollectionFilter`＋`SavedSound` メタ往復 = **PASS**（all=3・feb=2・neko=2・febTaro=1・json title=tori・PASS=True）。
- 全アセンブリロード確認（typeof）：`Geidai.Collection`（`CollectionScreenController`/`SoundListView`）・`Geidai.Services`（`AudioService`/`StorageService`/`AtomicFile`/`StubPhotoPicker`）・`Geidai.Rec`（`RecScreenController`/`RecBootstrap`/`EffectPanelController`）。**Collection は Rec 非依存**。
- ファイル I/O 系（`AtomicFile`/`StorageService`）の実行時スモークは、AI Assistant Run Command の**書込承認ガード**（"User interactions are not supported"）のため MCP からは非実施。コード不具合ではなく、EditMode `AtomicFileTests`/`StorageCollectionTests`（Test Runner）で担保。

## 5. UI ハンドオフ点（前本 → Sさん / US-TECH-07）
- `SoundListItemView`/`SoundListView`：カード見た目・レイアウト・サムネ/プレースホルダ・ScrollRect 構成。
- `FilterSearchController`：月ドロップダウン・検索入力・クリアの配置/ラベル。
- `SoundDetailController`：詳細/編集パネルの見た目、写真表示枠、`InputField`（title/memo）・視聴/保存/削除/写真変更ボタン。
- `ConfirmDialog`/`ErrorPresenter`（Common 再利用）：削除確認・失敗文言/配色トーン。
- UI 枠は uGUI（`Button`/`Dropdown`/`InputField`/`Image`/`Text`）。必要に応じ TMP へ差し替え可。

## 6. 残タスク（MCP フォローアップ：実シーン配線）
> コードは完成。以下は Unity 上の GameObject 配線で、破壊回避のため別途 MCP セッションで実施する。

1. **Collection.unity 再構築**：Canvas＋`ResponsiveCanvasConfigurator`＋`SafeAreaFitter`、`CollectionScreenController` を配置し、`SoundListView`（item プレハブ＝`SoundListItemView`）・`FilterSearchController`・`SoundDetailController`・`ConfirmDialog`・`ErrorPresenter`・もどる `Button` を結線。
2. **旧コレクション/録音一式の最終整理**：`GoToSoundCollection`/`MySoundCollectionStorage`/`SoundSavePaths`/`VoiceRecordingSection`/`WavUtility`/`SoundEffectSettings` を新方式へ差し替え後に物理削除（Rec/Collection 再配線と同時）。
3. **共有 Audio の起動確認**：`AppManager` が起動シーンに存在し `IAudioService`=共有 `AudioService` が登録されること（Rec 単独起動時は `RecBootstrap.EnsureAudioService()` が保証）。
4. **実機ピッカー結線**：`IPhotoPicker` 実機実装（カメラ/ギャラリー）を作成し `ServiceRegistry` 登録（現状 `StubPhotoPicker`）。
5. 配線後、実機で「一覧表示→絞込/検索→視聴（保存エフェクト再適用）→タイトル/メモ/写真編集→削除（確認）」を通し確認（Build & Test）。

## 7. スコープ外（U4 では未実施）
- 実シーン配線（§6）・実機ネイティブ写真ピッカーの本結線。
- 旧 `MySoundCollection` 形式データの移行（対象外）。
- お題連携（U5）・ゲーム出題（U6）・クラウド/共有（Place 除外）。

## 8. トレース
US-COL-01→CollectionScreenController/SoundListView/SoundDetailController＋`IAudioService.Play(buffer,settings)`（保存エフェクト再適用視聴） ／ US-COL-02→SoundDetailController＋`SaveMeta`/`SavePhoto`/`RemovePhoto`/`DeleteSound`（原子的・確認付き） ／ US-COL-03→FilterSearchController＋`CollectionFilter`（月別/検索） ／ US-COL-04→`ListSoundsDetailed` 破損スキップ・空フォールバック ／ US-TECH-06→`AtomicFile` 全書込統一 ／ US-TECH-03→共有 `AudioService`/`EffectChain` 移設 ／ NFR-COL-P1→一覧プール/遅延サムネ/相対レイアウト ／ NFR-COL-R1〜R4→原子的置換/破損スキップ/対整合 ／ NFR-COL-T1/T2/T3→CollectionFilter PBT/JSON PBT/AtomicFile・Storage テスト ／ NFR-COL-M1/M4→`Geidai.Collection` 新設・共有 Audio ／ PRIVACY→ローカルのみ・PII 非ログ。
