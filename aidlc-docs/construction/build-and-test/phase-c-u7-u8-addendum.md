# Build and Test — Phase C Addendum（U7/U8）

**作成**: 2026-07-30  
**ブランチ**: `feature/sound-library-planning`  
**対象追加**: U7 Sound Library / U8 Sound Create  
**前提**: U1〜U6 Build and Test（2026-07-16）＋ U7/U8 Code Generation 完了（commit `4ff1262`）

> 本追記は既存 `build-instructions.md` / `unit-test-instructions.md` / `integration-test-instructions.md` / `performance-test-instructions.md` / `build-and-test-summary.md` を**上書きせず差分として追加**する。実行時は本追記＋既存手順を併用する。

---

## 1. ビルド差分（assemblies / シーン）

### 追加アセンブリ
```
Geidai.Common ← Geidai.Services ← { ..., Geidai.Library, Geidai.Create }
```
| Assembly | 役割 |
|---|---|
| `Geidai.Library` | 音図鑑 UI（一覧・ロック・試聴） |
| `Geidai.Create` | 音づくり UI（2音レシピ・保存・WAVE書き出し） |
| （Services 拡張） | `ProgressionService` / UnlockState・Recipe I/O / PlayLayers |

**一方向依存**: Library/Create → Services → Common。**Rec / Collection / 相互依存なし**。

### Build Settings 追加（MCP フォローアップ）
| SceneId | 想定シーン名 | 状態 |
|---|---|---|
| `Library` | `GeidaiLibrary` | 作成・配線・Build Settings 有効化済（2026-07-30） |
| `Create` | `GeidaiCreate` | 作成・配線・Build Settings 有効化済（2026-07-30） |

HomeMenuConfig / ModuleRouter / Navigation マップから両シーンへ遷移可能。既定カタログには試聴可能な1秒 WAVを2件割り当て済み。

### 展示ビルド観点（US-TECH-09 / NFR-13）
1. Development / Release で Android または iOS ビルド。
2. 初期カタログ（目標 50〜100 音）投入前後で **アプリサイズ**を記録。
3. 記録先: 本追記の「サイズ計測ログ」節または `device-verification-checklist.md` 追記。
4. 任意展示（2026-11-20〜12-02）向けは**音図鑑入り**インストール可能ビルドを目標。必須展示（2027-03）は別途確認。

---

## 2. ユニットテスト差分

### 追加 EditMode ファイル（`Assets/Scripts/Tests/EditMode/`）
| ユニット | ファイル | 対象 |
|---|---|---|
| U7 | `UnlockEvaluatorTests.cs` | 冪等・Combined・初期解除・投影 |
| U7 | `UnlockStateJsonTests.cs` | UnlockState JSON 往復 |
| U8 | `RecipeValidatorTests.cs` | クランプ PBT・CanSave・LayerCount |
| U8 | `SoundRecipeJsonTests.cs` | SoundRecipe JSON 往復 |

**合計ファイル**: 既存 17 ＋ 新規 4 ＝ **21**  
**実行**: 既存どおり Test Runner EditMode `Run All`  
**既知結果**: 2026-07-30 に全件再実行し **97 Pass / 0 Fail / 0 Skip**。PBT が検出した `NaN` クランプ不具合は修正後に再実行済み。

---

## 3. 統合シナリオ差分

### Scenario 7: 音図鑑一覧・ロック・試聴（Library ↔ Progression ↔ Content ↔ Audio）
- **セットアップ**: `CuratedSoundCatalog_Default` / `UnlockRulesCatalog_Default` を画面に割当。`AppManager` 起動済み。
- **手順**: Home → Library（シーン配線後）→ 初期解除音は試聴可 → ロック音は試聴不可 → GameClear イベント後に再投影で解除反映。
- **期待**: クラッシュなし。未知IDは無視。UnlockState は原子的保存。
- **現状**: シーン・全参照・Home 導線を MCP で検証済み。Play Mode の操作確認は次の手動テスト。

### Scenario 8: 音づくり 2音・保存・書き出し（Create ↔ Unlock ↔ Audio ↔ Storage）
- **手順**: 解除済み2音選択 → パラメータ調整 → プレビュー → レシピ保存 → 任意 WAVE 書き出し。
- **期待**: レシピ JSON のみ保存（同梱音非複製）。未解除素材は保存不可。書き出し失敗で不完全 wav を残さない。
- **後片付け**: `persistentDataPath/recipes/` と `exports/` を削除。

### Scenario 9: 進行イベント配線（Game1/Rec → Progression → Library）※フォローアップ
- Game1 クリア / Rec 保存成功から `IProgressionService.Notify*` を呼ぶ本番配線後に実施。

---

## 4. 性能・サイズ差分

| 指標 | 目標 | 由来 |
|---|---|---|
| カタログ一覧表示 | 体感即時（50〜100 件） | NFR-13/U7 |
| 2音プレビュー開始 | 体感即時 | NFR-U8 |
| レシピ保存/読込 | 体感即時・原子的 | NFR-14 |
| ビルドサイズ | カタログ投入前後を計測・記録 | NFR-13 / US-TECH-09 |

負荷/同時接続は従来どおり **N/A（オフライン）**。

---

## 5. サイズ計測ログ（記入用）

| 日付 | プラットフォーム | カタログ件数 | ビルド種別 | サイズ | 備考 |
|---|---|---|---|---|---|
| | Android / iOS | 2（サンプル） | | | 現状デフォルト asset |
| | | 50〜100 | | | 本番素材投入後 |

---

## 6. 合否（Phase C）

- [x] コンパイル Error 0 / Warning 0（U7/U8 生成＋CS0618 修正後）
- [x] EditMode 全件（21 ファイル）Pass — 97 Pass / 0 Fail
- [x] Scenario 7/8 シーン・参照・導線の静的検証
- [ ] Scenario 7/8 Play Mode 操作確認
- [ ] 展示向けサイズ計測（任意／任意展示前）
- [ ] Progression 本番配線（Scenario 9）

## 7. 次アクション推奨順
1. Play Mode で Scenario 7/8 を操作確認
2. カタログ本投入＋サイズ計測
3. Game1/Rec → Progression 通知配線
