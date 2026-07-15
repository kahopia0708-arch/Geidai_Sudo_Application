# U5 weekly theme — NFR Requirements Plan（非機能要件・技術選定 計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U5 weekly theme（お題）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / NFR Requirements（Part 1: Planning）
**入力**: `../u5-theme/functional-design/*`、`../../inception/requirements/requirements.md`（NFR-01〜12 / FR-13/14）、U1〜U4 NFR 成果物（`../u1-foundation|u2-foundation|u3-rec|u4-collection/nfr-requirements/*`）
**対象NFR**: ユーザビリティ(NFR-05)、信頼性/堅牢性(NFR-07)、パフォーマンス(NFR-06)、テスト容易性(NFR-09/PBT)、保守性(NFR-08/10)、レスポンシブ/SafeArea(NFR-11/12)、プライバシー(NFR-04＝お題は PII なしで N/A 見込み)

> 本ステージで U5 の**非機能目標の具体値**と**技術選定の差分**を確定する。
> **回答方法**: 各質問の `[Answer]:` に記号（例: `A`）。合う選択肢が無ければ「Other」。各質問に「(推奨)」あり。完了で「done」（または「全部推奨で」）。

---

## A. 実行チェックリスト（Part 2 で実行）
- [ ] `../u5-theme/nfr-requirements/nfr-requirements.md` を生成（U5 の NFR 目標・受入可能値）
- [ ] `../u5-theme/nfr-requirements/tech-stack-decisions.md` を生成（U5 の技術選定差分・根拠）
- [ ] 要件（NFR-04/05/06/07/08/09/11/12 / FR-13/14）・ストーリー（US-THEME-01/02/03 / US-TECH-07）とのトレース整合を確認

## B. 前提（U1〜U4 で確定済み・U5 も踏襲。原則 再質問しない）
- **プラットフォーム**（NFR-01）: iOS 15+ / Android 8.0(API26)+、スマホ〜タブレット、縦横両対応。
- **レスポンシブ**（NFR-11）: `ResponsiveCanvasConfigurator`、参照解像度 1080×1920、Scale With Screen Size、Match=0.5。固定 px 依存を排除。
- **SafeArea**（NFR-12）: `SafeAreaFitter`（`Screen.safeArea` 追従・向き/解像度変更で再計算）。
- **オフライン**（NFR-02）: 外部通信なし。可用性/スケーラビリティ(サーバ)/DR は N/A。
- **フェイルセーフ**（NFR-07）: 失敗は `Result`（理由コード）で表現、クラッシュさせない・フォールバック時は分かりやすい表示。
- **セキュリティ既定**: ログに不要情報を出さない（`SafeLogger`）、本番ビルドで詳細エラー非表示（SECURITY-09）。
- **エンジン/言語**: Unity 6000.4.2f1 / URP / uGUI / C#。**シーン操作は公式 Unity AI Assistant（Unity MCP Server）**（US-TECH-05）。
- **UI ハンドオフ**（US-TECH-07）: 枠組みは前本、意匠は S さん。
- **DI/サービス**: `ServiceRegistry`＋`IContentService`/`INavigationService`。純粋ロジックは Common へ。

> ※ 上記の変更が必要な場合のみ、該当質問で Other 指定してください。

## C. スコープ（U5 で確定する非機能の対象）
- **今週のお題表示**の応答性（`ContentService.GetCurrentTheme` ＋ `ThemeSelector.SelectIndex`）。
- **堅牢性**: 空/無効カタログのフォールバック、遷移失敗の安全処理（クラッシュしない）。
- **テスト戦略**: 純粋関数 `ThemeSelector.SelectIndex(date,count)` の PBT、`ContentService`/`ThemeCatalog` 単体。
- **保守性**: 新 `Geidai.Theme` アセンブリ配置、`ThemeCatalog`/`ThemeItem` の配置、`IContentService` 実装範囲、旧 `WeeklyTextController` の扱い。
- **プライバシー**: お題は制作側コンテンツ＝PII なし（NFR-04 は N/A 見込み）。
- **スコープ外**: 録音/加工（U3）、コレクション（U4）、ゲーム出題（U6）、共有/クラウド（Place 除外）。

---

## D. NFR・技術選定に関する質問（Q1〜Q6）

## Question 1（パフォーマンス目標 / NFR-06）
U5 の性能目標は？（お題表示・週選択・Rec 遷移が対象）

A) (推奨) お題は軽量テキストのため **表示は体感即時**（目安：画面表示/バナー描画で < 0.1s）。**週選択 `ThemeSelector.SelectIndex` は O(1) 純粋計算**（アロケーションなし・毎フレーム呼ばない＝表示時/Refresh 時のみ）。**Rec への遷移は U2 の同期シーンロード基準**（軽量シーン・体感即時）。詳細計測は Build & Test。

B) 具体数値は設定せず「体感で引っかからない」を定性目標にする。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 2（信頼性・堅牢性の受入基準 / NFR-07）
空/無効カタログ・遷移失敗時の受入基準は？

A) (推奨) **空/無効カタログ**（未設定・0 件・全項目 `text` 空）→ **フォールバック表示**（お題なしの分かりやすい表示・録音導線は無効 or ホーム誘導）でクラッシュしない。**お題タップ→Rec 遷移失敗**（シーン未登録等）→ `Result.Fail` を受け `ErrorPresenter` 表示・アプリ継続。**受入＝ (1) 空カタログ→フォールバック表示 (2) 遷移失敗注入→エラー表示のみでクラッシュなし (3) `ThemeContext` 未設定でも Rec が通常録音**。

B) 空カタログは想定外として最低限のガードのみ（詳細フォールバックは将来）。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 3（ユーザビリティ / NFR-05）
子ども向け表示の基準は？

A) (推奨) お題文字は**大きく・平易**（`UITheme` 準拠）。任意の**読み仮名/ヒント**で理解を補助。タップ対象（お題/録音導線）は**十分な当たり判定**。文言・装飾・レイアウトは **Sさん がシーンで調整可能**（US-TECH-07）＝ロジックは表示に非依存。**受入＝主要導線が子どもにも分かる大きさ/文言で提示できる（意匠調整の余地を残す）**。

B) 既定の最小 UI のみ（読み/ヒントは出さない）。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 4（テスト容易性 / NFR-09・PBT）
U5 の検証方針は？

A) (推奨) **純粋関数 `ThemeSelector.SelectIndex(date, count)` に PBT**（不変条件＝戻り値は `-1`[count<=0] または `0..count-1`／同一入力で決定的／`count` に対する剰余一致／年境界などの代表日付）。加えて **`ContentService`/`ThemeCatalog` の単体**（空カタログ→`NotFound` フォールバック・現在お題取得・`GetText("theme.current")`）。EditMode。実行は Build & Test に集約可。

B) PBT は行わず、単体テストと手動確認のみ。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 5（保守性・アセンブリ/データ配置 / NFR-08・NFR-10）
U5 の実装配置と `IContentService` 拡張の方針は？

A) (推奨) **新規アセンブリ `Geidai.Theme`**（依存は `Geidai.Common`/`Geidai.Services`/`UnityEngine.UI` の一方向）に UI（`WeeklyThemeController`/`WeeklyThemeScreenController`）を配置。**純粋 `ThemeSelector` は `Geidai.Common`**、**`ThemeItem`/`ThemeCatalog`(SO) も `Geidai.Common`**（ContentService から参照）。**`IContentService` を後方互換拡張**：`GetText("theme.current")` を `ThemeCatalog` ベースで実装＋`Result<ThemeItem> GetCurrentTheme()` を追加、`ContentService` は `Geidai.Services.Content` に実装（`ThemeContext` も同モジュール）。旧 `WeeklyTextController`（Assembly-CSharp）は**シーン差し替え後に削除**（当面残置・MCP フォローアップ）。

B) `Geidai.Theme` を作らず `Geidai.Foundation` に相乗り（アセンブリ増やさない）。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 6（お題データのプライバシー / NFR-04）
お題データ（`ThemeCatalog`）と `ThemeContext` の扱いは？

A) (推奨) お題は**制作側コンテンツ＝PII を含まない**（NFR-04 は本ユニットで **N/A**）。`ThemeCatalog` はアプリ同梱アセット、`ThemeContext` は実行時セッションのみ（永続化しない・保存メタ非記録）。**外部送信なし**（NFR-02 踏襲）。ログに不要情報を出さない（`SafeLogger`）。**受入＝ネットワーク送信が無いこと・`ThemeContext` が永続化されないこと**。

B) Other（[Answer]: の後に記述）

[Answer]:

---

## E. 完了条件
- Q1〜Q6 に回答 → 曖昧回答は追質問 → nfr-requirements / tech-stack-decisions を生成 → 承認ゲート。
- U1〜U4 の横断決定を踏襲し、U5 固有の差分（お題表示の応答性・空カタログ堅牢性・`ThemeSelector` PBT・`Geidai.Theme`/データ配置・`IContentService` 実装範囲・お題 PII なし）のみを明示する。
- 要件（NFR-04〜12 / FR-13/14）とストーリー（US-THEME-01/02/03 / US-TECH-07）へのトレースが取れている。
