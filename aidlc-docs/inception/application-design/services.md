# Services（サービス定義とオーケストレーション）

**プロジェクト**: 藝大 音響教育アプリ
**作成**: 2026-07-15 / AI-DLC Application Design（Part 2）
**更新**: 2026-07-30 / フェーズC（音図鑑・音づくり）差分
**方針**: Q2=軽量 Manager（最小シングルトン）＋ ScriptableObject 設定の併用

> サービスは横断的関心事を担い、各モジュールの画面コントローラから利用される。
> **プライバシー**: 役割名で記述し、個人名・個人予定は記載しない。

---

## サービス一覧

| サービス | 種別 | 責務 | 主な依存 | 対応要件 |
|---|---|---|---|---|
| AppManager | 起動/ライフサイクル | 初期化・初回判定・起動導線 | 全サービス | FR-01/03 |
| NavigationService | 遷移 | 型安全な画面遷移（enum SceneId）、Place 無効化、Library/Create 追加 | UnityEngine.SceneManagement | FR-01/02 |
| StorageService | 永続化 | WAV＋設定＋メタ＋プロファイル＋UnlockState＋SoundRecipe の原子的保存/堅牢な読込 | WavCodec, ファイルI/O | FR-08/12/23/27/28, NFR-07/14 |
| AudioService | 音声 | 録音(3秒固定)/再生/加工、レイヤー再生、レシピ再生／書き出し | Microphone/AudioFilter | FR-05/06/07/25/26/28 |
| PitchVariationService | 音声(ゲーム) | 出題用リアルタイムピッチ加工（非保存） | PitchMath | FR-19, NFR-06 |
| ContentService | コンテンツ | お題/ゲーム設定/UITheme/音図鑑カタログ/解除条件表の提供 | ScriptableObject/JSON | FR-14/18/20/22, US-TECH-07 |
| ProgressionService | 進行 | 達成イベント受付→解除判定→UnlockState更新（冪等） | UnlockEvaluator, Storage, Content | FR-22/23, NFR-14 |

---

## オーケストレーション（主要ユースケース）

### 起動フロー（US-REG-01, US-NAV-01）
1. `AppManager.Initialize()` が各サービスを初期化。
2. `AppManager.IsFirstLaunch()` を判定（StorageService.LoadProfile）。
3. 初回 → `NavigationService.GoTo(登録)`、以降 → `GoTo(Home)`。

### 録音〜保存フロー（US-REC-01〜03）
1. `RecScreenController.OnRecord()` → `AudioService.StartRecording(3s)`。
2. `OnStop()` → `AudioService.StopRecording()` で AudioBuffer 取得。
3. `EffectPanelController` の設定で `AudioService.Play(buffer, settings)`。
4. `OnSave()` → `StorageService.SaveSound(...)`（原子的）。
5. （フェーズC）保存成功時、録音課題イベントを `ProgressionService.NotifyRecordingChallenge(...)` へ。

### コレクション表示フロー（US-COL-01〜04）
1. `CollectionScreenController.Refresh()` → `StorageService.ListSounds()`（破損は読み飛ばし）。
2. 絞り込み・詳細編集・削除は既存どおり。

### weekly theme → Rec 導線（US-THEME-01/02）
1. `WeeklyThemeController` → `ContentService.GetCurrentTheme(now)`。
2. `OnThemeTapped()` → `NavigationService.GoTo(Rec)`。

### ①音合わせ 出題フロー（US-GAME1-01〜05）
1. `SoundMatchGameController.StartGame(config)`。
2. `QuestionBuilder` ＋ `PitchVariationService` で出題（非保存）。
3. クリア時 → `ProgressionService.NotifyGameCleared(stageOrDifficulty)`。

### 音図鑑フロー（US-LIB-01〜03）
1. `LibraryScreenController.Show()` → `ContentService.GetCuratedCatalog()` ＋ `StorageService.LoadUnlockState()`。
2. 一覧はロック状態付きで表示。アンロック済みのみ試聴。
3. 試聴は `AudioService.Play(curatedClip)`（同梱素材、読み取り専用）。

### 音づくりフロー（US-CREATE-01〜04）
1. `CreateScreenController` が UnlockState から選択可能な素材を取得。
2. 2音選択 → `AudioService.PlayLayers(layerA, layerB)` で試聴。
3. 加工調整 → プレビュー再適用。
4. 保存 → `StorageService.SaveRecipe(SoundRecipe)`（素材ID＋パラメータのみ）。
5. 任意書き出し → `AudioService.RenderRecipeToWav(recipe)` → `StorageService` で原子的保存。失敗時は不完全ファイルを残さない。

---

## 設計上の原則
- **単一責務**: 永続化は StorageService、進行は ProgressionService、音声は AudioService/PitchVariationService。
- **データ駆動**: カタログ・解除条件・UITheme は ScriptableObject/JSON（ContentService）。企画・デザイン担当がコード改修なしに調整可能。
- **オフライン前提**: すべてローカル。ユーザー間共有なし（NFR-02）。
- **レシピ優先**: 元同梱音声は複製しない。必要時のみ WAVE 書き出し（FR-27/28）。
- **堅牢性**: UnlockState/Recipe は原子的保存、未知ID・破損はフォールバック（NFR-07/14）。
- **共同開発境界**: 共通IF変更は PR レビュー。追加ゲームは縦割りで ProgressionService のイベント契約のみ依存（NFR-15）。
- **テスト容易性**: UnlockEvaluator、レシピシリアライズ、WavCodec/PitchMath を PBT 対象化（NFR-09）。
