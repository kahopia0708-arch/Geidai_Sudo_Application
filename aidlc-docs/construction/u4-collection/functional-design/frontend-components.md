# U4 Persistence/Collection — Frontend Components（画面構造）

**ユニット**: U4 Persistence/Collection
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Functional Design（Part 2）
**決定**: Q7=A（1画面：一覧＋絞込/検索＋詳細・編集＋空状態）
**技術非依存**: uGUI/TMP の具体・スクロール仮想化・写真ピッカー実装・実シーン配線は NFR Design / Code Generation（Unity MCP）。見た目調整は S さん（US-TECH-07）。

> UI 枠は U1 基盤（`ScreenRootBase`／`ResponsiveCanvasConfigurator`／`SafeAreaFitter`／`UITheme`／`ConfirmDialog`／`ErrorPresenter`）の上に構築。固定 px 依存を排除（BR-COL-60）。

---

## 1. 画面構成（Collection・1画面）

```
Collection (Canvas / ScreenRootBase = CollectionScreenController)
├── TopBar
│   ├── BackToHomeButton（U2 再利用：ホームへ）
│   └── Title「マイコレクション」
├── FilterSearchBar（FilterSearchController）
│   ├── MonthDropdown（月別絞込：全て / YYYY-MM …）
│   ├── SearchInput（キーワード：title/memo/nickname）
│   └── ClearButton（条件クリア）
├── ListArea（SoundListView・スクロール／レスポンシブ）
│   └── SoundListItem（× N）
│       ├── TitleText（title 空なら日付）
│       ├── DateText（createdAtIso）
│       ├── PhotoThumb（任意・hasPhoto 時）
│       ├── PlayButton（視聴）
│       └── OpenButton（詳細へ）
├── DetailPanel（SoundDetailController・初期非表示）
│   ├── TitleField（編集）
│   ├── PhotoView ＋ ChangePhotoButton / RemovePhotoButton
│   ├── MemoField（編集）
│   ├── NicknameText（読み取り・プロフィール由来）
│   ├── PlayButton / StopButton（保存エフェクト再適用再生）
│   ├── SaveButton（メタ保存＝原子的置換）
│   └── DeleteButton（→ ConfirmDialog）
├── EmptyState（空状態・初期非表示）
│   └── Message「まだ おとが ないよ」/ 検索時「みつからなかったよ」
├── ConfirmDialog（U1 再利用：削除確認・既定＝いいえ）
└── ErrorPresenter（U1 再利用：失敗の平易通知バナー）
```

---

## 2. 状態別の表示（活性/表示マトリクス）

| 状態 | ListArea | DetailPanel | EmptyState | 主操作 |
|---|:---:|:---:|:---:|---|
| Loading | （読込中） | 非表示 | 非表示 | ― |
| Empty（0件/初期） | 空 | 非表示 | **表示** | 戻る |
| Empty（検索0件） | 空 | 非表示 | **表示（検索文言）** | 条件クリア/戻る |
| Listing | **表示** | 非表示 | 非表示 | タップ視聴/開く/絞込/検索 |
| Playing | 表示（再生中表示） | （開いていれば表示） | 非表示 | 停止/切替 |
| Detail | 表示 | **表示** | 非表示 | 再生/編集/削除/戻る |
| Editing | 表示 | **表示（編集可）** | 非表示 | 保存/取消/写真変更 |
| Confirm（削除） | 表示 | 表示 | 非表示 | はい/いいえ |

---

## 3. コンポーネント責務・状態・入出力

### 3.1 `CollectionScreenController`（`ScreenRootBase` 継承）
- **責務**: 画面状態統括、`OnShow` で一覧読込（`IStorageService.ListSounds`）、子コントローラ調停、戻る/ホーム遷移（`NavigationService.GoTo(Home)`）。
- **状態**: `CollectionState`（Loading/Empty/Listing/Playing/Detail/Editing/Confirm）、現在の `CollectionQuery`。
- **入力**: 一覧読込結果（有効項目＋破損スキップ件数）。
- **出力**: 子ビューへの描画指示、`ErrorPresenter` 通知。
- **戻る**: `OnBackPressed` → Detail 表示中は一覧へ、一覧なら Home へ（未保存の編集があれば取消確認は任意）。

### 3.2 `SoundListView`
- **責務**: 有効 `SavedSound` の一覧描画（レスポンシブ・固定px排除）、項目の Play/Open 通知。
- **入力**: フィルタ済み項目リスト。
- **出力**: `onPlay(id)` / `onOpen(id)`。

### 3.3 `SoundDetailController`
- **責務**: 詳細表示、メタ編集（title/photo/memo）、再生/停止、削除起動。
- **入力**: 対象 `SavedSound`。
- **出力**: `onSaveMeta(updatedMeta)` / `onDelete(id)` / `onPlay(id)` / `onStop()`。
- **検証**: title/memo 長（BR-COL-12/13）。保存は原子的置換（失敗は既存維持）。

### 3.4 `FilterSearchController`
- **責務**: `CollectionQuery`（月/キーワード）の保持と純粋 `Filter` 適用、空状態制御。
- **入力**: 全有効項目、UI 変更（月選択/キーワード/クリア）。
- **出力**: フィルタ済みリスト、空状態フラグ。

---

## 4. ユーザー操作フロー（代表）
1. **一覧→視聴**: 開く → 一覧 → 項目 Play → 保存エフェクト再適用で再生 → 停止/別項目で切替。
2. **詳細→編集→保存**: 項目 Open → Detail → 編集 → title/写真/memo 変更 → 保存（原子的置換）→ Detail。
3. **削除**: Detail/一覧の削除 → ConfirmDialog（既定いいえ）→ はい → wav+meta+photo 削除 → 再読込 → 一覧/空状態。
4. **絞込・検索**: 月選択/キーワード入力 → 純粋 Filter → 該当表示 or 空状態 → クリアで復帰。
5. **堅牢性**: 破損/欠損項目は自動スキップ（ユーザーには他項目が正常表示）。全失敗は平易通知でクラッシュ回避。

---

## 5. フォーム検証（編集）
| フィールド | 規則 | 失敗時 |
|---|---|---|
| title | trim・最大長（目安40・NFR確定）・改行不可 | 平易メッセージ・保存不可 |
| memo | 最大長（目安200・NFR確定） | 平易メッセージ・保存不可 |
| 写真 | 端末ローカルのみ・対応拡張子（jpg/png） | 差し替え失敗通知（既存維持） |

---

## 6. S さんハンドオフ点（US-TECH-07 / NFR-11/12）
- 一覧アイテムの見た目（カード意匠・サムネ枠・余白・モチーフ＝カエル/おたまじゃくし/蓮）・配色・アイコン・文言。
- 空状態のイラスト/文言、確認ダイアログ/エラーバナーのトーン（`ConfirmDialog`/`ErrorPresenter`）。
- レイアウトは Anchor/レイアウトグループで柔軟化（固定px排除）。SafeArea を画面ルートに適用。縦横両対応。
- UI 枠は uGUI（`Dropdown`/`InputField`/`Button`/`Text`/`ScrollRect`/`Image`）で生成。必要に応じ TMP 差し替え可。

---

## 7. トレース
FR-09→ListArea/PlayButton/DeleteButton ／ FR-10→DetailPanel（title/photo/memo/nickname）／ FR-11→FilterSearchBar/FilterSearchController ／ FR-12→ローカル保存（原子的）／ US-COL-04・US-TECH-06→EmptyState・破損スキップ・原子性 ／ NFR-05→ConfirmDialog/空状態/平易通知 ／ NFR-11/12→固定px排除・SafeArea・縦横両対応 ／ US-TECH-07→§6 ハンドオフ。
