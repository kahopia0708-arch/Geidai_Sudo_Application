# U7/U8 実シーン配線 Summary

**実施**: 2026-07-30  
**対象**: Sound Library / Sound Create

## 完了内容

- `GeidaiLibrary.unity` を生成し、カタログ・解除ルール・一覧行・試聴/停止・戻る・Error表示を配線
- `GeidaiCreate.unity` を生成し、素材A/B・加工スライダー・レシピ保存/読込/削除・WAVE書出し・確認Dialogを配線
- HomeMenuConfig / ModuleId / ModuleRouter に「おとずかん」「おとづくり」を追加
- 両シーンを Build Settings に有効登録
- 1秒の試聴用 WAV 2件を既定カタログへ割り当て
- 保存レシピを開いた際に素材A/B選択を復元する処理を追加

## 検証結果

- Unity C# コンパイル: Error 0 / Warning 0
- MCP シーン参照検証: 全項目 PASS
- Home 導線・Build Settings: PASS
- 既定音声: 2件とも AudioClip 長さ 1秒
- EditMode: **97 Pass / 0 Fail / 0 Skip**

全件テスト中に FsCheck が `NaN` 入力を生成し、レシピの音量/リバーブが範囲内へ正規化されない不具合を検出した。`RecipeClamp` で音量を1、リバーブを0へ正規化する修正後、全件再実行で合格した。

## 次の手動確認

Unity Play Mode で Main画面から Home を開き、次を確認する。

1. 「おとずかん」からベルとドラムを試聴できる
2. 「おとづくり」で初期選択されたベル＋ドラムを加工・試聴し、保存・再読込・削除・WAVE書出しを操作できる
3. 各画面の「もどる」で Home へ戻る

### Play Mode フィードバック修正（2026-07-30）

- ミキシング確認用に既定のベル・ドラムを両方初期解除へ変更
- 音づくり入場時、素材A=ベル・素材B=ドラムを自動選択
- 2音のオフラインプレビュー生成を検証（WAV 88,244 bytes）
- EditMode 全件再実行: **97 Pass / 0 Fail**

本番カタログ投入時は、初期解除音と進行解除音を改めて設定する。Game1/Rec からの解除通知は別フォローアップ。
