# U5 weekly theme — Code Generation Plan（Part 1: 詳細計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U5 weekly theme（お題）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / Code Generation（Part 1: Planning）
**入力**: `../u5-theme/functional-design/*`, `../u5-theme/nfr-requirements/*`, `../u5-theme/nfr-design/*`（P1〜P5・logical-components）
**検証**: 公式 Unity AI Assistant（Unity MCP Server）でコンパイル/スモーク（US-TECH-05）

> 本計画は Part 2（生成）で実行する手順。各ステップ完了で `[x]` に更新する。
> **承認方法**: 計画に問題なければ「**Continue**」（または「done」）。修正点があれば指摘してください。

---

## 0. 方針・前提（確定済み設計の反映）
- **一方向依存**: `Geidai.Theme（UI）→ Geidai.Services（Content/Navigation）→ Geidai.Common（型/純粋/UI基盤）`。Assembly-CSharp（旧 `WeeklyTextController`）へは参照しない。
- **データ/純粋は Common**: `ThemeItem`・`ThemeCatalog`(SO)・`ThemeSelector`（純粋）。
- **実装/セッションは Services**: `IContentService` 後方互換拡張＋`ContentService` 実装＋`ThemeContext`（`Geidai.Services.Content`・既存名前空間）。
- **UI は新 `Geidai.Theme`**: `WeeklyThemeController`（再利用）＋`WeeklyThemeScreenController : ScreenRootBase`。
- **旧コード**: `WeeklyTextController`（Assembly-CSharp）は**残置**（コンパイル影響回避）→ シーン差し替え後に削除（MCP フォローアップ／BR-THEME-52）。
- **既存資産の再利用**: `Result`/`ResultCode.NotFound`、`SceneId.Theme`、`INavigationService.GoTo`、`ScreenRootBase`、`ErrorPresenter`、`UITheme`、`BackToHomeButton`、`ModuleRouter`（WeeklyTheme→Theme 配線済）。

---

## 1. 実装ステップ（Part 2 で実行）

### コード生成
- [ ] **Step0** MCP 接続確認・コンソール ベースライン（Error/Warning 現況）
- [ ] **Step1** `Assets/Scripts/Common/Content/ThemeItem.cs`（`Geidai.Common.Content`・Serializable：`id`/`text`/`reading`/`hint`＋`IsValid`＝`text` 非空）
- [ ] **Step2** `Assets/Scripts/Common/Content/ThemeCatalog.cs`（`ScriptableObject`・`List<ThemeItem> items`＋`ValidItems()`/`ValidCount`。`[CreateAssetMenu]` で S さん がアセット作成可）
- [ ] **Step3** `Assets/Scripts/Common/Content/ThemeSelector.cs`（static 純粋：`SelectIndex(DateTime date, int count)`＝週番号→剰余・`count<=0`→`-1`。既存 `WeeklyTextController` の週番号ロジックを純粋移植）
- [ ] **Step4** `IContentService` 拡張（`Geidai.Services.Content`）：`Result<ThemeItem> GetCurrentTheme()` 追加＋`void SetCatalog(ThemeCatalog)`（DI 用）。既存 `GetText(key)` は不変（後方互換）
- [ ] **Step5** `ContentService` 実装（`Geidai.Services.Content`）：`ThemeCatalog` 保持、`GetCurrentTheme()`＝有効項目抽出→`ThemeSelector.SelectIndex(now, validCount)`→`ThemeItem`／空・無効は `Fail(NotFound)`。`GetText("theme.current")` は本文/フォールバック、他キーは `NotImplemented`。時刻は注入可能（`Func<DateTime>` 既定 `DateTime.Now`）
- [ ] **Step6** `Assets/Scripts/Services/Content/ThemeContext.cs`（`Geidai.Services.Content`・セッション：`ThemeItem Current`/`HasValue`/`Set`/`Clear`。非永続）
- [ ] **Step7** `Assets/Scripts/Theme/Geidai.Theme.asmdef`（参照＝`Geidai.Common`/`Geidai.Services`/`UnityEngine.UI`・autoReferenced=true・**Rec 非依存**）
- [ ] **Step8** `Assets/Scripts/Theme/ThemeBootstrap.cs`（static：`IContentService` を解決/未登録なら `ContentService` 登録、`ThemeCatalog` を注入。`RecBootstrap`/`CollectionBootstrap` と同パターン）
- [ ] **Step9** `Assets/Scripts/Theme/WeeklyThemeController.cs`（MonoBehaviour 再利用：`themeText`/`readingText`/`hintText`/`recordButton`/`emptyState`。`Refresh()`＝`GetCurrentTheme()`→反映/`emptyState`。`recordButton`→`ThemeContext.Set`→`GoTo(Rec)`＋失敗 `ErrorPresenter`）
- [ ] **Step10** `Assets/Scripts/Theme/WeeklyThemeScreenController.cs`（`ScreenRootBase`：`WeeklyThemeController`＋`BackToHomeButton` を内包、`OnBackPressed`→ホーム。`OnShow` で `Refresh()`）

### テスト
- [ ] **Step11** EditMode テスト（`Geidai.Tests` に `Geidai.Theme` 参照追加）
  - `ThemeSelectorTests.cs`（PBT：戻り値 `-1`[count<=0] or `0..count-1`／決定的／剰余一致／年境界代表日付）
  - `ContentServiceThemeTests.cs`（空カタログ→`NotFound`／有効カタログ→現在お題取得／`GetText("theme.current")`／`text` 空除外）

### 検証・記録
- [ ] **Step12** MCP 検証：`AssetDatabase.Refresh`→コンパイル Error 0 目標、`ThemeSelector`/`ContentService` の純粋スモーク（可能な範囲）。既定 `ThemeCatalog` アセット生成（既存オノマトペ移行）を試行（不可なら MCP フォローアップに記載）
- [ ] **Step13** `../u5-theme/code/code-summary.md` 生成＋`stories.md` の US-THEME-01/02/03・US-TECH-07 に実装状況追記＋commit

---

## 2. 生成物一覧（想定パス）
| 種別 | パス |
|---|---|
| 型（Common） | `Assets/Scripts/Common/Content/ThemeItem.cs` |
| データ（Common/SO） | `Assets/Scripts/Common/Content/ThemeCatalog.cs` |
| 純粋（Common） | `Assets/Scripts/Common/Content/ThemeSelector.cs` |
| IF 拡張（Services） | `Assets/Scripts/Services/Content/IContentService.cs`（編集） |
| 実装（Services） | `Assets/Scripts/Services/Content/ContentService.cs`（編集） |
| セッション（Services） | `Assets/Scripts/Services/Content/ThemeContext.cs` |
| asmdef（Theme） | `Assets/Scripts/Theme/Geidai.Theme.asmdef` |
| 初期化（Theme） | `Assets/Scripts/Theme/ThemeBootstrap.cs` |
| UI 部品（Theme） | `Assets/Scripts/Theme/WeeklyThemeController.cs` |
| UI 画面（Theme） | `Assets/Scripts/Theme/WeeklyThemeScreenController.cs` |
| テスト（EditMode） | `Assets/Scripts/Tests/EditMode/ThemeSelectorTests.cs`, `ContentServiceThemeTests.cs` |
| ドキュメント | `aidlc-docs/construction/u5-theme/code/code-summary.md` |

## 3. スコープ外（フォローアップ）
- Theme シーンの実配線（`WeeklyThemeScreenController`/`WeeklyThemeController` 配置・`ThemeCatalog` 注入）、Home 上部バナー配置、Rec お題ラベル（任意）、旧 `WeeklyTextController` の物理削除、Build Settings 登録 → **MCP フォローアップ**（code-summary に明記）。
- ゲーム用コンテンツ取得（U6）。

## 4. リスク・緩和
- **MCP でプロジェクトアセンブリ参照実行が制限される**（U3/U4 既知）→ ロジック検証は EditMode Test Runner に集約。MCP は純粋関数の軽スモーク＋コンパイル確認中心。
- **Assembly-CSharp と新アセンブリの参照制約** → データ/純粋を `Geidai.Common` に置き回避（既定踏襲）。
- **`ThemeCatalog` 未注入で無表示** → `NotFound` フォールバック（BR-THEME-21/41）。

## 5. 完了条件
- Step0〜13 完了、コンパイル Error 0、EditMode テスト整備、code-summary/stories 更新・commit。
- 一方向依存（`Theme→Services→Common`）維持・`IContentService` 後方互換・旧コード非破壊。
