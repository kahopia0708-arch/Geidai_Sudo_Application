# U5 weekly theme — Code Summary（コード生成サマリ）

**ユニット**: U5 weekly theme（お題）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / Code Generation（Part 2 完了）
**検証**: 公式 Unity AI Assistant（Unity MCP Server / `user-unity-mcp`）

---

## 1. 概要
「今週のお題」を表示し、タップで Rec に進む機能を実装。お題データは差し替え可能な ScriptableObject（`ThemeCatalog`）、週選択は純粋関数（`ThemeSelector`）、取得は `IContentService` 後方互換拡張で提供。UI は新アセンブリ `Geidai.Theme` に再利用部品＋専用画面として配置。一方向依存 `Geidai.Theme → Geidai.Services → Geidai.Common` を維持。

## 2. 新規ファイル
| パス | 役割 | アセンブリ/名前空間 |
|---|---|---|
| `Assets/Scripts/Common/Content/ThemeItem.cs` | お題1件（id/text/reading/hint・`IsValid`） | `Geidai.Common.Content` |
| `Assets/Scripts/Common/Content/ThemeCatalog.cs` | お題一覧 SO（`ValidItems`/`ValidCount`/`SetItems`・`[CreateAssetMenu]`） | `Geidai.Common.Content` |
| `Assets/Scripts/Common/Content/ThemeSelector.cs` | 週選択の純粋関数（`SelectIndex`/`WeekOfYear`） | `Geidai.Common.Content` |
| `Assets/Scripts/Services/Content/ThemeContext.cs` | お題→Rec 受け渡しセッション（非永続） | `Geidai.Services.Content` |
| `Assets/Scripts/Theme/Geidai.Theme.asmdef` | 新アセンブリ（Common/Services/UnityEngine.UI・**Rec 非依存**） | `Geidai.Theme` |
| `Assets/Scripts/Theme/ThemeBootstrap.cs` | `IContentService`/`ThemeContext` の確保・カタログ注入 | `Geidai.Theme` |
| `Assets/Scripts/Theme/WeeklyThemeController.cs` | お題表示の再利用部品（バナー/画面共用） | `Geidai.Theme` |
| `Assets/Scripts/Theme/WeeklyThemeScreenController.cs` | 専用お題画面（`ScreenRootBase`・戻る=ホーム） | `Geidai.Theme` |
| `Assets/Scripts/Tests/EditMode/ThemeSelectorTests.cs` | `ThemeSelector` の PBT＋例示 | `Geidai.Tests` |
| `Assets/Scripts/Tests/EditMode/ContentServiceThemeTests.cs` | `ContentService`（お題取得）単体 | `Geidai.Tests` |
| `Assets/Settings/ThemeCatalog.asset` | 既定お題カタログ（13 オノマトペ移行・MCP 生成） | アセット |

## 3. 変更ファイル
| パス | 変更内容 |
|---|---|
| `Assets/Scripts/Services/Content/IContentService.cs` | `Result<ThemeItem> GetCurrentTheme()`／`void SetCatalog(ThemeCatalog)` を追加（既存 `GetText` は不変＝後方互換） |
| `Assets/Scripts/Services/Content/ContentService.cs` | `NotImplemented` の器から本実装へ。`ThemeCatalog`＋`ThemeSelector` で今週のお題導出、空/無効は `Fail(NotFound)`、`GetText("theme.current")` 実装、時刻注入対応 |
| `Assets/Scripts/Tests/EditMode/Geidai.Tests.asmdef` | `Geidai.Theme` を参照に追加 |

## 4. 依存構造（循環なし）
```
Geidai.Theme (UI: WeeklyThemeController/ScreenController/ThemeBootstrap)
   └─> Geidai.Services (IContentService/ContentService/ThemeContext/INavigationService/ServiceRegistry)
          └─> Geidai.Common (ThemeItem/ThemeCatalog/ThemeSelector/Result/SceneId/ScreenRootBase/ErrorPresenter)
```
- Assembly-CSharp（旧 `WeeklyTextController`）への参照なし。Collection/Rec への依存なし。

## 5. NFR / 設計トレース
- **P1 純粋週選択**（NFR-U5-01/04）: `ThemeSelector.SelectIndex`（O(1)・決定的・時刻注入）。
- **P2 空フォールバック集約**（NFR-U5-02）: `ContentService.GetCurrentTheme`→UI `emptyState`。
- **P3 遷移・受け渡し安全**（NFR-U5-02）: `ThemeContext`→`GoTo(Rec)`・失敗 `ErrorPresenter`・未設定でも通常録音・非永続。
- **P4 配置・IF 拡張**（NFR-U5-05）: 新 `Geidai.Theme`／型・純粋は `Common`／`IContentService` 後方互換拡張。
- **P5 表示 UI 両対応**（NFR-U5-03/05）: 再利用 `WeeklyThemeController`＋専用 `WeeklyThemeScreenController`・意匠 Sさん。
- **プライバシー**（NFR-U5-06）: お題は PII なし・`ThemeContext` 非永続・外部送信なし。

## 6. MCP 検証結果（`user-unity-mcp`）
- ベースライン: Error 0 / Warning 0。
- `AssetDatabase.Refresh`（ドメインリロードで一時切断→再接続）後: **コンパイル Error 0 / Warning 0**。
- スモーク（`Unity_RunCommand`）:
  - `ThemeSelector`: `i0=0, i1=1（翌週+1）, neg=-1（count<=0）, ok=True`。
  - `ContentService`: `empty=NotFound, cur=DonDon, txt=DonDon, unknown=NotImplemented, ok=True`。
- 既定 `ThemeCatalog.asset` を `Assets/Settings/` に生成（13 項目）。
- EditMode テスト（`ThemeSelectorTests`/`ContentServiceThemeTests`）は Unity Test Runner で実行（MCP はプロジェクトアセンブリの Test 実行を直接行わないため・U3/U4 と同方針）。純粋ロジックは上記スモークで健全性を確認済み。

## 7. UI ハンドオフ点（Sさん / US-TECH-07）
- お題本文/読み/ヒントのフォント・配色・レイアウト（`UITheme` 準拠）。
- `emptyState`（お題なし）の見た目。
- 専用画面 or ホーム上部バナーのどちらで出すか（両対応の土台を提供）。
- 「ろくおんする」導線ボタンの意匠・当たり判定。

## 8. 残タスク（MCP フォローアップ）
- Theme シーン作成/更新：`WeeklyThemeScreenController`＋`WeeklyThemeController` 配置、`ThemeCatalog`（既定アセット）注入、レスポンシブ/SafeArea 参照設定。
- Home 上部バナー（任意）に `WeeklyThemeController` 配置。
- Rec 画面に「お題ラベル」（任意）追加（`ThemeContext.Current` 参照）。
- 旧 `WeeklyTextController` をシーンから外し、スクリプト削除（BR-THEME-52）。
- Build Settings に Theme シーン登録（未登録なら）。
- （任意）`AppManager` で起動時に `ThemeCatalog` を `IContentService` へ注入 or 各画面で `ThemeBootstrap` に委譲。
