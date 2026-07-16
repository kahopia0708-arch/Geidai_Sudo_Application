# MCP フォローアップ — 実シーン配線 Summary

**作成**: 2026-07-16 / AI-DLC CONSTRUCTION Follow-up  
**検証**: 公式 Unity AI Assistant（`user-unity-mcp`）＋ユーザー Play 確認（2026-07-16）  
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
| Play 確認（ユーザー） | ✅ 2026-07-16「ここまでの修正は確認できました」 |

---

## 2. シーン導線（実行時）
```
Main画面 (Boot) --SceneSwitcher/Nav--> GeidaiHome
GeidaiHome --HomeMenuConfig--> GeidaiRec / GeidaiCollection / game_Home / GeidaiTheme / GeidaiRegister
game_Home --StartGameButton.GoTo(Game1)--> GeidaiGame1
GeidaiTheme --WeeklyThemeController--> GeidaiRec
GeidaiRec --GoBack()--> 直前画面（お題→録音なら Theme / ホーム直なら Home）
各モジュール Back（Theme/Game1/Collection/Register） --> GeidaiHome
```

---

## 3. Editor ユーティリティ
- `Assets/Editor/GeidaiSceneBootstrap.cs`
- Menu: `Geidai/Scenes/Build All Geidai Scenes` / `Update Build Settings Only`
- 再生成時は同メニューを再実行（意匠は上書きされる点に注意）
- 主な生成物: ColorTint ボタン、Dropdown Template+Toggle、Collection ScrollRect+ItemPrefab、SafeArea 内「もどる」

---

## 4. 残（Sさん / 手動）
- レイアウト・配色・イラスト・カエル成長スプライト（US-TECH-07）
- Rec のエフェクト UI 詳細・Collection 一覧アイテムの見た目磨き
- 無効化済み旧シーンの整理削除（任意）
- Unity Test Runner 全 EditMode / 実機性能（build-and-test 手順）
- Player 実ビルド（Android/iOS）

---

## 5. 起動確認の最短手順
1. Build Settings で `Main画面` が先頭・有効であることを確認
2. Play → ボタンで GeidaiHome へ
3. ホームメニューから Rec / Collection / ゲーム / お題 / せってい を開く
4. お題 → 録音 → もどる でお題に戻ること
5. 録音保存後、コレクションに行が出ること／設定再入でプロフィールが表示されること
6. game_Home の開始ボタンで GeidaiGame1 が開くこと

---

## 6. ホットフィックス履歴（2026-07-16・ユーザー確認済）

| # | 症状 | 原因 | 対応 | Commit |
|---|---|---|---|---|
| 1 | 毎フレーム Input 例外 | 旧 `UnityEngine.Input` | Input System `Keyboard.current` | `104b669` |
| 2 | ホーム真っ白 | 日本語グリフ無し／メニュー非表示／Title 位置 | OS フォント・`SetActive`・SafeArea 内 | `4725474` |
| 3 | AudioListener 警告 | Camera に Listener 無し | 全 Geidai シーンに追加 | `12496e2` |
| 4 | もどる無し／設定 Dropdown エラー／お題から戻れない／ボタン反応不明 | SafeArea 外・Template 未設定・Edit 時 onClick・状態 UI 無し | Bootstrap 再生成・BackToHome・statusText・ColorTint | `0137719` |
| 5 | 録音がコレクションに出ない／設定が空 | 一覧 UI 未配線／Register が常に New | ScrollRect+ItemPrefab／`TryLoadExisting` | `5a4b1b2` |
| 6 | 音合わせドラッグでブロックがジャンプ | ポインタ＝中心直結 | 掴みオフセット保持 | `81f3343` |
| 7 | お題→録音→もどるがホーム | Rec が常に `GoTo(Home)` | `GoBack()`＋Home フォールバック | `ea61662` |

**補足（#5）**: ディスク上の `profile.json` / `sounds/*.wav+meta` は当初から保存成功済み。表示・読込側の不具合だった。
