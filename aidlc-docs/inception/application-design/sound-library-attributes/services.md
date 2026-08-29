# Services — サウンドライブラリ属性

**作成**: 2026-08-29

## オーケストレーション

| サービス | 役割 | 本ワークでの変更 |
|---|---|---|
| `IContentService` | テーマ・図鑑カタログ供給 | `TimbreTagCatalog` の Get/Set 追加 |
| `IProgressionService` | アンロック | 変更なし（定義フィールド増に追随する Project のみ） |
| `IAudioService` | `PlayCuratedClip` | 変更なし |
| `INavigationService` | Home ↔ Library | 変更なし |
| Editor（非ランタイム） | カタログ／語彙書込 | 新規 Window。SO を直接 Dirty/Save |

## パターン
- ランタイムは **読取専用**（カタログはビルド同梱 SO）
- 書込は **Editor のみ**
- 一覧のソート／フィルタは UI または純粋 `LibraryQuery`（Services にビジネスを置かない）
