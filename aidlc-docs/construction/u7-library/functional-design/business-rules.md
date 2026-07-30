# U7 Sound Library — Business Rules

**ユニット**: U7 Sound Library  
**作成**: 2026-07-30

## BR-LIB — カタログ
- **BR-LIB-01**: id と displayName と clipRef が揃わない定義は無効として一覧から除外し得る
- **BR-LIB-02**: カタログ音声は読み取り専用同梱。ユーザー領域へ複製しない
- **BR-LIB-03**: 初期規模は50〜100音を目安とし、実測で調整（NFR-13）

## BR-UNLOCK — 解除
- **BR-UNLOCK-01**: 解除条件は UnlockRulesCatalog のみ。コード埋め込み禁止
- **BR-UNLOCK-02**: Combined は requireAll=true なら全条件、false ならいずれか
- **BR-UNLOCK-03**: 同一イベントの再適用は UnlockState を変更しない（冪等）
- **BR-UNLOCK-04**: initiallyUnlocked=true の素材は起動時に解除済み扱い
- **BR-UNLOCK-05**: 経験値・コイン・ライフ・課金による解除は行わない

## BR-STATE — 永続化
- **BR-STATE-01**: UnlockState は原子的保存
- **BR-STATE-02**: 破損時は空フォールバック
- **BR-STATE-03**: 未知IDは保持しても表示時に無視してよい

## BR-UI — 表示／試聴
- **BR-UI-01**: locked 素材は試聴不可
- **BR-UI-02**: unlocked 素材はタップで試聴
- **BR-UI-03**: オフラインのみ。通信なし

## BR-REF — 参照
- **BR-REF-01**: Create/Game は CuratedSoundId のみで参照
- **BR-REF-02**: Library → Rec/Collection への逆依存を作らない

## Testable Properties (PBT-01)
- Unlock 冪等: apply(apply(state,e),e)=apply(state,e)
- 有効解除IDは常にカタログに存在する集合の部分集合
- UnlockState JSON ラウンドトリップ
