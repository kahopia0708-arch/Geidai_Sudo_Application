# U5 weekly theme — Business Rules（業務ルール）

**ユニット**: U5 weekly theme（お題）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / Functional Design（Part 2）

> 各ルールは Code Generation の受け入れ基準として使用。ID は `BR-THEME-xx`。

---

## 1. 週選択（Week Selection）
- **BR-THEME-01**: 「今週のお題」は表示時点の**端末ローカル日時**に基づいて決定する（サーバー・通信なし）。
- **BR-THEME-02**: 週番号は月曜起点で算出し、既存 `WeeklyTextController` の挙動（年初の最初の月曜基準・年初前は前年最終週扱い）を踏襲する。
- **BR-THEME-03**: 選択 index は `((weekNumber % count) + count) % count` で `0..count-1` に正規化する（負にならない）。
- **BR-THEME-04**: 週選択は純粋関数 `ThemeSelector.SelectIndex(date, count)` で実装し、副作用を持たない（同一入力→同一出力）。

## 2. お題データ（ThemeCatalog / ThemeItem）
- **BR-THEME-11**: `text` が空（空白のみ含む）の `ThemeItem` は無効とし、有効項目のみを選択対象にできる。
- **BR-THEME-12**: `ThemeCatalog` は Sさん がインスペクタで追加/編集/並べ替え可能（差し替えで反映・再ビルド不要 / FR-14）。
- **BR-THEME-13**: 既定の `ThemeCatalog` アセットは既存の固定オノマトペ（Kirakira/DonDon/FuwaFuwa/ChiriChiri/SariSari/GoboGobo/Fuwan/SariRa/ChiriRa/FuwaRa/DonSari/ChiriGobo/DonChiri）を初期値として持つ。
- **BR-THEME-14**: お題は制作側コンテンツであり、個人情報（PII）を含まない。

## 3. 表示・フォールバック
- **BR-THEME-21**: `ThemeCatalog` が未設定・空・全項目無効の場合は、フォールバック表示（お題なしの分かりやすい表示）とし、クラッシュしない（NFR-05/NFR-07）。
- **BR-THEME-22**: 表示文言・レイアウト・装飾は Sさん が調整可能（US-TECH-07）。ロジックは表示に依存しない。
- **BR-THEME-23**: 週が替われば次回表示で自動的に対応するお題へ切り替わる（状態を持たない）。実行中の即時切替は必須ではない。

## 4. お題→Rec 遷移
- **BR-THEME-31**: お題タップ時は `ThemeContext.current` に選択お題を設定してから `NavigationService.GoTo(Rec)` を実行する。遷移失敗は `Result` で受け、UI 通知（`ErrorPresenter`）してクラッシュしない。
- **BR-THEME-32**: `ThemeContext` は実行時セッションのみ（永続化しない）。保存メタ（`SoundClipMeta`）には記録しない（Q3=A・スコープ最小）。
- **BR-THEME-33**: Rec 画面でのお題表示は**任意**であり、`ThemeContext` 未設定でも Rec は通常どおり録音できる（US-THEME-02）。

## 5. ContentService
- **BR-THEME-41**: `ContentService.GetCurrentTheme()` はカタログ空/無効時に `Result.Fail(NotFound)` を返し、有効時に今週の `ThemeItem` を返す。
- **BR-THEME-42**: `ContentService.GetText("theme.current")` は今週のお題 `text` を返す。未対応キーは `Result.Fail(NotImplemented)`（U6 で拡張）。
- **BR-THEME-43**: `ThemeCatalog` は起動時に Service へ注入する（`ContentService` はカタログ未注入でも `NotFound` を返して安全に動く）。

## 6. 依存・アーキテクチャ
- **BR-THEME-51**: 依存は一方向 `Geidai.Theme → Geidai.Services → Geidai.Common`。循環を作らない。
- **BR-THEME-52**: 旧 `WeeklyTextController`（Assembly-CSharp）は新方式へ差し替え後に削除する（当面はコンパイル影響回避のため残置・MCP フォローアップ）。

## 7. トレース
US-THEME-01→BR-THEME-01〜04, 21, 23 ／ US-THEME-02→BR-THEME-31〜33 ／ US-THEME-03→BR-THEME-11〜13, 41〜43 ／ FR-13/14→全般 ／ NFR-05→BR-THEME-21/22 ／ NFR-09→BR-THEME-04（PBT） ／ US-TECH-07→BR-THEME-22。
