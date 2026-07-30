# U7 Sound Library — Domain Entities

**ユニット**: U7 Sound Library  
**作成**: 2026-07-30  
**トレース**: FR-20〜24 / US-LIB-01〜03

## 1. エンティティ一覧

| エンティティ | 種別 | 役割 | 供給元 |
|---|---|---|---|
| CuratedSoundId | 値（string） | 安定素材ID | カタログ |
| CuratedSoundDefinition | 値オブジェクト | 1音の定義 | CuratedSoundCatalog |
| CuratedSoundCatalog | ScriptableObject | 制作側音一覧 | Assets |
| UnlockConditionKind | 列挙 | GameClear / RecordingChallenge / Combined | ルール |
| UnlockRule | 値オブジェクト | 解除条件1件 | UnlockRulesCatalog |
| UnlockRulesCatalog | ScriptableObject | 解除条件表 | Assets |
| UnlockState | 永続状態 | 解除済みID集合 | unlock-state.json |
| ProgressionEvent | 値オブジェクト | 達成イベント | Rec/Game → ProgressionService |
| LibraryItemView | 表示投影 | ロック状態付き一覧行 | 実行時 |

## 2. CuratedSoundDefinition
| フィールド | 必須 | 説明 |
|---|---|---|
| id | 必須 | CuratedSoundId |
| displayName | 必須 | 表示名 |
| category | 任意 | 分類 |
| description | 任意 | 説明 |
| clipRef | 必須 | 同梱 AudioClip 参照 |
| initiallyUnlocked | 任意 | 初期解除（既定 false） |

## 3. UnlockRule
| フィールド | 説明 |
|---|---|
| soundId | 解除対象 |
| kind | GameClear / RecordingChallenge / Combined |
| gameKey | ステージ／難易度キー（任意） |
| recordingChallengeKey | 録音課題キー（任意） |
| requireAll | Combined 時に全条件必須か |

通貨・経験値・ライフフィールドは持たない。

## 4. UnlockState
| フィールド | 説明 |
|---|---|
| unlockedIds | string[] |
| version | スキーマ版 |

同一IDの再追加は冪等。未知IDは読み飛ばし。

## 5. ProgressionEvent
| フィールド | 説明 |
|---|---|
| type | GameCleared / RecordingSaved |
| key | ステージキー or 課題キー |
| occurredAtIso | 記録時刻（任意） |

## 6. 関係
Catalog ──定義──▶ Definition  
Rules ──対象──▶ Definition.id  
Event ──評価──▶ UnlockEvaluator ──更新──▶ UnlockState  
UnlockState ──投影──▶ LibraryItemView（locked/unlocked）
