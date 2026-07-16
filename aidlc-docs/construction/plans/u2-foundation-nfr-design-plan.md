# U2 Foundation — NFR Design Plan（計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U2 Foundation（起動・ホーム・登録・ナビ導線）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Design（Part 1: 計画）
**入力**: `../u2-foundation/nfr-requirements/nfr-requirements.md`, `tech-stack-decisions.md`, `../u2-foundation/functional-design/*`, U1 NFR Design 成果物（`../u1-foundation/nfr-design/*`）

> 目的: U2 の NFR（性能/信頼性/ユーザビリティ/セキュリティ/保守性）を**設計パターン**と**論理コンポーネント**へ落とし込む。数値は NFR Requirements で確定済み。ここでは「どう実現するか（パターン）」を決める。
> **回答方法**: 各質問の `[Answer]:` に記号（例: `A`）。合う選択肢が無ければ「Other」。各質問に「(推奨)」あり。完了で「done」（または「全部推奨で」）。

---

## A. 実行チェックリスト（Part 2 で実行）
- [x] `../u2-foundation/nfr-design/nfr-design-patterns.md` を生成（各 NFR の実現パターン）
- [x] `../u2-foundation/nfr-design/logical-components.md` を生成（論理コンポーネント・責務・連携）
- [x] NFR Requirements / Functional Design / Application Design とのトレース整合を確認

> **回答**: Q1〜Q6＝すべて A（推奨）。矛盾なし。Part 2 実行済み（2026-07-15）。

## B. カテゴリ適用性（このユニットでの判断）
- **Resilience（耐障害）**: 適用（起動判定の破損耐性・遷移 NotFound の安全処理・保存失敗の再試行）。ネットワーク再試行系は N/A。
- **Scalability（スケーラビリティ）**: N/A（単一端末・オフライン・少数画面）。
- **Performance（性能）**: 適用（画面遷移・起動・プロフィール保存・fps）。
- **Security（セキュリティ）**: 適用（登録 PII のローカル限定・入力検証集約・本番エラー秘匿）。U1 パターン踏襲。
- **Logical Components（論理部品）**: 適用（Boot 状態機械・BackHandler・ConfirmDialog・HomeMenuConfig・登録コントローラ）。

## B-2. U1 から継承する設計パターン（再質問しない・前提）
- **エラー伝搬**: `Result<T>`（成功/失敗＋理由）。致命的でない失敗はクラッシュさせない。
- **UI 基盤**: `ScreenRootBase` ＋ `ResponsiveCanvasConfigurator` ＋ `SafeAreaFitter`（表示時/向き変更で再適用）。
- **通知**: `ErrorPresenter`（子ども向けバナー）。
- **DI**: 軽量サービスロケータ（`ServiceRegistry`）＋インターフェース（`INavigationService`/`IStorageService`）。
- **セキュリティ**: `ValidationUtil` に検証集約、`SafeLogger` で PII マスク、本番で詳細エラー非表示。

> ※ 上記の変更が必要な場合のみ、該当質問で Other 指定してください。

---

## C. 明確化のための質問（Q1〜Q6）

### Question 1（Resilience — 起動判定パターン）
Boot 画面での初回判定と安全誘導の設計は？

A) (推奨) Boot を**軽量な状態機械**（Idle→Checking→Route/Error）。「はじめる」タップ後に `StorageService.LoadProfile()` を 1 回呼び、`Result` で分岐（`NotFound`→Register(New)、`Ok`→Home、`Corrupted`/`IOError`→**警告＋Register(New) へ安全誘導**）。判定はサービス層に委譲し、UI は分岐のみ（U2-BR-01〜04 と整合）。

B) Boot に判定ロジックを直接記述（サービス委譲なし・最短実装）。

C) Other (please describe after [Answer]: tag below)

[Answer]:A

### Question 2（Resilience — ナビゲーションの安全処理）
遷移失敗（未整備シーン等）の設計パターンは？

A) (推奨) 遷移は `NavigationService` が `Result` を返し、コントローラは失敗時に `ErrorPresenter` で「準備中」等を平易通知（クラッシュ回避）。未登録 `SceneId` は `NotFound`。`GoBack` は失敗時ホームへフォールバック。U1 の `Result` パターン踏襲。

B) 遷移失敗は例外を投げ、共通ハンドラ（try/catch 境界）で捕捉。

C) Other (please describe after [Answer]: tag below)

[Answer]:A

### Question 3（Logical Component — 戻る/終了の設計・誤操作防止）
端末バック（Android）と「もどる/ホーム」の設計は？

A) (推奨) 端末バックを**共通 BackHandler**（`ScreenRootBase.OnBackRequested` を各コントローラが override）で受け、画面種別で分岐：モジュール/登録→ホーム、ホーム→**終了確認ダイアログ**（再利用可能な `ConfirmDialog`、既定=いいえ）、登録(編集)→キャンセル（変更破棄でホーム）。ダイアログは共通論理部品として1つ用意。

B) 各画面が個別に端末バックを監視・処理（共通部品なし）。

C) Other (please describe after [Answer]: tag below)

[Answer]:A

### Question 4（Logical Component — ホームメニューのデータ駆動）
ホーム導線の構成データと描画の設計は？

A) (推奨) `HomeMenuConfig`（ScriptableObject）に `HomeMenuItem`（moduleId/label/iconKey/visible/enabled/order）のリストを保持。`HomeScreenController` は**可視項目のみ描画**し `moduleId→SceneId` で `NavigationService` を呼ぶ。Sさん はアセット編集で並び/ラベル/アイコン調整（US-TECH-07）。Place/テストは項目に含めない。

B) ホーム項目をコードにハードコード（データ分離なし・最短実装）。

C) Other (please describe after [Answer]: tag below)

[Answer]:A

### Question 5（Performance — 遷移/保存の実現パターン）
性能目標（遷移<0.3s / 起動 数秒 / 保存<0.1s / 60fps）の設計は？

A) (推奨) 遷移は**同期 `LoadScene` 基本**（軽量シーン）。必要な箇所のみ簡易ローディング表示。プロフィール保存は小容量 JSON を同期（<0.1s）。`UITheme`/メニューアセットはキャッシュ。文字列生成/GC を抑制。U1 性能パターン踏襲。

B) 全遷移を非同期 `LoadSceneAsync` ＋ローディング画面で統一。

C) Other (please describe after [Answer]: tag below)

[Answer]:A

### Question 6（Security/Resilience — 登録コントローラの設計）
登録/編集の検証・保存の設計パターンは？

A) (推奨) `UserRegistrationScreenController` は `RegistrationMode`（New/Edit）で初期化。確定時に **U1 `ValidationUtil` で検証→OK のみ** `UserProfile` 生成→`StorageService.SaveProfile`（`Result`）。失敗はフォーム維持＋`ErrorPresenter`。編集は既存値ロード、キャンセルで破棄。検証・保存はユーティリティ/サービスへ委譲（画面内で再実装しない）。PII はログ非出力・端末外送信なし。

B) 検証を画面内に実装（`ValidationUtil` を使わない）。

C) Other (please describe after [Answer]: tag below)

[Answer]:A

---

## D. 完了条件
- Q1〜Q6 に回答 → 矛盾チェック（曖昧回答は追質問）→ nfr-design-patterns.md / logical-components.md を生成 → 承認ゲート。
- U1 の設計パターンを踏襲し、U2 固有の論理部品（Boot 状態機械・BackHandler・ConfirmDialog・HomeMenuConfig）を明確化する。
