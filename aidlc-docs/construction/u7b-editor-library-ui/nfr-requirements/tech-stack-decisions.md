# U7b — Tech Stack Decisions

**作成**: 2026-08-29  
**回答**: Q1〜Q4 = 全 A

| 領域 | 選択 | 理由 |
|---|---|---|
| Editor UI | `EditorWindow` + IMGUI（`EditorGUILayout`） | 既存 Editor ツールと同型・追加パッケージ不要（Q2=A） |
| ランタイム UI | uGUI + `HomeUiTheme` / `UiFontResolver` | ホーム／設定と基調統一 |
| 一覧 | ScrollRect + 既存 List/Item View 拡張 | 100 件想定で仮想化不要（Q1=A） |
| 検証 | U7a `CuratedSoundValidation` / `CanRemoveTag` | 単一の正。Editor は呼び出すだけ |
| アセット I/O | `AssetDatabase` / `FileUtil`（Editor のみ） | WAV→`Assets/Audio/Library/{id}` |
| テスト | NUnit EditMode（＋必要時 FsCheck） | Q3=A。EditorWindow 自動操作は必須にしない |
| 新パッケージ | なし | UI Toolkit 未導入 |
| Infrastructure | SKIP | オフライン完結 |
