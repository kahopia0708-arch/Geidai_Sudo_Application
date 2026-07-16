# U5 weekly theme — Domain Entities（ドメインモデル）

**ユニット**: U5 weekly theme（お題）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / Functional Design（Part 2）
**決定**: Q1=A（ThemeCatalog SO＋ThemeItem）/ Q2=A（純粋 ThemeSelector）/ Q3=A（ThemeContext セッション）/ Q5=A（ContentService お題ベース実装）

> 技術非依存の概念モデル。実体（C# 型/アセンブリ）は Code Generation で確定。JsonUtility/ScriptableObject でシリアライズ可能な素直な構造に限定。

---

## 1. エンティティ一覧

| # | エンティティ | 種別 | 役割 | 永続/供給元 |
|---|---|---|---|---|
| 1 | `ThemeItem` | 値オブジェクト（Serializable） | 1 件のお題（オノマトペ等） | `ThemeCatalog` 内に埋め込み |
| 2 | `ThemeCatalog` | ScriptableObject | お題の一覧（Sさん が差し替え） | Unity アセット（`Assets/Settings/`） |
| 3 | `ThemeContext` | セッション状態（POCO） | 「いま選ばれているお題」を保持し Rec へ伝える | 実行時のみ（永続化しない） |
| 4 | `WeeklyThemeResult`（概念） | 表示用の投影 | 今週のお題（`ThemeItem`＋週番号）を UI に渡す | 実行時のみ（`ContentService` が生成） |

---

## 2. ThemeItem（お題 1 件）

| フィールド | 型 | 必須 | 説明 |
|---|---|---|---|
| `id` | string | 任意 | 安定 ID（差し替え時のトレース用・空可） |
| `text` | string | 必須 | お題本文（オノマトペ等・例: "キラキラ"/"Kirakira"） |
| `reading` | string | 任意 | 読み仮名（表示補助・空可） |
| `hint` | string | 任意 | ひとことヒント（"どんな音かな？" 等・空可） |

- **制約**: `text` が空の項目は「無効」として選択対象から除外し得る（BR-THEME-11 参照）。
- **PII なし**: お題は制作側コンテンツで個人情報を含まない。

## 3. ThemeCatalog（お題一覧・ScriptableObject）

| フィールド | 型 | 説明 |
|---|---|---|
| `items` | `List<ThemeItem>` | お題のリスト（Sさん がインスペクタで追加/編集/並べ替え） |

- **差し替え可能構成（FR-14 / US-THEME-03）**: `ThemeCatalog` アセットの `items` を編集するだけで反映。アプリの作り直し不要。
- **既定アセット**: 既存 `WeeklyTextController` の固定オノマトペ（Kirakira/DonDon/FuwaFuwa …）を初期値として移行（既定アセットは Code Generation で MCP 生成）。
- **供給**: `IContentService` 実装（`ContentService`）が `ThemeCatalog` を参照して「今週のお題」を導出する。カタログは DI/インスペクタ注入（Service へ渡す）。

## 4. ThemeContext（お題→Rec の受け渡し・Q3=A）

| フィールド | 型 | 説明 |
|---|---|---|
| `current` | `ThemeItem`（or null） | 直近にユーザーが選んだ/表示中のお題 |
| `hasValue` | bool（導出） | `current != null` |

- **役割**: お題タップで Rec へ遷移する際、「どのお題に対する録音か」を Rec が**任意で**表示できるよう保持（US-THEME-02）。
- **境界**: 実行時セッションのみ。**保存メタ（`SoundClipMeta`）には記録しない**（スコープ最小・Q3=A）。アプリ再起動でクリアされてよい。
- **配置**: `Geidai.Services.Content`（`ServiceRegistry` 経由 or static ホルダ）。Rec は `Geidai.Services` に依存済みのため循環を生まない。

## 5. WeeklyThemeResult（今週のお題・表示用投影）
- `ContentService.GetCurrentTheme()` の戻り（`Result<ThemeItem>`）として表現。UI へは `ThemeItem`（＋任意で週番号）を渡す。
- 週番号の導出は純粋関数 `ThemeSelector.SelectIndex(date, count)`（Q2=A）。

---

## 6. 関係（テキスト表現）
- `ThemeCatalog` 1 — 多 `ThemeItem`（埋め込み）。
- `ContentService` → `ThemeCatalog` を参照し、`ThemeSelector` で今週の index を決定 → `ThemeItem` を返す。
- `WeeklyThemeController`/`WeeklyThemeScreenController`（UI）→ `IContentService` から今週のお題を取得して表示、タップ時に `ThemeContext.current` を設定して `INavigationService.GoTo(Rec)`。
- Rec 画面（U3）→ 任意で `ThemeContext.current` を参照して表示（表示しなくても動作）。

## 7. 依存・境界
- 一方向：`Geidai.Theme` → `Geidai.Services`（`IContentService`/`INavigationService`/`ThemeContext`）→ `Geidai.Common`（`SceneId`/`Result`/`ScreenRootBase`）。
- `ThemeCatalog`/`ThemeItem` は横断データのため配置は Code Generation で確定（`Geidai.Common` もしくは `Geidai.Services.Content`。ContentService が参照するため `Geidai.Common` が有力）。
- 完全オフライン・外部通信なし。
