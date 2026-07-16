# U3 Rec — Domain Entities（ドメインモデル）

**ユニット**: U3 Rec（録音・加工・保存）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Functional Design（Part 2）
**決定**: Q1〜Q7＝すべて A（推奨）
**対応**: US-REC-01/02/03, US-TECH-03 / FR-05/06/07/08 / NFR-03/06/08 / SECURITY-15

> 本書は**技術非依存のドメインモデル**を定義する。JsonUtility 対応の具体構造・DSP フィルタ値・GC 対策は NFR Design / Code Generation で扱う。
> U1 で確定済みのモデル（`AudioBuffer`・`SoundEffectSettingsData`・`SoundClipMeta`・`SavedSound`）を**再利用**し、U3 では録音・加工・保存の**状態モデル**と**既存 `SoundEffectSettings` との対応関係**を定義する。

---

## 1. 再利用する U1 モデル（変更なし・参照のみ）

| モデル | 場所 | U3 での役割 |
|---|---|---|
| `AudioBuffer` | `Geidai.Common.Models` | 録音結果の生サンプル（44100Hz / モノラル / 3秒＝132300サンプル）。録音停止時に生成、保存時に `WavCodec` でエンコード。 |
| `SoundEffectSettingsData` | `Geidai.Common.Models` | 加工設定の**正準モデル**（Q2=A）。pitchSemitones ±12 / NoiseLevel(None/Low/Medium/High) / TimbreType(Original/Soft/Hard) / reverb 0〜1。 |
| `NoiseLevel` (enum) | `Geidai.Common.Models` | ノイズ低減 4段（0/弱/中/強）。 |
| `TimbreType` (enum) | `Geidai.Common.Models` | 音色 3種。UI「なし/ロボット/コーラス系」をマッピング（§4）。 |
| `SoundClipMeta` | `Geidai.Common.Models` | 保存音メタ（id=GUID / displayName / createdAtIso / wavFileName）。`CreateNew(displayName)` で採番。 |
| `SavedSound` | `Geidai.Common.Models` | `SoundClipMeta` ＋ `SoundEffectSettingsData` の対（BR-05）。保存の集約単位。 |
| `Result` / `Result<T>` | `Geidai.Common.Results` | 録音/保存/再生の成否表現（フェイルセーフ）。 |

---

## 2. U3 で新規に定義するドメイン概念（状態モデル）

### 2.1 RecordingState（録音セッションの状態・enum）
Rec 画面の中核状態。UI 表示と操作可否を規定する。

| 状態 | 意味 | 主な許可操作 |
|---|---|---|
| `Idle` | 初期／録音前 | 録音開始 |
| `NoMic` | マイク不在・権限拒否 | （録音不可）案内表示・再試行 |
| `Recording` | 録音中（3秒カウントダウン） | （自動停止待ち。手動停止は Q1=A では不要） |
| `Recorded` | 録音済み・プレビュー可 | 再生 / 加工調整 / 保存 / 録り直し |
| `Playing` | プレビュー再生中 | 停止 / （加工調整は継続可） |
| `Saving` | 保存処理中 | （待機） |
| `Saved` | 保存完了 | ホームへ / 続けて録り直し |

> 状態遷移の詳細は `business-logic-model.md` §2。

### 2.2 RecordingSession（録音セッション・値の集合）
1回の「録音→加工→保存」の作業単位を表す概念（実装は RecScreenController 内の状態として保持）。

| フィールド（概念） | 型（概念） | 説明 |
|---|---|---|
| `state` | RecordingState | 現在状態 |
| `buffer` | AudioBuffer? | 録音済みサンプル（`Recorded` 以降で有効） |
| `settings` | SoundEffectSettingsData | 現在の加工設定（初期＝既定値） |
| `hasUnsavedRecording` | bool | 未保存の録音があるか（離脱時の破棄確認・Q7=A） |
| `elapsedSeconds` | float | 録音経過（0→3・カウントダウン表示用） |

### 2.3 MicPermissionStatus（マイク権限状態・enum）
権限フェイルセーフ（Q4=A / SECURITY-15）の判定に用いる概念。

| 値 | 意味 |
|---|---|
| `Unknown` | 未確認（初期） |
| `Granted` | 許可済み（録音可） |
| `Denied` | 拒否（録音不可・案内） |
| `NoDevice` | マイクデバイス無し（録音不可・案内） |

### 2.4 EffectKind（加工種別・enum／バイパス管理）
バイパス（on/off）と UI パネルの単位（Q2=A・Q7=A）。

| 値 | 対応設定 | バイパス既定 |
|---|---|---|
| `Pitch` | pitchSemitones | on（有効） |
| `NoiseReduction` | noiseLevel | on |
| `Timbre` | timbre | on |
| `Reverb` | reverb | on |

> 「全体バイパス（allEffectsEnabled 相当）」は UI 上の一括 on/off として `frontend-components.md` に記載（保存モデルには持たせない）。

---

## 3. 保存物の構造（新形式・U1 と統一 / Q5=A）

保存は `SavedSound`（メタ＋設定）と `AudioBuffer`（音）を対で永続化する。

```
persistentDataPath/
└── sounds/
    ├── {id}.wav        ← AudioBuffer を WavCodec でエンコード（16bit PCM / 44100 / mono）
    └── {id}.meta.json  ← SavedSound（SoundClipMeta + SoundEffectSettingsData）
```

- `id` は `SoundClipMeta.CreateNew` の GUID（BR-04 / U1）。
- **非破壊**（Q3=A）: `.wav` は**録音そのまま（生）**。加工は `.meta.json` の `settings` として保存し、**再生時に再適用**する。
- 旧形式（`persistentDataPath/MySoundCollection/sound_*.wav` ＋ `.json` / グローバル `SoundEffectSettings`）は**廃止**し新形式へ統一。旧データ移行は U4 or 対象外。

---

## 4. 旧 `SoundEffectSettings`（グローバル）との対応関係

既存 `VoiceRecordingSection` / `SoundEffectSettings` の詳細パラメータを、正準モデル `SoundEffectSettingsData`（Q2=A）へ集約する対応表。**echo/distortion/lowPass/highPass は「音色プリセット」の内部実装**として畳み込み、保存モデルには出さない。

| 旧 `SoundEffectSettings` | 新 `SoundEffectSettingsData` | 変換方針（概念） |
|---|---|---|
| `pitchCents`（セント） | `pitchSemitones`（半音 ±12） | 100 セント = 1 半音（境界丸めルールは BR-REC-04）。UI は半音、内部再生は PitchMath でセント/比率換算。 |
| `noiseReductionAmount`（0〜1 連続） | `noiseLevel`（None/Low/Medium/High） | 4段の離散へマッピング（0=None / ~0.33=Low / ~0.66=Medium / 1=High）。 |
| `tonePresetIndex`（0/1/2）＋ `lowPassCutoff`/`highPassCutoff`/`distortionLevel` | `timbre`（Original/Soft/Hard） | 音色プリセット＝内部で lowpass/highpass/distortion を設定。UI「なし/ロボット/コーラス系」→ Original/Hard/Soft（命名は S さん調整可）。 |
| `reverbLevel`（-10000〜0 mB） | `reverb`（0〜1 正規化） | 0=なし〜1=最大。内部で AudioReverbFilter のレベルに換算。 |
| `echoDelay`/`echoDecayRatio` | （保存対象外） | MVP では音色プリセットに内包 or 不使用（BR-REC-05）。 |
| `displayName`/`wavFileName` | `SoundClipMeta.displayName`/`wavFileName` | メタ側へ移譲。 |

> UI 表示レンジ・内部フィルタ値・PitchMath 換算の**具体数値は NFR Design / Code Generation** で確定（技術非依存の原則）。

---

## 5. トレーサビリティ

| モデル/概念 | ストーリー | 要件 |
|---|---|---|
| AudioBuffer / RecordingState（3秒・自動停止） | US-REC-01 | FR-05 / NFR-03 |
| SoundEffectSettingsData / EffectKind（バイパス） | US-REC-02 | FR-06 / NFR-06 |
| SavedSound / 新形式保存（非破壊・対保存） | US-REC-03 | FR-08 / NFR-07 / SECURITY-15 |
| 旧→新モデル対応（一本化） | US-TECH-03 | FR-07 / NFR-08 |
| MicPermissionStatus（フェイルセーフ） | US-REC-01(AC3) | SECURITY-15 |
