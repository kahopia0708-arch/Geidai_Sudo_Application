# U7a — Tech Stack Decisions

**作成**: 2026-08-29

| 領域 | 選択 | 理由 |
|---|---|---|
| カタログ／語彙 | ScriptableObject | 既存 U7・差し替え容易 |
| ドメイン検証・Query | `Geidai.Common.Library` 純粋 C# | テスト容易・UI 非依存 |
| MIDI 未設定 | `int basePitchMidi` + 定数 `UnsetPitchMidi = -1` | Unity シリアライズ容易 |
| 帯域列挙 | `LoudnessBand` / `DurationBand` | 固定語彙（FD Q3=A） |
| テスト | NUnit + FsCheck（既存） | PBT 方針継続 |
| 新パッケージ | なし | 既存 asmdef に収める |
| ランタイム書込 | なし | Editor（U7b）のみ |
