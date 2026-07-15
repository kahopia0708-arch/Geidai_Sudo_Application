# U2 Foundation — NFR Requirements（非機能要件・受入値）

**ユニット**: U2 Foundation（起動・ホーム・登録・ナビ導線）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Requirements（Part 2）
**決定**: Q1〜Q6＝すべて A（推奨）
**入力**: `../functional-design/*`、`../../../inception/requirements/requirements.md`（NFR-01〜12）、U1 NFR 成果物
**トレース**: US-NAV-01/02, US-REG-01/02 / FR-01〜04, SECURITY-05, NFR-04/05/06/07/08/09/10/11/12

> U2（起動/ホーム/登録/ナビ）に適用する非機能目標と受入可能値。U1 で確定した横断決定を踏襲し、**U2 固有の差分**を明示する。完全オフライン・ローカルのみ（NFR-02）のため可用性/スケーラビリティ/DR は N/A。

---

## 0. U1 からの継承（再掲・変更なし）
| 項目 | 継承内容 | 出典 |
|---|---|---|
| プラットフォーム | iOS 15+ / Android 8.0(API26)+、スマホ〜タブレット、縦横両対応 | NFR-01 |
| レスポンシブ | `ResponsiveCanvasConfigurator`、参照 1080×1920、Scale With Screen Size、Match=0.5 | NFR-11 |
| SafeArea | `SafeAreaFitter`（`Screen.safeArea` 追従・向き/解像度変更で再計算） | NFR-12 |
| 永続化/シリアライズ | `persistentDataPath` ＋ `JsonUtility`（`profile.json`） | NFR-08 |
| セキュリティ既定 | PII 端末外送信禁止・ログ非出力（`SafeLogger`）、本番で詳細エラー非表示 | NFR-04/SECURITY-09 |
| オフライン | 外部通信なし | NFR-02 |

---

## 1. パフォーマンス（NFR-06 / Q1=A）
- **画面遷移**（Boot→Register/Home、Home→各モジュール、モジュール→Home）: 体感即時（目安 **< 0.3s**）。
- **起動**（Boot 表示〜`LoadProfile` 判定〜遷移）: 数秒以内。
- **プロフィール保存**（`SaveProfile`／JSON・小容量）: 体感即時（目安 **< 0.1s**）。
- **フレームレート**: ターゲット 60fps、最低 30fps を割らない（遷移アニメ含む）。
- **受入**: 目標レンジ端末で起動→登録→ホーム→モジュール空画面の一連が上記目安を満たす（詳細計測は Build & Test）。

## 2. ユーザビリティ：登録 UX・アクセシビリティ（NFR-05 / SECURITY-05 / Q2=A）
- **入力方式**: 生年＝ドロップダウン（1900〜今年・既定は未選択プレースホルダ）、ニックネーム＝1〜8 文字。
- **タップ領域**: 主要操作要素は十分大きく（目安 最小 ~44pt / 約 9mm 相当）。
- **表示**: かな中心＋アイコン併記。エラーは `ErrorPresenter` で「どこを直すか」分かる平易表示。
- **検証**: U1 `ValidationUtil.ValidateBirthYear/ValidateNickname` を再利用（実装重複なし）。
- **受入**: 子ども/初心者が支援なしで登録完了でき、不正入力時は該当項目が平易に示され保存されない。

## 3. ユーザビリティ：ホーム識別性・誤操作防止（NFR-05 / Q3=A）
- **識別**: 各導線はアイコン＋モチーフ（カエル/おたまじゃくし/蓮）＋かなラベル（`UITheme`/データ駆動、Sさん 調整）。
- **終了/戻る**: ホームで端末バック（Android）→**終了確認ダイアログ**（はい/いいえ・既定=いいえ）。モジュールの「もどる/ホーム」→ホーム。
- **受入**: ホームの各導線が識別でき、ホームでの端末バックで即終了しない（確認を挟む）。

## 4. 信頼性・堅牢性（NFR-07 / Q4=A）
- **遷移失敗**: `NavigationService` の `NotFound` → クラッシュせず `ErrorPresenter` で「準備中」等を平易通知。
- **プロフィール読込失敗**: `Corrupted`/`IOError` → 非クラッシュ＋警告＋登録(New) へ安全誘導（U2-BR-04 と整合）。
- **保存失敗**: `IOError` → 通知＋フォーム維持で再試行可。
- **原則**: 過度な自動リセット禁止。フォールバック発動時は**必ず警告**。
- **受入**: 未整備シーン遷移・破損 profile・保存失敗のいずれでもアプリが落ちず、警告/通知が出る。

## 5. セキュリティ/プライバシー（NFR-04 / SECURITY-05）
- 登録/編集の PII（生年・ニックネーム）は端末外送信禁止・ログ非出力（`SafeLogger`）。
- 入力検証は保存前に必ず実施（不正は保存拒否）。本番ビルドで詳細エラー非表示。
- **受入**: ネットワーク送信が無いこと、ログに生年/ニックネームが出ないことを確認。

## 6. テスト容易性（NFR-09 / Q5=A）
- **PBT**: **N/A**（U2 は新規の純粋関数を追加しない。検証ロジックは U1 `ValidationUtil` の既存 PBT でカバー）。
- **U2 の検証（PlayMode/統合テスト）**:
  - 起動判定分岐：`profile.json` 無し→Register(New)、有り→Home。
  - 登録検証の境界：生年 1900 / 今年 / 未来年（不可）、ニックネーム 0 / 1 / 8 / 9 文字・空白のみ。
  - `NotFound` 安全遷移：未整備シーン要求でクラッシュせず通知。
- **実行**: Build & Test に集約可（自動化は段階的）。
- **受入**: 上記シナリオがテスト（または再現手順）で確認できる。

## 7. 保守性（NFR-08 / NFR-10 / Q6=A）
- 遷移は全て `NavigationService` 経由に統一（コントローラから直接 `SceneManager` 禁止）。
- `SceneId` マップに **Register / GameSelect** を追加（後方互換な拡張）。
- ホームメニューは**データ駆動**（`HomeMenuItem` リスト）で Sさん が並び/ラベル/アイコン調整（US-TECH-07）。
- 既存 per-button スクリプト（`SceneSwitcher`/`GoTo*`/`ReturnHomeButton`）は置換/削除（`GoToPlace` 削除）。
- 変更管理：Git ブランチ＋PR レビュー＋変更メモ、シーン操作は公式 Unity MCP（US-TECH-05）。
- **受入**: 直接 `SceneManager` 呼び出しがコントローラに無く、ホーム項目の追加/並べ替えがデータ変更で完結。

## 8. レスポンシブ / SafeArea（NFR-11 / NFR-12・継承）
- Boot/Home/Registration の各ルートに `ResponsiveCanvasConfigurator` ＋ `SafeAreaFitter` を適用（U1 基盤）。
- **受入**: 主要アスペクト比（19.5:9〜4:3）・縦横、ノッチ/パンチホール端末で操作要素が safeArea 内に収まり破綻しない。

## 9. 可用性/スケーラビリティ/DR
- **N/A（ローカル・オフライン）**（NFR-02）。RESILIENCY のクラウド系は N/A。

---

## トレース早見
NFR-06→§1 / NFR-05→§2・§3 / NFR-07→§4 / NFR-04・SECURITY-05→§5 / NFR-09→§6 / NFR-08・NFR-10→§7 / NFR-11・NFR-12→§8 / NFR-02→§9。
US-NAV-01/02→§1・§3・§4・§7 / US-REG-01/02→§1・§2・§4・§5。
