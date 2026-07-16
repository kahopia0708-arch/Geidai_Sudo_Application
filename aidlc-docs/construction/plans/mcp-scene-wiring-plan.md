# MCP フォローアップ — 実シーン配線 Plan

**プロジェクト**: 藝大 須藤さんアプリ
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / Follow-up（Build and Test 残タスク）
**検証**: 公式 Unity AI Assistant（`user-unity-mcp`）
**ユーザー指示**: 「Use AI-DLC, 前述の作業を実施」

> 本計画は承認相当の実行指示（「実施」）を受けて Part 2 を即時実行する。各ステップ完了で `[x]` に更新する。

---

## 0. 方針
- **新シーン名は `Geidai*` プレフィックス**（既存 `Home`/`Rec`/`Game01`/`MySoundCollection` とファイル名衝突を避ける）
- **NavigationService.SceneMap** を新シーン名へ更新（Theme を登録、Register を実在させる）
- **見た目は最小骨組み**（Canvas + SafeArea + コントローラ + 既定アセット）。意匠は Sさん（US-TECH-07）
- **既存ゲーム選択**（`game_Home` / `StartGameButton`）は `NavigationService.GoTo(Game1)` に接続
- **旧 WeeklyTextController** は新 Theme 導線に置き換え後、スクリプト削除
- **Build Settings**: 新 Geidai シーンを有効登録、旧シーンは無効化（削除はしない＝ロールバック容易）

## 1. 実装ステップ
- [x] **Step0** MCP ベースライン（Error 0）
- [x] **Step1** `NavigationService` に Theme / 新シーン名を登録。`NavigationRoutingTests` を Theme 登録後の期待に更新
- [x] **Step2** `StartGameButton` を `INavigationService.GoTo(Game1)` 接続（sceneName フォールバック残置可）。併せて `SceneSwitcher`/`ReturnHomeButton`/`GoToRec`/`GoToSoundCollection` も Nav 接続
- [x] **Step3** `Assets/Editor/GeidaiSceneBootstrap.cs`（シーン骨組み生成の Editor ユーティリティ）
- [x] **Step4** MCP で `GeidaiSceneBootstrap.BuildAll()` 実行 → シーン生成＋アセット割当（HomeMenuConfig/ThemeCatalog/SoundMatchConfig 確認済）
- [x] **Step5** MCP で `EditorBuildSettings` 更新（新シーン有効・旧シーン無効）。Main画面に AppManager 追加
- [x] **Step6** 旧 `WeeklyTextController.cs` 削除（無効化済み `Home.unity` に Missing Script 残置＝ロールバック用）
- [x] **Step7** 検証（コンパイル Error 0・Build Settings・シーン存在）＋ docs/audit/commit

## 2. 生成シーン（想定）
| SceneId | シーン名 | パス | 主要コントローラ / アセット |
|---|---|---|---|
| Home | GeidaiHome | `Assets/Scenes/Geidai/GeidaiHome.unity` | HomeScreenController + HomeMenuConfig_Default |
| Register | GeidaiRegister | `Assets/Scenes/Geidai/GeidaiRegister.unity` | UserRegistrationScreenController |
| Rec | GeidaiRec | `Assets/Scenes/Geidai/GeidaiRec.unity` | RecScreenController + 子コントローラ枠 |
| Collection | GeidaiCollection | `Assets/Scenes/Geidai/GeidaiCollection.unity` | CollectionScreenController |
| Theme | GeidaiTheme | `Assets/Scenes/Geidai/GeidaiTheme.unity` | WeeklyThemeScreenController + ThemeCatalog |
| Game1 | GeidaiGame1 | `Assets/Scenes/Geidai/GeidaiGame1.unity` | SoundMatchGameController + SoundMatchConfig |
| Boot | Main画面 | 既存維持 | AppManager（既存） |
| GameSelect | game_Home | 既存維持 | StartGameButton → GoTo(Game1) |

## 3. スコープ外
- 意匠・イラスト・レイアウト微調整（Sさん）
- Player 実ビルド・実機 E2E（Build and Test 手順に従う）
- Place シーンの復活
