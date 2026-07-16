# 実機検証・Player ビルド・性能計測チェックリスト

**プロジェクト**: 藝大 須藤さんアプリ  
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / Build and Test Follow-up  
**関連**: `build-instructions.md` / `unit-test-instructions.md` / `performance-test-instructions.md` / `mcp-scene-wiring-summary.md`

> Editor Play での導線確認は済（2026-07-16）。本ドキュメントは**実機・署名ビルド・EditMode 全件・性能**の実行記録用。

---

## A. Player Settings 前提（自動化済／要確認）

| 項目 | 期待 | 状態 |
|---|---|---|
| 画面向き | Auto Rotation + 縦/縦逆/横左/横右 | ✅ 設定済 |
| iOS `NSMicrophoneUsageDescription` | 子ども向け説明文あり | ✅ 設定済（空だったため追記） |
| Android マイク | `MicPermissionGate` → `Permission.Microphone`（実行時要求） | ✅ コード側あり |
| SafeArea | 各 Geidai シーンに `SafeAreaFitter` | ✅ シーン骨組み |
| 解像度 | `ResponsiveCanvasConfigurator` + CanvasScaler | ✅ シーン骨組み |
| Android minSdk | 25 / ARM64 | ✅ |
| iOS target | 15.0 | ✅ |
| 製品名 / Bundle ID | `おと` / `jp.geidai.sudo.oto` | ✅ 設定済（2026-07-16） |
| Version | `0.1.0` / Android code `1` / iOS build `1` | ✅ |

---

## B. EditMode 全件（Unity Test Runner）

### 実行コマンド
```bash
mkdir -p Logs Builds
"/Applications/Unity/Hub/Editor/6000.4.2f1/Unity.app/Contents/MacOS/Unity" \
  -runTests -batchmode -nographics \
  -projectPath "$(pwd)" \
  -testPlatform EditMode \
  -testResults ./Logs/editmode-results.xml \
  -logFile ./Logs/editmode.log
```
> Editor で同プロジェクトを開いたままだとロックする場合あり。失敗時は Editor を閉じて再実行。

### 記録
| 日時 | 結果 | 備考 |
|---|---|---|
| 2026-07-16T11:55:45+09:00 | **Pass 85 / Fail 0** | `GeidaiTestRunner`。詳細は `editmode-results.md` |

---

## C. Player 実ビルド

### C-1. Android Development APK（署名なし・デバッグキーストア）
Editor: `Geidai/Build/Android Development APK`  
CLI:
```bash
"/Applications/Unity/Hub/Editor/6000.4.2f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -quit -nographics \
  -projectPath "$(pwd)" \
  -buildTarget Android \
  -executeMethod Geidai.EditorTools.GeidaiBuildScript.BuildAndroidDevelopment \
  -logFile ./Logs/build-android.log
```
- 出力: `Builds/Android/GeidaiSudo.apk`
- インストール: `adb install -r Builds/Android/GeidaiSudo.apk`

### C-2. iOS Xcode プロジェクト
Editor: `Geidai/Build/iOS Xcode Project`  
CLI: `-executeMethod Geidai.EditorTools.GeidaiBuildScript.BuildIosXcode`（`-buildTarget iOS`）
- 出力: `Builds/iOS/`
- 続き: Xcode で Team / Signing を設定 → 実機 Run / Archive
- **要**: Apple Developer Team ID（ユーザー手元）

### C-3. ストア配布用署名（手動）
| 項目 | 担当 | メモ |
|---|---|---|
| Android keystore / alias | ユーザー | Player Settings > Publishing Settings |
| iOS Provisioning / Team | ユーザー | Xcode Signing & Capabilities |
| Bundle ID / 製品名変更 | 済（`jp.geidai.sudo.oto` / `おと`） | ストア用キーストア・Apple Team は手動 |
| Android Development APK | `Geidai/Build/Android Development APK` | 出力 `Builds/Android/Oto.apk` |

---

## D. 実機確認チェックリスト（マイク・向き・SafeArea・解像度）

対象端末: Android ______ / iOS ______  
ビルド: APK / Development / AdHoc / ______

| # | 確認項目 | Android | iOS | メモ |
|---|---|---|---|---|
| D1 | 初回録音でマイク許可ダイアログが出る | ☐ | ☐ | |
| D2 | 許可後に録音→再生→保存できる | ☐ | ☐ | |
| D3 | 拒否時にクラッシュせず平易メッセージ | ☐ | ☐ | |
| D4 | 縦向きで全画面 SafeArea（ノッチ非干渉） | ☐ | ☐ | |
| D5 | 横向きでも SafeArea・ボタン到達可 | ☐ | ☐ | |
| D6 | 回転時にレイアウト破綻なし | ☐ | ☐ | |
| D7 | 小画面（〜5"）で文字・ボタンが潰れていない | ☐ | ☐ | |
| D8 | 大画面/タブレット相当で余白が極端でない | ☐ | ☐ | |
| D9 | お題→録音→もどる→お題 | ☐ | ☐ | |
| D10 | コレクションに保存音が一覧表示 | ☐ | ☐ | |
| D11 | 設定再入でプロフィール表示 | ☐ | ☐ | |

### ビルド記録
| 日時 | 成果物 | 結果 |
|---|---|---|
| 2026-07-16 | `Builds/Android/GeidaiSudo.apk`（約97MB・Development・旧名） | ✅ 成功（リネーム前） |
| — | `Builds/Android/Oto.apk`（新パッケージ `jp.geidai.sudo.oto`） | ⏳ 再ビルド推奨 |
| — | iOS Xcode (`Builds/iOS/`) | ⏳ 未（Apple Team + Metal Toolchain 要確認） |

---

## E. 端末性能計測（Development Build + Profiler）

手順の詳細は `performance-test-instructions.md`。記録欄:

| 指標 | 目標 | Android 実測 | iOS 実測 | Pass? |
|---|---|---|---|---|
| 一覧スクロール fps | 60 | | | ☐ |
| タップ→発音 | < 0.1s | | | ☐ |
| お題表示 | < 0.1s | | | ☐ |
| 100件 ListSounds | < 0.5s | | | ☐ |
| 起動〜ホーム | 体感即時 | | | ☐ |
| GC スパイク | 小さい | | | ☐ |

Profiler キャプチャ保存先: `Logs/perf-*.data`（任意）

---

## F. 実行順序（推奨）
1. EditMode 全件（本機 CLI）→ 結果を §B に記入
2. Android Development APK 生成 → 実機インストール → §D
3. iOS Xcode 生成 → Team 署名 → 実機 → §D
4. Development Build で Profiler 接続 → §E
5. 結果を `build-and-test-summary.md` / `audit.md` に追記
