# U7 Sound Library — NFR Design Patterns

**作成**: 2026-07-30

## P1. データ駆動カタログ
CuratedSoundCatalog / UnlockRulesCatalog を ContentService 経由で供給。コード変更なしで素材・条件を更新。

## P2. 冪等プログレッション
ProgressionService + UnlockEvaluator。同一イベント再適用で状態不変。PBT で担保。

## P3. 原子的 UnlockState
AtomicFile で unlock-state.json を置換。破損時は空。

## P4. モジュール境界
`Geidai.Library → Services → Common`。Create/Game は ID 参照のみ。

## P5. 容量・性能
AudioClip は圧縮設定を実測。一覧は必要時ロード／簡易仮想化余地を残す。
