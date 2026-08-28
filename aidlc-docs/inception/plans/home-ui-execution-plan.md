# ホーム UI 整備 — Execution Plan

**承認**: 2026-08-28（Requirements OK）  
**ブランチ**: `feature/home-ui-redesign`

## 実行ステージ

| ステージ | 判定 | 深度 |
|---|---|---|
| User Stories | SKIP | 既存 US-NAV-02 拡張で足りる |
| Application Design | Minimal | UI コンポーネント追加のみ |
| Units Generation | SKIP | U2 Foundation 内改修 |
| Functional Design | SKIP | 要件で十分 |
| NFR Requirements/Design | SKIP | 既存 NFR 準拠 |
| Code Generation | **EXECUTE** | Home + GameSelect + 起動 |
| Build and Test | **EXECUTE** | EditMode + シーン静的検証 |

## 実装順序

1. [x] プレースホルダー PNG + `HomeMenuIconCatalog`
2. [x] `HomeMenuButtonView` / Profile UI コンポーネント
3. [x] `HomeScreenController` 拡張（ゲート・4ボタン・アイコン）
4. [x] `GeidaiGameSelect` + `GameSelectScreenController`
5. [x] `NavigationService` / Build Settings / `HomeMenuConfig`
6. [x] `GeidaiSceneBootstrap.BuildHome` 刷新
7. [x] EditMode テスト

## リスク

| リスク | 対策 |
|---|---|
| シーン YAML 手編集の破損 | Bootstrap 経由で再生成 |
| 日本語フォント | 既存 OS フォント解決を再利用 |
