# U4 Persistence/Collection — NFR Design Patterns（実現パターン）

**ユニット**: U4 Persistence/Collection
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Design（Part 2）
**決定**: Q1=A（`AtomicFile` 集約）/ Q2=A（`ListSounds` 読込集約・破損スキップ）/ Q3=A（相対レイアウト＋サムネ遅延＋仮想化可能）/ Q4=A（共有 Audio 再生 `IAudioService.Play(buffer,settings)`）/ Q5=A（純粋 `CollectionFilter`＋`IPhotoPicker` 抽象）/ Q6=A（`Geidai.Collection`＋`IStorageService` 後方互換拡張）
**入力**: `../nfr-requirements/nfr-requirements.md`, `tech-stack-decisions.md`, `../functional-design/*`, U1〜U3 NFR Design 成果物

> 目的: U4 の NFR を「どう実現するか（パターン）」へ落とす。数値・受入は NFR Requirements で確定済み。U1〜U3 の横断パターン（`Result`／`ScreenRootBase`＋レスポンシブ／`ServiceRegistry` DI／`SafeLogger`／`WavCodec`）は継承。

---

## 0. 継承パターン（U1〜U3・再掲）
- **エラー伝搬**: `Result`/`Result<T>`（`ResultCode`）。致命的でない失敗はクラッシュさせない。
- **UI 基盤**: `ScreenRootBase`（表示/離脱ライフサイクル・`OnBackRequested`）＋`ResponsiveCanvasConfigurator`＋`SafeAreaFitter`。
- **通知/確認**: `ErrorPresenter`（子ども向けバナー）／`ConfirmDialog`（既定=いいえ）。
- **DI**: `ServiceRegistry`（サービスロケータ）＋ IF（`IStorageService`/`IAudioService`/`INavigationService`）。
- **性能/GC**: 同期API基本・重処理のみ非同期/コルーチン・バッファ再利用・参照キャッシュ。
- **セキュリティ**: PII/音声/写真は端末外送信なし・`SafeLogger` 非ログ・本番で詳細エラー非表示。

---

## 1. Resilience — 原子的置換（`AtomicFile`）※U4 の主眼（Q1=A / NFR-COL-R1）

**パターン**: 全書込を「一時ファイル→原子的置換」に統一する共通ヘルパー。

- **配置**: `Geidai.Services`（静的 `AtomicFile`）。`Geidai.Common` に依存させない（I/O は Services 層）。
- **API**:
  - `Result WriteAllBytesAtomic(string path, byte[] data)`
  - `Result WriteAllTextAtomic(string path, string text)`
  - `Result ReplaceOrMove(string tmpPath, string finalPath)`（内部利用）
- **手順**:
  1. `Directory.CreateDirectory(dir)`。
  2. `{finalPath}.tmp` へ全内容を書き、`FileStream.Flush(true)`／`Dispose`（＝flush/close）。
  3. 本ファイルへ**原子的置換**：既存あり=`File.Replace(tmp, final, null)`／新規=`File.Move(tmp, final)`（`File.Move(tmp, final, overwrite:true)` 相当が無い環境は「final 削除→Move」で近似）。プラットフォーム差はヘルパー内に閉じ込める。
  4. 例外時は `{finalPath}.tmp` を削除（ベストエフォート）し `Result.Fail(IOError)`。**本ファイルは触れないので旧内容が無傷**。
- **適用先**: `profile.json` / `{id}.meta.json` / `{id}.wav` / `{id}.photo.<ext>`。`StorageService` の全書込を本ヘルパー経由へ統一。
- **U3 互換**: `SaveSound`（wav→meta・失敗時 wav 削除）を、両ファイルとも `AtomicFile` 書込へ強化（**シグネチャ不変**）。meta 失敗時は書いた wav を削除して対を残さない方針は維持。
- **受入**: 書込を中断注入（例外/途中終了）→ 本ファイルは旧内容のまま（新規時は生成されないだけ）。`.tmp` の残骸は次回起動時のクリーンアップ対象にできる（任意）。

## 2. Resilience — 読込の破損スキップ／空フォールバック（Q2=A / NFR-COL-R2/R3）

**パターン**: 読込を `StorageService.ListSounds()` に集約し、破損は握りつぶさず**当該のみスキップ**。

- `sounds/*.meta.json` を列挙し、各 meta を try/catch で読む。
  - パース不可・`meta==null`・**対 wav 欠損** → スキップ（`skippedCount++`・`SafeLogger.Warn` は非PII）。
  - 有効なもののみ収集。
- 返却は `Result<List<SavedSound>>`。詳細が要る場合は `LoadOutcome{ items, skippedCount }`（functional-design の `LoadOutcome`）で件数を伝える。
- ディレクトリ無し/0 件 → 空リスト（例外にしない）。**どの段階の例外も最悪は空リストへフォールバック**。
- `LoadSound(id)` 単体も同様（破損/欠損は `Result.Fail`、呼び出し側は一覧維持）。
- **受入**: meta 故意破損→一覧は他項目を正常表示／全滅→空状態（`CollectionScreenController` が `Empty`）。

## 3. Performance/Scalability — 一覧描画・スクロール・サムネ（Q3=A / NFR-COL-P1/P2/UI1）

**パターン**: 相対レイアウト＋サムネ遅延読み＋仮想化可能な `SoundListView`。

- **レイアウト**: `LayoutGroup`＋`ContentSizeFitter`／Anchor で**固定px依存を排除**（旧 `itemWidth 850px` を相対化）。縦横両対応。
- **サムネ遅延読み**: 写真は一覧生成時に読まず、**表示（可視化）時に非同期ロード**しプレースホルダ表示。`Texture2D` は不要時に `Destroy`／`Resources.UnloadUnusedAssets` 相当で GC を抑制。
- **表示 VM キャッシュ**: `SavedSound`→表示用 VM（タイトル・月・サムネパス等）を一度投影しキャッシュ（毎フレーム再計算しない）。
- **仮想化可能な抽象**: `SoundListView` は「可視範囲のみ生成/再利用（プール）」できる構造にしておく。数百件で fps 低下が見えたら仮想化を有効化（実装粒度は Code Generation 判断）。数十件規模では素朴生成でも可。
- **フィルタ適用**: `CollectionFilter.Filter` は O(n)（n=数百で問題なし）。結果差分のみ再描画。
- **受入**: 100 件で表示 < 0.5s・スクロール 60fps（最低 30fps）。詳細計測は Build & Test。

## 4. Performance/Maintainability — 共有 Audio 再生・エフェクト再適用（Q4=A / NFR-COL-M4）

**パターン**: エフェクト適用付き再生を **Services 層の共有実装**へ集約し、Rec/Collection の双方が利用（`Collection→Rec` 依存を作らない）。

- **IF 拡張（後方互換）**: `IAudioService` に `Result Play(AudioBuffer buffer, SoundEffectSettingsData settings)` を追加。既存 `Play(AudioBuffer)`／録音系メソッドは不変。
- **エフェクト適用の移設**: U3 `EffectChain`（AudioSource＋各 AudioFilter を束ねる）相当を **`Geidai.Services.Audio` の共有実装**へ寄せる。`SoundEffectSettingsData`→具体フィルタ値の写像はここに集約（`SoundEffectMapper` の数値換算は `Geidai.Common.Audio` のまま利用）。
- **シーンまたぎの可用性**: 共有 Audio サービス実装は**自前 AudioSource を必要時に生成/確保**（Collection シーンでも発音可）。実装は `ServiceRegistry` に登録（`AppManager` 起動時 or 各シーン初期化時）。
- **U3 互換**: `RecAudioService` は共有実装へ移設 or 委譲（**録音側の挙動は不変**）。録音時の `EffectPanelController` ライブプレビューも同一実装を利用。
- **利用**: `CollectionScreenController`/`SoundDetailController` は `ServiceRegistry.Resolve<IAudioService>()` → `Play(buffer, settings)`。再生失敗は `Result` で通知し一覧維持。
- **受入**: コレクション視聴が録音時と同じ聴こえ（保存エフェクト再適用）／`Geidai.Collection` は `Geidai.Rec` に依存しない。

## 5. Testability/Security — 純粋フィルタ＋写真 I/O 抽象（Q5=A / NFR-COL-T1/T2/Priv1/Priv2）

**パターン**: 絞込/検索を純粋関数化（PBT）、写真取得を抽象化（端末内・非ログ）。

- **`CollectionFilter.Filter(IReadOnlyList<SavedSound>, CollectionQuery) -> List<SavedSound>`**: 副作用なし・決定的。
  - 月別: `createdAtIso`→`YYYY-MM` を導出し `yearMonth` と一致。
  - 検索: `title`/`memo`/`nickname` を正規化（Trim・小文字化）して `keyword` を含むか。
  - 月＋検索は **AND**。
  - **不変条件（PBT）**: 結果 ⊆ 入力／条件空 → 全件／冪等（同入力で同結果）／AND 合成の単調性。
- **配置**: テスト容易性優先で `Geidai.Common`（共有 util）または `Geidai.Collection` 内 util（`Geidai.Tests` から参照可能な側）。最終配置は Code Generation で確定（PBT が回る側）。
- **メタ JSON 往復（PBT）**: `SavedSound` serialize↔deserialize、拡張フィールド（`title`/`photoFileName`/`memo`/`nickname`）欠損時は既定値（後方互換）。
- **写真 I/O 抽象**: `IPhotoPicker`（`Geidai.Services` IF）＋U4 スタブ。選択→一時パス→`sounds/{id}.photo.<ext>` へ **`AtomicFile` で原子的コピー**→`photoFileName` 更新→meta 保存。**クラウド送信なし・PII（実体パス/メモ/ニックネーム）を `SafeLogger` に出さない**。実機ネイティブピッカーはフォローアップ。

## 6. DI/Logical Components — `Geidai.Collection` 構成・`IStorageService` 拡張（Q6=A / NFR-COL-M1/M2/M5）

**パターン**: 新規アセンブリに画面群、永続化 IF を後方互換拡張し `AtomicFile` へ統一。

- **アセンブリ**: 新規 `Geidai.Collection`（`Collection → Services → Common`＋`UnityEngine.UI` の**一方向**）。`Geidai.Tests` に `Geidai.Collection` 参照を追加。`Geidai.Rec` へは依存しない。
- **画面コントローラ**:
  - `CollectionScreenController`（`ScreenRootBase` 継承）＝状態統括・一覧読込・子調停・戻る/ホーム。
  - `SoundListView`＝一覧描画（相対レイアウト・仮想化可能・タップ通知）。
  - `SoundDetailController`＝詳細/編集（title/photo/memo）・削除起動・視聴。
  - `FilterSearchController`＝`CollectionQuery` 保持・純粋 `Filter` 適用・空状態制御。
  - ロジックは POCO/静的へ寄せ MonoBehaviour 依存を最小化（テスト容易性）。
- **`IStorageService` 後方互換拡張**（既存シグネチャ不変）:
  - `Result DeleteSound(string id)`（`{id}.wav`＋`{id}.meta.json`＋`{id}.photo.*` を一括削除・欠損無視）。
  - `Result SaveMeta(SoundClipMeta meta)`（メタのみ `AtomicFile` で原子的置換・wav 不変）。※`UpdateSound(SavedSound)` としても可（Code Generation で確定）。
  - 既存 `SaveProfile`/`SaveSound` を **`AtomicFile` 経由の原子的置換へ内部強化**。
- **再利用**: 削除確認=`ConfirmDialog`、失敗通知=`ErrorPresenter`、遷移=`NavigationService`。
- **旧コード集約**: 旧 `MySoundCollectionStorage`/`SoundSavePaths` は新方式へ集約。物理削除はシーン再配線と同時（MCP フォローアップ）。

---

## 7. カテゴリ別サマリ
| カテゴリ | 適用 | 主パターン |
|---|---|---|
| Resilience | ✅（主眼） | `AtomicFile`（原子的置換）・`ListSounds` 破損スキップ・空フォールバック・対整合・削除ベストエフォート |
| Performance | ✅ | 相対レイアウト・サムネ遅延読み・表示VMキャッシュ・O(n) フィルタ・共有 Audio 再生 |
| Scalability | 限定 | `SoundListView` 仮想化可能（数百件で有効化）。サーバスケールは N/A |
| Security | ✅ | PII 端末内限定・非ログ（`SafeLogger`）・写真クラウド送信なし |
| Logical Components | ✅ | `AtomicFile`・`IStorageService` 拡張・`CollectionFilter`・共有 Audio・`IPhotoPicker`・コレクション画面群 |

## 8. トレース
NFR-COL-R1→§1 ／ R2/R3→§2 ／ P1/P2/UI1→§3 ／ M4→§4 ／ T1/T2/Priv1/Priv2→§5 ／ M1/M2/M5→§6。Functional Design §2（読込）→§2 ／ §3（視聴）→§4 ／ §4（編集）→§1/§5 ／ §5（削除）→§6 ／ §6（絞込検索）→§5 ／ §7（原子性）→§1/§2。
