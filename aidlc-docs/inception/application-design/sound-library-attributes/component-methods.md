# Component Methods — サウンドライブラリ属性

**作成**: 2026-08-29  
**注**: 詳細ビジネスルールは Functional Design（per-unit）

## CuratedSoundDefinition
| メンバー | 目的 |
|---|---|
| フィールド群 | id, encyclopediaNumber, displayName, reading, description, imageRef, timbreTagId, basePitchMidi, loudnessBand, durationBand, pairKey, allowPitchShift, difficultyTags, category, initiallyUnlocked, clipRef |
| `IsValid` | 必須（id/name/number/timbre/category/clip）充足 |

## TimbreTagCatalog
| メソッド | 入出力 | 目的 |
|---|---|---|
| `FindById(string)` | id → Def? | 語彙解決 |
| `Add/Update/Remove`（Editor 経由） | Def | 語彙CRUD。Remove は参照チェック |

## CuratedSoundCatalog
| メソッド | 入出力 | 目的 |
|---|---|---|
| `ValidItems()` | → List | 有効定義のみ |
| `FindById(string)` | → Def? | ID 解決 |
| `ContainsId / ContainsNumber` | → bool | 重複検査 |
| `Upsert(Def)`（Editor） | Def → Result | 追加／更新 |

## LibraryQuery（純粋）
| メソッド | 入出力 | 目的 |
|---|---|---|
| `SortByEncyclopediaNumber(items)` | list → list | 昇順 |
| `Filter(items, category?, timbreTagId?)` | → list | 「すべて」は null/empty |

## LibraryItemView
| メソッド | 目的 |
|---|---|
| `From(def, unlocked, tagLabel?, sprite?)` | 投影拡張（number/image/timbre） |

## IContentService
| メソッド | 目的 |
|---|---|
| `GetCuratedCatalog()` | 既存維持 |
| `GetTimbreTagCatalog()`（追加） | 語彙取得。未設定時 NotFound |

## CuratedSoundCatalogEditorWindow
| 操作 | 目的 |
|---|---|
| ImportWav | AudioClip 生成（`Assets/Audio/Library/` 等） |
| EditAttributes / SaveToCatalog | 必須検証・重複拒否 |
| ManageTimbreTags | 語彙 CRUD UI |

## LibraryScreenController
| メソッド | 目的 |
|---|---|
| `Reload()` | カタログ＋Unlock 投影→ソート→フィルタ適用 |
| `OnCategoryChanged` / `OnTimbreChanged` | 絞り込み再適用 |
| `OnPlayRequested` | 解除済みのみ試聴 |
