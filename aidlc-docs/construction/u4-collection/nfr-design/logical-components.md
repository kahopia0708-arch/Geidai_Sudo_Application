# U4 Persistence/Collection — Logical Components（論理コンポーネント）

**ユニット**: U4 Persistence/Collection
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Design（Part 2）
**入力**: `nfr-design-patterns.md`, `../nfr-requirements/*`, `../functional-design/*`, U1〜U3 論理コンポーネント

> NFR Design で決めたパターンを支える論理部品を定義。実クラス/シグネチャ詳細・シーン配線は Code Generation。配置アセンブリは `Geidai.Common` / `Geidai.Services` / `Geidai.Collection`（一方向 `Collection → Services → Common`）。

---

## 1. コンポーネント一覧

| # | コンポーネント | 種別 | 配置（asmdef） | 責務 | 対応 NFR / パターン |
|---|---|---|---|---|---|
| 1 | `AtomicFile` | 静的ユーティリティ | `Geidai.Services` | temp→原子的置換の共通書込（bytes/text）・例外時 tmp 破棄 | §1 / NFR-COL-R1 |
| 2 | `IStorageService`（拡張） | インターフェース | `Geidai.Services` | 既存＋`DeleteSound`／`SaveMeta`、全書込を `AtomicFile` 経由へ | §1/§2/§6 / R1/R4/M2 |
| 3 | `StorageService`（強化） | 実装（POCO/Mono不問） | `Geidai.Services` | 破損スキップ読込・原子的書込・対ファイル削除 | §1/§2 / R1/R2/R3/R4 |
| 4 | `LoadOutcome` | データ構造 | `Geidai.Common` | 一覧読込結果（`items`＋`skippedCount`） | §2 / R2 |
| 5 | `CollectionQuery` | データ構造 | `Geidai.Common` | 絞込条件（`yearMonth`／`keyword`） | §5 / T1 |
| 6 | `CollectionFilter` | 静的・純粋関数 | `Geidai.Common`（or Collection util） | 月別＋検索フィルタ（AND・部分一致・冪等） | §5 / T1（PBT） |
| 7 | `IPhotoPicker` | インターフェース | `Geidai.Services` | 写真取得の抽象（端末内・非送信） | §5 / Priv1/Priv2 |
| 8 | `StubPhotoPicker` | 実装（U4 スタブ） | `Geidai.Services`（or Collection） | エディタ/テスト用の写真選択スタブ | §5 |
| 9 | `IAudioService`（拡張） | インターフェース | `Geidai.Services` | `Play(AudioBuffer, SoundEffectSettingsData)` 追加（後方互換） | §4 / M4 |
| 10 | 共有 Audio 実装（`AudioService`） | 実装（Mono/常駐） | `Geidai.Services.Audio` | エフェクト適用付き再生（EffectChain 相当）・自前 AudioSource | §4 / M4 |
| 11 | `CollectionScreenController` | MonoBehaviour（`ScreenRootBase`） | `Geidai.Collection` | 画面状態統括・一覧読込・子調停・戻る/ホーム | §3/§6 |
| 12 | `SoundListView` | MonoBehaviour | `Geidai.Collection` | 一覧描画（相対レイアウト・仮想化可能・タップ通知） | §3 / P1/UI1 |
| 13 | `SoundDetailController` | MonoBehaviour | `Geidai.Collection` | 詳細/編集（title/photo/memo）・視聴・削除起動 | §4/§5/§6 |
| 14 | `FilterSearchController` | MonoBehaviour | `Geidai.Collection` | `CollectionQuery` 保持・純粋 `Filter` 適用・空状態制御 | §5 |
| 15 | `SoundItemViewModel` | データ構造 | `Geidai.Collection` | 表示用投影（タイトル/月/サムネパス）・キャッシュ | §3 / P1 |

**再利用（U1〜U3・新規作成しない）**: `Result`/`ResultCode`、`SoundClipMeta`（拡張済）、`SavedSound`、`SoundEffectSettingsData`、`AudioBuffer`、`WavCodec`、`SoundEffectMapper`、`ScreenRootBase`、`ResponsiveCanvasConfigurator`、`SafeAreaFitter`、`ConfirmDialog`、`ErrorPresenter`、`SafeLogger`、`ServiceRegistry`、`INavigationService`/`NavigationService`。

---

## 2. コンポーネント詳細

### 2.1 `AtomicFile`（静的 / `Geidai.Services`）
- `Result WriteAllBytesAtomic(string path, byte[] data)`
- `Result WriteAllTextAtomic(string path, string text)`
- 内部: `Directory.CreateDirectory` → `{path}.tmp` 書込＋flush/close → `File.Replace`（既存）/`File.Move`（新規）→ 例外時 `.tmp` 破棄。
- 依存: なし（`System.IO`）。副作用は I/O のみで**本ファイルは置換の瞬間まで不変**。

### 2.2 `IStorageService`（拡張・後方互換）
既存: `LoadProfile`/`SaveProfile`/`ListSounds`/`LoadSound`/`SaveSound`。
追加:
- `Result DeleteSound(string id)` — `{id}.wav`＋`{id}.meta.json`＋`{id}.photo.*` を削除（欠損無視・ベストエフォート）。
- `Result SaveMeta(SoundClipMeta meta)` — メタのみ `AtomicFile` で原子的置換（wav 不変）。※`UpdateSound(SavedSound)` 代替可。
> 既存シグネチャは不変。内部で `SaveProfile`/`SaveSound` を `AtomicFile` 経由へ強化。

### 2.3 `StorageService`（強化）
- `ListSounds()`: `sounds/*.meta.json` 走査・破損/対 wav 欠損スキップ・`Result<List<SavedSound>>`（or `LoadOutcome`）。
- `SaveSound(SavedSound, AudioBuffer)`: wav→meta を `AtomicFile` で書込、meta 失敗時 wav 削除（対整合）。
- `SaveMeta`/`DeleteSound`: 上記 IF に従う。
- 例外は捕捉し `Result`／`SafeLogger`（非PII）。クラッシュしない。

### 2.4 `LoadOutcome`（`Geidai.Common`）
- `List<SavedSound> items`、`int skippedCount`。UI で「一部読み飛ばし」を穏当に扱う（必須ではない）。

### 2.5 `CollectionQuery`（`Geidai.Common`）
- `string yearMonth`（`YYYY-MM` or 空=全月）、`string keyword`（空=無条件）。イミュータブル志向。

### 2.6 `CollectionFilter`（静的・純粋 / PBT）
- `List<SavedSound> Filter(IReadOnlyList<SavedSound> items, CollectionQuery query)`。
- 月導出（`createdAtIso`→`YYYY-MM`）・検索（`title`/`memo`/`nickname` 正規化部分一致）・AND。
- 不変条件: 結果⊆入力・条件空→全件・冪等。→ FsCheck で検証。

### 2.7 `IPhotoPicker` / `StubPhotoPicker`
- `void Pick(Action<Result<string>> onResult)`（一時パスを返す）。実装はプラットフォーム/スタブに閉じる。
- U4 は `StubPhotoPicker`（固定/ダミー）でフロー成立。実機ピッカーはフォローアップ。
- 取得後は `AtomicFile` で `sounds/{id}.photo.<ext>` へコピー→`SaveMeta`。クラウド送信なし。

### 2.8 `IAudioService`（拡張）＋ 共有 Audio 実装
- 追加: `Result Play(AudioBuffer buffer, SoundEffectSettingsData settings)`（既存 `Play(AudioBuffer)`/録音系は不変）。
- 実装（`Geidai.Services.Audio`）: エフェクト適用（EffectChain 相当）＋自前 AudioSource（必要時生成/確保）でシーンまたぎ発音可。`ServiceRegistry` に登録。
- U3 `RecAudioService`/`EffectChain` は本実装へ移設/委譲（録音側挙動は不変）。

### 2.9 コレクション画面群（`Geidai.Collection`）
- `CollectionScreenController`（`ScreenRootBase`）: `OnShow`→`ListSounds`→`Filter`→描画。`OnBackRequested`→ホーム（`NavigationService`）。
- `SoundListView`: VM リストを相対レイアウトで描画・可視範囲生成（仮想化可能）・タップ通知。
- `SoundDetailController`: 詳細/編集/視聴（`IAudioService.Play(buffer,settings)`）/削除（`ConfirmDialog`→`DeleteSound`）。
- `FilterSearchController`: `CollectionQuery` 更新→`CollectionFilter`→再描画・空状態。

---

## 3. 依存関係（一方向）

```text
Geidai.Collection
   ├─ CollectionScreenController / SoundListView / SoundDetailController / FilterSearchController / SoundItemViewModel
   ▼
Geidai.Services
   ├─ IStorageService(+DeleteSound/SaveMeta) / StorageService / AtomicFile
   ├─ IAudioService(+Play(buffer,settings)) / AudioService(共有・EffectChain 相当)
   ├─ IPhotoPicker / StubPhotoPicker / ServiceRegistry / NavigationService
   ▼
Geidai.Common
   ├─ Result / SoundClipMeta / SavedSound / SoundEffectSettingsData / AudioBuffer
   ├─ WavCodec / SoundEffectMapper / CollectionQuery / CollectionFilter / LoadOutcome
```

- `Geidai.Collection` は **`Geidai.Rec` に依存しない**（視聴は Services 経由）。
- `Geidai.Tests` は `Geidai.Common`/`Services`/`Foundation`/`Rec`/`Collection` を参照（EditMode）。

## 4. テスト対応（NFR-09）
- **PBT（`Geidai.Tests`＋FsCheck）**: `CollectionFilter`（不変条件）、`SavedSound`/`SoundClipMeta` の JSON 往復（拡張フィールド後方互換）。
- **EditMode/統合**: `AtomicFile`（成功で新値・中断で旧値維持＝故障注入）、`StorageService.ListSounds`（破損 meta/対 wav 欠損スキップ）、`DeleteSound`（対ファイル削除）。
- 実行は Build & Test に集約可。

## 5. Code Generation への申し送り
- `CollectionFilter`/`CollectionQuery`/`LoadOutcome` の最終配置（`Common` 共有 か `Collection` 内）は PBT が回る側で確定。
- 共有 Audio 実装の物理配置（`Geidai.Services` 内 or 新 `Geidai.Services.Audio` サブ/別 asmdef）と U3 `EffectChain`/`RecAudioService` の移設方法を確定（後方互換・録音側不変）。
- `SaveMeta` vs `UpdateSound(SavedSound)` の最終シグネチャ確定。
- 実機 `IPhotoPicker`・旧 `MySoundCollectionStorage`/`SoundSavePaths` 物理削除・シーン再配線は **MCP フォローアップ**。

## 6. トレース
§1→AtomicFile/StorageService ／ §2→StorageService.ListSounds/LoadOutcome ／ §3→SoundListView/SoundItemViewModel ／ §4→IAudioService/AudioService ／ §5→CollectionFilter/CollectionQuery/IPhotoPicker ／ §6→Geidai.Collection 画面群/IStorageService 拡張。US-COL-01〜04・US-TECH-06 を網羅。
