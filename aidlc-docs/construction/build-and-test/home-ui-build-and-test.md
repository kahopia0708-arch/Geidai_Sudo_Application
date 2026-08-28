# Build and Test — Home UI 整備

**ブランチ**: `feature/home-ui-redesign`  
**作成**: 2026-08-28

## 1. Unity シーン生成（必須）

コード変更後、**Unity Editor で以下を1回実行**してください（プロジェクトを開いた状態）。

```
メニュー → Geidai → Scenes → Build Home UI (Redesign)
```

これにより以下が生成／更新されます。

- `GeidaiHome.unity`（新デザイン）
- `GeidaiGameSelect.unity`（新規）
- `HomeMenuButton.prefab` / `HomeProfileBadge.prefab` / `HomeProfilePanel.prefab`
- `HomeMenuIconCatalog_Default.asset`
- Build Settings（先頭 = GeidaiHome、Main画面 / game_Home = disabled）

> Unity MCP / batchmode が使えない場合も、このメニュー実行で完了します。

## 2. ビルド

通常どおり Unity Build Settings から Android / iOS。

## 3. EditMode テスト

Test Runner → EditMode → Run All

追加: `HomeStartupGateTests.cs`（3件）

## 4. Play Mode 確認チェックリスト

- [ ] Play 開始 → プロフィール未登録なら Register へ（Boot なし）
- [ ] 登録後 Home → 背景ブルーグレー、4ボタン、右上バッジ
- [ ] おとあつめ → Collection
- [ ] おとあそび → GeidaiGameSelect → ①音合わせ
- [ ] おとつくり / おとずかん → 各シーン
- [ ] バッジ → プロフィールパネル → せってい → Register 編集
- [ ] 端末バック → 終了確認

## 5. 合否

- [x] C# コンパイル（Editor スクリプト含む）
- [x] Unity メニュー `Build Home UI (Redesign)` 実行（2026-08-28 MCP）
- [ ] EditMode 全件 Pass（Test Runner）
- [ ] Play Mode チェックリスト
