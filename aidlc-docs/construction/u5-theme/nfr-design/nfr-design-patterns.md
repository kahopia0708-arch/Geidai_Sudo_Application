# U5 weekly theme — NFR Design Patterns（実現パターン）

**ユニット**: U5 weekly theme（お題）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / NFR Design（Part 2）
**決定**: Q1〜Q5＝すべて A（推奨）
**前提**: U1〜U4 の設計パターン（`Result<T>`／`ScreenRootBase`＋レスポンシブ/SafeArea／`ErrorPresenter`／`ServiceRegistry` DI／`SafeLogger`／純粋関数化）を踏襲。本書は **U5 固有の実現パターン** を定義する。

> 数値目標は NFR Requirements（NFR-U5-01〜06）で確定済み。ここでは「どう実現するか」を定める。

---

## P1. 週選択＝純粋関数パターン（NFR-U5-01/04 / Q1=A）
- **パターン**: 決定的な純粋関数へロジックを隔離し、時刻・UI から分離。
- **実装方針**:
  - `ThemeSelector.SelectIndex(DateTime date, int count) : int`（`Geidai.Common`・static）。
    1. `count <= 0` → `-1`。
    2. その年の最初の月曜日を基準に経過週から週番号 `w` を算出（`date` が最初の月曜より前なら前年扱い＝既存 `WeeklyTextController` の挙動を踏襲）。
    3. `index = ((w % count) + count) % count`（0..count-1 に正規化）。
  - **時刻は引数注入**（呼び出し側が `DateTime.Now`＝端末ローカルを渡す／テストは固定日付）。
  - **O(1)・アロケーションなし**。**毎フレーム呼ばない**（画面表示時／`Refresh()` 時のみ）。
- **受入**: 戻り値は `-1`（count<=0）または `0..count-1`／同一入力で決定的／`count` に対する剰余一致（PBT）。

## P2. 空/無効カタログのフォールバック集約（NFR-U5-02 / Q2=A）
- **パターン**: 取得の単一窓口（`ContentService`）にフォールバック判定を集約し、UI は結果 `Result` に従うだけにする。
- **実装方針**:
  - `ContentService.GetCurrentTheme() : Result<ThemeItem>`
    - `ThemeCatalog == null` または **有効項目 0**（`text` 空を除外した件数=0）→ `Result.Fail(NotFound, "おだいが まだ ないよ")`。
    - `ThemeSelector.SelectIndex(now, validCount)` が有効 → 対応する有効 `ThemeItem` を `Result.Ok`。
  - `GetText("theme.current")` は成功時に本文、Fail 時は空/フォールバック文字列。
  - 有効項目の抽出（`text` 空除外）は `ContentService` 側で実施。
  - UI（`WeeklyThemeController`）は **Fail 受領→`emptyState` 表示・本文非表示・録音導線無効**（クラッシュしない）。
- **受入**: 空カタログ注入→フォールバック表示（クラッシュなし）。

## P3. 遷移・受け渡しの安全パターン（NFR-U5-02 / Q3=A）
- **パターン**: セッション状態の受け渡し＋`Result` による遷移失敗の非致命化。
- **実装方針**:
  - お題タップ→ `ThemeContext.current = selectedItem`（`Geidai.Services.Content`・実行時のみ）→ `INavigationService.GoTo(SceneId.Rec)`。
  - 遷移は `Result` で受け、失敗（シーン未登録等）は `ErrorPresenter` 表示・アプリ継続。
  - `ThemeContext` は **永続化しない・保存メタ非記録**。アプリ再起動でクリア可。
  - Rec 側は `ThemeContext.current` を**任意参照**（未設定でも通常録音）。
  - `ThemeContext` は `ServiceRegistry` 経由 or static ホルダで解決し、`Geidai.Theme → Geidai.Services` の一方向依存を維持。
- **受入**: 遷移失敗注入→エラー表示のみでクラッシュなし／`ThemeContext` 未設定でも Rec 通常録音。

## P4. 配置・IContentService 後方互換拡張（NFR-U5-05 / Q4=A）
- **パターン**: 純粋ロジック/データ型を横断層（`Common`）へ、実装/セッションを `Services` へ、UI を専用アセンブリへ分離した一方向依存。
- **実装方針**:
  - 新規 `Geidai.Theme`（UI）：`Geidai.Common`/`Geidai.Services`/`UnityEngine.UI` の一方向参照。
  - `ThemeItem`/`ThemeCatalog`(SO)・`ThemeSelector`（純粋）は `Geidai.Common`。
  - `IContentService` を**後方互換拡張**：既存 `GetText(key)` は維持、`Result<ThemeItem> GetCurrentTheme()` を追加。
  - `ContentService`（実装）・`ThemeContext` は `Geidai.Services.Content`。
  - 起動時に `ThemeCatalog` を Service へ注入（`AppManager` or `ThemeBootstrap`・未注入でも `NotFound` で安全）。
  - ロジックは POCO/静的へ寄せ MonoBehaviour 依存を最小化。
- **受入**: 一方向依存で循環なし・既存 `IContentService` 利用に影響なし（後方互換）。

## P5. 表示 UI・両対応・旧 Controller 差替（NFR-U5-03/05 / Q5=A）
- **パターン**: 再利用可能な表示部品＋画面ホストの分離（データ駆動・意匠分離）。
- **実装方針**:
  - `WeeklyThemeController`（MonoBehaviour・再利用）＝`IContentService` から今週のお題取得→`Text`（本文/読み/ヒント）反映、`recordButton` で `ThemeContext` 設定→Rec 遷移、Fail 時 `emptyState`。
  - `WeeklyThemeScreenController : ScreenRootBase`＝専用「お題」画面（`SceneId.Theme`）に `WeeklyThemeController` を内包＋`BackToHomeButton`。
  - **両対応**：同じ `WeeklyThemeController` を Home 上部バナーにも配置可能（実配置は Sさん）。
  - 文言/意匠/レイアウトは `UITheme` 準拠で Sさん 調整可（US-TECH-07）。レスポンシブ/SafeArea 配下。
  - 旧 `WeeklyTextController`（Assembly-CSharp）は**当面残置**（コンパイル影響回避）→**シーン差し替え後に削除**（MCP フォローアップ／BR-THEME-52）。Rec お題ラベルは任意追加（必須改修なし）。
- **受入**: 大きく平易な表示・意匠調整の余地・一方向依存維持。

## セキュリティ/プライバシー（NFR-U5-06・継続）
- お題は PII なし（NFR-04 N/A）。`ThemeContext` 非永続。外部送信なし（NFR-02）。`SafeLogger` で不要情報を出さない。

---

## トレース（パターン → NFR/機能）
| パターン | NFR | Functional/BR |
|---|---|---|
| P1 純粋週選択 | NFR-U5-01/04 | business-logic-model §2・BR-THEME-01〜04 |
| P2 空フォールバック集約 | NFR-U5-02 | BR-THEME-21/41・business-logic-model UC-1 |
| P3 遷移・受け渡し安全 | NFR-U5-02 | BR-THEME-31〜33・UC-3 |
| P4 配置・IF 拡張 | NFR-U5-05 | BR-THEME-41〜43/51/52 |
| P5 表示 UI・両対応 | NFR-U5-03/05 | frontend-components・BR-THEME-22 |
