# U8 Sound Create — Business Rules

**作成**: 2026-07-30

## BR-CREATE
- **BR-CREATE-01**: 選択できるのは UnlockState 上の素材のみ
- **BR-CREATE-02**: MVP の同時レイヤーは最大2
- **BR-CREATE-03**: 保存はレシピのみ。同梱音を複製しない
- **BR-CREATE-04**: レシピJSONは原子的保存
- **BR-CREATE-05**: 書き出しは明示操作時のみ
- **BR-CREATE-06**: 書き出し失敗で不完全ファイルを残さない
- **BR-CREATE-07**: 欠損参照はクラッシュせず不足を示す
- **BR-CREATE-08**: Create → Rec/Collection への依存を作らない
- **BR-CREATE-09**: 加工範囲の最終数値は実装前にクランプ表で固定（暫定: volume 0..1, pitch -12..12, reverb 0..1）

## PBT
- Recipe JSON ラウンドトリップ
- レイヤー数不変（保存前後）
- クランプ後パラメータが範囲内
