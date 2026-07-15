# U5 weekly theme — NFR Design Plan（計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U5 weekly theme（お題）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / NFR Design（Part 1: 計画）
**入力**: `../u5-theme/nfr-requirements/nfr-requirements.md`, `tech-stack-decisions.md`, `../u5-theme/functional-design/*`, U1〜U4 NFR Design 成果物（`../u1-foundation|u2-foundation|u3-rec|u4-collection/nfr-design/*`）

> 目的: U5 の NFR（表示性能・空カタログ堅牢性・ユーザビリティ・テスト容易性・保守性/配置）を**設計パターン**と**論理コンポーネント**へ落とし込む。数値は NFR Requirements で確定済み。ここでは「どう実現するか（パターン）」を決める。
> **回答方法**: 各質問の `[Answer]:` に記号（例: `A`）。合う選択肢が無ければ「Other」。各質問に「(推奨)」あり。完了で「done」（または「全部推奨で」）。

---

## A. 実行チェックリスト（Part 2 で実行）
- [x] `../u5-theme/nfr-design/nfr-design-patterns.md` を生成（各 NFR の実現パターン）
- [x] `../u5-theme/nfr-design/logical-components.md` を生成（論理コンポーネント・責務・連携）
- [x] NFR Requirements / Functional Design とのトレース整合を確認

> **回答**: Q1〜Q5＝すべて A（推奨）。矛盾なし。Part 2 実行済み（2026-07-16）。

## B. カテゴリ適用性（このユニットでの判断）
- **Resilience（耐障害）**: 適用（空/無効カタログのフォールバック・遷移失敗の安全処理）。ネットワーク/永続化再試行系は N/A（お題は同梱アセット・書込ほぼ無し）。
- **Performance（性能）**: 適用（軽量テキスト表示・週選択 O(1)・呼び出し頻度制御）。GC はほぼ無視できる規模。
- **Scalability（スケーラビリティ）**: 限定適用（お題件数は数十件想定・剰余選択で件数増に耐性）。サーバスケールは N/A。
- **Security（セキュリティ）**: 限定適用（お題は PII なし＝NFR-04 は N/A。外部送信なし・非ログは踏襲）。
- **Logical Components（論理部品）**: 適用（`ThemeSelector`・`ThemeItem`/`ThemeCatalog`・`ContentService`/`ThemeContext`・`WeeklyThemeController`/`WeeklyThemeScreenController`）。

## B-2. U1〜U4 から継承する設計パターン（再質問しない・前提）
- **エラー伝搬**: `Result<T>`（成功/失敗＋理由コード）。致命的でない失敗はクラッシュさせない。
- **UI 基盤**: `ScreenRootBase` ＋ `ResponsiveCanvasConfigurator` ＋ `SafeAreaFitter`（表示時/向き変更で再適用）。固定px依存排除。
- **通知**: `ErrorPresenter`（子ども向けバナー）。
- **DI**: 軽量サービスロケータ（`ServiceRegistry`）＋インターフェース（`IContentService`/`INavigationService`）。
- **性能/GC**: 同期API基本・純粋計算はアロケーション回避、呼び出し頻度を表示イベントに限定。
- **セキュリティ**: 端末外送信なし、`SafeLogger` で非ログ、本番で詳細エラー非表示。
- **テスト**: 純粋関数化＋I/O 抽象化（インターフェース）で PBT/モック可能に。
- **横断データ配置**: 純粋ロジック・データ型は `Geidai.Common`（Assembly-CSharp 依存を回避）。

> ※ 上記の変更が必要な場合のみ、該当質問で Other 指定してください。

---

## C. 明確化のための質問（Q1〜Q5）

### Question 1（Performance/Testability — 週選択の純粋関数パターン）
「今週のお題」選択をどう実装するか？

A) (推奨) **`ThemeSelector.SelectIndex(DateTime date, int count)` を `Geidai.Common` の静的純粋関数**として実装（既存 `WeeklyTextController` の週番号ロジックを純粋化）。手順＝(1) `count<=0`→`-1`／(2) その年の最初の月曜を基準に経過週で週番号 `w` を算出（年初前は前年扱い＝既存挙動踏襲）／(3) `index = ((w % count) + count) % count`。**時刻は引数注入**（呼び出し側が `DateTime.Now` を渡す＝テスト時は固定日付）。O(1)・アロケーションなし・**毎フレーム呼ばない**（表示時/`Refresh()` 時のみ）。**受入＝戻り値は `-1` または `0..count-1`・決定的・剰余一致（PBT）**（NFR-U5-01/04）。

B) 週選択を `ContentService` 内にインライン実装（純粋分離なし）。

C) Other（[Answer]: の後に記述）

[Answer]:A

### Question 2（Resilience — 空/無効カタログのフォールバック）
カタログ未設定・0 件・全項目無効時の安全パターンは？

A) (推奨) **`ContentService` に集約**：`GetCurrentTheme()` は (1) `ThemeCatalog==null` or 有効項目 0 → `Result.Fail(NotFound, "おだいが まだ ないよ")`／(2) `ThemeSelector.SelectIndex` が有効 index → 該当 `ThemeItem` を `Result.Ok`。`GetText("theme.current")` も同様に Fail 時は空/フォールバック文字列。UI（`WeeklyThemeController`）は **Fail 受領で `emptyState` 表示・本文非表示・録音導線無効**（クラッシュしない）。有効項目の抽出（`text` 空を除外）も `ContentService` 側で行う。**受入＝空カタログ注入→フォールバック表示（クラッシュなし）**（NFR-U5-02）。

B) UI 側で null/空を都度チェック（集約なし）。

C) Other（[Answer]: の後に記述）

[Answer]:A

### Question 3（Resilience — お題→Rec 遷移と受け渡しの安全パターン）
お題タップから Rec への遷移をどう安全に実現するか？

A) (推奨) **`ThemeContext`（`Geidai.Services.Content`・実行時セッション）に選択お題を保持**してから `INavigationService.GoTo(SceneId.Rec)` を実行。遷移は `Result` で受け、失敗（シーン未登録等）は `ErrorPresenter` 表示・アプリ継続。`ThemeContext` は**永続化しない・保存メタ非記録**。Rec 側は `ThemeContext.current` を**任意参照**（未設定でも通常録音）。`ThemeContext` は `ServiceRegistry` 経由 or static ホルダで解決し、`Geidai.Theme→Geidai.Services` の一方向依存を保つ。**受入＝遷移失敗注入→エラー表示のみでクラッシュなし／未設定でも Rec 通常録音**（NFR-U5-02）。

B) お題テキストを `PlayerPrefs` 等に一時保存して受け渡す。

C) Other（[Answer]: の後に記述）

[Answer]:A

### Question 4（Maintainability — アセンブリ配置・IContentService 拡張・データ型）
U5 の論理コンポーネント配置と `IContentService` 拡張の設計は？

A) (推奨) **新規 `Geidai.Theme`**（`Theme → Services → Common`＋`UnityEngine.UI` 一方向）に UI（`WeeklyThemeController` 再利用部品＋`WeeklyThemeScreenController : ScreenRootBase`）を配置。**`ThemeItem`/`ThemeCatalog`(SO)・`ThemeSelector`（純粋）は `Geidai.Common`**（ContentService から参照）。**`IContentService` を後方互換拡張**：`GetText("theme.current")` を `ThemeCatalog` ベース実装＋`Result<ThemeItem> GetCurrentTheme()` を追加。`ContentService`（実装）と `ThemeContext` は `Geidai.Services.Content`。起動時に `ThemeCatalog` を Service へ注入（`AppManager` or `ThemeBootstrap`・未注入でも `NotFound` で安全）。ロジックは POCO/静的へ寄せ MonoBehaviour 依存最小化。**受入＝一方向依存で循環なし・既存 `IContentService` 利用に影響なし（後方互換）**（NFR-U5-05）。

B) `Geidai.Theme` を作らず `Geidai.Foundation` に相乗り（アセンブリ増やさない）。

C) Other（[Answer]: の後に記述）

[Answer]:A

### Question 5（Usability/UI — 表示 UI と両対応・旧 Controller 差替）
お題表示 UI の構成と既存 `WeeklyTextController` の扱いは？

A) (推奨) **再利用部品 `WeeklyThemeController`（MonoBehaviour）**＝`IContentService` から今週のお題を取得し `Text`（本文/読み/ヒント）へ反映、`recordButton` で `ThemeContext` 設定→Rec 遷移、Fail 時 `emptyState`。これを **専用 `Theme` 画面（`WeeklyThemeScreenController`）にも Home 上部バナーにも配置可能**（両対応の土台・実配置は Sさん）。文言/意匠/レイアウトは `UITheme` 準拠で **Sさん 調整可**（US-TECH-07）。旧 `WeeklyTextController`（Assembly-CSharp）は**当面残置**（コンパイル影響回避）し、**シーン差し替え後に削除**（MCP フォローアップ／BR-THEME-52）。Rec 画面のお題ラベルは任意追加（必須改修なし）。**受入＝大きく平易な表示・意匠調整の余地・一方向依存維持**（NFR-U5-03/05）。

B) 専用画面のみ（再利用部品にしない）。

C) Other（[Answer]: の後に記述）

[Answer]:A

---

## D. 完了条件
- Q1〜Q5 に回答 → 矛盾チェック（曖昧回答は追質問）→ nfr-design-patterns.md / logical-components.md を生成 → 承認ゲート。
- U1〜U4 の設計パターンを踏襲し、U5 固有の論理部品（純粋 `ThemeSelector`・`ThemeItem`/`ThemeCatalog`・`ContentService`/`ThemeContext`・`WeeklyThemeController`/`WeeklyThemeScreenController`）を明確化する。
- NFR Requirements（NFR-U5-01〜06）・Functional Design（domain-entities/business-logic-model/business-rules/frontend-components）へのトレースが取れている。
