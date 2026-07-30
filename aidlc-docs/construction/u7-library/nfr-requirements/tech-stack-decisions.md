# U7 Sound Library — Tech Stack Decisions

**作成**: 2026-07-30

| 領域 | 決定 | 根拠 |
|---|---|---|
| カタログ | ScriptableObject | 企画・デザインが差し替え容易 |
| 解除状態 | JSON + AtomicFile | U4 堅牢性踏襲 |
| 進行 | ProgressionService | Rec/Game からイベント通知のみ |
| 再生 | IAudioService | 共有実装、非依存 |
| AsmDef | Geidai.Library | 一方向依存 |
| インフラ | N/A | オフライン |
