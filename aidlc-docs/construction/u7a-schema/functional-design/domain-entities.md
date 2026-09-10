# U7a — Domain Entities

**ユニット**: U7a Schema & Catalog API  
**作成**: 2026-08-29  
**回答**: Q1〜Q4 すべて A

## 1. 列挙

### LoudnessBand
`None` | `Soft` | `Mid` | `Loud`

### DurationBand
`None` | `Short` | `Mid` | `Long`

## 2. TimbreTagDefinition
| フィールド | 必須 | 説明 |
|---|---|---|
| id | 必須 | 安定 ID（例: bell, drum） |
| displayName | 必須 | 表示名 |
| sortOrder | 任意 | Editor／フィルタ表示順 |

## 3. TimbreTagCatalog（SO）
| メンバー | 説明 |
|---|---|
| tags | `List<TimbreTagDefinition>` |
| FindById / ContainsId | 解決・重複検査 |
| CanRemove(id, catalogSounds) | 参照ゼロなら true（Q1=A） |

既定タグ（サンプル用）: `bell`, `drum`, `environment`, `voice`, `other`

## 4. CuratedSoundDefinition（同一型・フィールド拡張）
| フィールド | 必須 | 説明 |
|---|---|---|
| id | 必須 | 安定素材 ID |
| encyclopediaNumber | 必須 | 図鑑ナンバー（≥1、カタログ内一意） |
| displayName | 必須 | 表示名 |
| reading | 任意 | ふりがな |
| description | 任意 | 説明 |
| imageRef | 任意 | Sprite |
| timbreTagId | 必須 | TimbreTagCatalog の id |
| basePitchMidi | 任意 | 0〜127。未設定は「未定義」扱い（Q2=A）。実装は nullable int または -1 センチネル（FD: **nullable 相当として -1 = unset**） |
| loudnessBand | 任意 | 既定 None |
| durationBand | 任意 | 既定 None |
| pairKey | 任意 | 神経衰弱用 |
| allowPitchShift | 必須 | 既定 true |
| difficultyTags | 任意 | string[] またはカンマ区切り（実装は `string[]`） |
| category | 必須 | 図鑑カテゴリ（絞り込み） |
| initiallyUnlocked | 必須 | 既定 false |
| clipRef | 必須 | AudioClip |

`IsValid`: id / encyclopediaNumber≥1 / displayName / timbreTagId / category / clipRef が充足。timbreTagId の語彙存在は Catalog 検証で担保。

## 5. LibraryItemView（拡張）
既存に加え: `encyclopediaNumber`, `timbreTagId`, `timbreDisplayName`, `image`（Sprite）

## 6. LibraryQuery（純粋）
入力: 定義リスト＋任意フィルタ → ソート済みリスト（副作用なし）
