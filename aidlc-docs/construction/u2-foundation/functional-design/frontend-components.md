# U2 Foundation — Frontend Components（画面構造・状態・ハンドオフ）

**ユニット**: U2 Foundation（起動・ホーム・登録・ナビ導線）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Functional Design（Part 2）
**方針**: Q1〜Q7＝すべて A（推奨）
**トレース**: US-NAV-01/02, US-REG-01/02, US-TECH-07 / FR-01/02/03/04, NFR-05/11/12

> 技術非依存の画面構造・状態・フロー・ハンドオフ点を定義。CanvasScaler の具体値・Prefab 実体・シーン配線は NFR Design / Code Generation で扱う。
> **U1 基盤の踏襲**: すべての画面ルートは `ScreenRootBase` を継承し、`ResponsiveCanvasConfigurator`（レスポンシブ）＋`SafeAreaFitter`（セーフエリア）＋`UITheme`（色/フォント/アイコン）を適用する（NFR-11/12）。前本が枠組みを作り、Sさん が詳細調整（US-TECH-07）。

---

## 1. 画面（Screen）一覧
| 画面 | ルート | 目的 | 主な子要素（枠） |
|---|---|---|---|
| Boot | `BootScreenController` | 起動起点・状態判定 | タイトル/ロゴ枠、「はじめる」ボタン、ローディング表示（任意） |
| Home | `HomeScreenController` | モジュール導線・設定入口 | メニュー領域（`HomeMenuItem` 群）、設定/プロフィール入口、（端末バック→終了確認） |
| Registration | `UserRegistrationScreenController` | 登録/編集の入力・保存 | 生年ドロップダウン、ニックネーム入力、確定ボタン、（編集時）キャンセル、通知領域 |

---

## 2. 画面別 構造・状態

### 2.1 Boot
- **状態**: `Idle`（起点待ち）→ `Checking`（`LoadProfile` 実行中）→ 遷移（Register/Home）／`Error`（破損時通知）。
- **入力**: 「はじめる」タップのみ。
- **出力**: `NavigationService.GoTo(Register[New] | Home)`。
- **枠のみ**: 背景・ロゴ・ボタン配置は枠を用意し、モチーフ/配色は `UITheme`（Sさん 調整）。

### 2.2 Home
- **状態**: `Ready`（メニュー描画済み）。端末バック時 `ConfirmExit`（終了確認モーダル）。
- **データ**: 可視 `HomeMenuItem` のリスト（順序=表示順、Place/テストは除外）。
- **相互作用**: 項目タップ→`moduleId→SceneId`→`NavigationService.GoTo`。未整備先は `NotFound`→通知。設定/プロフィール入口→`Register[Edit]`。
- **枠のみ**: メニューのレイアウト（グリッド/リスト）、アイコン/モチーフ、ラベル文言は `UITheme`/データ分離で Sさん 調整（US-TECH-07 / NFR-05）。

### 2.3 Registration（New / Edit 兼用）
- **モード**: `RegistrationMode`（New=空、Edit=既存値初期表示）。
- **状態**: `Editing`（入力中）→ `Validating` → `Saving` → 遷移／`Invalid`（該当項目通知、留まる）／`SaveError`（通知、留まる）。
- **入力要素**: 生年＝ドロップダウン（1900〜今年）、ニックネーム＝1〜8 文字入力。
- **検証**: U1 `ValidationUtil`。通過時のみ `SaveProfile`。
- **出力**: New=Home へ、Edit=Home へ戻る（キャンセルは破棄で戻る）。
- **枠のみ**: フォームの見た目・入力補助・エラー表示スタイルは枠＋`UITheme`（Sさん 調整）。

---

## 3. 共通 UI 挙動（U1 基盤の適用）
- **レスポンシブ**（NFR-11）: 全画面で `ResponsiveCanvasConfigurator` を適用し、多様な画面サイズ/解像度・縦横両対応で破綻しない（具体値は NFR Design）。
- **セーフエリア**（NFR-12）: 主要 UI は `SafeAreaFitter` 配下に配置し、ノッチ/角丸/システムバーに被らない。
- **通知**: エラー/警告は `ErrorPresenter` の子ども向けバナーで統一（BR/US-TECH-04）。
- **戻る/ホーム**: 各モジュール画面に共通の「もどる/ホーム」導線（`NavigationService.GoTo(Home)`）。

---

## 4. UI ハンドオフ点（前本 → Sさん / US-TECH-07）
| ハンドオフ点 | 前本（枠組み） | Sさん（詳細調整） |
|---|---|---|
| 配色・フォント・アイコン | `UITheme` 参照フックを配線 | `UITheme` の値・アセット差し替え |
| ホームメニュー | `HomeMenuItem` データ駆動で描画枠 | ラベル/モチーフ/並び順・アイコン画像 |
| 画面レイアウト | Prefab テンプレ＋アンカー/セーフエリア枠 | 余白・サイズ・装飾・アニメの微調整 |
| 通知表示 | `ErrorPresenter` 呼び出し配線 | バナーの見た目・文言トーン |

- 調整点は ScriptableObject（`UITheme` 等）／Prefab／データとして**コードから分離**し、Sさん がコード改変なしに調整できる状態で引き渡す（US-TECH-07）。

---

## 5. 既存（brownfield）からの移行
- 既存 per-button 遷移（`SceneSwitcher`/`GoTo*`/`ReturnHomeButton`）は本 U2 のコントローラ＋`NavigationService` へ統一・置換（`GoToPlace` は削除）。
- 既存 Rec/コレクション/ゲーム選択の各シーンは遷移先として接続（`SceneId.Rec`/`Collection`/`GameSelect`）。weekly theme 専用画面が無い間は `NotFound` 安全通知。
- 実シーンの配線・Prefab 化・アンカー調整は Code Generation 以降で Unity MCP により実施（本書は構造/状態/フローの定義に限定）。
