# U2 Foundation — Logical Components（NFR 支援 論理コンポーネント）

**ユニット**: U2 Foundation（起動・ホーム・登録・ナビ導線）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Design（Part 2）
**決定**: Q1〜Q6＝すべて A（推奨）
**トレース**: NFR-04/05/06/07/08/09/10 / US-NAV-01/02, US-REG-01/02

> U2 の NFR を支える論理コンポーネント（責務・連携・所在）。技術非依存の論理定義（具体 UI/シーン配線は Code Generation）。`Geidai.Foundation` を主アセンブリとする。

---

## 0. 再利用（U1 由来・新規実装しない）
| コンポーネント | 所在 | U2 での役割 |
|---|---|---|
| `Result` / `Result<T>` / `ResultCode` | `Geidai.Common.Results` | 判定/遷移/保存の成否伝搬 |
| `ScreenRootBase` | `Geidai.Common.UI` | 画面ライフサイクル・Responsive/SafeArea 適用・`OnBackRequested` |
| `ResponsiveCanvasConfigurator` / `SafeAreaFitter` / `UITheme` | `Geidai.Common.UI` | レスポンシブ/SafeArea/見た目データ |
| `ErrorPresenter` | `Geidai.Common.UI` | 子ども向けエラー/警告通知 |
| `ValidationUtil` / `SafeLogger` | `Geidai.Common.Utils` | 入力検証集約 / PII マスクログ |
| `ServiceRegistry` / `AppManager` | `Geidai.Services` | サービス参照解決 / 起動ブート |
| `INavigationService`/`NavigationService`（`SceneId`） | `Geidai.Services.Navigation` | 遷移（`Result`）・`SceneId` マップ |
| `IStorageService`/`StorageService` | `Geidai.Services.Storage` | `LoadProfile`/`SaveProfile` |
| `UserProfile` | `Geidai.Common.Models` | 登録/編集対象 |

---

## 1. 新規 論理コンポーネント（U2）

### 1.1 `BootScreenController`（起動状態機械）
- **所在**: `Geidai.Foundation`（`ScreenRootBase` 継承）。
- **責務**: 起点提示（「はじめる」）→ `StorageService.LoadProfile()` → `Result` で分岐（§NFR-Design §1）。
- **連携**: `IStorageService`（読込）、`INavigationService`（遷移）、`ErrorPresenter`（破損時警告）。
- **NFR**: 信頼性（安全誘導）・テスト容易性（分岐網羅）。
- **状態**: `Idle / Checking / Routing / Error`。

### 1.2 `HomeScreenController`（ホーム）
- **所在**: `Geidai.Foundation`（`ScreenRootBase` 継承）。
- **責務**: `HomeMenuConfig` の可視項目を描画、項目タップで `moduleId→SceneId` 解決→`NavigationService.GoTo`。端末バック→`ConfirmDialog`（終了確認）。
- **連携**: `HomeMenuConfig`、`INavigationService`、`ConfirmDialog`、`ErrorPresenter`（NotFound 通知）、`UITheme`。
- **NFR**: ユーザビリティ（識別性・誤操作防止）・保守性（データ駆動）。

### 1.3 `UserRegistrationScreenController`（登録/編集）
- **所在**: `Geidai.Foundation`（`ScreenRootBase` 継承）。
- **責務**: `RegistrationMode`（New/Edit）初期化、入力→`ValidationUtil` 検証→OK のみ `UserProfile` 生成→`SaveProfile`（`Result`）。失敗はフォーム維持＋通知。Edit は既存値ロード、キャンセルで破棄。
- **連携**: `IStorageService`、`ValidationUtil`、`ErrorPresenter`、`INavigationService`。
- **NFR**: セキュリティ（検証集約・PII 秘匿）・信頼性（保存失敗の再試行）。

### 1.4 `HomeMenuConfig` / `HomeMenuItem`（メニュー構成データ）
- **所在**: `Geidai.Foundation`（`HomeMenuConfig` は ScriptableObject、`HomeMenuItem` はシリアライズ可能な値）。
- **責務**: ホーム導線の定義（`moduleId/label/iconKey/visible/enabled/order`）を**データとして保持**。Place/テストは含めない。
- **連携**: `HomeScreenController` が参照。Sさん がアセット編集で調整（US-TECH-07）。
- **NFR**: 保守性・ユーザビリティ（コード非依存の調整）。

### 1.5 `ConfirmDialog`（確認ダイアログ）
- **所在**: `Geidai.Common.UI`（横断再利用のため Common に配置）または `Geidai.Foundation`（U2 専用なら）。**推奨: `Geidai.Common.UI`**（後続ユニットでも再利用可能）。
- **責務**: はい/いいえの確認 UI（既定フォーカス=いいえ）。コールバック/`Result` で結果返却。
- **連携**: `HomeScreenController`（終了確認）、将来の削除確認（U4）等でも再利用。
- **NFR**: ユーザビリティ（誤操作防止）・保守性（共通化）。

### 1.6 `BackHandler`（端末バック処理・論理）
- **所在**: `Geidai.Common.UI`（`ScreenRootBase.OnBackRequested()` として抽象化）。
- **責務**: 端末バック（Android）を受け取り、アクティブ画面の `OnBackRequested()` を呼ぶ。各コントローラが override して分岐（§NFR-Design §3）。
- **連携**: 各 `ScreenRootBase` 派生、`ConfirmDialog`、`INavigationService`。
- **NFR**: ユーザビリティ（一貫した戻る挙動）・保守性（分散排除）。

### 1.7 `SceneId` 拡張（`Register` / `GameSelect`）
- **所在**: `Geidai.Common.Models`（U1 の列挙に後方互換で追記）。
- **責務**: `NavigationService` マップに `Register`（登録シーン）・`GameSelect`（ゲーム選択＝既存 `game_Home`）を登録。`Theme` は U5 まで未登録（`NotFound` 安全処理）。
- **NFR**: 保守性・信頼性。

---

## 2. 連携図（テキスト）
```
AppManager(起動) → BootScreenController
   BootScreenController → IStorageService.LoadProfile → Result
       → INavigationService.GoTo(Register/Home)（失敗時 ErrorPresenter）
HomeScreenController → HomeMenuConfig(可視項目) → INavigationService.GoTo(moduleId→SceneId)
   HomeScreenController → (端末バック) → ConfirmDialog(終了確認)
UserRegistrationScreenController → ValidationUtil(検証) → IStorageService.SaveProfile → Result
   （失敗時 ErrorPresenter・フォーム維持）
全 ScreenRootBase 派生 → ResponsiveCanvasConfigurator + SafeAreaFitter（表示/向き変更で再適用）
端末バック → BackHandler → activeScreen.OnBackRequested()
```

## 3. 配置方針（アセンブリ）
- **`Geidai.Foundation`（新規）**: Boot/Home/Registration Controller、`HomeMenuConfig`/`HomeMenuItem`。依存＝`Geidai.Common`, `Geidai.Services`, `UnityEngine.UI`。
- **`Geidai.Common.UI`（追記）**: `ConfirmDialog`、`ScreenRootBase.OnBackRequested`（BackHandler 抽象）。
- **`Geidai.Common.Models`（追記）**: `SceneId` に `Register`/`GameSelect`。
- **循環依存なし**（Foundation→Services→Common の一方向）。

---

## トレース早見
BootScreenController→NFR-07/09 / HomeScreenController→NFR-05/08 / UserRegistrationScreenController→NFR-04/05/07 / HomeMenuConfig→NFR-05/08・US-TECH-07 / ConfirmDialog→NFR-05 / BackHandler→NFR-05/08 / SceneId 拡張→NFR-07/08。
