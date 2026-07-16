# U6 Game①音合わせ — Functional Design Plan（Part 1: 計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U6 Game①音合わせ（最終ユニット）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / Functional Design（Part 1: Planning）
**入力**: `../../inception/user-stories/stories.md`（US-GAME1-01〜05）、`requirements.md`（FR-15〜19 / NFR-03/06/05）、`application-design`（SoundMatchGameController/ChoiceItemView/QuestionBuilder/ResultEffectController/SoundMatchConfig/PitchVariationService）、既存 `PitchMath`・`IAudioService`・`IStorageService`（ListSounds/LoadSoundBuffer）

> 目的: ①音合わせゲーム（出題→タップ確認→ドラッグ解答→正誤判定→カエル進化演出）の機能設計方針を確定する。
> **回答方法**: 各質問の `[Answer]:` に記号（例: `A`）。合う選択肢が無ければ「Other」。各質問に「(推奨)」あり。完了で「done」（または「全部推奨で」）。

---

## A. スコープ（U6 で扱う）
- **出題**: ユーザーの保存音を素材に、リアルタイムでピッチ加工した「お手本音（カエル）」＋複数の「選択肢音（おたまじゃくし）」を提示（FR-15/19）。
- **操作**: 選択肢タップで確認再生、ドラッグでお手本へ解答（FR-15）。
- **判定・演出**: 正誤判定→正解でおたまじゃくし→カエル進化、不正解は再挑戦（ペナルティなし）（FR-17）。
- **パラメータ**: 出題数・選択肢数・難易度（セント段階）を SO でデータ駆動・Sさん 調整（FR-18）。
- **聞き分け要素**: 音高（ピッチ）を主軸に、音色・強弱は拡張余地を残す（FR-16・研究会後確定は暫定）。
- **非保存**: 生成した加工音は保存しない（一時的 / FR-19）。

## B. スコープ外（後続/フォローアップ）
- ②〜⑧ゲーム（将来 UF）、共有/Place。
- 音の長さの扱い・難易度セント値の最終確定（研究会後・暫定値で実装）。
- 実シーン配線・イラスト/演出アニメの作り込み（Sさん・MCP フォローアップ）。
- 音色/強弱の本格 DSP（U6 は拡張余地のみ・ピッチ主軸）。

## C. 前提（U1〜U5 で確定・踏襲）
- 一方向依存 `Geidai.Game1 → Geidai.Services → Geidai.Common`（＋`UnityEngine.UI`）。保存音取得は `IStorageService`（Collection 非依存）。
- `Result<T>`／`ScreenRootBase`＋レスポンシブ/SafeArea／`ErrorPresenter`／`ServiceRegistry` DI／`SafeLogger`／純粋関数化＋PBT。
- 音声は `IAudioService`（再生）＋新 `PitchVariationService`（出題用ピッチ加工・非保存）。ピッチ変換は `PitchMath`（cents↔ratio）。
- データ駆動は ScriptableObject（`SoundMatchConfig`）。UI 意匠は Sさん（US-TECH-07）。

---

## D. 明確化のための質問（Q1〜Q7）

### Question 1（出題の素材選択：どの保存音を使うか / FR-19・US-GAME1-05）
A) (推奨) `IStorageService.ListSounds()` から**1件を選ぶ**（既定はランダム・シード可能）。その音を基準に「お手本」と「選択肢」を生成。**保存音が 0 件**のときは、同梱の**デフォルト出題音（`SoundMatchConfig` に参照する fallback `AudioClip`）**で成立させる（無ければ「ろくおんしてね」フォールバック表示でホーム誘導）。
B) 保存音必須（0 件ならゲーム開始不可・録音へ誘導のみ）。
C) その他（自由記述）。

[Answer]:A

### Question 2（出題ロジックの純粋化 / QuestionBuilder・NFR-09）
A) (推奨) **`QuestionBuilder` を純粋関数**にする：`Question Build(baseSoundId, SoundMatchConfig config, int seed)` が返すのは**音そのものではなくメタ**＝「お手本のピッチ（セント）」「選択肢数ぶんのピッチ（セント）配列」「正解 index」。制約＝(1) 選択肢に**必ず 1 つ正解**（お手本と同一セント）(2) 不正解は**難易度セント以上**離す (3) 同じ seed で決定的。実際の発音は再生時に `PitchVariationService`＋`IAudioService` が適用（非保存）。**PBT 対象**（正解が1つ・距離条件・決定的）。
B) `QuestionBuilder` は MonoBehaviour で音バッファを直接生成（純粋分離しない）。
C) その他（自由記述）。

[Answer]:A

### Question 3（ピッチ加工の実現方式 / PitchVariationService・NFR-03/06）
A) (推奨) **再生時ピッチ**：`PitchVariationService` は「基準バッファ＋セント」を受け、`AudioSource.pitch = PitchMath.CentsToRatio(cents)` を設定して再生する**軽量方式**（バッファを作り直さない＝低遅延・低GC・非保存）。±10/±20 セント程度は長さ変化が僅少で実用的。将来の音色/強弱は拡張余地として IF に残す。
B) 事前リサンプルで**加工済みバッファを生成**（メモリ/CPU 大・非保存でも都度生成）。
C) その他（自由記述）。

[Answer]:A

### Question 4（難易度・出題パラメータ / SoundMatchConfig・FR-18）
A) (推奨) **`SoundMatchConfig`（ScriptableObject）** に `questionCount`（出題数）／`choiceCount`（選択肢数）／`difficulties`（段階：かんたん/ふつう/むずかしい/とても難しい＝**セント間隔** 例 200/100/50/20）／任意 `fallbackClip`（保存音 0 件時）を持ち、Sさん が調整可能（データ駆動）。実行時に選択された難易度のセント値を `QuestionBuilder` へ渡す。値はクランプ（choiceCount≥2 等）。**暫定セント値は研究会後に SO 編集で更新**（再ビルド不要）。
B) パラメータはコードの定数（SO にしない）。
C) その他（自由記述）。

[Answer]:A

### Question 5（操作：タップ確認・ドラッグ解答・判定 / FR-15）
A) (推奨) **`ChoiceItemView`（おたまじゃくし1件）** がタップ＝確認再生、ドラッグ＝uGUI の EventSystem（`IBeginDragHandler`/`IDragHandler`/`IEndDragHandler`）で実装。お手本（カエル）ドロップ領域に重なって離したら解答確定。判定は**純粋**（選択 index == 正解 index）で `SoundMatchGameController` が評価→`ResultEffectController` へ。誤操作/領域外ドロップは元位置へ戻す（やり直し可）。
B) ドラッグではなくタップ選択＋決定ボタンで解答（簡素）。
C) その他（自由記述）。

[Answer]:A

### Question 6（正解演出・不正解時 / ResultEffectController・FR-17）
A) (推奨) **`ResultEffectController`** が正解で「おたまじゃくし→カエル進化」演出（アニメ/パーティクル/効果音のフック）を再生、不正解は**やさしい再挑戦**（過度なペナルティなし・もう一度うながす）。演出アセット・タイミング・文言は Sさん 調整（US-TECH-07）。ロジック（進行/次の問題へ）は演出と分離。全問終了で結果まとめ（正解数）を表示。
B) 演出は最小（テキストのみ）で将来作り込み。
C) その他（自由記述）。

[Answer]:A

### Question 7（アセンブリ配置・既存ゲーム選択 UI の扱い / NFR-08）
A) (推奨) **新規 `Geidai.Game1`**（`Game1 → Services → Common`＋`UnityEngine.UI` 一方向）に `SoundMatchGameController : ScreenRootBase`／`ChoiceItemView`／`ResultEffectController` を配置。**`QuestionBuilder`（純粋）は `Geidai.Common.Game`**、**`PitchVariationService` は `Geidai.Services.Audio`（IF＋実装、`IAudioService` は再生に利用）**、**`SoundMatchConfig`（SO）は `Geidai.Common.Game`**。既存 brownfield のゲーム選択 UI（`GameListUI`/`GameCardUI`/`StartGameButton`＝Assembly-CSharp）は**残置**し、①音合わせへの導線は `NavigationService.GoTo(Game1)`／`ModuleRouter` で接続（実配線は MCP フォローアップ）。旧ゲーム関連の物理削除は行わない。
B) `Geidai.Game1` を作らず既存 Assembly-CSharp に実装（アセンブリ増やさない）。
C) その他（自由記述）。

[Answer]:A

---

## E. Part 2 生成予定物（回答確定後）
- [x] `construction/u6-game1/functional-design/domain-entities.md`（`Question`/`ChoiceSpec`/`SoundMatchConfig`/`GameSession`/`PitchVariation` 概念）
- [x] `construction/u6-game1/functional-design/business-logic-model.md`（開始→出題生成→タップ確認→ドラッグ解答→判定→演出→次問/終了・Mermaid・保存音取得/ピッチ加工経路）
- [x] `construction/u6-game1/functional-design/business-rules.md`（BR-GAME1-xx：素材選択・出題制約[正解1つ/距離]・非保存・難易度クランプ・判定・再挑戦・依存）
- [x] `construction/u6-game1/functional-design/frontend-components.md`（SoundMatchGameController/ChoiceItemView/ResultEffectController の構成・状態・操作・Sさん ハンドオフ点）

> **回答**: Q1〜Q7＝すべて A（推奨）。矛盾なし。Part 2 実行済み（2026-07-16）。

## F. 完了条件（Functional Design）
- 回答（Q1〜Q7）確定・矛盾/曖昧なし。
- 上記 4 成果物を生成し、純粋 QuestionBuilder・非保存ピッチ加工・データ駆動 Config・一方向依存（`Game1→Services→Common`）の方針が明確。
- 要件（FR-15〜19 / NFR-03/05/06）・ストーリー（US-GAME1-01〜05）へのトレースが取れている。
- Part 2 完了後に承認ゲート提示。
