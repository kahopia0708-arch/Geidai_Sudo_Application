# MCP フォローアップ — 実シーン配線 Summary

**作成**: 2026-07-16 / AI-DLC CONSTRUCTION Follow-up  
**検証**: 公式 Unity AI Assistant（`user-unity-mcp`）  
**計画**: `../plans/mcp-scene-wiring-plan.md`

---

## 1. 実施内容
| 項目 | 結果 |
|---|---|
| Geidai シーン 6 本生成 | ✅ `Assets/Scenes/Geidai/Geidai{Home,Register,Rec,Collection,Theme,Game1}.unity` |
| 既定アセット割当 | ✅ HomeMenuConfig_Default / ThemeCatalog / SoundMatchConfig |
| NavigationService マップ更新 | ✅ Theme 含む全 SceneId → Geidai*（GameSelect=game_Home / Boot=Main画面） |
| 既存ゲーム選択 → GoTo(Game1) | ✅ `StartGameButton` + `SceneSwitcher` / `ReturnHomeButton` / `GoToRec` / `GoToSoundCollection` |
| Build Settings | ✅ 新シーン有効・旧 Home/Rec/Game01/MySoundCollection/Place/SampleScene 無効 |
| 旧 WeeklyTextController | ✅ スクリプト削除（無効化済み `Home.unity` に Missing Script 残る＝ロールバック用） |
| コンパイル | ✅ Error 0 / Warning 0 |

## 2. シーン導線（実行時）
```
Main画面 (Boot) --SceneSwitcher/Nav--> GeidaiHome
GeidaiHome --HomeMenuConfig--> GeidaiRec / GeidaiCollection / game_Home / GeidaiTheme / GeidaiRegister
game_Home --StartGameButton.GoTo(Game1)--> GeidaiGame1
GeidaiTheme --WeeklyThemeController--> GeidaiRec
各画面 Back --> GeidaiHome
```

## 3. Editor ユーティリティ
- `Assets/Editor/GeidaiSceneBootstrap.cs`
- Menu: `Geidai/Scenes/Build All Geidai Scenes` / `Update Build Settings Only`
- 再生成時は同メニューを再実行（意匠は上書きされる点に注意）

## 4. 残（Sさん / 手動）
- レイアウト・配色・イラスト・カエル成長スプライト（US-TECH-07）
- Rec のエフェクト UI 詳細・Collection 一覧アイテムプレハブの見た目
- 無効化済み旧シーンの整理削除（任意）
- PlayMode E2E / 実機性能（build-and-test 手順）

## 5. 起動確認の最短手順
1. Build Settings で `Main画面` が先頭・有効であることを確認
2. Play → ボタンで GeidaiHome へ
3. ホームメニューから Rec / Collection / ゲーム / お題 / せってい を開く
4. game_Home の開始ボタンで GeidaiGame1 が開くこと
