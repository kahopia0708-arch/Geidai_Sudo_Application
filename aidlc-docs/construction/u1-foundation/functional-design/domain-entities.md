# U1 基盤 — Domain Entities（ドメインモデル）

**ユニット**: U1 基盤
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Functional Design（Part 2）
**決定**: Q1=B（生年=選択式/ニックネーム1〜8）, Q2=A（GUID＋ファイル）, Q3=A（Effect範囲）, Q4=A（44100/mono/16bit/3s）, Q5=A（6 SceneId）, Q6=A＋（過度なフォールバック禁止・警告）
**性質**: 技術非依存の業務モデル定義（実装型・シリアライズ詳細は Code Generation 段階）。

---

## 1. UserProfile（ユーザー登録）
| フィールド | 型（概念） | 説明 | 制約 |
|---|---|---|---|
| birthYear | 整数 | 生まれた年 | 選択式（ドロップダウン）。1900〜当年。未来年不可（Q1=B） |
| nickname | 文字列 | ニックネーム | 前後空白除去後 1〜8 文字、空不可（Q1=B） |

- 目的: 端末ローカルのユーザー識別（PII、端末外送信禁止 / NFR-04）。
- 永続化: `profile.json`（単一）。

## 2. SoundClipMeta（保存音メタデータ）
| フィールド | 型 | 説明 | 既定 |
|---|---|---|---|
| id | 文字列(GUID) | 一意識別子（Q2=A） | 生成時に採番 |
| createdAt | 日時 | 保存日時 | 保存時刻 |
| title | 文字列 | タイトル | 既定＝日付（FR-10） |
| photoPath | 文字列? | 写真ファイル参照（任意） | なし |
| memo | 文字列? | メモ（任意） | なし |
| nickname | 文字列 | 保存時のニックネーム | UserProfile 由来 |

## 3. SoundEffectSettings（加工設定） / Q3=A
| フィールド | 型 | 範囲 | 既定 |
|---|---|---|---|
| pitchSemitones | 数値 | -12 〜 +12 半音 | 0 |
| noiseReduction | 列挙 | Off / Weak / Medium / Strong | Off |
| timbre | 列挙 | None / Robot / Chorus系 | None |
| reverb | 数値 | 0.0 〜 1.0 | 0.0 |
| bypassPitch / bypassNoise / bypassTimbre / bypassReverb | 真偽 | - | すべて off |

- 音色・ノイズ低減の具体 DSP は Functional/Code 段階で詳細化。数値は暫定（研究会後に更新前提）。

## 4. AudioBuffer（PCM データ）/ Q4=A
| フィールド | 型 | 説明 |
|---|---|---|
| samples | float配列 | PCM サンプル列（モノラル） |
| sampleRate | 整数 | 44100（固定） |
| channels | 整数 | 1（モノラル固定。ステレオ入力はモノラル化） |
| lengthSeconds | 数値 | 3.0（固定） |

- 3秒固定 → サンプル数 = 44100 × 3 = **132,300**。

## 5. SavedSound（保存音の集約）
- 構成: `SoundClipMeta Meta` ＋ `SoundEffectSettings Effects` ＋ `WavPath`（`sounds/{id}.wav`）。
- メタ/設定は `sounds/{id}.meta.json` に対で保存（Q2=A）。
- 片方（wav or meta）欠損時は該当項目を読み飛ばし＋警告（Q6 補足）。

## 6. 列挙・値オブジェクト
- **SceneId**（Q5=A）: `Main, Home, Rec, Collection, WeeklyTheme, SoundMatchGame`（Place 含めない）。
- **NoiseReductionLevel**: `Off, Weak, Medium, Strong`。
- **TimbreType**: `None, Robot, Chorus`。
- **OperationResult / ValidationError**: 成功/失敗と失敗理由（業務ルールは `business-rules.md`）。

## 7. エンティティ関連（概念）
```
UserProfile (1) --- (0..*) SavedSound
SavedSound (1) --- (1) SoundClipMeta
SavedSound (1) --- (1) SoundEffectSettings
SavedSound (1) --- (1) WAVファイル(AudioBuffer由来)
```

### テキスト代替
- UserProfile は 0 個以上の SavedSound を持つ（識別子 nickname で関連）。
- SavedSound は SoundClipMeta・SoundEffectSettings・WAVファイルを各 1 つ持つ。

## トレース
UserProfile→US-REG（U2で利用）/ SoundClipMeta・SoundEffectSettings・SavedSound・AudioBuffer・WavCodec→US-REC/US-COL（U3/U4）/ SceneId→US-NAV・US-TECH-04。
