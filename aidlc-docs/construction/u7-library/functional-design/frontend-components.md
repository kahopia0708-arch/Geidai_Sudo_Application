# U7 Sound Library — Frontend Components

**ユニット**: U7 Sound Library  
**作成**: 2026-07-30

## 1. 画面構成
- **LibraryScreenController** (: ScreenRootBase)
  - 一覧読込、ロック投影、試聴、ホームへ戻る
- **CuratedSoundListView**
  - スクロール一覧。分類フィルタは任意（後続可）
- **CuratedSoundItemView**
  - 表示名、分類、ロックアイコン、試聴ボタン
- **EmptyState / ErrorPresenter**
  - 空カタログ・読込失敗

## 2. 状態
| 状態 | 説明 |
|---|---|
| Loading | カタログ／UnlockState 読込中 |
| Ready | 一覧表示 |
| Playing | 試聴中 |
| Empty | 有効定義ゼロ |
| Error | 読込失敗（安全メッセージ） |

## 3. 操作
- 項目タップ（unlocked）→ 試聴開始／停止
- もどる → NavigationService.GoBack / Home
- 表示中に UnlockState 更新 → 再投影

## 4. ハンドオフ（企画・デザイン）
- アイコン、ロック表現、分類色、文言
- CuratedSoundCatalog / UnlockRulesCatalog の内容編集
- Prefab 見た目（コード最小）

## 5. 非対象
- 音づくりUI（U8）
- 通貨UI、ランキング、共有ボタン
