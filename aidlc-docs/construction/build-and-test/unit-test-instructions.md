# Unit Test Execution（ユニットテスト実行手順）

**プロジェクト**: 藝大 須藤さんアプリ
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / Build and Test
**テスト基盤**: Unity Test Runner（NUnit）＋ Property-Based Testing（FsCheck）。全 **EditMode**。

> 各ユニットの Code Generation で生成した **17 本の EditMode テスト**を統合実行する。純粋関数・データ整合・永続化ロジックを対象（`AudioSource`/`Microphone`/シーン配線を伴う実発音・UI 操作は PlayMode/手動＝integration/performance で扱う）。

---

## テスト一覧（`Assets/Scripts/Tests/EditMode/`）
| ユニット | テストファイル | 対象 | 種別 |
|---|---|---|---|
| U1 | `WavCodecTests.cs` | WAV エンコード/デコード往復 | PBT |
| U1 | `PitchMathTests.cs` | cents↔ratio↔semitone 変換往復 | PBT |
| U1 | `SerializationTests.cs` | モデルの JSON 直列化往復 | PBT/例示 |
| U2 | `StartupRouterTests.cs` | 起動ルーティング（初回/既存） | 例示 |
| U2 | `NavigationRoutingTests.cs` | ナビ安全性・`ModuleRouter` 対応 | 例示 |
| U2 | `HomeMenuConfigTests.cs` | ホームメニュー構成（データ駆動） | 例示 |
| U3 | `SoundEffectMapperTests.cs` | エフェクト値の純粋変換 | PBT |
| U3 | `RecordingClockTests.cs` | 3秒固定録音の時間管理（浮動小数許容） | 例示 |
| U3 | `SaveSoundTests.cs` | `StorageService.SaveSound`（wav＋meta 対） | 例示 |
| U4 | `CollectionFilterTests.cs` | 月絞込＋キーワード検索（純粋） | PBT/例示 |
| U4 | `SavedSoundJsonTests.cs` | meta JSON 往復・後方互換 | PBT/例示 |
| U4 | `AtomicFileTests.cs` | 原子的書込（temp→置換）・例外時掃除 | 例示 |
| U4 | `StorageCollectionTests.cs` | 一覧集約・破損スキップ・削除整合 | 例示 |
| U5 | `ThemeSelectorTests.cs` | 週選択（O(1)・決定的・範囲） | PBT/例示 |
| U5 | `ContentServiceThemeTests.cs` | お題取得・空/無効フォールバック | 例示 |
| U6 | `QuestionBuilderTests.cs` | 出題生成（正解1つ/距離/決定的） | PBT/例示 |
| U6 | `SoundMatchConfigTests.cs` | パラメータのクランプ/フォールバック | 例示 |

---

## Run Unit Tests（実行）

### 方法A: Unity Editor（推奨・GUI）
1. `Window > General > Test Runner` を開く。
2. `EditMode` タブを選択。
3. `Run All` を実行。

### 方法B: CLI（バッチ・CI 向け）
```bash
"/Applications/Unity/Hub/Editor/6000.4.2f1/Unity.app/Contents/MacOS/Unity" \
  -runTests -batchmode \
  -projectPath "$(pwd)" \
  -testPlatform EditMode \
  -testResults ./Logs/editmode-results.xml \
  -logFile ./Logs/editmode.log
```
- 終了コード 0 かつ `./Logs/editmode-results.xml`（NUnit3 形式）で `result="Passed"` を確認。

### 方法C: 公式 Unity MCP（`user-unity-mcp`）
- `Unity_RunCommand` は**プロジェクトアセンブリの Test 直接実行は非対応**（U3〜U6 で確認済の制限）。
- 代替として、純粋関数の**スモーク**（`QuestionBuilder`/`ThemeSelector`/`CollectionFilter`/`SoundEffectMapper` 等）を `Unity_RunCommand` で実行し健全性を確認済み。網羅テストは方法A/Bで実施する。

### 1. 全テスト実行
上記いずれかで EditMode 全件を実行。

### 2. 結果の確認
- **期待**: 17 ファイル・全テストケース **Pass / 0 Failures**。
- **カバレッジ**: 対象は「純粋ロジック＋永続化」中心（数値カバレッジは任意。Code Coverage パッケージ導入時は `Window > Analysis > Code Coverage` で計測）。
- **レポート**: `./Logs/editmode-results.xml`（CLI）/ Test Runner 画面（GUI）。

### 3. 失敗時の対応
1. Test Runner または `editmode-results.xml` で失敗ケースを特定。
2. 該当アセンブリのコードを修正（例: 既知の `RecordingClock` 浮動小数／`NoiseLevel` 列挙名など、code-summary の Errors 参照）。
3. 全 Pass まで再実行。

---

## 注意（PBT / 決定性）
- PBT（FsCheck）は乱択入力で不変条件を検証（`QuickCheckThrowOnFailure`）。失敗時は縮小反例がログに出るため、それを固定シードの例示テストに落として回帰防止する。
- 出題・週選択・エフェクト変換は**決定的**（同一入力→同一出力）。フレーク時はまず時刻/乱数の注入経路（`ThemeSelector` の時刻注入、`QuestionBuilder` の seed）を確認する。
