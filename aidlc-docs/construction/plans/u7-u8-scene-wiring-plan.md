# U7/U8 実シーン配線 Plan

**作成**: 2026-07-30  
**対象**: `GeidaiLibrary` / `GeidaiCreate` / Home 導線 / Build Settings  
**実行指示**: 「2. アプリ画面でテストする前に必要な作業 を進めて」

> 本指示を計画承認として扱い、各ステップ完了時に同じ作業内で `[x]` へ更新する。

## 実行ステップ

- [x] **Step 0** 最新 Source of Truth・既存 `GeidaiSceneBootstrap`・U7/U8 Controller 契約を確認
- [x] **Step 1** `ModuleId` / `ModuleRouter` / HomeMenuConfig を Library・Create 対応へ後方互換拡張
- [x] **Step 2** `GeidaiSceneBootstrap.BuildAll()` に Library/Create 生成を追加
- [x] **Step 3** Library UI（一覧行 Prefab・Catalog/Rules・戻る/停止/Error）を配線
- [x] **Step 4** Create UI（A/B選択・加工・一覧・保存/削除/書出し・Dialog）を配線
- [x] **Step 5** 試聴用 WAV 2件を用意し、既定 CuratedSoundCatalog に割り当て
- [x] **Step 6** Home 導線と Build Settings に `GeidaiLibrary` / `GeidaiCreate` を登録
- [x] **Step 7** Unity MCP で BuildAll 実行、コンパイル Error/Warning 0・シーン/参照/導線を検証
- [x] **Step 8** Build and Test・state・audit を更新してコミット

## 完了条件

- Home から「音図鑑」「音づくり」を開ける
- Library にサンプル2音が表示され、初期解除音だけ再生できる
- Create に解除済み素材が表示され、1〜2音の選択・加工・保存・削除・書出し操作ができる
- `GeidaiLibrary` / `GeidaiCreate` が Build Settings で有効
- Unity Console Error 0 / Warning 0

## Play Mode フィードバック対応（2026-07-30）

- [x] 既定カタログで異なる2音を初期解除し、ミキシング可能にする
- [x] 音づくり入場時に素材A/Bを自動選択し、即時プレビュー可能にする
- [x] プレビュー経路・EditMode全件を再検証する
- [x] audit / summary / state を更新してコミットする

## プレビュー操作フィードバック対応（2026-07-30）

- [x] 「きく・とめる・ほぞん」をスクロール外へ固定表示する
- [x] 素材選択と加工スライダー変更をライブプレビューへ接続する
- [x] 再生中はクリップを再始動せず、音量・ピッチ・リバーブ・音色を更新する
- [x] Create シーン再生成・コンパイル・テスト・記録・コミット
