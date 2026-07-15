# U5 weekly theme — Tech Stack Decisions（技術選定差分・根拠）

**ユニット**: U5 weekly theme（お題）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / NFR Requirements（Part 2）
**方針**: U1〜U4 で確定した技術スタックを踏襲。本書は **U5 固有の差分** のみを記載。

---

## 1. 継続採用（U1〜U4 で確定・再掲）
| 項目 | 決定 | 備考 |
|---|---|---|
| エンジン/言語 | Unity 6000.4.2f1 / URP / C# | 既存 |
| UI | uGUI＋TextMesh Pro | 既存 |
| DI | `ServiceRegistry`＋インターフェース | `IContentService`/`INavigationService` |
| レスポンシブ/SafeArea | `ResponsiveCanvasConfigurator`/`SafeAreaFitter` | U1 基盤 |
| フェイルセーフ | `Result<T>`＋理由コード | クラッシュさせない |
| ロギング | `SafeLogger` | 不要情報を出さない |
| 永続化 | `Application.persistentDataPath`（本ユニットは書込ほぼ無し） | お題は同梱アセット |
| シーン操作 | 公式 Unity AI Assistant（Unity MCP Server） | US-TECH-05 |

## 2. U5 固有の技術決定

### 2.1 お題データ構造（Q1/FR-14 → ScriptableObject）
- **決定**: `ThemeCatalog`（ScriptableObject）＋`ThemeItem`（Serializable）。
- **根拠**: Sさん がインスペクタで追加/編集/並べ替え可能・再ビルド不要（差し替え可能構成）。JSON より Unity ネイティブで扱いやすく型安全。
- **配置**: `Geidai.Common`（`ContentService` から参照するため横断層）。既定アセットは `Assets/Settings/` に MCP 生成。

### 2.2 週選択（Q2 → 純粋関数）
- **決定**: `ThemeSelector.SelectIndex(DateTime date, int count)` を `Geidai.Common` の **static 純粋関数**として実装。既存 `WeeklyTextController` の週番号ロジックを純粋化。
- **根拠**: 決定的・O(1)・アロケーションなしで PBT 可能（NFR-09）。UI/時刻取得と分離。
- **時刻取得**: 呼び出し側が `DateTime.Now`（端末ローカル）を渡す（テスト時は固定日付を注入）。

### 2.3 お題→Rec 受け渡し（Q3 → セッション状態）
- **決定**: `ThemeContext`（`Geidai.Services.Content`・実行時のみ）に選択お題を保持。`ServiceRegistry` 経由 or static ホルダ。
- **根拠**: 保存メタ（`SoundClipMeta`）を汚さずスコープ最小。Rec は `Geidai.Services` 依存済みで循環なし。永続化しない（NFR-04/N/A・非永続）。

### 2.4 `IContentService` 実装範囲（Q5 → 後方互換拡張）
- **決定**: `ContentService`（`Geidai.Services.Content`）が `ThemeCatalog` を参照し、
  - `GetText("theme.current")` → 今週のお題テキスト（既存 IF・後方互換）。
  - `Result<ThemeItem> GetCurrentTheme()` を**追加**（専用取得 API）。
- **根拠**: 既存 `IContentService` 利用箇所に影響を与えず、お題オブジェクト取得を型安全に。ゲーム用パラメータ取得は U6。
- **注入**: 起動時に `ThemeCatalog` を Service へ注入（`AppManager` or `ThemeBootstrap`）。未注入でも `NotFound` で安全に動く。

### 2.5 アセンブリ配置（Q5 → 新 `Geidai.Theme`）
- **決定**: 新規 `Geidai.Theme` アセンブリに UI（`WeeklyThemeController`/`WeeklyThemeScreenController`）。依存 `Geidai.Common`/`Geidai.Services`/`UnityEngine.UI` の一方向。
- **根拠**: モジュール分離・一方向依存の維持（既存パターン踏襲）。純粋ロジック（`ThemeSelector`）とデータ（`ThemeCatalog`/`ThemeItem`）は `Geidai.Common` に置き再利用可能に。
- **旧コード**: `WeeklyTextController`（Assembly-CSharp）はシーン差し替え後に削除（当面残置・MCP フォローアップ／BR-THEME-52）。

### 2.6 表示場所（Q6 補足 → 両対応の土台）
- **決定**: 専用 `Theme` 画面（`WeeklyThemeScreenController`）＋再利用 `WeeklyThemeController`（Home 上部バナーにも貼れる）。実配置は Sさん がシーンで選択。
- **根拠**: 企画の「ホーム上部にお題」案と独立画面案の双方に対応でき、意匠調整を分離（US-TECH-07）。

## 3. テスト技術（Q4）
- **PBT**: 既存の EditMode PBT パターン（U1/U3/U4 と同様）で `ThemeSelector.SelectIndex` を検証。
- **単体**: `ContentService`/`ThemeCatalog`（空カタログ→フォールバック・現在お題取得）。
- **配置**: `Geidai.Tests`（EditMode）に追加。`Geidai.Theme`/`Geidai.Common` を参照。

## 4. リスクと緩和
| リスク | 緩和 |
|---|---|
| 週番号ロジックの年境界の齟齬 | 純粋関数化＋PBT（代表日付・剰余一致）で担保 |
| `ThemeCatalog` 未注入で無表示 | `NotFound` フォールバック（BR-THEME-21/41） |
| 旧 `WeeklyTextController` との二重表示 | 差し替え後に旧を削除（MCP フォローアップ） |
| Assembly-CSharp と新アセンブリの参照制約 | データ/純粋ロジックを `Geidai.Common` に置き Assembly-CSharp 依存を回避 |

## 5. トレース
Q1→2.1 ／ Q2(週選択)→2.2 ／ Q3(受け渡し)→2.3 ／ Q4→3 ／ Q5→2.4/2.5 ／ Q6→2.3(非永続)/2.6。要件 FR-13/14・NFR-04〜12、ストーリー US-THEME-01/02/03・US-TECH-05/07 に整合。
