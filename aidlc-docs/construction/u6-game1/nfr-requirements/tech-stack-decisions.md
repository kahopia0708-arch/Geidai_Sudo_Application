# U6 Game①音合わせ — Tech Stack Decisions（技術選定差分・根拠）

**ユニット**: U6 Game①音合わせ（最終ユニット）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / NFR Requirements（Part 2）
**方針**: U1〜U5 で確定した技術スタックを踏襲。本書は **U6 固有の差分** のみを記載。

---

## 1. 継続採用（U1〜U5 で確定・再掲）
| 項目 | 決定 | 備考 |
|---|---|---|
| エンジン/言語 | Unity 6000.4.2f1 / URP / C# | 既存 |
| UI | uGUI＋TextMesh Pro＋EventSystem | ドラッグは EventSystem |
| DI | `ServiceRegistry`＋インターフェース | `IStorageService`/`IAudioService`/`INavigationService` |
| レスポンシブ/SafeArea | `ResponsiveCanvasConfigurator`/`SafeAreaFitter` | U1 基盤 |
| フェイルセーフ | `Result<T>`＋理由コード | クラッシュさせない |
| ロギング | `SafeLogger` | PII/不要情報を出さない |
| 音声/ピッチ | `IAudioService`＋`PitchMath`（cents↔ratio・PBT） | 既存 |
| シーン操作 | 公式 Unity AI Assistant（Unity MCP Server） | US-TECH-05 |

## 2. U6 固有の技術決定

### 2.1 リアルタイムピッチ加工（Q1/Q2 → 再生時 pitch）
- **決定**: `PitchVariationService`（`Geidai.Services.Audio`）が「基準 `AudioBuffer` ＋ セント」を受け、再生用 `AudioSource.pitch = (float)PitchMath.CentsToRatio(cents)` を設定して発音。
- **根拠**: 加工済みバッファを作らず**低遅延・低GC・非保存**（NFR-03/06・FR-19）。±10/±20 セントでは長さ変化が僅少で実用的。
- **拡張余地**: 音色/強弱は将来の IF 拡張として残す（U6 はピッチ主軸）。
- **停止/差替**: 連続タップは現在再生を停止してから再生（発音重複回避）。`IsPlaying` で完了検知。

### 2.2 出題生成（Q3/Q4 → 純粋 QuestionBuilder）
- **決定**: `QuestionBuilder`（`Geidai.Common.Game`・static 純粋）＝`Build(baseSoundId, SoundMatchConfig, DifficultyLevel, int seed)` が `Question`（メタ）を返す。乱数は `seed` から生成（`System.Random(seed)`）で決定的。
- **根拠**: 決定的・副作用なしで **PBT 可能**（正解1つ・距離条件・選択肢数・決定性）。音生成と分離。
- **不変条件**: 正解ちょうど1つ／不正解は `centsStep` 以上離す・重複なし／選択肢数=`config.choiceCount`。

### 2.3 データ駆動パラメータ（Q4 補足 → SoundMatchConfig SO）
- **決定**: `SoundMatchConfig`（ScriptableObject・`Geidai.Common.Game`・`[CreateAssetMenu]`）に questionCount/choiceCount/difficulties/fallbackClip。クランプは読み取り時に適用。
- **根拠**: Sさん がインスペクタで調整可能（FR-18）・研究会後のセント値更新が再ビルド不要。
- **既定アセット**: `Assets/Settings/SoundMatchConfig.asset` を暫定値（かんたん200/ふつう100/むずかしい50/とても難しい20 セント等）で MCP 生成。

### 2.4 素材取得（Q5 → IStorageService・Collection 非依存）
- **決定**: 保存音は `IStorageService.ListSounds()`＋`LoadSoundBuffer(id)` で取得。`Geidai.Game1` は `Geidai.Collection` を参照しない。
- **根拠**: 一方向依存維持・Collection UI から独立。0 件は `fallbackClip`→フォールバック表示。

### 2.5 アセンブリ配置（Q5 → 新 Geidai.Game1）
- **決定**: 新規 `Geidai.Game1`（`Geidai.Common`/`Geidai.Services`/`UnityEngine.UI` 一方向）に UI/コントローラ。純粋・SO・型は `Geidai.Common.Game`、`PitchVariationService` は `Geidai.Services.Audio`。
- **根拠**: モジュール分離・一方向依存（既存パターン踏襲）。
- **既存資産**: brownfield ゲーム選択 UI（Assembly-CSharp）は残置し `NavigationService`/`ModuleRouter` で接続（物理削除しない）。

### 2.6 操作技術（Q5 補足 → uGUI ドラッグ）
- **決定**: `ChoiceItemView` は `IBeginDragHandler`/`IDragHandler`/`IEndDragHandler`＋`FrogTargetView` のドロップ領域判定。タップ確認は `Button`/`IPointerClickHandler`。
- **根拠**: 標準 uGUI で実装し追加依存なし。領域外は元位置復帰でやり直し可。

## 3. テスト技術（Q4）
- **PBT**: 既存 EditMode PBT パターン（FsCheck）で `QuestionBuilder`。
- **単体**: `SoundMatchConfig` クランプ・`PitchVariationService` の cents→pitch 換算（`PitchMath` 委譲）。
- **配置**: `Geidai.Tests`（EditMode）に `Geidai.Game1` 参照を追加。

## 4. リスクと緩和
| リスク | 緩和 |
|---|---|
| セント差が小さいと聞き分け困難 | 難易度 `centsStep` を SO で調整（研究会後に更新） |
| 基準音の長さ/音量ばらつき | 開始時ロード＋（必要なら）正規化は将来。まずは再生時ピッチのみ |
| 保存音 0 件 | `fallbackClip`→フォールバック表示→録音誘導 |
| Assembly-CSharp のゲーム選択 UI との二重管理 | 新ゲーム本体は Geidai.Game1、選択 UI は残置し導線のみ接続（MCP フォローアップ） |

## 5. トレース
Q1/Q2→2.1 ／ Q3→2.2 ／ Q4→2.2/2.3/§3 ／ Q5→2.4/2.5/2.6 ／ Q6→2.4（端末内・非保存）。要件 FR-15〜19・NFR-03〜12、ストーリー US-GAME1-01〜05・US-TECH-05/07 に整合。
