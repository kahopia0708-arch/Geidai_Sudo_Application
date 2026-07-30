# U8 Sound Create — NFR Requirements

**作成**: 2026-07-30  
**Infrastructure Design**: SKIP

| ID | 内容 |
|---|---|
| NFR-U8-01 | 2音試聴・加工反映は体感遅延少 |
| NFR-U8-02 | レシピ保存 < 体感0.5s、原子的 |
| NFR-U8-03 | 書き出し失敗時クリーンアップ |
| NFR-U8-04 | 低GC。不要なクリップ複製を避ける |
| NFR-U8-05 | Recipe JSON / パラメータクランプ PBT |
| NFR-U8-06 | `Geidai.Create` は Library UI 非依存（UnlockState/IDのみ） |
| NFR-U8-07 | オフライン。PII非ログ |

## Tech decisions
- 再生: AudioService レイヤーAPI拡張
- 保存: recipes/*.json + AtomicFile
- 書き出し: 必要時のみ PCM render → WAV
- UI: uGUI
- AsmDef: Geidai.Create
