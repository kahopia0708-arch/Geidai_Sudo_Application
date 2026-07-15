# U2 Foundation — Code Generation Plan（Part 1: 計画）

**ユニット**: U2 Foundation（起動・ホーム・登録・ナビ導線）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Code Generation（Part 1）
**プロジェクト種別**: Brownfield（Unity 6000.4.2f1 / URP / uGUI / C#）
**Workspace Root**: `/Users/maemoto/Documents/GitHub/Geidai_Sudo_Application`
**入力**: `../u2-foundation/functional-design/*`, `../u2-foundation/nfr-requirements/*`, `../u2-foundation/nfr-design/*`, U1 生成コード（`Assets/Scripts/Common|Services`）

> 本プランは Code Generation の**唯一の正**（single source of truth）。Part 2 では上から順に実行し、各ステップ完了時に `[x]` を付ける。

---

## 0. 生成方針（重要）
- **アプリコードは Workspace 直下（`Assets/`）に生成**。ドキュメントのみ `aidlc-docs/`。
- **Brownfield 非破壊 / 一方向依存**: 新規 U2 コードは **`Geidai.Foundation`**（新 asmdef）に分離。依存は `Foundation → Services → Common` の一方向。
- **U1 Common/Services の後方互換な拡張のみ**: `SceneId` に列挙値追加、`NavigationService` にマップ追加、`ScreenRootBase` に端末バック入力処理を追加（既存 API/意味は不変）。
- **UI は uGUI（`UnityEngine.UI`）で枠組みを生成**（`Dropdown`/`InputField`/`Button`/`Text`）。TMP への差し替えは Sさん ハンドオフ点（US-TECH-07・追加 asmdef 依存を避けるため枠は uGUI）。
- **`.meta` は Unity が生成**。手動生成しない。
- **シーン配線は Unity 公式 MCP（`user-unity-mcp`）で実施**（US-TECH-05）。実シーン（Boot/Home/Register）の GameObject 配線・Build Settings 登録は Step 12 で best-effort、未接続時は §5 フォールバック。
- **テストは生成中心**（本実行は Build & Test。ただし純粋ロジックは MCP `Unity_RunCommand` で同期スモーク）。

### Unity MCP 活用（US-TECH-05）— 公式 Unity AI Assistant（`user-unity-mcp`）
- `Unity_GetConsoleLogs`（Error/Warning 確認）／`Unity_RunCommand`（C# コンパイル＆実行：AssetDatabase 更新・アセット/シーン生成・Build Settings 変更・スモーク）を使用。
- サードパーティブリッジは使用しない（U1 と同方針）。

### 生成先フォルダ構成（新規/追記）
```
Assets/Scripts/
├── Foundation/            (Geidai.Foundation.asmdef → refs Geidai.Common, Geidai.Services, UnityEngine.UI) ★新規
│   ├── ModuleId.cs                 (enum: Rec, Collection, GameSelect, WeeklyTheme, ProfileEdit)
│   ├── RegistrationMode.cs         (enum: New, Edit)
│   ├── ModuleRouter.cs             (ModuleId→SceneId 変換)
│   ├── StartupRouter.cs            (純粋: LoadProfile Result→起動遷移決定＋警告要否)
│   ├── HomeMenuItem.cs             ([Serializable] moduleId/label/iconKey/visible/enabled/order)
│   ├── HomeMenuConfig.cs           (ScriptableObject: List<HomeMenuItem> ＋ VisibleSorted())
│   ├── BootScreenController.cs     (ScreenRootBase; 状態機械 Idle→Checking→Route/Error)
│   ├── HomeScreenController.cs     (HomeMenuConfig 描画＋遷移＋端末バック=終了確認)
│   ├── UserRegistrationScreenController.cs (New/Edit; 検証→保存)
│   └── BackToHomeButton.cs         (モジュール画面用の「もどる/ホーム」再利用部品)
├── Common/UI/             (Geidai.Common)  ※追記/修正
│   ├── ScreenRootBase.cs           (修正: 端末バック入力→OnBackPressed へ橋渡し)
│   └── ConfirmDialog.cs            (新規: はい/いいえ、既定=いいえ)
├── Common/Models/SceneId.cs        (修正: GameSelect 追加)
└── Services/Navigation/NavigationService.cs (修正: Register/GameSelect マップ追加)

Assets/Scripts/Tests/EditMode/ (Geidai.Tests)  ※追記（refs に Geidai.Foundation 追加）
├── StartupRouterTests.cs           (起動遷移決定の分岐網羅)
├── NavigationRoutingTests.cs       (未登録 SceneId→NotFound 安全処理)
└── HomeMenuConfigTests.cs          (可視項目フィルタ/並び)

削除:
└── Assets/Scripts/GoToPlace.cs     (Place 除外・大文字小文字バグ解消 / BR-11)
```

---

## 1. 対象ストーリー（U2 / トレース）
- **US-NAV-01** 起動→（初回=登録／既存=ホーム）→各モジュール空画面への一貫遷移 → `BootScreenController`, `StartupRouter`, `NavigationService`
- **US-NAV-02** ホームのモジュール導線（Place/テスト除外・モチーフ識別） → `HomeScreenController`, `HomeMenuConfig`, `ModuleRouter`
- **US-REG-01** 初回ユーザー登録（生年・ニックネーム） → `UserRegistrationScreenController`（New）＋ U1 `ValidationUtil`/`StorageService`
- **US-REG-02** プロフィール編集 → `UserRegistrationScreenController`（Edit）
- 併せて: NFR-05（識別性・誤操作防止：`ConfirmDialog`）, NFR-07（`Result`/安全遷移）, NFR-08（ナビ統一・データ駆動）, US-TECH-04/07

## 2. 依存・インターフェース
- `Geidai.Foundation` は `Geidai.Services`（`INavigationService`/`IStorageService`/`ServiceRegistry`/`AppManager`）と `Geidai.Common`（`Result`/`UserProfile`/`SceneId`/`ValidationUtil`/`ScreenRootBase`/`ErrorPresenter`/`UITheme`）に依存。
- 外部 API/ネットワークなし（完全オフライン / NFR-02）。
- `ConfirmDialog` は横断再利用のため `Geidai.Common.UI` に配置（U4 の削除確認等でも利用）。

---

## 実行ステップ（Part 2 でこの順に実行）

### Step 0: MCP 接続確認・ベースライン（US-TECH-05）
- [x] `Unity_GetConsoleLogs` でベースライン取得（Error 0 を確認：errorCount=0/warningCount=0）
- [x] 接続済み（`user-unity-mcp` serverStatus=ready）。§5 フォールバック不要
- _トレース: US-TECH-05 / NFR-10_

### Step 1: Geidai.Foundation asmdef とフォルダ
- [x] `Assets/Scripts/Foundation/Geidai.Foundation.asmdef`（references: Geidai.Common, Geidai.Services, UnityEngine.UI；autoReferenced=true）
- _トレース: NFR-08_

### Step 2: 列挙・ドメイン（Common 拡張 ＋ Foundation）
- [x] `Common/Models/SceneId.cs` に **`GameSelect` 追加**（既存値の順序は不変・後方互換）
- [x] `Foundation/ModuleId.cs`（Rec, Collection, GameSelect, WeeklyTheme, ProfileEdit）
- [x] `Foundation/RegistrationMode.cs`（New, Edit）
- [x] `Foundation/ModuleRouter.cs`（`ModuleId`→`SceneId`：Rec→Rec, Collection→Collection, GameSelect→GameSelect, WeeklyTheme→Theme, ProfileEdit→Register）
- _トレース: domain-entities.md / BR-10/11 / NFR-08_

### Step 3: Navigation マップ拡張（Services 修正）
- [x] `Services/Navigation/NavigationService.cs` の SceneMap に `Register→"Register"`、`GameSelect→"game_Home"` を追加（`Theme` は U5 まで未登録＝`NotFound` 安全処理）
- _トレース: FR-02 / BR-13/14 / nfr-design §2_

### Step 4: ホームメニュー データ駆動（Foundation）
- [x] `Foundation/HomeMenuItem.cs`（`[Serializable]`：moduleId, label, iconKey, visible, enabled, order）
- [x] `Foundation/HomeMenuConfig.cs`（ScriptableObject：`List<HomeMenuItem> items`＋`List<HomeMenuItem> VisibleSorted()`。Place/テストは含めない）
- _トレース: US-NAV-02 / BR-10〜12 / US-TECH-07 / nfr-design logical §1.4_

### Step 5: 共通 UI（Common 追記/修正）
- [x] `Common/UI/ConfirmDialog.cs`（新規：`Show(title,message,onYes,onNo)`、既定フォーカス=いいえ、`Hide()`／`IsOpen`；uGUI）
- [x] `Common/UI/ScreenRootBase.cs`（修正：`[SerializeField] bool listenForSystemBack=true`＋`Update()` で `IsVisible && Escape`→`OnBackPressed()` 呼出。既存 `OnBackPressed()`/`BackRequested` は維持）
- _トレース: NFR-05（誤操作防止）/ nfr-design §3 / logical §1.5・1.6_

### Step 6: 起動ルーター（純粋・Foundation）
- [x] `Foundation/StartupRouter.cs`（`StartupDecision Resolve(Result<UserProfile>)`：成功&値有→Home/警告なし、`NotFound`→Register/警告なし、`Corrupted`/`IOError`/その他→Register/**警告あり**（BR-04））
- _トレース: US-NAV-01 / BR-01〜04 / NFR-07/09 / nfr-design §1_

### Step 7: Boot コントローラ（Foundation）
- [x] `Foundation/BootScreenController.cs`（`ScreenRootBase` 継承。状態 Idle→Checking→Routing/Error。`OnBeginTapped()`（「はじめる」）→`IStorageService.LoadProfile()`→`StartupRouter.Resolve`→`INavigationService.GoTo`。警告時は `ErrorPresenter.ShowWarning`。判定はサービス委譲、UI は分岐のみ）
- _トレース: US-NAV-01 / BR-01〜04 / nfr-design §1_

### Step 8: Home コントローラ（Foundation）
- [x] `Foundation/HomeScreenController.cs`（`ScreenRootBase` 継承。`HomeMenuConfig.VisibleSorted()` を描画（ボタン生成/バインド）、タップで `ModuleRouter`→`INavigationService.GoTo`、失敗は `ErrorPresenter`（NotFound=準備中）。`OnBackPressed()` override→`ConfirmDialog`（終了確認・既定いいえ→はいで Quit）。安定名 `home-menu-{moduleId}` を付与）
- _トレース: US-NAV-02 / BR-10〜15 / NFR-05 / nfr-design §3_

### Step 9: 登録コントローラ（Foundation）
- [x] `Foundation/UserRegistrationScreenController.cs`（`ScreenRootBase` 継承。`Initialize(RegistrationMode)`：Edit は `LoadProfile` で初期値。生年ドロップダウン（1900〜今年生成＋プレースホルダ）＋ニックネーム入力。確定で `ValidationUtil.ValidateBirthYear/Nickname`→OK のみ `UserProfile` 生成→`SaveProfile`（`Result`）→Home へ。失敗はフォーム維持＋`ErrorPresenter`。`OnBackPressed`/キャンセル=Home。PII は `SafeLogger` 非出力）
- _トレース: US-REG-01/02 / BR-05〜09 / NFR-04/05/07 / SECURITY-05_

### Step 10: モジュール戻る部品 ＋ Place 削除
- [x] `Foundation/BackToHomeButton.cs`（モジュール画面の「もどる/ホーム」を `INavigationService.GoTo(Home)` で行う再利用部品。既存 `ReturnHomeButton` の後継）
- [x] `Assets/Scripts/GoToPlace.cs`（＋`.meta`）を**削除**（Place 除外・"place" 大文字小文字バグ解消 / BR-11）。※ Home シーン上の Place ボタン除去は MCP 配線フォローアップ
- _トレース: BR-11/15/16 / NFR-08_

### Step 11: テスト生成（EditMode）
- [x] `Tests/EditMode/Geidai.Tests.asmdef` の references に `Geidai.Foundation` を追加
- [x] `Tests/EditMode/StartupRouterTests.cs`（Home/Register(NotFound)/Register+警告(Corrupted/IOError) の分岐網羅）
- [x] `Tests/EditMode/NavigationRoutingTests.cs`（`GoTo(Theme)`→`NotFound`；`ModuleRouter` マップ検証。※実ロードを伴う経路は Build & Test）
- [x] `Tests/EditMode/HomeMenuConfigTests.cs`（`VisibleSorted()` が非表示除外・order 昇順、空設定）
- _トレース: NFR-09 / nfr-requirements §6_

### Step 12: MCP 検証・アセット/シーン（best-effort）
- [x] `Unity_RunCommand` で `AssetDatabase.Refresh()`→`Unity_GetConsoleLogs`（**Error 0 / Warning 0** 確認）
- [x] `Unity_RunCommand` で `HomeMenuConfig` 既定アセット生成（`Assets/Settings/HomeMenuConfig_Default.asset`：Rec/Collection/GameSelect/WeeklyTheme＋ProfileEdit＝5項目、Place/テスト無し）
- [x] `Unity_RunCommand` で `StartupRouter`/`ModuleRouter` 同期スモーク（**全 PASS**）
- [x] （best-effort）Build Settings 確認：既存シーン（Main画面/Home/Rec/MySoundCollection/game_Home/Game01）は登録済み。**`Register.unity` 雛形作成＋実シーン UI 配線（Boot/Home/Register の GameObject 結線）は破壊回避のため MCP フォローアップ（§5・code-summary に手順明記）**
- _トレース: US-TECH-05 / NFR-10_

### Step 13: コード生成サマリ（ドキュメント）
- [x] `aidlc-docs/construction/u2-foundation/code/code-summary.md`（生成/修正/削除ファイル一覧、名前空間、MCP 検証結果、TODO、シーン配線の MCP 手順、Sさん ハンドオフ点）
- _注: サマリのみ aidlc-docs 配下。コードは Assets 配下。_

### Step 14: ストーリー完了マーク
- [x] `stories.md` の US-NAV-01/02・US-REG-01/02 の U2 実装分を実装済みマーク（実シーン配線の残タスクは注記）
- _トレース: US-NAV/REG_

---

## 3. スコープ外（U2 では実施しない）
- 各モジュール中身（Rec/Collection/Theme/Game の内部：U3〜U6）。
- 永続化の原子的置換・破損復旧の本実装（U4）。
- 既存 per-button スクリプト（`SceneSwitcher`/`GoToRec`/`GoToSoundCollection`/`ReturnHomeButton`/`StartGameButton`）の物理削除は、対象シーンの MCP 再配線と同時に実施（本ユニットでは新方式を提供し、Place のみ削除）。

## 4. 完了条件
- Step 0〜14 のチェックボックスが全て `[x]`。
- 新規コードが `Geidai.Foundation` で生成され、`Foundation→Services→Common` の一方向依存でコンパイル Error 0。
- `SceneId`/`NavigationService` 拡張が後方互換（既存が壊れない）。
- EditMode テストが生成済み（StartupRouter/Navigation NotFound/HomeMenuConfig）。同期スモークがグリーン。
- `HomeMenuConfig_Default.asset` が生成され、Place/テストを含まない。
- code-summary.md にシーン配線 MCP 手順・Sさん ハンドオフ点が明記。

## 5. MCP 未接続時のフォールバック
1. 本ツールで `.cs`/`.asmdef` を `Assets/` に直接生成（コードは完成）。
2. MCP 検証（コンパイル確認・アセット生成・スモーク・シーン配線）は**保留チェック**として残す。
3. Unity 起動後に MCP で一括検証し、チェックを完了。

---

## 承認のお願い
本プラン（全 15 ステップ / Step 0〜14）で U2 のコード生成を進めてよいか、ご確認ください。
- **Request Changes**: ステップ/対象ファイル/方針の修正を指定
- **Continue（承認）**: Part 2（コード生成）を開始
