# U2 Foundation — Functional Design Plan（機能設計 計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U2 Foundation（起動・ホーム・登録・ナビ導線）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Functional Design（Part 1: Planning）
**入力**: `../../inception/application-design/unit-of-work.md`、`unit-of-work-story-map.md`、`../../inception/requirements/requirements.md`、`../../inception/reverse-engineering/plan-vs-implementation-gap.md`、U1 成果物（`../u1-foundation/*`、`Assets/Scripts/Common|Services`）
**含むストーリー**: US-NAV-01, US-NAV-02, US-REG-01, US-REG-02（対応要件: FR-01/02/03/04, SECURITY-05）

> 本ステージは**技術非依存の業務ロジック/ドメイン/業務ルール/画面構造**を詳細化する。CanvasScaler の数値・SafeArea 実装方式など技術パラメータは **NFR Design**、実シーンへの適用は **Code Generation 以降（Unity MCP）** で扱う。
> **回答方法**: 各質問の `[Answer]:` に記号（例: `A`）を記入してください。合う選択肢が無ければ「Other」。各質問に「(推奨)」案あり。完了で「done」（または「全部推奨で」）。

---

## A. 実行チェックリスト（Part 2 で実行）
- [x] `../u2-foundation/functional-design/domain-entities.md`（U2 で扱うモデル：初回起動状態・ホームメニュー項目・モジュール識別／UserProfile は U1 を再利用）
- [x] `../u2-foundation/functional-design/business-logic-model.md`（起動判定→登録/ホーム、登録・編集フロー、ナビ導線のふるまいとデータフロー）
- [x] `../u2-foundation/functional-design/business-rules.md`（初回起動判定・検証再利用・Place 除外・遷移安全）
- [x] `../u2-foundation/functional-design/frontend-components.md`（Boot/Home/UserRegistration 各画面の構造・状態・操作フロー・ハンドオフ点）
- [x] 要件（FR-01〜04 / SECURITY-05）・ストーリー（US-NAV/REG）とのトレース整合確認

> **回答**: Q1〜Q7＝すべて A（推奨）。矛盾なし。Part 2 実行済み（2026-07-15）。

## B. スコープ（U2 で確定する対象）
- **画面コントローラ（ふるまい）**: `BootScreenController` / `HomeScreenController` / `UserRegistrationScreenController`（いずれも U1 `ScreenRootBase` を継承）
- **フロー**: 起動→（初回=登録／既存=ホーム）→各モジュール空画面への遷移／登録情報の編集
- **業務ルール**: 初回起動判定、入力検証（U1 `ValidationUtil` 再利用）、Place 導線除外、未対応シーンへの安全遷移
- **U1 依存の利用**: `NavigationService`（`SceneId`）、`StorageService`（`LoadProfile`/`SaveProfile`）、`UserProfile`、`UITheme`、`ErrorPresenter`
- **スコープ外**: Rec/Collection/Theme/Game の中身（各モジュールは「空画面テンプレートへ遷移」まで）、永続化の堅牢化（U4）

## C. 既存実装（brownfield）との関係（要判断の背景）
- `Main画面.unity`（起動）・`Home.unity`（ホーム）は既存。遷移は per-button スクリプト（`SceneSwitcher` / `GoToRec` / `GoToSoundCollection` / `ReturnHomeButton` / `GoToPlace` 等）で直接 `SceneManager.LoadScene` している。
- **初回ユーザー登録は未実装（gap ❌ G）**。新規に登録画面/フローを設計する。
- **Place** は既存に導線あり（`GoToPlace`／"place" 文字列と `Place.unity` の大文字小文字不一致バグ）。U1 の `SceneId` は Place を除外済み。U2 で導線から除外する。

---

## D. 設計に関する質問（Q1〜Q7）

## Question 1（登録画面の構成 / フロー）
初回ユーザー登録（生年・ニックネーム）の画面構成は？（U1 の `SceneId.Register` を活かす前提）

A) (推奨) **専用シーン `Register` として分離**（`NavigationService` に `Register` を登録）。初回=Main→Register→Home、登録済み=Main→Home。編集時も同シーンを「編集モード」で再利用。

B) 独立シーンにせず、Main もしくは Home 上の**オーバーレイ/パネル**として表示（シーン追加なし）。

C) Other（[Answer]: の後に記述）

[Answer]:A

## Question 2（起動フロー／初回判定のふるまい）
`Main画面`（起動）からの遷移のふるまいは？

A) (推奨) 起動時に `StorageService.LoadProfile()` で判定。**プロフィール無し→Register、有り→Home** へ。Main は「はじめる」等のタップ起点を経てから遷移（子ども配慮の明示的開始）。

B) Main を出さず、起動直後に自動で Register/Home へ（タップ起点なし）。

C) Other（[Answer]: の後に記述）

[Answer]:A

## Question 3（ホームのメニュー構成 / Place 除外）
ホーム画面に並べるモジュール導線は？（MVP）

A) (推奨) **Rec / コレクション / ゲーム選択 / weekly theme** の4導線＋（設定/プロフィール編集の入口）。**共有(Place)・テストは非表示**（導線から除外）。各導線はアイコン/モチーフ（カエル・おたまじゃくし・蓮）で識別（NFR-05）。

B) 上記に加え、将来枠（Place/テスト）を「準備中」として**グレーアウト表示**（タップ不可）。

C) Other（[Answer]: の後に記述）

[Answer]:A

## Question 4（プロフィール編集の入口）
登録後のプロフィール編集（US-REG-02）へのアクセス経路は？

A) (推奨) **ホームの設定/プロフィールアイコン**から編集（＝Register シーンを編集モードで開く）。

B) 専用の設定シーンを新設し、その中に編集を置く。

C) Other（[Answer]: の後に記述）

[Answer]:A

## Question 5（入力仕様・検証・初回判定の根拠）
登録の入力仕様・検証・初回判定の扱いは？（U1 資産の再利用前提）

A) (推奨) **生年＝ドロップダウン選択（1900〜今年）／ニックネーム＝1〜8文字**（U1 `ValidationUtil.ValidateBirthYear/ValidateNickname` を再利用）。不正時は保存拒否＋`ErrorPresenter` で平易通知。初回判定は `profile.json` の有無（`LoadProfile` が `NotFound`）。登録内容は端末外へ送信しない（NFR-04）。

B) 生年＝自由入力（数値）＋範囲検証。その他は A と同じ。

C) Other（[Answer]: の後に記述）

[Answer]:A

## Question 6（brownfield 導線の統合方針）
既存の per-button 遷移スクリプト（`SceneSwitcher`/`GoTo*`/`ReturnHomeButton`）の扱いは？

A) (推奨) **新コントローラ（Home/Boot/Registration）が `NavigationService` 経由で遷移**する方式に統一。既存 per-button スクリプトは U2 で置き換え/除去（`GoToPlace` は削除、Place 大文字小文字バグは導線除外で解消）。実シーンの配線は Code Generation 以降で Unity MCP により実施。

B) 既存 per-button スクリプトは残しつつ、内部を `NavigationService` 呼び出しに**ラップ**（最小改修）。

C) Other（[Answer]: の後に記述）

[Answer]:A

## Question 7（戻る/終了などのエッジケース）
「もどる/ホーム」および端末バック（Android）等のふるまいは？

A) (推奨) 各モジュール画面の「もどる/ホーム」→ホームへ。**ホームで端末バック→終了確認**（誤操作防止）。存在しない/未対応シーンへの遷移は `NavigationService` が `Result(NotFound)` を返し**クラッシュせず**、`ErrorPresenter` で通知（BR/US-TECH-04 と整合）。

B) 終了確認は行わず、ホームで端末バック→即終了。その他は A と同じ。

C) Other（[Answer]: の後に記述）

[Answer]:A

---

## E. 完了条件
- Q1〜Q7 に回答 → 矛盾チェック（曖昧回答は追質問）→ domain-entities / business-logic-model / business-rules / frontend-components を生成 → 承認ゲート。
- 生成物は技術非依存（数値パラメータ・シーン配線は NFR Design / Code Generation で扱う）。
- 既存実装との差分（初回登録の新設・Place 除外・導線統合）が設計に反映されている。
