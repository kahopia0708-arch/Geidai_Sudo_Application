# U7b Code Summary — Editor & Library UI

**ユニット**: U7b  
**日時**: 2026-08-29  
**ブランチ**: `feature/sound-library-attributes`

## 1. 概要
コンテンツ担当向け Editor 登録ウィンドウと、プレイヤー図鑑のフィルタ／詳細／HomeUiTheme を実装した。

## 2. Created
| パス | 内容 |
|---|---|
| `Assets/Scripts/Common/Library/LibraryFilterOptions.cs` | フィルタ選択肢・選択維持 |
| `Assets/Editor/CuratedSoundCatalogEditorOps.cs` | WAV インポート／Save／タグ CRUD |
| `Assets/Editor/CuratedSoundCatalogEditorWindow.cs` | `Geidai/Library/Curated Sound Catalog` |
| `Assets/Scripts/Library/LibraryDetailPanel.cs` | 詳細パネル |
| `Assets/Art/Library/Icons/placeholder.png` | 画像プレースホルダー |
| `Assets/Audio/Library/` | WAV 配置フォルダ |
| `Assets/Scripts/Tests/EditMode/LibraryFilterOptionsTests.cs` | EditMode |

## 3. Modified
| パス | 内容 |
|---|---|
| `CuratedSoundCatalog.cs` | `Upsert` |
| `CuratedSoundItemView.cs` | ナンバー／画像／行選択 |
| `CuratedSoundListView.cs` | `ItemSelected`／placeholder |
| `LibraryScreenController.cs` | フィルタ・詳細・Theme・Timbre |
| `Geidai.Library.asmdef` | Foundation 参照 |
| `GeidaiSceneBootstrap.BuildLibrary` | フィルタ／Detail／Timbre／placeholder 配線 |

## 4. 使い方
- Editor: メニュー **Geidai → Library → Curated Sound Catalog**
- シーン再生成: **Geidai → Scenes → Build All Geidai Scenes**（または `BuildLibrary()`）
- Play: ホーム「おとずかん」→ カテゴリ／音色絞込・行選択で詳細・解除済み試聴

## 5. 検証
- Unity コンパイル: Error 0
- EditMode `LibraryFilterOptionsTests` / `CuratedSoundValidationTests`: PASS
- `GeidaiLibrary` シーン再生成済（BuildLibrary）
