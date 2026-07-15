# Application Design（統合サマリ）

**プロジェクト**: 藝大 須藤さんアプリ（「音」から始まる、耳のためのアプリケーション）
**作成**: 2026-07-15 / AI-DLC Application Design（Part 2）
**構成文書**: `components.md` / `component-methods.md` / `services.md` / `component-dependency.md`
**入力**: `../requirements/requirements.md`、`../user-stories/stories.md`、`../plans/execution-plan.md`

---

## 1. 設計方針サマリ（承認済み Q1〜Q7）
- **Q1 モジュール構成**: 機能モジュール（Foundation/Rec/Collection/Theme/Game1）＋ Common/Services、各に Assembly Definition。
- **Q2 サービス層**: 軽量 Manager（最小シングルトン）＋ ScriptableObject 設定。
- **Q3 永続化**: 単一 StorageService に集約（原子的保存・破損フォールバック / NFR-07）。
- **Q4 ナビゲーション**: NavigationService＋enum `SceneId` で型安全化、Place 無効化（FR-02）。
- **Q5 音声**: AudioService に集約、録音は `VoiceRecordingSection` 一本化（FR-07）、ゲーム加工は PitchVariationService 分離・非保存（FR-19）。
- **Q6 データ駆動**: お題/ゲーム設定/UITheme を ScriptableObject（一部 JSON）で外部化、Sさん がコード改修なしで調整（US-TECH-07）。
- **Q7 UI 基盤**: 共通 UI 基盤（SafeAreaFitter/ResponsiveCanvasConfigurator/ScreenRootBase/UITheme）＋ Prefab テンプレートで前本→Sさん ハンドオフ。

## 2. アーキテクチャ概観
- 既存 Unity シーン分割型を踏襲しつつ、**UI（画面コントローラ）／ドメイン（データモデル・純粋関数）／サービス（横断）** を分離。
- 依存は一方向: `モジュール → Services → Common`（循環なし）。
- 完全オフライン・ローカル永続化（サーバー/クラウド/外部API なし）。

## 3. コンポーネント／サービス一覧（要約）
- **Services**: AppManager, NavigationService, StorageService, AudioService, PitchVariationService, ContentService
- **Common**: データモデル（UserProfile/SoundClipMeta/SoundEffectSettings/SavedSound/AudioBuffer）、UI基盤（SafeAreaFitter/ResponsiveCanvasConfigurator/ScreenRootBase/UITheme）、純粋関数（WavCodec/PitchMath）
- **Foundation(U1)**: Boot/Home/UserRegistration
- **Rec(U2)**: RecScreen/Recording/EffectPanel/SavePrompt
- **Collection(U3)**: CollectionScreen/SoundList/SoundDetail/FilterSearch
- **Theme(U4)**: WeeklyTheme/ThemeCatalog(SO)
- **Game1(U5)**: SoundMatchGame/ChoiceItem/QuestionBuilder/ResultEffect/SoundMatchConfig(SO)

詳細は各構成文書を参照。

## 4. ユニット対応（Units Generation への橋渡し）
| ユニット | 主コンポーネント | 主サービス | 主要件 |
|---|---|---|---|
| U1 Foundation/UI基盤 | Boot/Home/UserRegistration, UI基盤一式 | AppManager, NavigationService, ContentService | FR-01/02/03/04, NFR-11/12 |
| U2 Rec | RecScreen/Recording/EffectPanel | AudioService, StorageService | FR-05/06/07/08 |
| U3 Persistence/Collection | Collection一式, データモデル | StorageService | FR-09〜12, NFR-07 |
| U4 weekly theme | WeeklyTheme/ThemeCatalog | ContentService, NavigationService | FR-13/14 |
| U5 Game①音合わせ | SoundMatchGame/QuestionBuilder | AudioService, PitchVariationService, StorageService | FR-15〜19 |

## 5. 非機能・品質の設計反映
- **レスポンシブ/SafeArea（NFR-11/12）**: 共通 UI 基盤で全画面に適用、縦横両対応。
- **堅牢性（NFR-07）**: StorageService の原子的保存・破損読み飛ばし・空フォールバック。
- **プライバシー/セキュリティ（NFR-04/Security）**: PII は端末内のみ、ログ非出力、登録入力検証。
- **パフォーマンス（NFR-06）**: リアルタイム加工は PitchVariationService に集約し実機性能を確保。
- **テスト容易性（NFR-09/PBT）**: WavCodec/PitchMath 等の純粋関数を分離しラウンドトリップ/不変条件で検証。
- **UI 開発フロー（§7/US-TECH-07）**: ScriptableObject＋Prefab テンプレートで Sさん の詳細調整をコード非依存化。

## 6. 未確定・後続で確定（Functional Design 以降）
- `Result`/`ValidationError`/`Question`/`MonthFilter`/`EffectType`/`ThemeEntry` 等の型詳細と業務ルール。
- 音色エフェクト（ロボット/コーラス系）の具体 DSP、ノイズリダクション段階の実装。
- ①音合わせの音長の扱い・難易度セント値の具体（研究会後に確定 / 暫定・更新前提）。
- AsmDef への段階移行手順（既存 Assembly-CSharp からの分割）。
