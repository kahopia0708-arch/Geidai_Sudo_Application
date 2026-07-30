# U7 Sound Library — NFR Requirements

**ユニット**: U7  
**作成**: 2026-07-30  
**Infrastructure Design**: SKIP（オフライン）

## 要件
| ID | 内容 |
|---|---|
| NFR-U7-01 | 一覧表示は体感即時。50〜100件でスクロール可能 |
| NFR-U7-02 | 試聴開始は体感即時。専用 AudioService 再生 |
| NFR-U7-03 | UnlockState 原子的保存。破損時空フォールバック |
| NFR-U7-04 | 完全オフライン。PII非ログ |
| NFR-U7-05 | UnlockEvaluator / UnlockState JSON を PBT |
| NFR-U7-06 | `Geidai.Library` は Rec/Collection 非依存 |
| NFR-U7-07 | 圧縮後ビルド容量を実測し素材数・圧縮を調整 |

## Tech stack decisions
- Catalog/Rules: ScriptableObject
- UnlockState: JsonUtility + AtomicFile
- UI: uGUI + ScreenRootBase + SafeArea
- ProgressionService: Services 層
- テスト: EditMode + FsCheck 系（既存方針）
