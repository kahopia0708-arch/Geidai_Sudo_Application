# Build Instructions（ビルド手順）

**プロジェクト**: 藝大 須藤さんアプリ（「音」から始まる、耳のためのアプリケーション）
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / Build and Test  
**更新**: 2026-07-30 / Phase C（U7 Sound Library / U8 Sound Create）  
**対象**: 全 8 ユニット（U1〜U6 ＋ U7/U8）統合ビルド。U7/U8 差分は `phase-c-u7-u8-addendum.md`。

> 本アプリは**完全オフラインの Unity モバイルアプリ**（サーバー/クラウド無し）。ビルドは Unity Editor / Unity Hub 経由で行う。

---

## Prerequisites（前提）
- **エンジン**: Unity `6000.4.2f1`（Unity 6・URP）※ `ProjectSettings/ProjectVersion.txt` と一致必須
- **エディタ拡張**: 公式 Unity AI Assistant（`com.unity.ai.assistant`）＝Unity MCP Server（`user-unity-mcp`）で検証に利用（任意）
- **必須パッケージ**（`Packages/manifest.json` で解決）:
  - `com.unity.inputsystem`（Input System / Home の Input Actions を使用）
  - `com.unity.textmeshpro`（TextMesh Pro）
  - `com.unity.test-framework`（NUnit / Unity Test Runner）
  - `com.unity.render-pipelines.universal`（URP）
- **テスト依存（Editor のみ）**: `nunit.framework.dll` / `FsCheck.dll` / `FSharp.Core.dll`（`Geidai.Tests` の `precompiledReferences`。`Assets/` 配下に配置済み前提）
- **モバイル SDK**:
  - Android: Android Build Support（SDK/NDK/OpenJDK）
  - iOS: iOS Build Support ＋ Xcode（macOS のみ）
- **システム要件**: macOS/Windows、メモリ 16GB 推奨、空きディスク 10GB 以上
- **環境変数**: 不要（外部サービス/シークレットなし＝オフライン）

## アセンブリ構成（一方向依存・循環なし）
```
Geidai.Common ← Geidai.Services ← { Geidai.Foundation, Geidai.Rec, Geidai.Collection, Geidai.Theme, Geidai.Game1, Geidai.Library, Geidai.Create }
Geidai.Tests（EditMode専用）→ Common/Services/Foundation/Rec/Collection/Theme を参照
```
| Assembly | 役割 |
|---|---|
| `Geidai.Common` | 型・純粋関数・UI 基盤（Result/SceneId/WavCodec/PitchMath/ScreenRootBase／Library・Create モデル等） |
| `Geidai.Services` | サービス（Storage/Navigation/Audio/Content/Progression/IO/Media/ServiceRegistry/AppManager） |
| `Geidai.Foundation` | Boot/Home/Registration/ルーティング |
| `Geidai.Rec` | 録音・エフェクト・保存 |
| `Geidai.Collection` | 保存音コレクション（一覧/検索/編集/削除） |
| `Geidai.Theme` | 週替わりお題 |
| `Geidai.Game1` | ①音合わせ ゲーム |
| `Geidai.Library` | 音図鑑（一覧・ロック・試聴） |
| `Geidai.Create` | 音づくり（2音レシピ・保存・WAVE書き出し） |
| `Geidai.Tests` | EditMode テスト（`UNITY_INCLUDE_TESTS` 制約・Editor 限定） |

---

## Build Steps（ビルド手順）

### 1. 依存関係の解決（パッケージ復元）
Unity Hub からプロジェクトを開くと `Packages/manifest.json` を基に自動復元される。CLI で行う場合:
```bash
# 例: プロジェクトを開いてコンパイル/インポートのみ実行（バッチ）
"/Applications/Unity/Hub/Editor/6000.4.2f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -quit -projectPath "$(pwd)" -logFile ./Logs/import.log
```
- 完了後 `./Logs/import.log` に `Compilation finished` 系ログが出て Error 0 であること。

### 2. 環境設定（Player Settings の確認）
| 項目 | 値 |
|---|---|
| Product Name（表示名） | `おと` |
| Company Name | `Geidai` |
| Package / Bundle ID | `jp.geidai.sudo.oto`（Android / iOS / Standalone 共通） |
| Version | `0.1.0`（プロトタイプ） |
| Android `versionCode` | `1` |
| iOS Build Number | `1` |
| 画面向き | Auto Rotation（縦/縦逆/横左/横右） |
| iOS マイク説明 | 子ども向け文言設定済 |
| Android 権限 | インターネット強制オフ（オフライン）。マイクは実行時要求 |

> 正式研究タイトルは「音」から始まる、耳のためのアプリケーションの考案。ホーム画面向けに表示名は短縮。ストア公開時の表示名変更は要相談。

Build Profiles: `Assets/Settings/Build Profiles/{Android,iOS,macOS}.asset`

### 3. Build Settings（シーン登録）の確認
有効シーン（MCP 配線後）:
`Main画面` / `GeidaiHome` / `GeidaiRegister` / `GeidaiRec` / `GeidaiCollection` / `GeidaiTheme` / `GeidaiGame1` / `game_Home`  
＋ Phase C（未配線）: `GeidaiLibrary` / `GeidaiCreate`（`phase-c-u7-u8-addendum.md`）  
（旧 Home/Rec/Game01 等は無効化済み。詳細は `mcp-scene-wiring-summary.md`）

実機・向き・マイク・性能の実行記録: **`device-verification-checklist.md`**

### 4. 全ユニットのビルド
Editor GUI: `File > Build Profiles`（Unity 6）でプラットフォームを選択 →「Build」。  
またはメニュー **`Geidai/Build/Android Development APK`** / **`Geidai/Build/iOS Xcode Project`**（`Assets/Editor/GeidaiBuildScript.cs`）。

CLI（Android Development APK 例）:
```bash
"/Applications/Unity/Hub/Editor/6000.4.2f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -quit -nographics \
  -projectPath "$(pwd)" \
  -buildTarget Android \
  -executeMethod Geidai.EditorTools.GeidaiBuildScript.BuildAndroidDevelopment \
  -logFile ./Logs/build-android.log
```
- 出力: `Builds/Android/Oto.apk`（Development・デバッグ署名）
- ストア用本番署名・iOS Team 設定は手動（`device-verification-checklist.md` 参照）

### 5. ビルド成功の確認
- **期待出力**: `Build succeeded`（Editor）/ ログに `[GeidaiBuild] ... result=Succeeded`。コンパイル **Error 0**。
- **成果物**:
  - Android: `.apk` / `.aab`
  - iOS: Xcode プロジェクト（→ Xcode で Archive）
- **許容される警告**: 旧ブラウンフィールドの未使用参照警告（`MySoundCollection` 等の旧シーン）。新 `Geidai.*` からの警告は 0 が目標。

---

## Troubleshooting（トラブルシュート）

### 依存エラーでビルド失敗
- **原因**: テスト用 DLL（`FsCheck.dll` / `FSharp.Core.dll` / `nunit.framework.dll`）欠落、またはパッケージ未復元。
- **対処**: `Packages/manifest.json` の再解決（`Library/` 削除→再オープン）。テスト DLL は `Geidai.Tests` の `precompiledReferences` にあるため `Assets/` 配下に存在するか確認。Player ビルドには EditMode テストは含まれない（`includePlatforms: Editor`）ので、テスト DLL 欠落は**テスト実行時のみ**の問題。

### コンパイルエラーでビルド失敗
- **原因**: アセンブリ参照の循環/欠落、Assembly-CSharp（旧実装）との型衝突。
- **対処**: 一方向依存（`Game1/Theme/Collection/Rec/Foundation → Services → Common`）を維持。新実装は Assembly-CSharp を参照しない（導線は `INavigationService` 経由）。公式 Unity MCP（`user-unity-mcp`）の `Unity_GetConsoleLogs`（logTypes: Error）で原因特定 → 該当アセンブリを修正 → `AssetDatabase.Refresh`。

### マイクが動作しない（実機）
- **原因**: iOS の Usage Description 未設定 / Android の権限未許可。
- **対処**: Player Settings の権限説明文を設定。実機で `MicPermissionGate` の許可ダイアログを許可。

### 画面が端末で破綻する / ノッチに被る
- **原因**: `ResponsiveCanvasConfigurator` / `SafeAreaFitter` 未付与。
- **対処**: 各画面の Canvas に付与し、Reference Resolution を統一（縦横両対応）。
