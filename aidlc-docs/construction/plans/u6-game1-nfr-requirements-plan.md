# U6 Game①音合わせ — NFR Requirements Plan（非機能要件・技術選定 計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U6 Game①音合わせ（最終ユニット）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / NFR Requirements（Part 1: Planning）
**入力**: `../u6-game1/functional-design/*`、`../../inception/requirements/requirements.md`（NFR-01〜12 / FR-15〜19）、U1〜U5 NFR 成果物
**対象NFR**: パフォーマンス/リアルタイム性(NFR-03/06)、ユーザビリティ(NFR-05)、信頼性/堅牢性(NFR-07)、テスト容易性(NFR-09/PBT)、保守性(NFR-08/10)、レスポンシブ/SafeArea(NFR-11/12)、プライバシー(NFR-04＝一時素材・非保存)

> 本ステージで U6 の**非機能目標の具体値**と**技術選定の差分**を確定する。
> **回答方法**: 各質問の `[Answer]:` に記号（例: `A`）。合う選択肢が無ければ「Other」。各質問に「(推奨)」あり。完了で「done」（または「全部推奨で」）。

---

## A. 実行チェックリスト（Part 2 で実行）
- [x] `../u6-game1/nfr-requirements/nfr-requirements.md` を生成（U6 の NFR 目標・受入可能値）
- [x] `../u6-game1/nfr-requirements/tech-stack-decisions.md` を生成（U6 の技術選定差分・根拠）
- [x] 要件（NFR-03/04/05/06/07/08/09/11/12 / FR-15〜19）・ストーリー（US-GAME1-01〜05 / US-TECH-07）とのトレース整合を確認

> **回答**: Q1〜Q6＝すべて A（推奨）。矛盾なし。Part 2 実行済み（2026-07-16）。

## B. 前提（U1〜U5 で確定済み・U6 も踏襲。原則 再質問しない）
- **プラットフォーム**（NFR-01）: iOS 15+ / Android 8.0(API26)+、スマホ〜タブレット、縦横両対応。
- **レスポンシブ**（NFR-11）: `ResponsiveCanvasConfigurator`、参照解像度 1080×1920、Scale With Screen Size、Match=0.5。固定 px 依存を排除。
- **SafeArea**（NFR-12）: `SafeAreaFitter`（`Screen.safeArea` 追従・向き/解像度変更で再計算）。
- **オフライン**（NFR-02）: 外部通信なし。可用性/スケーラビリティ(サーバ)/DR は N/A。
- **フェイルセーフ**（NFR-07）: 失敗は `Result`（理由コード）で表現、クラッシュさせない・フォールバック時は分かりやすい表示。
- **セキュリティ既定**: ログに不要情報を出さない（`SafeLogger`）、本番ビルドで詳細エラー非表示（SECURITY-09）。
- **エンジン/言語**: Unity 6000.4.2f1 / URP / uGUI / C#。**シーン操作は公式 Unity AI Assistant（Unity MCP Server）**（US-TECH-05）。
- **UI ハンドオフ**（US-TECH-07）: 枠組みは前本、意匠/演出は S さん。
- **DI/サービス**: `ServiceRegistry`＋`IStorageService`/`IAudioService`/`INavigationService`/`IContentService`。純粋ロジックは Common へ。
- **音声/ピッチ**: `IAudioService`（再生）＋`PitchMath`（cents↔ratio）。

> ※ 上記の変更が必要な場合のみ、該当質問で Other 指定してください。

## C. スコープ（U6 で確定する非機能の対象）
- **リアルタイムピッチ加工の応答性**（お手本/選択肢のタップ確認・出題生成）。
- **一時素材の扱い**（生成音の非保存・メモリ/GC）。
- **出題生成の決定性・堅牢性**（純粋 QuestionBuilder・保存音0件フォールバック）。
- **テスト戦略**（QuestionBuilder PBT・PitchVariation/Config クランプ単体）。
- **保守性**（新 `Geidai.Game1`・`PitchVariationService` 配置・既存ゲーム選択 UI との接続）。
- **プライバシー**（保存音を素材に使うが生成音は非保存・端末内・外部送信なし）。
- **スコープ外**: 録音/加工（U3）、コレクション（U4）、お題（U5）、②〜⑧ゲーム（将来）。

---

## D. NFR・技術選定に関する質問（Q1〜Q6）

## Question 1（リアルタイムピッチ加工の性能 / NFR-03/06）
U6 の性能・リアルタイム目標は？（タップ確認・出題生成が対象）

A) (推奨) **再生時ピッチ方式**（`AudioSource.pitch = PitchMath.CentsToRatio(cents)`）で **タップ確認の発音開始＝体感即時**（目安 < 0.1s・バッファ再生成なし）。**出題生成（QuestionBuilder）は純粋・O(選択肢数)** で体感遅延なし（目安 < 0.1s／1問）。ゲーム進行は **60fps・最低 30fps を割らない**（ドラッグ追従を含む）。基準音の読込（`LoadSoundBuffer`）はゲーム開始時に一度行いキャッシュ。詳細計測は Build & Test。

B) より厳しく：発音開始 < 0.05s、出題生成 < 0.02s。

C) 具体数値は設定せず「体感で引っかからない」を定性目標にする。

D) Other（[Answer]: の後に記述）

[Answer]:A

## Question 2（一時素材の扱い・メモリ/GC / NFR-06・FR-19）
生成/加工した出題音の扱いは？

A) (推奨) 出題用の加工音は**バッファを作らず再生時ピッチで実現**＝**非保存・低GC**。基準 `AudioBuffer` はゲーム中のみ保持（開始時ロード→終了時解放）。連続タップは現在再生を停止して差し替え（発音の重複回避）。**受入＝加工音ファイルが生成されない・ゲーム中のアロケーションが最小**。

B) 出題ごとに加工済みバッファを生成し、問題終了で破棄（メモリ増だが実装単純）。

C) Other（[Answer]: の後に記述）

[Answer]:A

## Question 3（出題の決定性・堅牢性 / NFR-07・US-GAME1-05）
出題生成の受入基準と素材欠落時の扱いは？

A) (推奨) **純粋 `QuestionBuilder`**＝同一 seed+config で**決定的**。**正解ちょうど1つ・不正解は難易度セント以上離す・選択肢重複なし**。**素材（保存音）0 件**は `SoundMatchConfig.fallbackClip` で成立、無ければ**フォールバック表示**（ろくおんしてね）→ホーム誘導（クラッシュしない）。基準バッファ読込失敗は別素材へフォールバック。**受入＝ (1) 同一seedで同一問題 (2) 常に正解1つ+距離条件 (3) 保存音0件→fallback/フォールバック表示でクラッシュなし**。

B) 出題はランダム（決定性を担保しない）。堅牢性は最小ガードのみ。

C) Other（[Answer]: の後に記述）

[Answer]:A

## Question 4（テスト容易性 / NFR-09・PBT）
U6 の検証方針は？

A) (推奨) **純粋関数に PBT**：`QuestionBuilder.Build(baseId, config, difficulty, seed)`（不変条件＝正解ちょうど1つ・不正解は `centsStep` 以上離れる・選択肢数=config・同一seedで決定的）と、`SoundMatchConfig` の**クランプ**（choiceCount≥2 等）＋`PitchMath`（既存 PBT）を活用。`PitchVariationService` のセント→pitch 換算は `PitchMath` 委譲を単体確認。ドラッグ/演出は EditMode 外（手動/シーン）。実行は Build & Test に集約可。

B) PBT は行わず、単体テストと手動確認のみ。

C) Other（[Answer]: の後に記述）

[Answer]:A

## Question 5（保守性・アセンブリ/配置 / NFR-08・NFR-10）
U6 の実装配置と既存ゲーム選択 UI の扱いは？

A) (推奨) **新規アセンブリ `Geidai.Game1`**（`Game1 → Services → Common`＋`UnityEngine.UI` 一方向）に `SoundMatchGameController`/`ChoiceItemView`/`FrogTargetView`/`ResultEffectController`。**純粋 `QuestionBuilder`・`SoundMatchConfig`(SO)・ゲーム型は `Geidai.Common.Game`**。**`PitchVariationService`（IF＋実装）は `Geidai.Services.Audio`**（`IAudioService` を再生に利用・`ServiceRegistry` 登録）。保存音取得は **`IStorageService`（Collection 非依存）**。既存 brownfield のゲーム選択 UI（`GameListUI`/`GameCardUI`/`StartGameButton`＝Assembly-CSharp）は**残置**し、導線は `NavigationService.GoTo(Game1)`／`ModuleRouter` で接続（実配線は MCP フォローアップ）。

B) `Geidai.Game1` を作らず既存 Assembly-CSharp に実装（アセンブリ増やさない）。

C) Other（[Answer]: の後に記述）

[Answer]:A

## Question 6（プライバシー / NFR-04）
保存音を素材に使う際の扱いは？

A) (推奨) 出題素材は端末内の保存音（`persistentDataPath`）のみを読み、**加工音は非保存・端末外送信なし**（NFR-02 踏襲）。ログに PII（音声パス実体等）を出さない（`SafeLogger`）。ゲーム進行状態（`GameSession`）は**非永続**。**受入＝ネットワーク送信が無いこと・加工音/進行が保存されないこと・PII 非ログ**。

B) Other（[Answer]: の後に記述）

[Answer]:A

---

## E. 完了条件
- Q1〜Q6 に回答 → 曖昧回答は追質問 → nfr-requirements / tech-stack-decisions を生成 → 承認ゲート。
- U1〜U5 の横断決定を踏襲し、U6 固有の差分（リアルタイムピッチ加工の応答性・非保存/一時素材・出題の決定性/PBT・`Geidai.Game1`/`PitchVariationService` 配置・素材プライバシー）のみを明示する。
- 要件（NFR-03〜12 / FR-15〜19）とストーリー（US-GAME1-01〜05 / US-TECH-07）へのトレースが取れている。
