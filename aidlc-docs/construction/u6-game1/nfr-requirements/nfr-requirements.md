# U6 Game①音合わせ — NFR Requirements（非機能要件・受入可能値）

**ユニット**: U6 Game①音合わせ（最終ユニット）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / NFR Requirements（Part 2）
**決定**: Q1〜Q6＝すべて A（推奨）
**前提**: U1〜U5 の横断決定（プラットフォーム/レスポンシブ/SafeArea/オフライン/フェイルセーフ/DI/音声）を踏襲。本書は **U6 固有の差分** のみを具体化する。

> 各 NFR は Code Generation／Build & Test の受入基準。トレース: NFR-03〜12 / FR-15〜19 / US-GAME1-01〜05 / US-TECH-07。

---

## NFR-U6-01 パフォーマンス・リアルタイム（NFR-03/06 / Q1=A）
- **タップ確認の発音開始＝体感即時**（目安 < 0.1s）。再生時ピッチ（`AudioSource.pitch = PitchMath.CentsToRatio(cents)`）でバッファ再生成なし。
- **出題生成（`QuestionBuilder`）は純粋・O(選択肢数)**＝体感遅延なし（目安 < 0.1s／1問）。
- **ゲーム進行 60fps・最低 30fps を割らない**（ドラッグ追従含む）。
- 基準音（`LoadSoundBuffer`）は**ゲーム開始時に一度ロードしキャッシュ**。
- **受入**: タップ確認・出題切替に体感遅延がない。ドラッグ中も fps 維持。詳細計測は Build & Test。

## NFR-U6-02 一時素材・メモリ/GC（NFR-06 / FR-19 / Q2=A）
- 出題用の加工音は**バッファを作らず再生時ピッチで実現**＝**非保存・低GC**。
- 基準 `AudioBuffer` は**ゲーム中のみ保持**（開始時ロード→終了時解放）。
- 連続タップは**現在再生を停止して差し替え**（発音重複回避）。
- **受入**: 加工音ファイルが生成されない・ゲーム中のアロケーションが最小。

## NFR-U6-03 出題の決定性・堅牢性（NFR-07 / US-GAME1-05 / Q3=A）
- **純粋 `QuestionBuilder`**：同一 seed+config で**決定的**。**正解ちょうど1つ・不正解は難易度セント以上離す・選択肢重複なし**。
- **素材（保存音）0 件**は `SoundMatchConfig.fallbackClip` で成立、無ければ**フォールバック表示**（ろくおんしてね）→ホーム誘導（クラッシュしない）。基準バッファ読込失敗は別素材へフォールバック。
- **受入**:
  1. 同一 seed で同一問題。
  2. 常に正解1つ＋距離条件を満たす。
  3. 保存音0件→fallback/フォールバック表示でクラッシュなし。

## NFR-U6-04 テスト容易性（NFR-09 / PBT / Q4=A）
- **純粋関数に PBT**：`QuestionBuilder.Build(baseId, config, difficulty, seed)`（不変条件＝正解ちょうど1つ・不正解は `centsStep` 以上・選択肢数=config・決定的）。
- `SoundMatchConfig` の**クランプ**（`choiceCount>=2`/`questionCount>=1`/`centsStep>=1`）を単体。
- `PitchVariationService` のセント→pitch 換算は `PitchMath`（既存 PBT）委譲を単体確認。
- ドラッグ/演出は EditMode 外（手動/シーン）。実行は Build & Test に集約可。
- **受入**: 上記 PBT/単体が PASS。

## NFR-U6-05 保守性・アセンブリ/配置（NFR-08/10 / Q5=A）
- **新規アセンブリ `Geidai.Game1`**（`Game1 → Services → Common`＋`UnityEngine.UI` 一方向）に `SoundMatchGameController`/`ChoiceItemView`/`FrogTargetView`/`ResultEffectController`。
- **純粋 `QuestionBuilder`・`SoundMatchConfig`(SO)・ゲーム型は `Geidai.Common.Game`**。
- **`PitchVariationService`（IF＋実装）は `Geidai.Services.Audio`**（`IAudioService` を再生に利用・`ServiceRegistry` 登録）。
- 保存音取得は **`IStorageService`（Collection 非依存）**。
- 既存 brownfield のゲーム選択 UI（`GameListUI`/`GameCardUI`/`StartGameButton`）は**残置**し、導線は `NavigationService.GoTo(Game1)`／`ModuleRouter` で接続（実配線は MCP フォローアップ）。
- **受入**: 依存が一方向（`Game1→Services→Common`）で循環なし。既存資産に影響を与えない。

## NFR-U6-06 プライバシー（NFR-04 / Q6=A）
- 出題素材は端末内の保存音（`persistentDataPath`）のみを読み、**加工音は非保存・端末外送信なし**（NFR-02 踏襲）。
- ゲーム進行状態（`GameSession`）は**非永続**。ログに PII（音声パス実体等）を出さない（`SafeLogger`）。
- **受入**: ネットワーク送信が無い・加工音/進行が保存されない・PII 非ログ。

## 継続（NFR-01/02/05/11/12）
- iOS 15+/Android 8.0+・縦横両対応（NFR-01）、完全オフライン（NFR-02）、大きく平易な UI（NFR-05）、`ResponsiveCanvasConfigurator`（NFR-11）、`SafeAreaFitter`（NFR-12）を **U1 基盤で充足** し U6 も準拠。

---

## N/A（本ユニット対象外・根拠）
| NFR | 判定 | 根拠 |
|---|---|---|
| NFR-03 可用性/DR（サーバ） | N/A | 完全オフライン・サーバなし（※本ユニットの NFR-03 はリアルタイム性能として NFR-U6-01 に反映） |
| SCAL（サーバ） | N/A | オフライン |

## トレース表
| NFR-U6 | 要件 | ストーリー |
|---|---|---|
| 01 リアルタイム性能 | NFR-03/06 | US-GAME1-01/05 |
| 02 一時素材・GC | NFR-06 | US-GAME1-05 |
| 03 出題の決定性・堅牢性 | NFR-07 | US-GAME1-02/05 |
| 04 テスト容易性 | NFR-09 | US-GAME1-01/04 |
| 05 保守性・配置 | NFR-08/10 | US-GAME1-01・US-TECH-07 |
| 06 プライバシー | NFR-04/NFR-02 | US-GAME1-05 |
