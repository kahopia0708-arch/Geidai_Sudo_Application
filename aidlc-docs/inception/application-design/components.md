# Components（コンポーネント定義）

**プロジェクト**: 藝大 音響教育アプリ
**作成**: 2026-07-15 / AI-DLC Application Design（Part 2）
**更新**: 2026-07-30 / フェーズC（音図鑑・音づくり）差分
**方針（application-design-plan.md 承認済み）**: Q1=機能モジュール＋AsmDef / Q2=軽量Manager＋ScriptableObject / Q3=単一StorageService / Q4=NavigationService＋enum / Q5=AudioService集約＋PitchVariationService分離 / Q6=ScriptableObject(+JSON)でデータ駆動 / Q7=共通UI基盤＋Prefabテンプレート
**入力**: `../requirements/requirements.md`、`../user-stories/stories.md`

> 本書は「高レベルの責務・インターフェース」を定義する。詳細な業務ルールは Construction の Functional Design（ユニットごと）で確定する。
> **プライバシー**: 役割名で記述し、個人名・個人予定は記載しない。

---

## モジュール構成（Assembly Definition 単位）

| モジュール | AsmDef（案） | 対応ユニット | 概要 |
|---|---|---|---|
| Common | `Geidai.Common` | 全ユニット基盤 | 共有データモデル・UI基盤・ユーティリティ |
| Services | `Geidai.Services` | 全ユニット基盤 | 横断サービス（App/Navigation/Storage/Audio/Pitch/Content/Progression） |
| Foundation | `Geidai.Foundation` | U2 | 起動・ホーム・ユーザー登録・ナビ導線 |
| Rec | `Geidai.Rec` | U3 | 録音・加工・保存 |
| Collection | `Geidai.Collection` | U4 | 一覧・視聴・削除・メタ編集・検索 |
| Theme | `Geidai.Theme` | U5 | weekly theme 表示・Rec導線 |
| Game1 | `Geidai.Game1` | U6 | ①音合わせ |
| Library | `Geidai.Library` | U7 | 音図鑑・閲覧・試聴・アンロック表示 |
| Create | `Geidai.Create` | U8 | 2音の音づくり・レシピ編集・書き出し |
| Tests | `Geidai.Tests`（EditMode/PlayMode） | 横断 | ユニット/PBT テスト |

依存方向（概略）: `Foundation/Rec/Collection/Theme/Game1/Library/Create → Services → Common`（逆依存なし）。詳細は `component-dependency.md`。

---

## 1. Common モジュール

### 1.1 データモデル（Data Models）
- **UserProfile**: ユーザー登録情報（生年、ニックネーム）。目的: 端末ローカルのユーザー識別。IF: 値オブジェクト（検証付き）。
- **SoundClipMeta**: 保存音のメタデータ（id、日付、タイトル、写真パス、メモ、ニックネーム）。目的: コレクション表示・検索。
- **SoundEffectSettings**: 加工設定（リバーブ、ノイズリダクション[0/弱/中/強]、ピッチ、音色[なし/ロボット/コーラス系]、各バイパス）。目的: 加工の再現・保存。
- **SavedSound**: 保存音の集約（WAV ファイル参照＋SoundClipMeta＋SoundEffectSettings）。目的: 保存/読込単位。
- **AudioBuffer**: 録音/加工中の PCM データ表現（サンプル列、サンプルレート、チャンネル）。目的: 処理受け渡し。
- **CuratedSoundId**: 制作側音素材の安定ID。目的: カタログ／アンロック／レシピ／ゲームの共通参照（FR-20/24）。
- **CuratedSoundDefinition**: 表示名・分類・説明・AudioClip参照・初期ロック状態。目的: 同梱カタログ項目。
- **UnlockRule**: 解除条件（ゲーム達成／録音課題／複合）。目的: データ駆動の進行条件（FR-22）。経験値・通貨・ライフは持たない。
- **UnlockState**: 端末ローカルの解除済み素材ID集合。目的: 再起動後も維持・冪等更新（FR-23）。
- **SoundRecipeLayer**: 素材ID＋レイヤー加工パラメータ（音量・ピッチ・リバーブ・音色）。目的: レシピの1層。
- **SoundRecipe**: 2レイヤーのレシピ集約（id、表示名、作成日時、layers）。目的: 再編集可能な保存単位（FR-27）。元同梱音声は複製しない。

### 1.2 UI 基盤コンポーネント（Q7）
- **SafeAreaFitter**（MonoBehaviour）: `Screen.safeArea` に追従して RectTransform を調整。責務: NFR-12。全画面ルートに付与。
- **ResponsiveCanvasConfigurator**（MonoBehaviour/設定）: CanvasScaler を Scale With Screen Size に統一し、縦横両対応の参照解像度・Match を適用。責務: NFR-11。
- **ScreenRootBase**（抽象 MonoBehaviour）: 各画面コントローラの基底（表示/非表示、戻る、SafeArea/Responsive 組込）。責務: 画面共通の枠組み。
- **UITheme**（ScriptableObject）: 配色・フォント・アイコン/モチーフ参照（カエル/おたまじゃくし/蓮）。責務: 企画・デザイン担当の見た目調整の受け皿（US-TECH-07）。

### 1.3 ユーティリティ
- **WavCodec**（静的/純粋クラス）: WAVE(16bit PCM) の encode/decode。責務: FR-08/28、PBT 対象（NFR-09）。
- **PitchMath**（静的/純粋クラス）: cents↔pitch 変換等。責務: FR-18/19、PBT 対象。
- **UnlockEvaluator**（純粋クラス）: 達成イベントと UnlockRule から解除結果を決定。冪等・決定的。PBT 対象（NFR-09/14）。

---

## 2. Services モジュール（横断サービス）
※ 詳細は `services.md`。ここでは責務の要約のみ。
- **AppManager**: 起動・初期化・グローバルライフサイクル・初回登録判定。
- **NavigationService**: 画面遷移（enum `SceneId`）。Place は無効化（FR-02）。Library / Create を追加。
- **StorageService**: ローカル永続化の一元管理（原子的保存・破損フォールバック / NFR-07）。UnlockState・SoundRecipe も扱う。
- **AudioService**: 録音/再生/加工の統括。レイヤー再生・レシピ再生／書き出しの受け口を拡張。
- **PitchVariationService**: 出題用リアルタイムピッチ加工（生成音は非保存 / FR-19）。
- **ContentService**: お題/ゲーム設定/UITheme/音図鑑カタログ/解除条件表の提供（Q6）。
- **ProgressionService**: ゲーム達成・録音課題イベントを受け、UnlockEvaluator＋Storage で解除状態を更新（FR-22/23）。

---

## 3. Foundation モジュール（U2）
- **BootScreenController**（: ScreenRootBase）: メイン画面。目的: 起動導線、初回は登録へ。IF: `ShowAsync()`。
- **HomeScreenController**（: ScreenRootBase）: ホーム画面。各モジュールへの導線（Place 非表示、Library/Create 追加可）。目的: FR-01。
- **UserRegistrationScreenController**（: ScreenRootBase）: 生年・ニックネーム登録/編集、入力検証。目的: FR-03/04、SECURITY-05。

## 4. Rec モジュール（U3）
- **RecScreenController**（: ScreenRootBase）: 録音画面の統括。目的: FR-05/06/08。
- **RecordingController**（MonoBehaviour）: 録音/停止/再生。目的: FR-05/07。
- **EffectPanelController**（MonoBehaviour）: 加工UI。目的: FR-06。
- **SavePromptController**（MonoBehaviour）: 保存操作。目的: FR-08。保存成功時に録音課題イベントを ProgressionService へ通知可能。

## 5. Collection モジュール（U4）
- **CollectionScreenController**（: ScreenRootBase）: 一覧・検索・絞り込みの統括。目的: FR-09/11。
- **SoundListView**（MonoBehaviour）: 保存音リスト表示・再生・削除。目的: FR-09。
- **SoundDetailController**（MonoBehaviour）: メタデータ表示/編集。目的: FR-10。
- **FilterSearchController**（MonoBehaviour）: 月別絞り込み・キーワード検索。目的: FR-11。

## 6. Theme モジュール（U5）
- **WeeklyThemeController**（: ScreenRootBase / または Home 内パネル）: 今週のお題表示、お題タップで Rec へ。目的: FR-13。
- **ThemeCatalog**（ScriptableObject）: お題データ。目的: FR-14、企画・デザイン差し替え（Q6）。

## 7. Game1 モジュール（U6）
- **SoundMatchGameController**（: ScreenRootBase）: ①音合わせの統括。目的: FR-15〜19。クリア時に ProgressionService へ達成イベント。
- **ChoiceItemView** / **FrogTargetView** / **QuestionBuilder** / **ResultEffectController** / **SoundMatchConfig**: 既存どおり。

## 8. Library モジュール（U7） `[C]`
- **LibraryScreenController**（: ScreenRootBase）: 音図鑑一覧・ロック表示・試聴。目的: US-LIB-01/02、FR-20〜23。
- **CuratedSoundCatalog**（ScriptableObject）: 制作側音素材定義の一覧（50〜100音目安）。目的: FR-20/21、NFR-13。
- **UnlockRulesCatalog**（ScriptableObject）: 解除条件表。目的: FR-22。
- **CuratedSoundListView**（MonoBehaviour）: 分類・ロック状態付きリスト。
- **CuratedSoundItemView**（MonoBehaviour）: 1件の表示・試聴・ロック表示。

## 9. Create モジュール（U8） `[C]`
- **CreateScreenController**（: ScreenRootBase）: 2音選択・加工・試聴・レシピ保存・書き出し。目的: US-CREATE-01〜04、FR-25〜29。
- **RecipeLayerPicker**（MonoBehaviour）: アンロック済み素材からレイヤー選択。
- **RecipeEffectPanel**（MonoBehaviour）: 音量・ピッチ・リバーブ・音色の調整UI。
- **RecipeListController**（MonoBehaviour）: 保存レシピの一覧・再編集・削除。
- **RecipeExportController**（MonoBehaviour）: 任意 WAVE 書き出し。目的: FR-28。

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
| ContentService / ProgressionService / UnlockEvaluator | US-LIB-02/03, US-TECH-09 | FR-22〜24, NFR-14/15 |
| LibraryScreen / CuratedSoundCatalog / UnlockRulesCatalog | US-LIB-01〜03 | FR-20〜24, NFR-13 |
| CreateScreen / SoundRecipe* / RecipeExport | US-CREATE-01〜04 | FR-25〜29, NFR-07/14 |
| ContentService / UITheme / *Config | US-TECH-07 | §7 UI開発フロー, NFR-05 |
| 展示ビルド手順・検証 | US-TECH-08 | NFR-16/17 |
