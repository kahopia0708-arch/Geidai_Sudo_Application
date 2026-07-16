# U5 weekly theme — Functional Design Plan（Part 1：計画＋明確化質問）

**ユニット**: U5 weekly theme（お題）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / Functional Design（Part 1）
**入力**: `unit-of-work.md`（U5）・`stories.md`（US-THEME-01/02/03）・`requirements.md`（FR-13/14）・企画資料（プロジェクト概要.md：ウィークリーテーマ＝音のお題・タップで Rec へ・ホーム上部配置想定・Sさんが内容差し替え）・既存 `WeeklyTextController.cs`（固定オノマトペ配列＋週番号選択）・`IContentService`（未実装の器）・`ModuleRouter`（WeeklyTheme→Theme 配線済）

> 本書は Part 1（計画と明確化）。各質問に `[Answer]:` で回答してください（無回答は「A（推奨）」で確定します）。回答確定後に Part 2 で設計成果物を生成します。

---

## 1. スコープ（U5 の責務）
- 週替わりの「音のお題」（オノマトペ等）を表示する（US-THEME-01 / FR-13）。
- お題タップから Rec へ遷移し、どのお題に対する録音かが（任意で）分かる（US-THEME-02 / FR-13）。
- お題テキストを Sさん が差し替え可能なデータ/設定として分離（US-THEME-03 / FR-14）。
- 既存の固定オノマトペ表示（`WeeklyTextController`）をデータ駆動へ移行し、`IContentService` を本実装。

**スコープ外**: ゲーム出題（U6）、クラウド/共有（Place 除外）、お題の自動配信・サーバー連携（完全オフライン）。

## 2. 依存
- U1（`IContentService` 器・`INavigationService`・`SceneId.Theme`・`ScreenRootBase`・UI基盤）
- U2（ホーム導線・`ModuleRouter.WeeklyTheme→Theme`）
- U3（遷移先 Rec）

---

## 3. 明確化質問（Part 1）

### Q1. お題データ構造（差し替え可能な構成 / FR-14）
A) (推奨) `ThemeCatalog`（ScriptableObject）に `ThemeItem`（`id` / `text`[オノマトペ] / 任意 `reading`[読み] / 任意 `hint`）のリストを持ち、Sさん がインスペクタで追加・編集・並べ替え可能。既存 `WeeklyTextController` の固定配列は `ThemeCatalog` へ移行（既定アセットを MCP で生成）。
B) JSON ファイル（Resources or persistentDataPath）で差し替え。
C) その他（自由記述）。

[Answer]:A

### Q2. 「今週のお題」選択ロジック
A) (推奨) 端末ローカル日時の週番号（月曜起点・既存 `WeeklyTextController` のロジック）を**純粋関数 `ThemeSelector.SelectIndex(DateTime date, int count)`** として切り出し（`count<=0` は安全に -1/空、`weekIndex % count`）。テスト可能・決定的。
B) 固定の開始日からの経過週数で選択。
C) その他（自由記述）。

[Answer]:A

### Q3. お題→Rec のお題情報の受け渡し（US-THEME-02 任意表示）
A) (推奨) 選択中のお題を軽量なセッション状態（`Geidai.Services.Content` の `ThemeContext`＝現在のお題を保持、`ServiceRegistry` 経由 or static）に載せ、Rec 画面が**任意で**「どのお題か」を表示できる。保存メタ（`SoundClipMeta`）には含めない（スコープ最小）。
B) お題テキストを保存メタにも記録する（Collection でお題が分かる）。
C) その他（自由記述）。

[Answer]:A

### Q4. 既存 `WeeklyTextController.cs`（Assembly-CSharp）の扱い
A) (推奨) 新 `Geidai.Theme` へ機能を移し、旧 `WeeklyTextController` は新方式へシーン差し替え後に削除（**MCP フォローアップ**）。当面はコンパイル影響回避のため残置し、新実装を並行提供。
B) 即削除。
C) その他（自由記述）。

[Answer]:A

### Q5. `IContentService` の実装範囲（U5）
A) (推奨) `IContentService.GetText(key)` を `ThemeCatalog` ベースで実装（例: `"theme.current"` で今週のお題テキストを返す）。加えてお題オブジェクト取得の専用 API（`Result<ThemeItem> GetCurrentTheme()`）を `Geidai.Services.Content` に**後方互換で追加**。ゲーム用パラメータ取得は U6。
B) `GetText` のみ実装（専用 API は作らない）。
C) その他（自由記述）。

[Answer]:A

### Q6. 表示場所（ホーム上部バナー vs 専用 Theme 画面）
A) (推奨) 両対応の土台：専用 `Theme` 画面（`WeeklyThemeScreenController : ScreenRootBase`）＋ 再利用可能な `WeeklyThemeController`（ホーム上部にも貼れる MonoBehaviour）。実配置（ホーム上部/独立画面）は Sさん がシーンで調整（US-TECH-07）。`ModuleRouter` は WeeklyTheme→Theme 配線済。
B) 専用画面のみ。
C) ホーム上部のみ。

[Answer]:A

### Q7. テスト方針（NFR-09 / PBT）
A) (推奨) 純粋 `ThemeSelector.SelectIndex(date, count)` を PBT（結果が `0..count-1` に収まる・`count` に対する剰余一致・決定的・境界[`count=0`→-1]）＋ `ContentService`／`ThemeCatalog` の単体（空カタログ→フォールバック・現在お題取得）。EditMode。
B) 単体テストのみ。
C) その他（自由記述）。

[Answer]:A

---

## 4. Part 2 生成予定物（回答確定後）
- [x] `construction/u5-theme/functional-design/domain-entities.md`（`ThemeItem` / `ThemeCatalog`(SO) / `ThemeContext` / 週選択の概念）
- [x] `construction/u5-theme/functional-design/business-logic-model.md`（週選択→お題表示→タップ→Rec 遷移のデータフロー・Mermaid・ContentService 取得経路）
- [x] `construction/u5-theme/functional-design/business-rules.md`（BR-THEME-xx：週選択・空カタログ フォールバック・差し替え反映・遷移安全）
- [x] `construction/u5-theme/functional-design/frontend-components.md`（`WeeklyThemeController` / `WeeklyThemeScreenController` の構成・状態・操作・Sさん ハンドオフ点）

## 5. 完了条件（Functional Design）
- 回答（Q1〜Q7）確定・矛盾/曖昧なし。
- 上記 4 成果物を生成し、後方互換・データ駆動・一方向依存（`Theme→Services→Common`）の方針が明確。
- Part 2 完了後に承認ゲート提示。
