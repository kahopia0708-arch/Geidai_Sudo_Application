# Component Methods（メソッド署名）

**プロジェクト**: 藝大 須藤さんアプリ
**作成**: 2026-07-15 / AI-DLC Application Design（Part 2）

> メソッド署名と高レベルの目的・入出力型を示す。**詳細な業務ルール・アルゴリズム・エラー分岐は Construction の Functional Design（ユニットごと）で確定**する。型は C# 想定の擬似署名（確定版は実装時）。

---

## Common / データモデル

### UserProfile
- `static Result<UserProfile> Create(int birthYear, string nickname)` — 検証付き生成（年範囲/長さ）。out: 妥当なら UserProfile、否なら検証エラー。
- `bool IsValid()` — 妥当性判定。

### SoundEffectSettings
- `static SoundEffectSettings Default()` — 既定値（全バイパスoff等）。
- `SoundEffectSettings WithPitch(float semitonesOrCents)` — 不変更新。

### SavedSound
- （集約）`SoundClipMeta Meta`, `SoundEffectSettings Effects`, `string WavPath`。

## Common / ユーティリティ（PBT 対象）

### WavCodec
- `static byte[] Encode(AudioBuffer buffer)` — PCM→WAVE(16bit)。
- `static AudioBuffer Decode(byte[] wav)` — WAVE→PCM。
- 不変条件: `Decode(Encode(x)) ≈ x`（ラウンドトリップ / NFR-09）。

### PitchMath
- `static float CentsToRatio(float cents)` — セント→再生レシオ。
- `static float RatioToCents(float ratio)` — 逆変換。
- 不変条件: `RatioToCents(CentsToRatio(c)) ≈ c`。

---

## Services

### AppManager
- `void Initialize()` — サービス初期化・依存解決。
- `bool IsFirstLaunch()` — 初回起動判定（UserProfile 有無）。
- `void StartApp()` — 初回は登録、以降はメイン/ホームへ。

### NavigationService
- `enum SceneId { Main, Home, Rec, Collection, WeeklyTheme, SoundMatchGame /*, Place=無効 */ }`
- `void GoTo(SceneId target)` — 型安全な遷移（FR-02 不具合解消）。
- `void GoHome()` / `void GoBack()` — ホーム/前画面。
- 制約: `Place` は導線から除外（有効化しない）。

### StorageService（単一集約 / NFR-07）
- `Result Save(SavedSound sound)` — 原子的保存（WAV＋設定＋メタ）。
- `IReadOnlyList<SavedSound> LoadAll()` — 破損項目は安全に読み飛ばし。
- `SavedSound? Load(string id)` / `Result Delete(string id)`
- `Result SaveProfile(UserProfile profile)` / `UserProfile? LoadProfile()`
- `T? LoadSettings<T>(string key)` / `Result SaveSettings<T>(string key, T value)`
- 契約: 保存は一時ファイル→原子的置換。読込は欠損/破損時フォールバック。

### AudioService（録音一本化 / FR-07）
- `void StartRecording(float fixedSeconds = 3f)` — 3秒固定録音（FR-05）。
- `AudioBuffer StopRecording()` — 録音停止しバッファ取得。
- `void Play(AudioBuffer buffer, SoundEffectSettings effects)` — 加工反映再生（FR-06）。
- `AudioBuffer ApplyEffects(AudioBuffer src, SoundEffectSettings effects)` — 保存用に加工適用。
- 制約: 実装は `VoiceRecordingSection`（Unity標準AudioFilter）に一本化。マイク権限拒否時は安全に失敗（SECURITY-15）。

### PitchVariationService（ゲーム用 / FR-19）
- `IReadOnlyList<AudioBuffer> GenerateVariations(AudioBuffer source, IReadOnlyList<float> cents)` — 例 {0, ±10, ±20} のリアルタイム加工。
- 制約: 生成音は**保存しない**。モバイルで実用的処理時間（NFR-06）。

### ContentService（データ駆動 / Q6）
- `ThemeCatalog GetThemeCatalog()` / `ThemeEntry GetCurrentTheme(DateTime now)`
- `SoundMatchConfig GetSoundMatchConfig()`
- `UITheme GetUITheme()`
- 契約: ScriptableObject/JSON から読み込み、Sさん がコード改修なしで差し替え可能。

---

## UI 基盤（Q7）

### ScreenRootBase（抽象）
- `virtual Task ShowAsync()` / `virtual Task HideAsync()`
- `protected void ApplySafeArea()` / `protected void ConfigureResponsive()`
- `virtual void OnBackPressed()`

### SafeAreaFitter
- `void Apply(Rect safeArea)` — safeArea に合わせ RectTransform を調整（縦横対応）。

### ResponsiveCanvasConfigurator
- `void Configure(Canvas canvas)` — CanvasScaler 設定を統一適用。

---

## Foundation（U1）

### HomeScreenController
- `void ShowModules(IEnumerable<SceneId> available)` — 利用可能モジュール表示（Place 除外）。
- `void OnModuleSelected(SceneId id)` — NavigationService.GoTo。

### UserRegistrationScreenController
- `void Submit(int birthYear, string nickname)` — 検証→StorageService.SaveProfile。
- `void ShowValidationError(ValidationError e)`

---

## Rec（U2）

### RecScreenController
- `void OnRecord()` / `void OnStop()` / `void OnPlay()`
- `void OnEffectChanged(SoundEffectSettings settings)`
- `void OnSave()` — AudioService.ApplyEffects→StorageService.Save。

### RecordingController
- `void BeginRecord(float seconds)` / `AudioBuffer EndRecord()` / `void PlayPreview(SoundEffectSettings s)`

### EffectPanelController
- `SoundEffectSettings CurrentSettings { get; }`
- `void SetBypass(EffectType type, bool bypass)`

---

## Collection（U3）

### CollectionScreenController
- `void Refresh()` — StorageService.LoadAll。
- `void ApplyFilter(MonthFilter month, string keyword)`

### SoundListView
- `void Bind(IReadOnlyList<SavedSound> items)` / `void OnPlay(string id)` / `void OnDelete(string id)`

### SoundDetailController
- `void Show(SavedSound sound)` / `void SaveMeta(SoundClipMeta meta)`

### FilterSearchController
- `event Action<MonthFilter, string> OnFilterChanged`

---

## Theme（U4）

### WeeklyThemeController
- `void ShowCurrentTheme()` — ContentService.GetCurrentTheme。
- `void OnThemeTapped()` — NavigationService.GoTo(Rec)（お題情報を受け渡し）。

---

## Game1（U5）

### SoundMatchGameController
- `void StartGame(SoundMatchConfig config)`
- `void OnChoiceSelected(int index)` / `void OnAnswerDropped(int choiceIndex, int targetIndex)`
- `void Evaluate()` — 正誤判定→ResultEffectController。

### QuestionBuilder
- `Question Build(SavedSound userSound, SoundMatchConfig config)` — PitchVariationService で選択肢生成（FR-19）。

### ResultEffectController
- `void PlayCorrect()` — おたまじゃくし→カエル演出（FR-17）。
- `void PlayRetry()`

> 注: `Result`/`ValidationError`/`Question`/`MonthFilter`/`EffectType`/`ThemeEntry` 等の型詳細と業務ルールは Functional Design で定義。
