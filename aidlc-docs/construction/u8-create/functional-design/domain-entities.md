# U8 Sound Create — Domain Entities

**作成**: 2026-07-30  
**トレース**: FR-25〜29 / US-CREATE-01〜04

## エンティティ
| 名 | 役割 |
|---|---|
| SoundRecipeId | レシピID |
| SoundRecipeLayer | curatedSoundId + volume/pitch/reverb/timbre |
| SoundRecipe | id, title, createdAtIso, layers[2] |
| RecipePlaybackSpec | 再生用に解決済みレイヤー |
| RecipeExportRequest | 書き出し要求 |

## SoundRecipeLayer フィールド
| フィールド | 説明 |
|---|---|
| curatedSoundId | 参照ID（必須） |
| volume | 0..1 |
| pitchSemitones | 整数範囲（FD確定値、暫定 -12..12） |
| reverb | 0..1 |
| timbre | none/robot/chorus |

## SoundRecipe
- layers はちょうど2、または1〜2（不足は再生時に不足表示）
- 元 AudioClip バイナリは含まない

## 永続化
`persistentDataPath/recipes/{id}.json`  
任意書き出し: `exports/{id}.wav`
