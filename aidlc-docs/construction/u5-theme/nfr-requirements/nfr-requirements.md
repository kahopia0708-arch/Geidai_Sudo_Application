# U5 weekly theme — NFR Requirements（非機能要件・受入可能値）

**ユニット**: U5 weekly theme（お題）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / NFR Requirements（Part 2）
**決定**: Q1〜Q6＝すべて A（推奨）
**前提**: U1〜U4 の横断決定（プラットフォーム/レスポンシブ/SafeArea/オフライン/フェイルセーフ/DI）を踏襲。本書は **U5 固有の差分** のみを具体化する。

> 各 NFR は Code Generation／Build & Test の受入基準。トレース: NFR-04〜12 / FR-13/14 / US-THEME-01/02/03 / US-TECH-07。

---

## NFR-U5-01 パフォーマンス（NFR-06 / Q1=A）
- **お題表示**: 画面表示/ホーム上部バナー描画で**体感即時**（目安 < 0.1s）。お題は軽量テキスト。
- **週選択**: `ThemeSelector.SelectIndex(date, count)` は **O(1) 純粋計算**・アロケーションなし。**毎フレーム呼ばない**（画面表示時／`Refresh()` 時のみ）。
- **Rec 遷移**: U2 の同期シーンロード基準（軽量シーン・体感即時）。
- **受入**: お題画面/バナー表示に体感遅延がない。週選択呼び出しが表示イベント時に限定される。詳細計測は Build & Test。

## NFR-U5-02 信頼性・堅牢性（NFR-07 / Q2=A）
- **空/無効カタログ**（未設定・0 件・全項目 `text` 空）→ **フォールバック表示**（お題なしの分かりやすい表示・録音導線は無効 or ホーム誘導）でクラッシュしない。
- **遷移失敗**（Rec シーン未登録等）→ `Result.Fail` を受け `ErrorPresenter` 表示・アプリ継続。
- **ThemeContext 未設定**でも Rec は通常録音（お題表示は任意）。
- **受入**:
  1. 空カタログ注入 → フォールバック表示（クラッシュなし）。
  2. 遷移失敗注入 → エラー表示のみでクラッシュなし。
  3. `ThemeContext` 未設定で Rec 起動 → 通常録音が成立。

## NFR-U5-03 ユーザビリティ（NFR-05 / Q3=A）
- お題文字は**大きく・平易**（`UITheme` 準拠）。任意の**読み仮名/ヒント**で理解を補助。
- タップ対象（お題/録音導線）は**十分な当たり判定**。
- 文言・装飾・レイアウトは **Sさん がシーンで調整可能**（US-TECH-07）＝ロジックは表示に非依存。
- **受入**: 主要導線が子どもにも分かる大きさ/文言で提示でき、意匠調整の余地を残す。

## NFR-U5-04 テスト容易性（NFR-09 / PBT / Q4=A）
- **純粋関数 `ThemeSelector.SelectIndex(date, count)` に PBT**:
  - 戻り値は `-1`（`count<=0`）または `0..count-1`。
  - 同一入力で決定的。
  - `count` に対する剰余一致（`index == weekNumber の正規化剰余`）。
  - 年境界などの代表日付。
- **単体**: `ContentService`/`ThemeCatalog`（空カタログ→`NotFound` フォールバック・現在お題取得・`GetText("theme.current")`）。
- 実行環境: EditMode。実行は Build & Test に集約可。
- **受入**: 上記 PBT/単体が PASS。

## NFR-U5-05 保守性・アセンブリ/データ配置（NFR-08/10 / Q5=A）
- **新規アセンブリ `Geidai.Theme`**（UI: `WeeklyThemeController`/`WeeklyThemeScreenController`）。依存は `Geidai.Common`/`Geidai.Services`/`UnityEngine.UI` の**一方向**。
- **純粋 `ThemeSelector` は `Geidai.Common`**。**`ThemeItem`/`ThemeCatalog`(SO) も `Geidai.Common`**（`ContentService` から参照するため）。
- **`IContentService` を後方互換拡張**：`GetText("theme.current")` を `ThemeCatalog` ベースで実装＋`Result<ThemeItem> GetCurrentTheme()` を追加。`ContentService` は `Geidai.Services.Content` に実装（`ThemeContext` も同モジュール）。
- 旧 `WeeklyTextController`（Assembly-CSharp）は**シーン差し替え後に削除**（当面残置・MCP フォローアップ）。
- **受入**: 依存が一方向（`Theme→Services→Common`）で循環なし。既存 `IContentService` 利用箇所に影響を与えない（後方互換）。

## NFR-U5-06 プライバシー（NFR-04 / Q6=A）
- お題は**制作側コンテンツ＝PII を含まない** → 本ユニットで **NFR-04 は N/A**。
- `ThemeCatalog` はアプリ同梱アセット。`ThemeContext` は実行時セッションのみ（**永続化しない**・保存メタ非記録）。
- **外部送信なし**（NFR-02 踏襲）。ログに不要情報を出さない（`SafeLogger`）。
- **受入**: ネットワーク送信が無い。`ThemeContext` が永続化されない。

## 継続（NFR-01/02/11/12）
- iOS 15+/Android 8.0+・縦横両対応（NFR-01）、完全オフライン（NFR-02）、`ResponsiveCanvasConfigurator`（NFR-11）、`SafeAreaFitter`（NFR-12）を **U1 基盤で充足** し U5 も準拠。

---

## N/A（本ユニット対象外・根拠）
| NFR | 判定 | 根拠 |
|---|---|---|
| NFR-03 可用性/DR | N/A | 完全オフライン・サーバなし |
| NFR-04 プライバシー | N/A（本ユニット） | お題は PII を含まない・`ThemeContext` 非永続 |
| SCAL（サーバ） | N/A | オフライン |

## トレース表
| NFR-U5 | 要件 | ストーリー |
|---|---|---|
| 01 パフォーマンス | NFR-06 | US-THEME-01/02 |
| 02 堅牢性 | NFR-07 | US-THEME-01/02 |
| 03 ユーザビリティ | NFR-05 | US-THEME-01・US-TECH-07 |
| 04 テスト容易性 | NFR-09 | US-THEME-01/03 |
| 05 保守性 | NFR-08/10 | US-THEME-03・US-TECH-07 |
| 06 プライバシー | NFR-04/NFR-02 | US-THEME-02 |
