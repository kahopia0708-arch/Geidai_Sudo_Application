# U2 Foundation — NFR Design Patterns（NFR 実現パターン）

**ユニット**: U2 Foundation（起動・ホーム・登録・ナビ導線）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Design（Part 2）
**決定**: Q1〜Q6＝すべて A（推奨）
**入力**: `../nfr-requirements/*`, `../functional-design/*`, U1 NFR Design 成果物
**トレース**: NFR-04/05/06/07/08/09/10 / US-NAV-01/02, US-REG-01/02

> U2 の NFR を「どう実現するか（パターン）」で定義。U1 の設計パターン（`Result<T>`・`ScreenRootBase`・`ErrorPresenter`・`ServiceRegistry`・`ValidationUtil`/`SafeLogger`）を踏襲し、U2 固有パターンを追加する。

---

## 0. 継承パターン（U1・変更なし）
- **エラー伝搬**: `Result<T>`（成功/失敗＋`ResultCode`）。致命的でない失敗は非クラッシュ。
- **UI 基盤**: `ScreenRootBase` ライフサイクルで `ResponsiveCanvasConfigurator`→`SafeAreaFitter` を必ず適用（表示時/向き・解像度変更で再適用）。
- **通知**: `ErrorPresenter`（子ども向けバナー、Error/Warning）。
- **DI**: 軽量サービスロケータ `ServiceRegistry` ＋ インターフェース参照。
- **セキュリティ**: `ValidationUtil` に検証集約、`SafeLogger` で PII マスク、本番で詳細エラー非表示。

---

## 1. Resilience — 起動判定パターン（NFR-07 / Q1=A）
**パターン**: Boot を軽量な状態機械にし、判定はサービス層へ委譲、UI は分岐のみ。
```
[Idle] --(「はじめる」tap)--> [Checking] --(LoadProfile: Result)-->
   Ok        → [Route:Home]
   NotFound  → [Route:Register(New)]
   Corrupted → [Error] → 警告(ErrorPresenter) → [Route:Register(New)]
   IOError   → [Error] → 警告(ErrorPresenter) → [Route:Register(New)]
```
- **原則**: 破損を正常と誤認する自動処理をしない。フォールバック時は必ず警告（U2-BR-04）。
- **利点**: 分岐が一点集約され、テスト（分岐網羅）が容易（NFR-09）。
- **状態の非永続**: `AppLaunchState` は導出値（`profile.json` の有無が唯一の真実源）。

## 2. Resilience — ナビゲーション安全処理パターン（NFR-07 / Q2=A）
**パターン**: `NavigationService.GoTo(SceneId)` は `Result` を返す。コントローラは失敗時に `ErrorPresenter` で平易通知（クラッシュ回避）。
- 未登録/未整備 `SceneId`（例: `Theme` 未整備期間）→ `NotFound` → 「準備中」通知。
- `GoBack()` は失敗時ホームへフォールバック。
- **禁止**: コントローラからの直接 `SceneManager.LoadScene`（保守性 NFR-08）。
- **境界**: 例外は投げず `Result` に正規化（U1 パターン）。想定外例外はサービス内で捕捉し `IOError`/`Unknown` に変換。

## 3. Usability/Logical — 戻る・終了パターン（NFR-05 / Q3=A）
**パターン**: 端末バックを共通 `BackHandler` で受け、`ScreenRootBase.OnBackRequested()` を各コントローラが override して画面種別で分岐。
| 画面 | 端末バック / 「もどる」 |
|---|---|
| モジュール画面 | ホームへ（`GoTo(Home)`） |
| ホーム | **終了確認ダイアログ**（`ConfirmDialog`、既定=いいえ） |
| 登録(New) | ホームへ戻る（初回は Boot 起点へ戻さない） |
| 登録(Edit) | キャンセル＝変更破棄でホームへ |
- **ConfirmDialog** は再利用可能な単一論理部品（はい/いいえ、既定フォーカス=いいえ、誤操作防止）。
- **利点**: 端末バックの分散処理を排除し、誤操作防止ポリシーを一元化。

## 4. Performance — 遷移/保存パターン（NFR-06 / Q5=A）
**パターン**: 同期主体＋必要箇所のみ軽量化。
- **遷移**: 同期 `LoadScene` を基本（軽量シーン、目安 <0.3s）。必要な画面のみ簡易ローディング表示。
- **保存**: プロフィールは小容量 JSON を同期保存（目安 <0.1s）。
- **キャッシュ**: `UITheme`・`HomeMenuConfig` はロード後キャッシュし再取得しない。
- **GC 抑制**: 毎フレーム/描画時の文字列生成・アロケーションを避ける（ラベルは初期化時に確定）。
- **fps**: 遷移アニメは軽量に。ターゲット 60fps／最低 30fps。

## 5. Security — 登録データ保護パターン（NFR-04 / SECURITY-05 / Q6=A）
**パターン**: 検証集約＋保存前ゲート＋PII 秘匿。
- 確定時に `ValidationUtil.ValidateBirthYear/ValidateNickname` を実行し、**全通過時のみ** `UserProfile` を生成・保存。
- 失敗はフォーム維持＋`ErrorPresenter`（該当項目を平易表示）。
- PII（生年/ニックネーム）は `SafeLogger` でログ非出力、端末外送信なし。
- 本番ビルドで詳細エラーメッセージ非表示（開発ビルドのみ詳細）。

## 6. Maintainability — ナビ統一・データ駆動（NFR-08/10 / Q4/Q6=A）
- 遷移は `INavigationService` に一本化、`SceneId` マップを拡張（`Register`/`GameSelect`）。
- ホームは `HomeMenuConfig`（データ）駆動で、コード改変なしに項目調整（US-TECH-07）。
- 既存 per-button スクリプトは新方式へ置換/削除（`GoToPlace` 削除）。
- 変更管理: Git ブランチ/PR、シーン操作は公式 Unity MCP。

## 7. Scalability / Availability / DR
- **N/A**（単一端末・完全オフライン・少数画面 / NFR-02）。

---

## トレース早見
NFR-07→§1・§2 / NFR-05→§3 / NFR-06→§4 / NFR-04・SECURITY-05→§5 / NFR-08・NFR-10→§6 / NFR-09→§1(分岐網羅)・§2 / NFR-02→§7。
