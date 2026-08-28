# ホーム画面 UI 整備 — 要件

**プロジェクト**: 藝大 音響教育アプリ（「音」）  
**ワークストリーム**: ホーム／メイン画面デザイン適用  
**作成**: 2026-08-28  
**入力**: `home-ui-requirement-questions.md`（回答済み）＋打ち合わせ資料スクショ  
**ブランチ**: `feature/home-ui-redesign`  
**企画の正**: Google Drive `プロジェクト概要.md`。本ファイルはホーム UI 改修の範囲のみを定義する。

---

## 1. Intent Analysis

| 項目 | 内容 |
|---|---|
| User request | メイン／ホーム画面を打ち合わせデザインに合わせて整備。プレースホルダー画像可 |
| Request type | Enhancement（UI／導線） |
| Scope | `GeidaiHome` 中心。起動シーン変更。`GeidaiGameSelect` 新規。Navigation／HomeMenu 更新 |
| Complexity | Moderate（新シーン1本・起動フロー変更・UI プレハブ刷新） |
| Requirements depth | Standard |

---

## 2. 回答の確定

| Q | 回答 | 確定内容 |
|---|---|---|
| 1 | A | **おとあつめ** → コレクション（`GeidaiCollection`） |
| 2 | A＋プロフィール | ろくおん・お題・設定はホームから**非表示**。設定（ニックネーム編集）は**プロフィールパネル**から Register 編集モードへ |
| 3 | A | プロフィール数値は**ラベルのみ**（値は「—」等プレースホルダー） |
| 4 | C | バッジ内プログレスバーは**装飾のみ**（固定セグメント） |
| 5 | Other | **Boot 画面は使わない**。Build Settings の先頭を **GeidaiHome**。プロフィール未設定時は **自動で Register** へ |
| 6 | A | プレースホルダー PNG は `Assets/Art/Home/Placeholders/`。`iconKey` で参照 |
| 7 | B | **おとあそび** → 新規 **`GeidaiGameSelect`**（`Assets/Scenes/Geidai/`）。`NavigationService` の GameSelect マップを更新 |

矛盾なし。

---

## 3. 機能要件

### FR-HOME-01 ビジュアル（打ち合わせスクショ準拠）

- 背景色: くすんだブルーグレー（目安 `#7A94B8` / RGB 122,148,184）
- メニューボタン: 白・大きく角丸・左アイコン＋右ラベル（縦4段）
- 右上: ニックネーム入りプロフィールバッジ（白・角丸）
- プロフィールパネル: 白・大きく角丸のオーバーレイ。「◯◯ のプロフィール」＋3行ラベル
- 既存 SafeArea / ResponsiveCanvas は維持

### FR-HOME-02 ホームメニュー（4項目）

| Order | ラベル | ModuleId | 遷移先 |
|---|---|---|---|
| 0 | おとあつめ | Collection | GeidaiCollection |
| 1 | おとあそび | GameSelect | GeidaiGameSelect（新規） |
| 2 | おとつくり | Create | GeidaiCreate |
| 3 | おとずかん | Library | GeidaiLibrary |

**非表示**（`HomeMenuConfig` で `visible=false` または項目削除）: Rec, WeeklyTheme, ProfileEdit（独立ボタンとして）

録音・お題シーンは Build Settings に残し、開発／既存画面から到達可能とする（Q2-A）。

### FR-HOME-03 メニューアイコン

- `HomeMenuItem.iconKey` で `Assets/Art/Home/Placeholders/` 内 Sprite を解決
- 初回提供キー例: `gather`（マイク）, `play`（なし／将来）, `create`（鍋）, `library`（本）
- **おとあそび**はスクショどおりアイコンなし可（iconKey 空で非表示）
- Sさんは PNG/Sprite 差し替えのみで見た目更新可能（コード変更不要）

### FR-HOME-04 プロフィールバッジ

- `IStorageService.LoadProfile()` の nickname を表示（未登録時はバッジ非表示または起動ガードで Register へ）
- 装飾プログレスバー: 固定セグメント（黄／灰）。ロジック連動なし（Q4-C）

### FR-HOME-05 プロフィールパネル

タップで表示／閉じる。文言（スクショ準拠）:

1. `{nickname} のプロフィール`
2. いままであつめたおと — 値: `—`
3. いままであつめたポイント — 値: `—`
4. あたらしい音まであと — 値: `—`

**設定へ**: パネル内ボタン（例:「せってい」）→ Register **編集モード**（既存 `UserRegistrationScreenController` の Edit）

### FR-HOME-06 起動フロー（Boot 廃止）

- Editor Build Settings の **index 0 = GeidaiHome**
- `HomeScreenController`（または専用ゲート）が `OnShow` で `StartupRouter.Resolve(LoadProfile())` を実行
  - 未登録（NotFound）→ 警告なしで Register
  - 破損等 → 警告のうえ Register（既存 BR-04）
  - 成功 → ホーム UI を描画
- `Main画面`（Boot）は Build Settings から外すか disabled（起動導線から除外）
- 既存 `BootScreenController` / `StartupRouter` テストは維持。Home 側ゲートの EditMode テストを追加

### FR-HOME-07 ゲーム選択シーン（GeidaiGameSelect）

- `Assets/Scenes/Geidai/GeidaiGameSelect.unity` を新規作成
- 最低限: ①音合わせ（GeidaiGame1）への導線、ホームへ戻る
- 見た目はホームと同系統のプレースホルダー UI（詳細デザインは後続／Sさん）
- `NavigationService`: `SceneId.GameSelect` → `"GeidaiGameSelect"`
- `game_Home.unity` は Build Settings に残してもよいが、本番導線からは切替

### FR-HOME-08 後方互換

- `ModuleId` / `ModuleRouter` / `HomeMenuConfig` のデータ駆動構造は維持
- 非表示項目を再度 visible にすれば旧7ボタン構成に戻せる

---

## 4. 非機能要件

| ID | 内容 |
|---|---|
| NFR-HOME-01 | 縦横両対応・SafeArea 内配置（既存 NFR-11/12 準拠） |
| NFR-HOME-02 | 子ども向け: 大きなタップ領域、平易なひらがなラベル |
| NFR-HOME-03 | PII（nickname）は UI 表示のみ。ログに出力しない（Security 準拠） |
| NFR-HOME-04 | プレースホルダー画像は軽量 PNG（後差し替え前提） |
| NFR-HOME-05 | EditMode: `StartupRouter` 既存＋Home ゲート分岐の単体テスト |

---

## 5. 非目的（本ワークストリーム外）

- ポイント／経験値システムの新規実装
- プロフィール数値の実データ連動（Q3-A）
- 録音・お題をコレクション内に統合（後続 PR）
- Boot 画面のデザイン整備（Q5: 不使用）
- ゲーム選択の全ゲーム一覧デザイン完成（骨組みのみ）

---

## 6. 成果物（Construction 想定）

| 種別 | パス／内容 |
|---|---|
| シーン | `GeidaiHome.unity` 更新、`GeidaiGameSelect.unity` 新規 |
| プレハブ | `HomeMenuButton` 刷新、`HomeProfileBadge` / `HomeProfilePanel` |
| コード | `HomeScreenController` 拡張、`HomeMenuIconResolver`、Home 起動ゲート、Navigation 更新 |
| アセット | `Assets/Art/Home/Placeholders/*.png` |
| 設定 | `HomeMenuConfig_Default.asset` 4項目化 |
| テスト | Home ゲート EditMode |
| ドキュメント | `docs/Sさん向けガイド.md` に Placeholders 追記（任意・短く） |

---

## 7. Extension Compliance

| Extension | 適用 | 備考 |
|---|---|---|
| Security Baseline | Compliant | PII 非ログ、端末内のみ |
| Resiliency Baseline | N/A | オフライン UI のみ |
| Property-Based Testing | N/A | 起動ゲートは決定的分岐テストで十分 |

---

## 8. トレーサビリティ

| 要件 | 既存 US/BR |
|---|---|
| FR-HOME-02 | US-NAV-02, BR-10 |
| FR-HOME-06 | US-NAV-01, BR-01〜04, StartupRouter |
| FR-HOME-03 | US-TECH-07（Sさん アセット編集） |

---

## 9. 受け入れ基準

1. Play 開始 → プロフィールありなら新デザインの GeidaiHome が表示される
2. プロフィールなし → Register に自動遷移（Boot タップ不要）
3. 4ボタンがスクショ構成どおり表示され、各画面へ遷移できる
4. プロフィールバッジタップ → パネル表示。設定 → Register 編集
5. おとあそび → GeidaiGameSelect → Game1 等へ遷移可能
6. EditMode 全件 Pass（既存＋Home ゲート追加）
