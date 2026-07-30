# Unit of Work（ユニット定義）

**プロジェクト**: 藝大 音響教育アプリ
**作成**: 2026-07-15 / AI-DLC Units Generation（Part 2）
**更新**: 2026-07-30 / フェーズC（U7 Library / U8 Create）追加
**入力**: `../requirements/requirements.md`、`../user-stories/stories.md`、`components.md`、`services.md`

> 単一 Unity アプリ（モジュール構成のモノリス）。ユニット＝論理モジュール。
> フェーズCは役割別ワークストリームで並行可能（共通IF合意後）。個人名・個人予定は記載しない。

---

## ユニット一覧

### U1 基盤（UI基盤 ＋ Services器） `[A]` — 実装済み
- **責務**: UI基盤、Services器、Common モデル／純粋関数。
- **含むストーリー**: US-TECH-01, 02, 04, 05, 07
- **フェーズC拡張**: ProgressionService IF、Curated/Unlock/Recipe モデル、SceneId(Library/Create)、UnlockEvaluator。

### U2 Foundation `[A]` — 実装済み
- **責務**: 起動・ホーム・登録・ナビ。Place除外。
- **フェーズC拡張**: ホームに音図鑑／音づくり導線を追加可能。

### U3 Rec `[A]` — 実装済み
- **責務**: 3秒録音・加工・保存。
- **フェーズC拡張**: 保存成功時の録音課題イベント通知。

### U4 Persistence / Collection `[A]` — 実装済み
- **責務**: 永続化堅牢化・コレクション。
- **フェーズC拡張**: UnlockState / Recipe / Export の保存API。

### U5 weekly theme `[B]` — 実装済み
- **責務**: お題表示・Rec導線・差し替え可能構成。

### U6 Game①音合わせ `[B]` — 実装済み
- **責務**: 音合わせ本編。
- **フェーズC拡張**: クリア時の達成イベント通知。制作側素材ID参照の拡張余地。

### U7 Sound Library（音図鑑・アンロック） `[C]` — 新規
- **責務**: 制作側カタログの閲覧・試聴、ロック表示、解除状態の反映。
- **含むストーリー**: US-LIB-01, US-LIB-02, US-LIB-03
- **主コンポーネント**: LibraryScreenController / CuratedSoundCatalog / UnlockRulesCatalog / CuratedSoundListView
- **依存**: U1（Content/Progression/Storage/Audio/UI）、U4（UnlockState保存API）
- **完了条件**: 50〜100音カタログをオフラインで閲覧・試聴でき、解除状態が再起動後も維持される。
- **担当ワークストリーム**: 基盤・統合＋企画・デザイン（素材／分類）

### U8 Sound Create（音を作る） `[C]` — 新規
- **責務**: 2音選択・加工・試聴・レシピ保存・任意WAVE書き出し。
- **含むストーリー**: US-CREATE-01, US-CREATE-02, US-CREATE-03, US-CREATE-04
- **主コンポーネント**: CreateScreenController / RecipeLayerPicker / RecipeEffectPanel / RecipeListController / RecipeExportController
- **依存**: U1（Audio/Storage）、U7（UnlockState・素材ID）
- **完了条件**: アンロック済み2音で試聴・加工し、レシピ保存・再編集・必要時書き出しができる。
- **担当ワークストリーム**: 音響実装＋企画・デザイン（UI）

---

## 将来ユニット（スコープ外・スタブ） `[将来]`
- **UF 将来**: FUT-01（追加ゲーム縦割り）、FUT-02（ユーザー間共有・見送り継続）、FUT-03（通貨ゲーミフィケーション）、FUT-04（テスト画面）、FUT-05（3音以上の本格音づくり）。

## 実装順序
1. **既存**: U1 → U2 → U3 → U4 → U5 → U6（完了）
2. **フェーズC共通契約**: U1/U4 のモデル・Storage・Progression IF 拡張
3. **U7 Sound Library**
4. **U8 Sound Create**（U7 の UnlockState に依存）
5. **連携仕上げ**: Rec/Game1 の達成イベント接続、ホーム導線、展示用試用ビルド（US-TECH-08）

## 並行開発メモ
- U7 UI／カタログ作業と U8 DSP試作は、共通IF確定後に並行可能。
- 追加ミニゲームは `Geidai.GameN` 縦割りとし、ProgressionService のイベント契約のみ共有する。
- コード編成: `Geidai.Library` / `Geidai.Create` AsmDef を追加。既存モジュールへの逆依存は作らない。
