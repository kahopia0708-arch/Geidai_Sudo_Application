# U4 Persistence/Collection — Tech Stack Decisions（技術選定・差分）

**ユニット**: U4 Persistence/Collection
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Requirements（Part 2）
**方針**: U1〜U3 の技術基盤を継承し、**U4 固有の差分のみ**を確定。具体 API・シーン配線は NFR Design / Code Generation。

---

## 1. 継承（U1〜U3 で確定・不変）
- Unity 6000.4.2f1 / URP / uGUI / C#、`Application.persistentDataPath` ＋ `JsonUtility`、WAV=`WavCodec`（16bit PCM）。
- `Result`/`ResultCode`、`ServiceRegistry`（サービスロケータ）、`ScreenRootBase`/`ConfirmDialog`/`ErrorPresenter`/`SafeLogger`。
- シーン操作は公式 Unity AI Assistant（Unity MCP Server）。テストは NUnit＋FsCheck（EditMode）。

## 2. U4 の技術決定（差分）

### 2.1 アセンブリ構成（Q5=A / NFR-08）
- 新規 **`Geidai.Collection`**（`Assets/Scripts/Collection/`）。
  - references: `Geidai.Common`, `Geidai.Services`, `UnityEngine.UI`（**一方向** `Collection → Services → Common`）。
  - **`Geidai.Rec` へは依存しない**（視聴の共有再生は Services 層で提供＝§2.4）。
- `Geidai.Tests` の references に `Geidai.Collection` を追加（EditMode テスト用）。

### 2.2 永続化の原子的置換（Q2=A / NFR-07）
- **書込パターン**: 一時ファイル（例 `{name}.tmp`）へ全内容を書き、`flush/close` 後に本ファイルへ**原子的置換**。
  - 候補 API: `File.Replace`（既存あり時・バックアップ任意）／`File.Move(overwrite)`（新規時）。プラットフォーム差の吸収・例外時クリーンアップは NFR Design で確定。
- 対象: `profile.json`／`{id}.meta.json`／`{id}.wav`／`{id}.photo.<ext>`。
- **`SaveSound` 強化**: U3 の「wav→meta・失敗時 wav 削除」を、両ファイルの原子的書込＋対整合へ強化（**シグネチャ後方互換**）。
- **読込**: 破損/欠損は try/catch で当該のみスキップ（U1 `ListSounds` 方針を全経路へ徹底）。

### 2.3 `IStorageService` 拡張（Q5=A / 後方互換）
既存（`LoadProfile`/`SaveProfile`/`ListSounds`/`LoadSound`/`SaveSound`）に**追加**（既存シグネチャ不変）:
- `Result DeleteSound(string id)`：`{id}.wav`＋`{id}.meta.json`＋`{id}.photo.*` を削除（欠損無視・`Result`）。
- `Result SaveMeta(SoundClipMeta meta)`（or `UpdateSound(SavedSound)`）：メタのみを原子的置換で更新（wav 不変）。
- （写真 I/O は §2.5 の `IPhotoPicker`／StorageService 側ヘルパーで扱う）。
- 内部実装で `profile`/`meta`/`wav` 書込を原子的置換ヘルパーへ統一。

### 2.4 共有再生（エフェクト再適用）（Q6=A / NFR-06・NFR-08）
- **`IAudioService` を後方互換拡張**：`Result Play(AudioBuffer buffer, SoundEffectSettingsData settings)` を追加（既存 `Play(AudioBuffer)` は不変）。
- エフェクト適用（現 `EffectChain` 相当）を **`Geidai.Services.Audio`（または共有 Audio モジュール）へ配置**し、Rec/Collection の双方が同一実装を利用。
  - U3 の `RecAudioService`/`EffectChain` は共有実装へ寄せる（**録音側の挙動は不変**・後方互換）。実配置（Services へ移動 or 共有 asmdef 新設）は NFR Design で確定。
- Collection は `ServiceRegistry.Resolve<IAudioService>()` で解決し、`Play(buffer, settings)` を呼ぶ（**`Collection→Rec` 依存なし**）。
- 注意: シーンをまたぐ `IAudioService` 実装の可用性（AudioSource の生成/常駐）は NFR Design で扱う（Collection シーンでも再生できるよう自前 AudioSource を確保 or 常駐サービス化）。

### 2.5 写真取得の抽象（Q4=A / Q5=A / NFR-04）
- **`IPhotoPicker`**（`Geidai.Services` 側 IF）：`PickAsync(callback)` 相当で一時パスを返す抽象。
- U4 は**スタブ実装**（テスト/エディタ用の固定パス or ダミー）を提供し、フロー（選択→`sounds/{id}.photo.*` へ原子的コピー→`photoFileName` 更新）を成立させる。
- 実機のネイティブピッカー（カメラ/ギャラリー）は**プラグイン/MCP フォローアップ**（NativeGallery 等の採否は Code Generation で判断）。クラウド送信は行わない。

### 2.6 メタ拡張のシリアライズ（Q2=A / NFR-09）
- `SoundClipMeta` に `title`/`photoFileName`/`memo`/`nickname` を追加（`[Serializable]`・`JsonUtility` 対応）。旧 JSON は欠損＝既定値で読める（後方互換）。
- PBT: `SavedSound` の serialize↔deserialize 往復、拡張フィールドの後方互換。

### 2.7 絞込/検索の純粋関数（Q4=A / NFR-09）
- `CollectionFilter.Filter(IReadOnlyList<SavedSound>, CollectionQuery) -> List<SavedSound>` を**純粋関数**として `Geidai.Common`（or `Geidai.Collection` 内 util）に配置（副作用なし・PBT）。
  - 月導出は `createdAtIso`→`YYYY-MM`、検索は `title/memo/nickname` 正規化部分一致、AND 合成。
  - 配置（Common 共有 か Collection 内）は NFR Design/Code Generation で確定（テスト容易性優先）。

## 3. テスト技術（Q4=A / NFR-09）
- EditMode（`Geidai.Tests`）＋ FsCheck：`CollectionFilter`（不変条件）、`SavedSound` メタ往復。
- EditMode/統合：原子的書込（成功で新値・中断で旧値維持）、破損スキップ、`DeleteSound`（対ファイル削除）。
- 実行は Build & Test に集約可。

## 4. スコープ外（U4 では扱わない）
- 実機ネイティブ写真ピッカーの本結線（フォローアップ）。
- 旧 `MySoundCollection` データ移行（対象外）。
- お題（U5）・ゲーム出題（U6）。
- クラウド/共有（Place 除外）。

## 5. トレース
NFR-06→§2.1/2.4/2.7 ／ NFR-07・US-TECH-06→§2.2/2.3 ／ NFR-04→§2.5 ／ NFR-08/10→§2.1〜2.4 ／ NFR-09→§2.6/2.7/§3。US-COL-01〜04 / US-TECH-06 を網羅。
