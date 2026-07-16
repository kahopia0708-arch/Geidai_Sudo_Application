# U6 Game①音合わせ — Domain Entities（ドメインモデル）

**ユニット**: U6 Game①音合わせ（最終ユニット）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / Functional Design（Part 2）
**決定**: Q1=A（ListSounds から素材選択）/ Q2=A（純粋 QuestionBuilder＝メタ生成）/ Q3=A（再生時ピッチ・非保存）/ Q4=A（SoundMatchConfig SO）

> 技術非依存の概念モデル。「出題は音そのものでなく**ピッチのメタ**で表現し、発音は再生時に適用（非保存）」が核。

---

## 1. エンティティ一覧

| # | エンティティ | 種別 | 役割 | 配置（想定） |
|---|---|---|---|---|
| 1 | `SoundMatchConfig` | ScriptableObject | 出題数/選択肢数/難易度（セント段階）/fallback | `Geidai.Common.Game` |
| 2 | `DifficultyLevel` | 値オブジェクト（Serializable） | 難易度1段階（名前＋セント間隔） | `Geidai.Common.Game` |
| 3 | `ChoiceSpec` | 値オブジェクト | 選択肢1件のピッチ指定（セント）＋正誤 | `Geidai.Common.Game` |
| 4 | `Question` | 値オブジェクト | 1問（基準音ID・お手本セント・選択肢セント配列・正解index） | `Geidai.Common.Game` |
| 5 | `GameSession` | 実行時状態（POCO） | 進行（現在の問・正解数・全問）・非永続 | `Geidai.Game1` or `Common.Game` |
| 6 | `PitchVariation`（概念） | 再生指定 | 「基準バッファ＋セント」を再生時に適用（非保存） | `Geidai.Services.Audio` |

---

## 2. SoundMatchConfig（出題パラメータ・SO / FR-18）
| フィールド | 型 | 説明 |
|---|---|---|
| `questionCount` | int | 1 ゲームの出題数（クランプ ≥1） |
| `choiceCount` | int | 1 問の選択肢数（クランプ ≥2） |
| `difficulties` | `List<DifficultyLevel>` | 難易度段階（かんたん/ふつう/むずかしい/とても難しい） |
| `fallbackClip` | AudioClip（任意） | 保存音 0 件時の出題素材（無ければフォールバック表示） |

- Sさん がインスペクタで調整可能（データ駆動 / 研究会後にセント値更新・再ビルド不要）。

### DifficultyLevel
| フィールド | 型 | 説明 |
|---|---|---|
| `label` | string | 表示名（例: "ふつう"） |
| `centsStep` | int | 選択肢間の最小ピッチ間隔（セント。例 200/100/50/20） |

## 3. ChoiceSpec（選択肢1件）
| フィールド | 型 | 説明 |
|---|---|---|
| `cents` | int | 基準音からのピッチオフセット（セント） |
| `isCorrect` | bool | お手本と一致する正解か |

## 4. Question（1問 / FR-15/19）
| フィールド | 型 | 説明 |
|---|---|---|
| `baseSoundId` | string | 素材にした保存音の ID（fallback 時は空） |
| `targetCents` | int | お手本（カエル）のピッチ（セント） |
| `choices` | `List<ChoiceSpec>` | 選択肢（おたまじゃくし）のピッチ指定 |
| `correctIndex` | int | `choices` 内の正解 index |

- **不変条件**（BR で規定）: 正解はちょうど 1 つ、不正解は `targetCents` から `centsStep` 以上離れる、同一 seed で決定的。

## 5. GameSession（進行・非永続）
| フィールド | 型 | 説明 |
|---|---|---|
| `questions` | `List<Question>` | 生成済みの全問 |
| `currentIndex` | int | 現在の問番号 |
| `correctCount` | int | 正解数 |
| `isFinished` | bool（導出） | `currentIndex >= questions.Count` |

- 実行時のみ。保存しない（FR-19 の非保存方針と一貫）。

## 6. PitchVariation（再生指定・非保存 / FR-19・Q3=A）
- 概念: 「基準 `AudioBuffer` ＋ セント」を **再生時に `AudioSource.pitch = PitchMath.CentsToRatio(cents)`** で適用。
- 加工済みバッファは**生成・保存しない**（低遅延・低GC）。`PitchVariationService`（`Geidai.Services.Audio`）が担う。

---

## 7. 関係（テキスト表現）
- `SoundMatchConfig`（難易度選択）＋ 保存音（`IStorageService.ListSounds`→基準）→ `QuestionBuilder.Build(baseSoundId, config, difficulty, seed)` → `Question`。
- `Question` → `GameSession` に積む → `SoundMatchGameController` が1問ずつ提示。
- お手本/選択肢の発音 → `PitchVariationService`（基準バッファ＝`IStorageService.LoadSoundBuffer` or `fallbackClip`）＋`IAudioService`。
- 解答（選択 index）→ `correctIndex` と比較 → `GameSession.correctCount` 更新 → `ResultEffectController`。

## 8. 依存・境界
- 一方向：`Geidai.Game1 → Geidai.Services（IStorageService/IAudioService/PitchVariationService/INavigationService）→ Geidai.Common（PitchMath/Result/SavedSound/AudioBuffer/Game 型）`。
- 保存音取得は `IStorageService`（**Collection 非依存**）。生成音は非保存。完全オフライン。
