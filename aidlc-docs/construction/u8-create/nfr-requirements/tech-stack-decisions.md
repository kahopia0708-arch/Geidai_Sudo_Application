# U8 Sound Create — Tech Stack Decisions

**作成**: 2026-07-30

| 領域 | 決定 |
|---|---|
| レシピ | JSON（素材ID＋パラメータ） |
| 再生 | 再生時エフェクト適用（非破壊） |
| 書き出し | 明示時のみ WAVE 16bit PCM |
| モジュール | Geidai.Create → Services → Common |
| インフラ | N/A |
