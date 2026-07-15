# U2 Foundation — NFR Requirements Plan（非機能要件・技術選定 計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U2 Foundation（起動・ホーム・登録・ナビ導線）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Requirements（Part 1: Planning）
**入力**: `../u2-foundation/functional-design/*`、`../../inception/requirements/requirements.md`（NFR-01〜12）、U1 NFR 成果物（`../u1-foundation/nfr-requirements/*`）
**対象NFR**: パフォーマンス(NFR-06)、ユーザビリティ(NFR-05)、信頼性/堅牢性(NFR-07)、プライバシー/セキュリティ(NFR-04/SECURITY-05)、テスト容易性(NFR-09)、保守性(NFR-08/10)、レスポンシブ/SafeArea(NFR-11/12)

> 本ステージで U2 の**非機能目標の具体値**と**技術選定の差分**を確定する。
> **回答方法**: 各質問の `[Answer]:` に記号（例: `A`）。合う選択肢が無ければ「Other」。各質問に「(推奨)」あり。完了で「done」（または「全部推奨で」）。

---

## A. 実行チェックリスト（Part 2 で実行）
- [ ] `../u2-foundation/nfr-requirements/nfr-requirements.md` を生成（U2 の NFR 目標・受入可能値）
- [ ] `../u2-foundation/nfr-requirements/tech-stack-decisions.md` を生成（U2 の技術選定差分・根拠）
- [ ] 要件（NFR-01〜12）・ストーリー（US-NAV/REG）とのトレース整合を確認

## B. 前提（U1 で確定済み・U2 も踏襲。原則 再質問しない）
- **プラットフォーム**（NFR-01）: iOS 15+ / Android 8.0(API26)+、スマホ〜タブレット、縦横両対応。
- **レスポンシブ**（NFR-11）: `ResponsiveCanvasConfigurator`、参照解像度 1080×1920、Scale With Screen Size、Match=0.5。
- **SafeArea**（NFR-12）: `SafeAreaFitter`（`Screen.safeArea` 追従・向き/解像度変更で再計算）。
- **オフライン**（NFR-02）: 外部通信なし。可用性/スケーラビリティ/DR は N/A。
- **永続化/シリアライズ**（NFR-08）: `Application.persistentDataPath` ＋ Unity 標準 `JsonUtility`（`profile.json`）。
- **エンジン/言語**: Unity 6000.4.2f1 / URP / uGUI / C#。**シーン操作は 公式 Unity AI Assistant（Unity MCP Server）**（US-TECH-05）。
- **セキュリティ既定**: PII 端末外送信禁止・ログ非出力（`SafeLogger`）、本番ビルドで詳細エラー非表示（SECURITY-09）。

> ※ 上記の変更が必要な場合のみ、該当質問で Other 指定してください。

## C. スコープ（U2 で確定する非機能の対象）
- 起動判定・画面遷移・登録/編集・ホーム導線の**性能・ユーザビリティ・信頼性**。
- 子ども/初心者向け UX（タップ領域・平易表示・誤操作防止）。
- ナビゲーションの安全性（未整備シーンへの `NotFound` 安全処理）。
- **スコープ外**: 各モジュール中身（U3〜U6）、永続化の原子的置換の本実装（U4）。

---

## D. NFR・技術選定に関する質問（Q1〜Q6）

## Question 1（パフォーマンス目標 / NFR-06）
U2 の性能目標は？（起動判定・遷移・登録保存が対象）

A) (推奨) **U1 を踏襲**：画面遷移=体感即時（目安 < 0.3s）、起動（プロフィール読込含む）=数秒以内、**プロフィール保存（JSON・小容量）=体感即時（目安 < 0.1s）**。ターゲット 60fps／最低 30fps を割らない。

B) より厳しく：遷移 < 0.2s、起動 < 2s。

C) 具体数値は設定せず「体感で引っかからない」を定性目標にする。

D) Other (please describe after [Answer]: tag below)

[Answer]:

## Question 2（登録 UX・アクセシビリティ / NFR-05・SECURITY-05）
子ども/初心者向けの登録入力の非機能方針は？

A) (推奨) 生年＝ドロップダウン（1900〜今年・既定は未選択プレースホルダ）、ニックネーム＝1〜8文字。**タップ領域は十分大きく**（目安：最小 ~44pt/9mm 相当）、かな中心＋アイコン併記。エラーは `ErrorPresenter` で平易表示（どこを直すか分かる）。検証は U1 `ValidationUtil` を再利用。

B) 生年もホイール/ピッカー UI にする（ドロップダウン以外）。

C) Other (please describe after [Answer]: tag below)

[Answer]:

## Question 3（ホーム・導線のユーザビリティ / 誤操作防止 / NFR-05）
ホームの識別性と終了/戻るの安全性は？

A) (推奨) 各導線はアイコン＋モチーフ（カエル/おたまじゃくし/蓮）＋かなラベルで識別（`UITheme`/データ駆動、Sさん 調整）。**ホームで端末バック→終了確認ダイアログ**（はい/いいえ・既定=いいえ）。モジュールの「もどる/ホーム」はホームへ。

B) 終了確認は行わない（ホームで端末バック→即終了）。その他は A と同じ。

C) Other (please describe after [Answer]: tag below)

[Answer]:

## Question 4（信頼性・堅牢性 / NFR-07）
U2 フローの失敗時のふるまいは？

A) (推奨) `NavigationService` の `NotFound`→クラッシュせず `ErrorPresenter` で「準備中」等を平易通知。プロフィール読込 `Corrupted/IOError`→非クラッシュ＋警告＋登録(New)へ安全誘導（U2-BR-04 と整合）。保存 `IOError`→通知＋フォーム維持で再試行可。**過度な自動リセットはせず、フォールバック時は必ず警告**。

B) 破損時は自動で既定プロフィールを作って続行（警告のみ）。

C) Other (please describe after [Answer]: tag below)

[Answer]:

## Question 5（テスト容易性 / NFR-09・PBT）
U2 の検証方針は？（U2 は新規の純粋関数を追加しない前提）

A) (推奨) **PBT は N/A**（純粋関数は U1 で実装・検証済み。検証ロジックは U1 `ValidationUtil` の既存 PBT でカバー）。U2 は **PlayMode/統合テスト**で「起動判定分岐（初回→登録／既存→ホーム）」「登録検証の境界（生年 1900/今年/未来年、ニックネーム 0/1/8/9 文字・空白のみ）」「`NotFound` 安全遷移」を検証。実行は Build & Test に集約可。

B) `ValidationUtil` に対する追加の PBT（生年/ニックネーム）を U2 でも増強する。

C) 当面は手動確認中心（自動テストは Build & Test 段階で最小限）。

D) Other (please describe after [Answer]: tag below)

[Answer]:

## Question 6（保守性・ナビ統合 / NFR-08・NFR-10）
遷移・ホームメニューの実装方針（保守性）は？

A) (推奨) 遷移は**全て `NavigationService` 経由に統一**（コントローラから直接 `SceneManager` 禁止）。`SceneId` マップに **Register / GameSelect** を追加。ホームメニューは**データ駆動**（`HomeMenuItem` リスト）で Sさん が並び/ラベル/アイコンを調整（US-TECH-07）。既存 per-button スクリプト（`SceneSwitcher`/`GoTo*`/`ReturnHomeButton`）は置換/削除（`GoToPlace` 削除）。

B) 最小改修：既存 per-button スクリプトは残し、内部を `NavigationService` 呼び出しにラップ。

C) Other (please describe after [Answer]: tag below)

[Answer]:

---

## E. 完了条件
- Q1〜Q6 に回答 → 曖昧回答は追質問 → nfr-requirements / tech-stack-decisions を生成 → 承認ゲート。
- U1 の横断決定を踏襲し、U2 固有の差分のみを明示する。
- 要件（NFR-01〜12）とストーリー（US-NAV/REG）へのトレースが取れている。
