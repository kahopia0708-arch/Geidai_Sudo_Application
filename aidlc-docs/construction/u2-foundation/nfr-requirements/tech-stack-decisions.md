# U2 Foundation — Tech Stack Decisions（技術選定・差分・根拠）

**ユニット**: U2 Foundation（起動・ホーム・登録・ナビ導線）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Requirements（Part 2）
**決定**: Q1〜Q6＝すべて A（推奨）

> U2 は横断基盤（U1）の技術選定に**原則従う**。本書は U2 固有の追加/差分のみを記す。

---

## 0. U1 から継承（変更なし）
- **エンジン/言語**: Unity 6000.4.2f1 / URP / uGUI / C#。
- **モジュール分割**: Assembly Definition（`Geidai.Foundation` が U2 の主コード。`Geidai.Common`/`Geidai.Services` に依存）。
- **永続化/シリアライズ**: `Application.persistentDataPath` ＋ `JsonUtility`（`profile.json`）。
- **UI 基盤**: `ResponsiveCanvasConfigurator`（1080×1920/Match0.5）＋ `SafeAreaFitter` ＋ `UITheme`。
- **シーン操作/変更管理**: 公式 Unity AI Assistant（Unity MCP Server）＋ Git ブランチ/PR。
- **プラットフォーム/オフライン**: iOS15+/Android8+、完全オフライン。

---

## 1. モジュール / コード配置（U2 差分）
- **新規アセンブリ**: `Geidai.Foundation`（`Assets/Scripts/Foundation/`）。参照＝`Geidai.Common`, `Geidai.Services`, `UnityEngine.UI`。
- **含むクラス**: `BootScreenController` / `HomeScreenController` / `UserRegistrationScreenController`（いずれも U1 `ScreenRootBase` 継承）、ホームメニューのデータ型（`HomeMenuItem`）と設定アセット。
- **根拠**: モジュール境界を保ちつつ U1 基盤を再利用（NFR-08）。

## 2. ナビゲーション（U2 差分・NFR-08/Q6=A）
- **統一方針**: 画面遷移は `INavigationService`/`NavigationService`（U1）経由に一本化。コントローラから直接 `SceneManager` を呼ばない。
- **SceneId 拡張**: `Register`（運用開始）／**`GameSelect` を追加**（既存 `game_Home` に対応）。`Theme` は U5 でシーン整備するまで未登録（要求時は `NotFound` 安全処理）。
- **既存置換**: `SceneSwitcher`/`GoToRec`/`GoToSoundCollection`/`ReturnHomeButton`/`GoToPlace` を U2 コントローラ＋`NavigationService` に置換・除去（`GoToPlace` 削除、Place 大文字小文字バグは導線除外で解消）。
- **根拠**: 保守性・遷移安全（NFR-07/08）、Place 除外（BR）。

## 3. ホームメニュー（データ駆動・NFR-05/08・US-TECH-07）
- **構造**: `HomeMenuItem`（moduleId/label/iconKey/visible/enabled）のリストをデータ（ScriptableObject 等）として保持。
- **調整**: 並び順/ラベル/アイコン/モチーフは `UITheme`＋メニューデータで Sさん がコード改変なく調整。
- **根拠**: 見た目/文言の分離（US-TECH-07）、MVP 外（Place/テスト）の非表示制御。

## 4. 登録 UI（NFR-05/Q2=A）
- **入力**: 生年＝ドロップダウン（TMP_Dropdown 想定／1900〜今年を生成）、ニックネーム＝TMP_InputField（1〜8 文字）。
- **検証**: U1 `ValidationUtil` を再利用（実装追加なし）。
- **通知**: `ErrorPresenter`（U1）で平易表示。
- **根拠**: 子ども向け UX とアクセシビリティ（タップ領域）、実装重複回避。

## 5. 終了確認ダイアログ（NFR-05/Q3=A）
- **方式**: ホームで端末バック（Android の Back）→確認ダイアログ（はい/いいえ・既定=いいえ）。共通の確認 UI を `Geidai.Common`/`UITheme` を用いて実装（軽量な自前ダイアログ、追加パッケージ不要）。
- **根拠**: 誤操作防止（子ども配慮）、追加依存の最小化。

## 6. テスト（NFR-09/Q5=A）
- **PBT**: U2 では **N/A**（新規純粋関数なし）。
- **PlayMode/統合テスト**: `com.unity.test-framework`（PlayMode）で起動判定分岐・登録検証境界・`NotFound` 安全遷移を検証。実行は Build & Test に集約可。
- **追加パッケージ**: なし（FsCheck は U1 で導入済み・U2 追加不要）。
- **根拠**: U2 はフロー/UI 主体で、単体不変条件より結線・分岐の検証が有効。

## 7. 追加パッケージ（U2）
| 目的 | パッケージ | 状態 |
|---|---|---|
| ドロップダウン/入力 | TextMesh Pro（`com.unity.textmeshpro`／uGUI 同梱） | 既存/確認 |
| PlayMode テスト | `com.unity.test-framework` | 既存/確認 |
| （新規追加） | — | なし |

## トレース
`Geidai.Foundation`→NFR-08 / NavigationService 統一・SceneId 拡張→FR-02・NFR-07/08 / ホームデータ駆動→NFR-05・US-TECH-07 / 登録UI→FR-03/04・NFR-05・SECURITY-05 / 終了確認→NFR-05 / PlayMode テスト→NFR-09。
