# Components（コンポーネント定義）

**プロジェクト**: 藝大 須藤さんアプリ
**作成**: 2026-07-15 / AI-DLC Application Design（Part 2）
**方針（application-design-plan.md 承認済み）**: Q1=機能モジュール＋AsmDef / Q2=軽量Manager＋ScriptableObject / Q3=単一StorageService / Q4=NavigationService＋enum / Q5=AudioService集約＋PitchVariationService分離 / Q6=ScriptableObject(+JSON)でデータ駆動 / Q7=共通UI基盤＋Prefabテンプレート
**入力**: `../requirements/requirements.md`、`../user-stories/stories.md`

> 本書は「高レベルの責務・インターフェース」を定義する。詳細な業務ルールは Construction の Functional Design（ユニットごと）で確定する。

---

## モジュール構成（Assembly Definition 単位）

| モジュール | AsmDef（案） | 対応ユニット | 概要 |
|---|---|---|---|
| Common | `Geidai.Common` | 全ユニット基盤 | 共有データモデル・UI基盤・ユーティリティ |
| Services | `Geidai.Services` | 全ユニット基盤 | 横断サービス（App/Navigation/Storage/Audio/Pitch/Content） |
| Foundation | `Geidai.Foundation` | U1 | 起動・ホーム・ユーザー登録・ナビ導線 |
| Rec | `Geidai.Rec` | U2 | 録音・加工・保存 |
| Collection | `Geidai.Collection` | U3 | 一覧・視聴・削除・メタ編集・検索 |
| Theme | `Geidai.Theme` | U4 | weekly theme 表示・Rec導線 |
| Game1 | `Geidai.Game1` | U5 | ①音合わせ |
| Tests | `Geidai.Tests`（EditMode/PlayMode） | 横断 | ユニット/PBT テスト |

依存方向（概略）: `Foundation/Rec/Collection/Theme/Game1 → Services → Common`（逆依存なし）。詳細は `component-dependency.md`。

---

## 1. Common モジュール

### 1.1 データモデル（Data Models）
- **UserProfile**: ユーザー登録情報（生年、ニックネーム）。目的: 端末ローカルのユーザー識別。IF: 値オブジェクト（検証付き）。
- **SoundClipMeta**: 保存音のメタデータ（id、日付、タイトル、写真パス、メモ、ニックネーム）。目的: コレクション表示・検索。
- **SoundEffectSettings**: 加工設定（リバーブ、ノイズリダクション[0/弱/中/強]、ピッチ、音色[なし/ロボット/コーラス系]、各バイパス）。目的: 加工の再現・保存。
- **SavedSound**: 保存音の集約（WAV ファイル参照＋SoundClipMeta＋SoundEffectSettings）。目的: 保存/読込単位。
- **AudioBuffer**: 録音/加工中の PCM データ表現（サンプル列、サンプルレート、チャンネル）。目的: 処理受け渡し。

### 1.2 UI 基盤コンポーネント（Q7）
- **SafeAreaFitter**（MonoBehaviour）: `Screen.safeArea` に追従して RectTransform を調整。責務: NFR-12。全画面ルートに付与。
- **ResponsiveCanvasConfigurator**（MonoBehaviour/設定）: CanvasScaler を Scale With Screen Size に統一し、縦横両対応の参照解像度・Match を適用。責務: NFR-11。
- **ScreenRootBase**（抽象 MonoBehaviour）: 各画面コントローラの基底（表示/非表示、戻る、SafeArea/Responsive 組込）。責務: 画面共通の枠組み。
- **UITheme**（ScriptableObject）: 配色・フォント・アイコン/モチーフ参照（カエル/おたまじゃくし/蓮）。責務: Sさん の見た目調整の受け皿（US-TECH-07）。

### 1.3 ユーティリティ
- **WavCodec**（静的/純粋クラス）: WAVE(16bit PCM) の encode/decode。責務: FR-08、PBT 対象（NFR-09）。
- **PitchMath**（静的/純粋クラス）: cents↔pitch 変換等。責務: FR-18/19、PBT 対象。

---

## 2. Services モジュール（横断サービス）
※ 詳細は `services.md`。ここでは責務の要約のみ。
- **AppManager**: 起動・初期化・グローバルライフサイクル・初回登録判定。
- **NavigationService**: 画面遷移（enum `SceneId`）。Place は無効化（FR-02）。
- **StorageService**: ローカル永続化の一元管理（原子的保存・破損フォールバック / NFR-07）。
- **AudioService**: 録音/再生/加工の統括（`VoiceRecordingSection` に一本化 / FR-07）。
- **PitchVariationService**: ゲーム出題用のリアルタイムピッチ加工（生成音は非保存 / FR-19）。
- **ContentService**: ScriptableObject/JSON のコンテンツ（お題・ゲーム設定・UITheme）提供（Q6）。

---

## 3. Foundation モジュール（U1）
- **BootScreenController**（: ScreenRootBase）: メイン画面。目的: 起動導線、初回は登録へ。IF: `ShowAsync()`。
- **HomeScreenController**（: ScreenRootBase）: ホーム画面。各モジュールへの導線（Place 非表示）。目的: FR-01。
- **UserRegistrationScreenController**（: ScreenRootBase）: 生年・ニックネーム登録/編集、入力検証。目的: FR-03/04、SECURITY-05。

## 4. Rec モジュール（U2）
- **RecScreenController**（: ScreenRootBase）: 録音画面の統括。目的: FR-05/06/08。
- **RecordingController**（MonoBehaviour）: `VoiceRecordingSection`（AudioFilter）をラップし録音/停止/再生。目的: FR-05/07。
- **EffectPanelController**（MonoBehaviour）: 加工UI（リバーブ/ノイズ低減/ピッチ/音色、バイパス）。目的: FR-06。
- **SavePromptController**（MonoBehaviour）: 保存操作（メタ初期値=日付）。目的: FR-08。

## 5. Collection モジュール（U3）
- **CollectionScreenController**（: ScreenRootBase）: 一覧・検索・絞り込みの統括。目的: FR-09/11。
- **SoundListView**（MonoBehaviour）: 保存音リスト表示・再生・削除。目的: FR-09。
- **SoundDetailController**（MonoBehaviour）: メタデータ表示/編集（タイトル/写真/メモ）。目的: FR-10。
- **FilterSearchController**（MonoBehaviour）: 月別絞り込み・キーワード検索。目的: FR-11。

## 6. Theme モジュール（U4）
- **WeeklyThemeController**（: ScreenRootBase / または Home 内パネル）: 今週のお題表示、お題タップで Rec へ。目的: FR-13。
- **ThemeCatalog**（ScriptableObject）: お題データ（週→お題テキスト/オノマトペ）。目的: FR-14、Sさん 差し替え（Q6）。

## 7. Game1 モジュール（U5）
- **SoundMatchGameController**（: ScreenRootBase）: ①音合わせの統括（出題→解答→判定→演出）。目的: FR-15〜19。
- **ChoiceItemView**（MonoBehaviour）: 選択肢（おたまじゃくし）1件。タップ確認・ドラッグ解答。目的: FR-15。
- **QuestionBuilder**（純粋/コンポーネント）: 保存音＋PitchVariationService で出題生成（オリジナル/±セント）。目的: FR-19。
- **ResultEffectController**（MonoBehaviour）: 正解演出（おたまじゃくし→カエル）。目的: FR-17。
- **SoundMatchConfig**（ScriptableObject）: 出題数/選択肢数/難易度（セント段階）。目的: FR-18、Sさん 調整（Q6）。

---

## トレーサビリティ（Component → Story/要件）

| コンポーネント/サービス | 主なストーリー | 要件 |
|---|---|---|
| SafeAreaFitter / ResponsiveCanvasConfigurator / ScreenRootBase / UITheme | US-TECH-01/02/07 | NFR-11/12 |
| NavigationService | US-NAV-01/02, US-TECH-04 | FR-01/02 |
| UserRegistrationScreenController | US-REG-01/02 | FR-03/04 |
| RecordingController / AudioService / EffectPanelController | US-REC-01/02, US-TECH-03 | FR-05/06/07 |
| StorageService / WavCodec / SavePromptController | US-REC-03, US-COL-04, US-TECH-06 | FR-08/12, NFR-07 |
| CollectionScreen/List/Detail/Filter | US-COL-01〜03 | FR-09/10/11 |
| WeeklyThemeController / ThemeCatalog | US-THEME-01〜03 | FR-13/14 |
| SoundMatchGameController / QuestionBuilder / PitchVariationService / SoundMatchConfig | US-GAME1-01〜05 | FR-15〜19 |
| ContentService / UITheme / *Config | US-TECH-07 | §7 UI開発フロー, NFR-05 |
