# Services（サービス定義とオーケストレーション）

**プロジェクト**: 藝大 須藤さんアプリ
**作成**: 2026-07-15 / AI-DLC Application Design（Part 2）
**方針**: Q2=軽量 Manager（最小シングルトン）＋ ScriptableObject 設定の併用

> サービスは横断的関心事（起動・遷移・永続化・音声・コンテンツ）を担い、各モジュールの画面コントローラから利用される。UI/ドメインからインフラ詳細（ファイルI/O、AudioFilter）を隠蔽する。

---

## サービス一覧

| サービス | 種別 | 責務 | 主な依存 | 対応要件 |
|---|---|---|---|---|
| AppManager | 起動/ライフサイクル | 初期化・初回判定・起動導線 | 全サービス | FR-01/03 |
| NavigationService | 遷移 | 型安全な画面遷移（enum SceneId）、Place 無効化 | UnityEngine.SceneManagement | FR-01/02 |
| StorageService | 永続化 | WAV＋設定＋メタ＋プロファイルの原子的保存/堅牢な読込 | WavCodec, ファイルI/O | FR-08/12, NFR-07 |
| AudioService | 音声 | 録音(3秒固定)/再生/加工の統括、VoiceRecordingSection 一本化 | UnityEngine Microphone/AudioFilter | FR-05/06/07 |
| PitchVariationService | 音声(ゲーム) | 出題用リアルタイムピッチ加工（非保存） | PitchMath | FR-19, NFR-06 |
| ContentService | コンテンツ | お題/ゲーム設定/UITheme の提供（SO/JSON） | ScriptableObject/JSON | FR-14/18, US-TECH-07 |

---

## オーケストレーション（主要ユースケース）

### 起動フロー（US-REG-01, US-NAV-01）
1. `AppManager.Initialize()` が各サービスを初期化。
2. `AppManager.IsFirstLaunch()` を判定（StorageService.LoadProfile）。
3. 初回 → `NavigationService.GoTo(Main→登録)`、以降 → `GoTo(Home)`。

### 録音〜保存フロー（US-REC-01〜03）
1. `RecScreenController.OnRecord()` → `AudioService.StartRecording(3s)`。
2. `OnStop()` → `AudioService.StopRecording()` で AudioBuffer 取得。
3. `EffectPanelController` の設定で `AudioService.Play(buffer, settings)`（プレビュー加工反映）。
4. `OnSave()` → `AudioService.ApplyEffects()` → `StorageService.Save(SavedSound)`（原子的）。

### コレクション表示フロー（US-COL-01〜04）
1. `CollectionScreenController.Refresh()` → `StorageService.LoadAll()`（破損は読み飛ばし）。
2. `FilterSearchController` の条件で一覧を絞り込み。
3. 詳細/編集 → `SoundDetailController.SaveMeta()` → `StorageService.Save`。

### weekly theme → Rec 導線（US-THEME-01/02）
1. `WeeklyThemeController.ShowCurrentTheme()` → `ContentService.GetCurrentTheme(now)`。
2. `OnThemeTapped()` → `NavigationService.GoTo(Rec)`（お題コンテキスト受け渡し）。

### ①音合わせ 出題フロー（US-GAME1-01〜05）
1. `SoundMatchGameController.StartGame(config)`（config は `ContentService.GetSoundMatchConfig()`）。
2. `QuestionBuilder.Build(userSound, config)` → `PitchVariationService.GenerateVariations()` で選択肢生成（非保存）。
3. 解答→`Evaluate()`→正誤→`ResultEffectController.PlayCorrect()/PlayRetry()`。

---

## 設計上の原則
- **単一責務**: 永続化は StorageService に一元化（分散させない / Q3）。音声は AudioService/PitchVariationService に集約（Q5）。
- **データ駆動**: お題・ゲーム設定・UITheme は ScriptableObject/JSON 経由（ContentService）で、Sさん がコード改修なしに調整可能（Q6, US-TECH-07）。
- **オフライン前提**: すべてローカル。ネットワーク/サーバー依存なし（NFR-02）。
- **堅牢性**: 保存は原子的、読込はフォールバック（NFR-07）。マイク権限/ I/O 失敗は安全処理（SECURITY-15）。
- **テスト容易性**: WavCodec/PitchMath など純粋関数を分離し PBT 対象化（NFR-09）。
