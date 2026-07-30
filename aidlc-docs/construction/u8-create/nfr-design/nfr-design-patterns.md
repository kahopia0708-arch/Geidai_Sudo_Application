# U8 Sound Create — NFR Design Patterns

**作成**: 2026-07-30

## P1. レシピ優先保存
完成音を常時焼かず、ID＋パラメータで再構成。容量と再編集性を両立。

## P2. 再生時エフェクト
プレビューは AudioFilter / pitch 等で非破壊適用。書き出し時のみレンダリング。

## P3. 原子的I/Oと失敗安全
Recipe/Export とも AtomicFile。失敗時ロールバック。

## P4. 境界
Create は UnlockState と Catalog ID のみ使用。Library 画面アセンブリ非参照。

## P5. 性能
2ボイス同時再生を基準に計測。クリップキャッシュを開始時に確保。
