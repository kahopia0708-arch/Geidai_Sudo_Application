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
- [x] **Step0** MCP 接続確認・コンソール ベースライン（Error 0/Warning 0）
- [x] **Step1** `Assets/Scripts/Common/Content/ThemeItem.cs`（`Geidai.Common.Content`・Serializable：`id`/`text`/`reading`/`hint`＋`IsValid`＝`text` 非空）
- [x] **Step2** `Assets/Scripts/Common/Content/ThemeCatalog.cs`（`ScriptableObject`・`List<ThemeItem> items`＋`ValidItems()`/`ValidCount`/`SetItems`。`[CreateAssetMenu]`）
- [x] **Step3** `Assets/Scripts/Common/Content/ThemeSelector.cs`（static 純粋：`SelectIndex(DateTime date, int count)`＝週番号→剰余・`count<=0`→`-1`。既存週番号ロジックを純粋移植）
- [x] **Step4** `IContentService` 拡張：`Result<ThemeItem> GetCurrentTheme()`＋`void SetCatalog(ThemeCatalog)`。既存 `GetText(key)` は不変（後方互換）
- [x] **Step5** `ContentService` 実装：`ThemeCatalog`＋`ThemeSelector` で今週のお題導出／空・無効は `Fail(NotFound)`。`GetText("theme.current")` 実装・他キー `NotImplemented`。時刻注入（`Func<DateTime>`）
- [x] **Step6** `Assets/Scripts/Services/Content/ThemeContext.cs`（セッション：`Current`/`HasValue`/`Set`/`Clear`・非永続）
- [x] **Step7** `Assets/Scripts/Theme/Geidai.Theme.asmdef`（`Geidai.Common`/`Geidai.Services`/`UnityEngine.UI`・**Rec 非依存**）
- [x] **Step8** `Assets/Scripts/Theme/ThemeBootstrap.cs`（`IContentService`/`ThemeContext` 確保・カタログ注入）
- [x] **Step9** `Assets/Scripts/Theme/WeeklyThemeController.cs`（再利用部品：`Refresh()`→反映/`emptyState`・`recordButton`→`ThemeContext.Set`→`GoTo(Rec)`＋失敗 `ErrorPresenter`）
- [x] **Step10** `Assets/Scripts/Theme/WeeklyThemeScreenController.cs`（`ScreenRootBase`・`OnShow` で `Refresh()`・`OnBackPressed`→ホーム。`BackToHomeButton` はシーン配置で併用）

### テスト
- [x] **Step11** EditMode テスト（`Geidai.Tests` に `Geidai.Theme` 参照追加）
  - `ThemeSelectorTests.cs`（PBT：`-1`[count<=0]/`0..count-1`／決定的／剰余一致／週回転／代表日付）
  - `ContentServiceThemeTests.cs`（空/無効カタログ→`NotFound`／有効→現在お題／`GetText("theme.current")`／`text` 空除外／`SetCatalog`）

### 検証・記録
- [x] **Step12** MCP 検証：`AssetDatabase.Refresh`→**コンパイル Error 0/Warning 0**、`ThemeSelector`/`ContentService` 純粋スモーク PASS、既定 `ThemeCatalog.asset`（13 オノマトペ）生成
- [x] **Step13** `../u5-theme/code/code-summary.md` 生成＋`stories.md`（US-THEME-01/02/03）実装状況追記＋commit

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
