# U7a Code Summary — Schema & Catalog API

**ユニット**: U7a  
**日時**: 2026-08-29  
**ブランチ**: `feature/sound-library-attributes`

## 1. 概要
制作側同梱音の属性スキーマを拡張し、音色語彙・検証・図鑑クエリ API を追加した。Editor UI / プレイヤー絞り込み UI は U7b。

## 2. 追加・変更ファイル（主なもの）

### Common.Library
| ファイル | 内容 |
|---|---|
| `SoundAttributeBands.cs` | `LoudnessBand` / `DurationBand` |
| `TimbreTagDefinition.cs` / `TimbreTagCatalog.cs` | 制御語彙 SO |
| `CuratedSoundDefinition.cs` | 新必須フィールド・`UnsetPitchMidi`・強化 `IsValid` |
| `CuratedSoundValidation.cs` | Upsert 検証・`CanRemoveTag` |
| `CuratedSoundCatalog.cs` | `ContainsId` / `ContainsEncyclopediaNumber` |
| `LibraryQuery.cs` | 図鑑番号ソート・カテゴリ／音色フィルタ |
| `LibraryItemView.cs` / `UnlockEvaluator.cs` | 投影拡張（timbre・image・number） |

### Services / Library 配線
- `IContentService` / `ContentService`: TimbreTagCatalog Get/Set
- `LibraryBootstrap` / `LibraryScreenController`: 任意注入＋ソート投影

### Settings
- `TimbreTagCatalog_Default.asset`（bell / drum / environment / voice / other）
- `CuratedSoundCatalog_Default.asset` を新スキーマで再登録（demo_bell=1, demo_drum=2）

### Tests
- `CuratedSoundValidationTests.cs`（検証・Filter・Sort・軽量 PBT）
- `UnlockEvaluatorTests` の `Def()` を新必須フィールド対応

## 3. 非対象（U7b）
- Editor WAV→属性登録ウィンドウ
- Library 画面のカテゴリ／音色ドロップダウン UI
- HomeUiTheme 適用・画像アセット本番配置

## 4. 検証
- Unity コンパイル: Error 0（MCP `recompile`）
- EditMode `CuratedSoundValidationTests`: 6/6 PASS
- EditMode `UnlockEvaluatorTests`: 5/5 PASS
