# Unit of Work（ユニット定義）

**プロジェクト**: 藝大 須藤さんアプリ
**作成**: 2026-07-15 / AI-DLC Units Generation（Part 2）
**決定（unit-of-work-plan.md ＋ 明確化）**: Q1=B（6ユニット・UI基盤を独立）/ 明確化=A（U1 に UI基盤＋Services器 を集約）/ Q2=A（Services器 先行）/ Q3=A（依存順）/ Q4=A（単独開発＋Sさん UI・逐次）/ Q5=A（単一 Unity アプリ・モノリス）
**入力**: `../requirements/requirements.md`、`../user-stories/stories.md`、`components.md`、`services.md`

> 本プロジェクトは**単一 Unity アプリ（モジュール構成のモノリス）**。各ユニット＝論理モジュール（独立デプロイ単位ではない）。ユニットは逐次実装し、各ユニット完了時に Sさん へ UI 詳細調整をハンドオフ（US-TECH-07）。

---

## ユニット一覧（6ユニット）

### U1 基盤（UI基盤 ＋ Services器） `[A]`
- **責務**: 全ユニットの土台。
  - UI基盤: `SafeAreaFitter`、`ResponsiveCanvasConfigurator`、`ScreenRootBase`、`UITheme`（SO）＋ 画面ルート Prefab テンプレート。
  - Services器: `AppManager`、`NavigationService`（enum `SceneId`・Place 無効化）、`StorageService`（IF＋最小実装）、`AudioService`（IF＋器）、`ContentService`（IF＋器）。
  - Common: データモデル（UserProfile/SoundClipMeta/SoundEffectSettings/SavedSound/AudioBuffer）、純粋関数（WavCodec/PitchMath）。
  - 開発規約: Unity MCP 経由のシーン操作（US-TECH-05）。
- **含むストーリー**: US-TECH-01, US-TECH-02, US-TECH-04, US-TECH-05, US-TECH-07
- **主コンポーネント**: UI基盤一式 / 6サービスの器 / Common
- **完了条件**: 端末横断で破綻しない空画面テンプレートが表示でき、型安全な遷移が動作。各サービス IF が確定。

### U2 Foundation（起動・ホーム・登録・ナビ導線） `[A]`
- **責務**: メイン→ホーム→各モジュールの導線、初回ユーザー登録・編集・入力検証。Place は導線から除外。
- **含むストーリー**: US-NAV-01, US-NAV-02, US-REG-01, US-REG-02
- **主コンポーネント**: BootScreenController / HomeScreenController / UserRegistrationScreenController
- **依存**: U1（UI基盤・NavigationService・StorageService器・UserProfile）
- **完了条件**: 登録→ホーム→各モジュール空画面への遷移が一貫動作。

### U3 Rec（録音・加工・保存） `[A]`
- **責務**: 3秒録音、加工（リバーブ/ノイズ低減/ピッチ/音色・バイパス）、WAVE保存（設定対保存）。録音実装を `VoiceRecordingSection` に一本化。
- **含むストーリー**: US-REC-01, US-REC-02, US-REC-03, US-TECH-03
- **主コンポーネント**: RecScreenController / RecordingController / EffectPanelController / SavePromptController
- **依存**: U1（AudioService・StorageService器・WavCodec・SoundEffectSettings）
- **完了条件**: 録音→加工プレビュー→WAV保存（メタ/設定対）まで動作。旧録音実装（RecorderWithEffects 等）整理。

### U4 Persistence / Collection（永続化本実装・コレクション） `[A]`
- **責務**: StorageService の堅牢性本実装（原子的保存・破損フォールバック）。一覧/視聴/削除、メタ拡張、月別絞り込み・検索。
- **含むストーリー**: US-COL-01, US-COL-02, US-COL-03, US-COL-04, US-TECH-06
- **主コンポーネント**: CollectionScreenController / SoundListView / SoundDetailController / FilterSearchController（＋StorageService 強化）
- **依存**: U1（StorageService器・データモデル）、U3（保存物）
- **完了条件**: 保存音の一覧/検索/メタ編集が動作し、破損時も安全にフォールバック。

### U5 weekly theme（お題） `[B]`
- **責務**: 週替わりお題表示、お題タップから Rec へ、お題の差し替え可能構成（ScriptableObject）。
- **含むストーリー**: US-THEME-01, US-THEME-02, US-THEME-03
- **主コンポーネント**: WeeklyThemeController / ThemeCatalog（SO）
- **依存**: U1（ContentService・NavigationService）、U2（ホーム導線）、U3（Rec遷移先）
- **完了条件**: 今週のお題表示→Rec遷移が動作し、Sさん がお題を差し替え可能。

### U6 Game①音合わせ `[B]`
- **責務**: 出題（お手本/選択肢）・タップ確認・ドラッグ解答・正誤判定・演出（カエル進化）、共通パラメータ（出題数/選択肢数/難易度＝セント）、ユーザー保存音のリアルタイムピッチ加工出題（非保存）。
- **含むストーリー**: US-GAME1-01, US-GAME1-02, US-GAME1-03, US-GAME1-04, US-GAME1-05
- **主コンポーネント**: SoundMatchGameController / ChoiceItemView / QuestionBuilder / ResultEffectController / SoundMatchConfig（SO）
- **依存**: U1（AudioService・PitchVariationService・PitchMath・ContentService）、U4（保存音の取得）
- **完了条件**: 保存音からの出題→解答→判定→演出が動作。難易度はセントで調整可能。

---

## 将来ユニット（スコープ外・スタブ） `[将来]`
- **UF 将来**: FUT-01（②〜⑧ゲーム）、FUT-02（③共有/Place）、FUT-03（ゲーミフィケーション）、FUT-04（⑤テスト画面）。今回の MVP ユニットには含めない（研究会/優先度確定後に別途ユニット化）。

## 実装順序（Q3=A / 依存順・逐次）
U1 → U2 → U3 → U4 → U5 → U6

## コード編成メモ（モノリス）
- 単一 Unity プロジェクト内で、Assembly Definition（`Geidai.Common` / `Geidai.Services` / `Geidai.Foundation` / `Geidai.Rec` / `Geidai.Collection` / `Geidai.Theme` / `Geidai.Game1` / `Geidai.Tests`）によりモジュール境界を表現。ユニットはこれらモジュールに対応（U1=Common+Services+UI基盤）。
