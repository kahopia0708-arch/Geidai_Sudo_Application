# U2 Foundation — Code Generation Summary（コード生成サマリ）

**ユニット**: U2 Foundation（起動・ホーム・登録・ナビ導線）
**生成日**: 2026-07-15 / AI-DLC CONSTRUCTION / Code Generation（Part 2）
**種別**: Brownfield（Unity 6000.4.2f1 / URP / uGUI / C#）
**MCP**: 公式 Unity AI Assistant（`user-unity-mcp`）で検証（Error 0 / スモーク全 PASS）

> アプリコードは `Assets/` 配下。本書は要約（`aidlc-docs/`）。

---

## 1. 生成/修正/削除ファイル一覧

### 新規（`Geidai.Foundation` / `Assets/Scripts/Foundation/`）
- Created: `Geidai.Foundation.asmdef`（refs: Geidai.Common, Geidai.Services, UnityEngine.UI）
- Created: `ModuleId.cs`（enum: Rec, Collection, GameSelect, WeeklyTheme, ProfileEdit）
- Created: `RegistrationMode.cs`（enum: New, Edit）
- Created: `ModuleRouter.cs`（ModuleId→SceneId 変換／純粋）
- Created: `StartupRouter.cs`（起動判定の純粋関数＋`StartupDecision`）
- Created: `HomeMenuItem.cs`（`[Serializable]` メニュー項目）
- Created: `HomeMenuConfig.cs`（ScriptableObject／`VisibleSorted()`）
- Created: `BootScreenController.cs`（起動状態機械）
- Created: `HomeScreenController.cs`（データ駆動メニュー＋終了確認）
- Created: `UserRegistrationScreenController.cs`（New/Edit・検証・保存）
- Created: `BackToHomeButton.cs`（モジュール戻る再利用部品）

### 新規（`Geidai.Common.UI` / `Assets/Scripts/Common/UI/`）
- Created: `ConfirmDialog.cs`（はい/いいえ・既定=いいえ・横断再利用）

### 修正（後方互換）
- Modified: `Assets/Scripts/Common/Models/SceneId.cs`（末尾に `GameSelect` 追加）
- Modified: `Assets/Scripts/Common/UI/ScreenRootBase.cs`（端末バック入力 `Update()`＋`listenForSystemBack`）
- Modified: `Assets/Scripts/Services/Navigation/NavigationService.cs`（SceneMap に `Register`/`GameSelect` 追加）
- Modified: `Assets/Scripts/Tests/EditMode/Geidai.Tests.asmdef`（references に `Geidai.Foundation` 追加）

### 削除
- Deleted: `Assets/Scripts/GoToPlace.cs`（＋`.meta`）※ Place 除外・"place" 大文字小文字バグ解消（BR-11）

### テスト（EditMode / `Geidai.Tests`）
- Created: `StartupRouterTests.cs`（Home / Register(NotFound) / Register+警告(Corrupted・IOError) 分岐網羅）
- Created: `NavigationRoutingTests.cs`（`GoTo(Theme)`→NotFound、`ModuleRouter` マップ検証）
- Created: `HomeMenuConfigTests.cs`（`VisibleSorted()` 非表示除外・order 昇順・空設定）

### アセット（MCP 生成）
- Created: `Assets/Settings/HomeMenuConfig_Default.asset`（5項目：ろくおん/コレクション/ゲーム/こんしゅうのおだい/せってい。Place・テスト無し）

---

## 2. 名前空間・依存
- `Geidai.Foundation` → `Geidai.Services` → `Geidai.Common`（一方向・循環なし）。
- `ConfirmDialog` は横断再利用のため `Geidai.Common.UI`。
- 外部 API/ネットワークなし（NFR-02）。

## 3. MCP 検証結果（`user-unity-mcp`）
- ベースライン `Unity_GetConsoleLogs`：Error 0 / Warning 0。
- 取り込み後コンパイル：**Error 0 / Warning 0**（`isCompilationSuccessful=true`）。
- 同期スモーク（`Unity_RunCommand`）：
  - `StartupRouter`：withProfile→Home / NotFound→Register / Corrupted→Register+警告 = **全 True**。
  - `ModuleRouter`：Rec/GameSelect/ProfileEdit/WeeklyTheme マップ = **True**。
  - `HomeMenuConfig_Default`：items=5 / VisibleSorted=5。
- Build Settings：Main画面/Home/Rec/MySoundCollection/game_Home/Game01 登録済み。

## 4. UI ハンドオフ点（前本 → Sさん / US-TECH-07）
- `HomeMenuConfig_Default.asset`：ラベル/アイコンキー/並び順（order）/可視をアセット編集で調整可（コード非依存）。
- `UITheme`（U1）＋各画面 Prefab：配色/フォント/アイコン/レイアウトの微調整。
- `ConfirmDialog`/`ErrorPresenter`：バナー・ダイアログの見た目/文言トーン。
- UI 枠は uGUI（`Dropdown`/`InputField`/`Button`/`Text`）で生成。必要に応じ TMP へ差し替え可（Sさん）。

## 5. 残タスク（MCP フォローアップ：実シーン配線）
> コードは完成。以下は Unity 上での GameObject 配線で、破壊回避のため別途 MCP セッションで実施する。

1. **Register.unity 新規作成**：Canvas＋`ResponsiveCanvasConfigurator`＋`SafeAreaFitter`、生年 `Dropdown`／ニックネーム `InputField`／確定・キャンセル `Button`、`UserRegistrationScreenController` をアタッチし各参照を結線。Build Settings に追加。
2. **Main画面.unity（Boot）**：`AppManager`（U1）＋`BootScreenController` を配置、「はじめる」`Button` を `OnBeginTapped` に結線。起動シーンを Main画面 に設定（現状 build index 0 は SampleScene）。
3. **Home.unity**：`HomeScreenController` を配置し `HomeMenuConfig_Default` を割当。メニュー `menuContainer`／`menuButtonPrefab`／`ConfirmDialog`／`ErrorPresenter` を結線。既存 per-button（`GoToRec`/`GoToSoundCollection`/`ReturnHomeButton`/Place ボタン）を除去し新方式に統一。
4. **モジュール各シーン（Rec/MySoundCollection/game_Home）**：「もどる/ホーム」ボタンに `BackToHomeButton` を付与（`ReturnHomeButton` を置換）。
5. 配線後、PlayMode で「起動→(初回)登録→ホーム→各モジュール→戻る」を通し確認（Build & Test）。

## 6. スコープ外（U2 では未実施）
- 各モジュール中身（U3〜U6）。
- 永続化の原子的置換・破損復旧の本実装（U4）。
- 既存 per-button スクリプト（`SceneSwitcher`/`GoToRec`/`GoToSoundCollection`/`ReturnHomeButton`/`StartGameButton`）の物理削除（シーン再配線と同時：上記 §5）。

## 7. トレース
US-NAV-01→BootScreenController/StartupRouter/NavigationService ／ US-NAV-02→HomeScreenController/HomeMenuConfig/ModuleRouter ／ US-REG-01/02→UserRegistrationScreenController＋U1 ValidationUtil/StorageService ／ NFR-05→ConfirmDialog/ErrorPresenter ／ NFR-07→StartupRouter/Result 安全遷移 ／ NFR-08→ナビ統一・データ駆動 ／ NFR-09→EditMode テスト ／ US-TECH-04/05/07。
