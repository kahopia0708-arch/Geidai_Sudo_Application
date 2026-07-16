# U6 Game①音合わせ — NFR Design Plan（計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U6 Game①音合わせ（最終ユニット）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / NFR Design（Part 1: 計画）
**入力**: `../u6-game1/nfr-requirements/nfr-requirements.md`, `tech-stack-decisions.md`, `../u6-game1/functional-design/*`, U1〜U5 NFR Design 成果物

> 目的: U6 の NFR（リアルタイム性能・非保存/GC・出題の決定性/堅牢性・テスト容易性・保守性/配置）を**設計パターン**と**論理コンポーネント**へ落とし込む。数値は NFR Requirements で確定済み。ここでは「どう実現するか（パターン）」を決める。
> **回答方法**: 各質問の `[Answer]:` に記号（例: `A`）。合う選択肢が無ければ「Other」。各質問に「(推奨)」あり。完了で「done」（または「全部推奨で」）。

---

## A. 実行チェックリスト（Part 2 で実行）
- [x] `../u6-game1/nfr-design/nfr-design-patterns.md` を生成（各 NFR の実現パターン）
- [x] `../u6-game1/nfr-design/logical-components.md` を生成（論理コンポーネント・責務・連携）
- [x] NFR Requirements / Functional Design とのトレース整合を確認

> **回答**: Q1〜Q5＝すべて A（推奨）。矛盾なし。Part 2 実行済み（2026-07-16）。

## B. カテゴリ適用性（このユニットでの判断）
- **Resilience（耐障害）**: 適用（保存音0件フォールバック・素材読込失敗のスキップ・遷移/再生失敗の非致命化）。
- **Performance（性能）**: 適用（再生時ピッチの低遅延・出題生成 O(n)・ドラッグ 60fps・基準音キャッシュ・GC 抑制）。
- **Scalability**: 限定適用（選択肢/出題数は小規模）。サーバスケールは N/A。
- **Security/Privacy**: 適用（保存音は端末内・加工音/進行 非保存・PII 非ログ）。
- **Logical Components**: 適用（QuestionBuilder[純粋]・PitchVariationService・SoundMatchConfig・GameSession・画面/選択肢/演出コントローラ）。

## B-2. U1〜U5 から継承する設計パターン（再質問しない・前提）
- **エラー伝搬**: `Result<T>`。致命的でない失敗はクラッシュさせない・`ErrorPresenter` 通知。
- **UI 基盤**: `ScreenRootBase`＋`ResponsiveCanvasConfigurator`＋`SafeAreaFitter`（表示時/向き変更で再適用）。
- **DI**: `ServiceRegistry`＋インターフェース（`IStorageService`/`IAudioService`/`INavigationService`）。
- **性能/GC**: 純粋計算はアロケーション回避、重い処理は開始時に集約（基準音ロード）。
- **セキュリティ**: 端末外送信なし・`SafeLogger` で非ログ・本番で詳細エラー非表示。
- **テスト**: 純粋関数化＋I/O 抽象化で PBT/モック可能に（FsCheck EditMode）。
- **横断データ配置**: 純粋ロジック・データ型・SO は `Geidai.Common`（Assembly-CSharp 依存回避）。
- **音声/ピッチ**: `IAudioService`（再生）＋`PitchMath`（cents↔ratio・PBT）。

> ※ 上記の変更が必要な場合のみ、該当質問で Other 指定してください。

---

## C. 明確化のための質問（Q1〜Q5）

### Question 1（Performance — リアルタイムピッチ再生パターン）
タップ確認の発音を低遅延・非保存で出す実現パターンは？

A) (推奨) **`PitchVariationService`（`Geidai.Services.Audio`・IF＋実装）** が「基準 `AudioBuffer`＋セント」を受け、**専用再生リグ（`AudioSource`＋`AudioClip`）に `pitch = (float)PitchMath.CentsToRatio(cents)` を設定して再生**。加工済みバッファは作らない（低GC・非保存）。基準 `AudioClip` はゲーム開始時に基準バッファから一度生成しキャッシュ（PCM→`AudioClip.SetData`）。連続タップは `Stop()`→再生で差し替え。`IsPlaying` で完了検知。`ServiceRegistry` 登録（`AudioService` と別サービス）。**受入＝発音開始 体感即時・加工音ファイル非生成**（NFR-U6-01/02）。

B) 出題ごとに加工済み `AudioBuffer`/`AudioClip` を生成し問題終了で破棄（メモリ増）。

C) Other（[Answer]: の後に記述）

[Answer]:A

### Question 2（Performance/Testability — 出題生成の純粋パターン）
決定的で PBT 可能な出題生成の実現は？

A) (推奨) **`QuestionBuilder`（`Geidai.Common.Game`・static 純粋）**：`Build(baseSoundId, SoundMatchConfig config, DifficultyLevel diff, int seed)`。`System.Random(seed)` で `targetCents` と不正解セント（`targetCents ± k*diff.centsStep`・重複なし）を決定 → 正解を含めシャッフル → `Question{ baseSoundId, targetCents, choices, correctIndex }`。副作用なし・O(choiceCount)。**PBT**（正解ちょうど1つ・不正解は `centsStep` 以上・選択肢数=config・同一 seed 決定的）。**受入＝上記不変条件を満たす**（NFR-U6-03/04）。

B) `SoundMatchGameController` 内でランダム生成（純粋分離しない）。

C) Other（[Answer]: の後に記述）

[Answer]:A

### Question 3（Resilience — 素材選択・フォールバック集約）
保存音0件/読込失敗時の安全パターンは？

A) (推奨) **`SoundMatchGameController` の開始処理に集約**：`IStorageService.ListSounds()`→有効素材を選択（seed 可）。`LoadSoundBuffer(id)` 失敗は次候補へ。有効素材が無い場合は `SoundMatchConfig.fallbackClip`（`AudioClip`→基準バッファ）で成立、それも無ければ **`Empty` 状態（フォールバック表示：ろくおんしてね）→ ホーム誘導**（クラッシュしない）。全ての失敗は `Result`＋`ErrorPresenter`。**受入＝0件/失敗でも Empty/フォールバックで継続**（NFR-U6-03）。

B) 素材必須（0件はゲーム開始不可）で最小ガードのみ。

C) Other（[Answer]: の後に記述）

[Answer]:A

### Question 4（Maintainability — アセンブリ配置・データ型・Config）
U6 の論理コンポーネント配置と Config の設計は？

A) (推奨) **新規 `Geidai.Game1`**（`Game1 → Services → Common`＋`UnityEngine.UI` 一方向）に `SoundMatchGameController : ScreenRootBase`／`ChoiceItemView`／`FrogTargetView`／`ResultEffectController`。**純粋 `QuestionBuilder`・`SoundMatchConfig`(SO)・`DifficultyLevel`/`ChoiceSpec`/`Question` は `Geidai.Common.Game`**。**`PitchVariationService`（IF＋実装）は `Geidai.Services.Audio`**。`GameSession` は実行時状態（`Geidai.Game1` or `Common.Game`・非永続）。保存音取得は `IStorageService`（**Collection 非依存**）。`SoundMatchConfig` はインスペクタ注入 or `ContentService` 経由取得（既定アセットは MCP 生成）。**受入＝一方向依存で循環なし・既存資産非破壊**（NFR-U6-05）。

B) `Geidai.Game1` を作らず既存 Assembly-CSharp に実装。

C) Other（[Answer]: の後に記述）

[Answer]:A

### Question 5（Usability/UI — 操作・演出パターン、既存 UI 接続）
タップ確認・ドラッグ解答・演出の実現と既存ゲーム選択 UI 接続は？

A) (推奨) **`ChoiceItemView`** はタップ（確認再生）＋uGUI ドラッグ（`IBeginDragHandler`/`IDragHandler`/`IEndDragHandler`）。**`FrogTargetView`** はタップ（お手本再生）＋ドロップ領域（`RectTransform` 重なり判定）。ドロップで `SoundMatchGameController.OnAnswer(index)`（**純粋判定** `index==correctIndex`）→ **`ResultEffectController`**：正解=カエル進化演出、不正解=やさしい再挑戦（ペナルティなし）、全問終了=結果まとめ。領域外ドロップは元位置復帰。意匠/演出/文言は `UITheme` 準拠で Sさん 調整（US-TECH-07）。既存 brownfield ゲーム選択 UI（Assembly-CSharp）は残置し、①音合わせへは `NavigationService.GoTo(Game1)`／`ModuleRouter` で接続（実配線は MCP フォローアップ）。**受入＝タップ/ドラッグ/判定/演出が分離され、大きく分かりやすい・一方向依存維持**（NFR-U6-05・NFR-05）。

B) ドラッグでなくタップ選択＋決定ボタン（簡素）。

C) Other（[Answer]: の後に記述）

[Answer]:A

---

## D. 完了条件
- Q1〜Q5 に回答 → 矛盾チェック（曖昧回答は追質問）→ nfr-design-patterns.md / logical-components.md を生成 → 承認ゲート。
- U1〜U5 の設計パターンを踏襲し、U6 固有の論理部品（`QuestionBuilder`・`PitchVariationService`・`SoundMatchConfig`・`SoundMatchGameController`/`ChoiceItemView`/`FrogTargetView`/`ResultEffectController`）を明確化する。
- NFR Requirements（NFR-U6-01〜06）・Functional Design（domain-entities/business-logic-model/business-rules/frontend-components）へのトレースが取れている。
