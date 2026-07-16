# U5 weekly theme — Frontend Components（UI 構成・ハンドオフ）

**ユニット**: U5 weekly theme（お題）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / Functional Design（Part 2）
**決定**: Q6=A（専用 Theme 画面＋再利用 WeeklyThemeController の両対応）

> 前本さん が基本 UI 枠組みを実装 → Sさん が詳細な見た目/配置を調整（US-TECH-07）。データ駆動・`UITheme`・レスポンシブ/SafeArea（U1 基盤）を踏襲。

---

## 1. コンポーネント一覧

| コンポーネント | 種別 | 役割 | 配置先 |
|---|---|---|---|
| `WeeklyThemeScreenController` | `ScreenRootBase` サブクラス | 専用「お題」画面（`SceneId.Theme`）。今週のお題表示＋Rec 導線＋戻る | Theme シーンのルート |
| `WeeklyThemeController` | MonoBehaviour（再利用） | 今週のお題を表示する部品。ホーム上部バナー等どこにでも貼れる | Home 上部/Theme 画面内 など |
| `BackToHomeButton`（既存 U2） | MonoBehaviour | ホームへ戻る | Theme 画面 |
| `ErrorPresenter`（既存 U1） | MonoBehaviour | 遷移/取得エラーの通知バナー | 画面共通 |

---

## 2. WeeklyThemeController（再利用部品）
- **依存**: `IContentService`（お題取得）、`ThemeContext`（選択保持）、`INavigationService`（Rec 遷移）。`ServiceRegistry` から解決。
- **UI 参照（SerializeField）**:
  - `Text themeText`（お題本文・必須）
  - `Text readingText`（読み・任意）
  - `Text hintText`（ヒント・任意）
  - `Button recordButton`（「このおとを ろくおん！」等・任意）
  - `GameObject emptyState`（お題なし時のフォールバック表示）
- **振る舞い**:
  - `OnEnable`/`Refresh()` で `GetCurrentTheme()` → 成功なら各 Text に反映、失敗なら `emptyState` 表示（本文非表示）。
  - `recordButton` タップ → `ThemeContext.current` 設定 → `NavigationService.GoTo(Rec)`（失敗は `ErrorPresenter`）。
- **ハンドオフ点（Sさん）**: 文言/フォント/色は `UITheme`、レイアウト（縦横・バナー高さ）、`emptyState` の見た目、装飾（イラスト/背景）。ロジックは不変。

## 3. WeeklyThemeScreenController（専用画面）
- **継承**: `ScreenRootBase`（`OnBackRequested` でホームへ）。
- **構成**: 画面内に `WeeklyThemeController`（大きめ表示）＋`BackToHomeButton`。レスポンシブ Canvas / SafeArea 配下。
- **役割**: ホームの「今週のお題」メニュー（`ModuleId.WeeklyTheme` → `SceneId.Theme`）から遷移して開く独立画面。
- **状態**:

| 状態 | 表示 | 遷移 |
|---|---|---|
| Loading | 取得中（一瞬） | 取得完了で Show/Empty へ |
| Show | お題本文＋Rec 導線 | Rec タップ→Rec 画面 / 戻る→Home |
| Empty | フォールバック（お題なし） | 戻る→Home |

## 4. Home 上部バナー配置（任意）
- 同じ `WeeklyThemeController` を Home 画面の上部に貼るだけで「今週のお題」バナーになる（Q6=A）。
- 実際にホーム上部へ出すか、独立画面のみにするかは Sさん がシーンで選択（両対応の土台のみ提供）。

## 5. Rec 側の任意表示（US-THEME-02）
- Rec 画面（U3 `RecScreenController`）に「お題ラベル」を任意で追加可能。`ThemeContext.current` があれば表示、なければ非表示。
- U5 では Rec 画面のロジック変更は最小（表示は任意のため必須改修なし）。ラベル追加は Sさん/前本さん のシーン作業（MCP フォローアップ）。

## 6. レスポンシブ / SafeArea / アクセシビリティ
- U1 の `ResponsiveCanvasConfigurator` / `SafeAreaFitter` 配下に配置（縦横両対応・NFR-01/02）。
- 文字は大きく・平易（NFR-05）。読み/ヒントで理解を補助。

## 7. MCP フォローアップ（シーン配線・Code Generation 後）
- 既定 `ThemeCatalog` アセット生成（既存オノマトペを移行）。
- Theme シーン作成/更新：`WeeklyThemeScreenController`＋`WeeklyThemeController` 配置、`ContentService` へ `ThemeCatalog` 注入。
- Home 上部バナー（任意）配置、Rec お題ラベル（任意）配置。
- 旧 `WeeklyTextController` を差し替え後に削除（BR-THEME-52）。
- Build Settings に Theme シーン登録（未登録なら）。

## 8. トレース
US-THEME-01→WeeklyThemeController/ScreenController（表示） ／ US-THEME-02→Rec 任意表示・recordButton 導線 ／ US-THEME-03→ThemeCatalog 差し替え（データ駆動） ／ US-TECH-07→ハンドオフ点 ／ NFR-01/02→レスポンシブ/SafeArea ／ NFR-05→平易表示。
