# U6 Game①音合わせ — Business Rules（業務ルール）

**ユニット**: U6 Game①音合わせ
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / Functional Design（Part 2）

> 各ルールは Code Generation の受け入れ基準。ID は `BR-GAME1-xx`。

---

## 1. 素材選択
- **BR-GAME1-01**: 出題素材は `IStorageService.ListSounds()` から選ぶ（既定ランダム・seed 指定可）。
- **BR-GAME1-02**: 保存音 0 件のときは `SoundMatchConfig.fallbackClip` を素材にする。fallback も無ければ**フォールバック表示**（「ろくおんしてね」等）でホーム誘導し、クラッシュしない。
- **BR-GAME1-03**: 基準バッファ読込に失敗した素材はスキップし、別素材（保存音/fallback）で成立を試みる。

## 2. 出題生成（純粋 QuestionBuilder / FR-15/19）
- **BR-GAME1-11**: `Question.choices` には**正解がちょうど 1 つ**含まれる（`isCorrect==true` が 1 件、`correctIndex` が指す）。
- **BR-GAME1-12**: 不正解の選択肢は `targetCents` から**難易度 `centsStep` 以上**離れる（聞き分け可能性の担保）。選択肢セントは重複しない。
- **BR-GAME1-13**: 同一 `seed`＋同一 `config` で**決定的**に同じ問題を生成する（純粋関数・PBT）。
- **BR-GAME1-14**: 選択肢数は `config.choiceCount`、出題数は `config.questionCount` に一致する（クランプ後）。
- **BR-GAME1-15**: 生成するのは**メタ（セント）のみ**。音バッファは生成しない。

## 3. ピッチ加工・再生（非保存 / FR-19・NFR-03/06）
- **BR-GAME1-21**: 出題用ピッチ加工は**再生時**に `AudioSource.pitch = PitchMath.CentsToRatio(cents)` で適用する。加工済み音声を**生成・保存しない**（一時的）。
- **BR-GAME1-22**: 再生は非破壊（基準バッファを変更しない）。連続タップは現在再生を停止して差し替える。
- **BR-GAME1-23**: モバイルで体感遅延の少ない実用的な処理時間に収める（バッファ再生成を避ける）。

## 4. パラメータ（SoundMatchConfig / FR-18）
- **BR-GAME1-31**: `SoundMatchConfig`（ScriptableObject）で 出題数/選択肢数/難易度（セント段階）/fallback を Sさん が調整可能（データ駆動・再ビルド不要）。
- **BR-GAME1-32**: 異常値はクランプ：`choiceCount >= 2`、`questionCount >= 1`、`centsStep >= 1`。
- **BR-GAME1-33**: 難易度段階（かんたん/ふつう/むずかしい/とても難しい）は暫定セント値で実装し、研究会後に SO 編集で更新する。

## 5. 操作・判定（FR-15）
- **BR-GAME1-41**: 選択肢/お手本はタップで確認再生できる。
- **BR-GAME1-42**: 解答はドラッグでお手本ドロップ領域へ。領域外で離した場合は元位置へ戻す（やり直し可）。
- **BR-GAME1-43**: 判定は純粋（`chosenIndex == correctIndex`）。副作用（集計・演出）は Controller が行う。

## 6. 演出・進行（FR-17）
- **BR-GAME1-51**: 正解時は「おたまじゃくし→カエル進化」演出を再生する。演出アセット/タイミング/文言は Sさん 調整（US-TECH-07）。
- **BR-GAME1-52**: 不正解は過度なペナルティなしで再挑戦できる。
- **BR-GAME1-53**: 全問終了で結果（正解数）を表示し、「もう一度」/「ホーム」を選べる。

## 7. 依存・アーキテクチャ
- **BR-GAME1-61**: 依存は一方向 `Geidai.Game1 → Geidai.Services → Geidai.Common`。保存音取得は `IStorageService`（**Collection 非依存**）。
- **BR-GAME1-62**: 純粋 `QuestionBuilder`・`SoundMatchConfig`(SO) は `Geidai.Common.Game`、`PitchVariationService` は `Geidai.Services.Audio` に配置。
- **BR-GAME1-63**: 既存 brownfield のゲーム選択 UI（`GameListUI`/`GameCardUI`/`StartGameButton`）は残置し、①音合わせへの導線は `NavigationService`/`ModuleRouter` で接続（実配線は MCP フォローアップ）。

## 8. トレース
US-GAME1-01→BR-GAME1-11/41/42/43 ／ US-GAME1-02→BR-GAME1-12（ピッチ主軸・拡張余地） ／ US-GAME1-03→BR-GAME1-51〜53 ／ US-GAME1-04→BR-GAME1-31〜33 ／ US-GAME1-05→BR-GAME1-01〜03/15/21。FR-15〜19・NFR-03/05/06 に整合。
