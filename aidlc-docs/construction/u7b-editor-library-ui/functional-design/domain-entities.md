# U7b — Domain Entities

**ユニット**: U7b Editor & Library UI  
**作成**: 2026-08-29  
**回答**: Q1〜Q6 すべて A  
**継承**: U7a ドメイン（`CuratedSoundDefinition` / `TimbreTagCatalog` / `LibraryQuery` / `LibraryItemView`）

## 1. Editor セッション（概念）

| 概念 | 説明 |
|---|---|
| `CatalogEditSession` | 対象 `CuratedSoundCatalog` + `TimbreTagCatalog` を保持する Editor 作業状態 |
| `SoundDraft` | フォーム上の編集中定義（未保存可）。保存時に `CuratedSoundValidation.ValidateForUpsert` |
| `TimbreDraft` | 語彙 CRUD 用の一時行 |

永続化先は既存 SO。ランタイムエンティティは増やさない。

## 2. フィルタ状態（プレイヤー）

| フィールド | 型 | 説明 |
|---|---|---|
| `categoryFilter` | string? | null/空 = 「すべて」。値はカタログ由来のユニーク category（Q4=A） |
| `timbreFilter` | string? | null/空 = 「すべて」。値は `TimbreTagCatalog` の id |
| `selectedId` | string? | 詳細パネルに出す選択中の音 id（Q3=A） |

## 3. 表示用拡張（UI 投影）

| 概念 | 説明 |
|---|---|
| `LibraryRowView` | 行: encyclopediaNumber, displayName, image（無ければ placeholder Sprite）, lock, play |
| `LibraryDetailView` | 詳細: description, reading, category, timbreDisplayName（選択時のみ） |
| `PlaceholderSprite` | `Assets/Art/Library/Icons/placeholder` 共有参照（Q5=A） |

## 4. アセット規約

| 種別 | パス |
|---|---|
| AudioClip | `Assets/Audio/Library/{id}.{ext}`（Q2=A） |
| Icons | `Assets/Art/Library/Icons/`（placeholder 含む） |
| Catalog SO | 既存 `Assets/Settings/CuratedSoundCatalog_*.asset` |
| Timbre SO | 既存 `Assets/Settings/TimbreTagCatalog_*.asset` |
