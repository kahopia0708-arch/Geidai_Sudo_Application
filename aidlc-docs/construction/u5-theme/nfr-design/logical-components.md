# U5 weekly theme — Logical Components（論理コンポーネント）

**ユニット**: U5 weekly theme（お題）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / NFR Design（Part 2）
**方針**: NFR Design Patterns（P1〜P5）を支える論理部品を定義。物理実装（C# 型/シグネチャ）は Code Generation で確定。一方向依存 `Geidai.Theme → Geidai.Services → Geidai.Common`。

---

## 1. コンポーネント一覧と配置

| # | コンポーネント | 種別 | 配置（アセンブリ） | 対応パターン |
|---|---|---|---|---|
| 1 | `ThemeItem` | 値オブジェクト（Serializable） | `Geidai.Common` | P4 |
| 2 | `ThemeCatalog` | ScriptableObject | `Geidai.Common` | P4 |
| 3 | `ThemeSelector` | 静的純粋関数 | `Geidai.Common` | P1 |
| 4 | `IContentService`（拡張） | インターフェース | `Geidai.Services`（既存） | P2/P4 |
| 5 | `ContentService`（実装拡張） | クラス | `Geidai.Services.Content` | P2/P4 |
| 6 | `ThemeContext` | セッション状態（POCO/static ホルダ） | `Geidai.Services.Content` | P3 |
| 7 | `ThemeBootstrap`（任意） | 静的初期化 | `Geidai.Theme` or `Geidai.Services.Content` | P4 |
| 8 | `WeeklyThemeController` | MonoBehaviour（再利用部品） | `Geidai.Theme` | P5/P2/P3 |
| 9 | `WeeklyThemeScreenController` | `ScreenRootBase` サブクラス | `Geidai.Theme` | P5 |

---

## 2. 各コンポーネントの責務

### 2.1 `ThemeItem`（Geidai.Common）
- フィールド: `id`（任意）/`text`（必須）/`reading`（任意）/`hint`（任意）。
- 制約: `text` 空は無効（選択対象外・BR-THEME-11）。PII を含まない。

### 2.2 `ThemeCatalog : ScriptableObject`（Geidai.Common）
- フィールド: `List<ThemeItem> items`。
- 責務: Sさん がインスペクタで編集可能なお題一覧（差し替え可能構成・FR-14）。
- 既定アセット: 既存オノマトペを初期値に（`Assets/Settings/` に MCP 生成）。

### 2.3 `ThemeSelector`（Geidai.Common・static 純粋）
- `int SelectIndex(DateTime date, int count)`：週番号→剰余で index（P1）。`count<=0`→`-1`。
- 副作用なし・O(1)・PBT 対象。

### 2.4 `IContentService`（拡張・Geidai.Services）
- 既存: `Result<string> GetText(string key)`（後方互換で維持）。
- 追加: `Result<ThemeItem> GetCurrentTheme()`。
- 任意: `void SetCatalog(ThemeCatalog catalog)`（DI 注入用）。

### 2.5 `ContentService`（実装拡張・Geidai.Services.Content）
- 保持: 注入された `ThemeCatalog`（未注入可）。
- `GetCurrentTheme()`: 有効項目抽出→`ThemeSelector.SelectIndex(now, validCount)`→該当 `ThemeItem` を `Result.Ok`／空・無効は `Result.Fail(NotFound)`（P2）。
- `GetText("theme.current")`: 成功時 本文／Fail 時 空・フォールバック文字列。未対応キーは `Result.Fail(NotImplemented)`（U6 拡張）。
- 時刻は内部で `DateTime.Now`（テスト用に注入可能な形も許容）。

### 2.6 `ThemeContext`（Geidai.Services.Content）
- 保持: `ThemeItem current`（or null）、導出 `hasValue`。
- 責務: お題→Rec の受け渡し（P3）。**非永続・保存メタ非記録**。
- 解決: `ServiceRegistry` 経由 or static ホルダ。

### 2.7 `ThemeBootstrap`（任意）
- 責務: 起動時に `ContentService` へ `ThemeCatalog` を注入・`IContentService` 未登録なら登録（`RecBootstrap`/`CollectionBootstrap` と同パターン）。未注入でも `NotFound` で安全動作。

### 2.8 `WeeklyThemeController`（Geidai.Theme・再利用）
- 依存: `IContentService`/`ThemeContext`/`INavigationService`（`ServiceRegistry` 解決）。
- UI 参照: `themeText`（必須）/`readingText`/`hintText`/`recordButton`/`emptyState`（SerializeField）。
- 振る舞い: `OnEnable`/`Refresh()` で `GetCurrentTheme()`→成功で反映・Fail で `emptyState`（P2）。`recordButton`→`ThemeContext.current` 設定→`GoTo(Rec)`（失敗は `ErrorPresenter`・P3）。
- 意匠は Sさん 調整可（ロジック非依存・P5）。

### 2.9 `WeeklyThemeScreenController : ScreenRootBase`（Geidai.Theme）
- 責務: 専用「お題」画面（`SceneId.Theme`）。`WeeklyThemeController`＋`BackToHomeButton` を内包。
- `OnBackRequested`→ホームへ。レスポンシブ/SafeArea 配下（P5）。

---

## 3. 連携（テキスト表現）
- `WeeklyThemeController`/`WeeklyThemeScreenController` → `IContentService.GetCurrentTheme()` → `ContentService` → `ThemeCatalog`＋`ThemeSelector`。
- タップ → `ThemeContext.current` 設定 → `INavigationService.GoTo(Rec)`。
- Rec（U3）→ 任意で `ThemeContext.current` 参照（表示は任意）。
- 起動 → `ThemeBootstrap`（任意）→ `ContentService` に `ThemeCatalog` 注入。

## 4. 依存方向（循環なし）
```
Geidai.Theme (UI)
   └─> Geidai.Services (IContentService/ContentService/ThemeContext/INavigationService)
          └─> Geidai.Common (ThemeItem/ThemeCatalog/ThemeSelector/Result/SceneId/ScreenRootBase)
```
- Assembly-CSharp（旧 `WeeklyTextController`）への参照は作らない。旧は差し替え後に削除（BR-THEME-52）。

## 5. テスト対応（NFR-U5-04）
- `ThemeSelector.SelectIndex`：PBT（`-1`/`0..count-1`・決定的・剰余一致・年境界代表日付）。
- `ContentService`/`ThemeCatalog`：単体（空カタログ→`NotFound`・現在お題取得・`GetText("theme.current")`）。
- 配置: `Geidai.Tests`（EditMode）に追加（`Geidai.Theme`/`Geidai.Common` 参照）。

## 6. トレース
P1→2.3/5 ／ P2→2.4/2.5/2.8 ／ P3→2.6/2.8 ／ P4→2.1〜2.7・§4 ／ P5→2.8/2.9。NFR-U5-01〜06・Functional Design（domain-entities/business-logic-model/business-rules/frontend-components）に整合。
