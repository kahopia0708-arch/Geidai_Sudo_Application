# U1 基盤 — Code Generation Plan（Part 1: 計画）

**ユニット**: U1 基盤（UI基盤 ＋ Services器 ＋ Common）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Code Generation（Part 1）
**プロジェクト種別**: Brownfield（Unity 6000.4.2f1 / URP / uGUI / C#）
**Workspace Root**: `/Users/maemoto/Documents/GitHub/Geidai_Sudo_Application`
**入力**: `../u1-foundation/functional-design/*`, `../u1-foundation/nfr-requirements/*`, `../u1-foundation/nfr-design/*`, `../../inception/application-design/*`

> 本プランは Code Generation の**唯一の正**（single source of truth）。Part 2 では本プランの手順を上から順に実行し、各ステップ完了時に `[x]` を付ける。

---

## 0. 生成方針（重要）

- **アプリコードは Workspace 直下（`Assets/`）に生成**。ドキュメントのみ `aidlc-docs/`。
- **Brownfield 非破壊**: 既存スクリプト（`SoundEffectSettings.cs`, `WavUtility.cs`, `MySoundCollectionStorage.cs` 等）は **U1 では変更しない**。U1 の新規コードは **`Geidai.*` 名前空間**＋新規フォルダに分離し、既存 `Assembly-CSharp` とクラス名衝突を避ける（録音実装の一本化・旧コード整理は U3/U4 で実施）。
- **AsmDef 導入**: `Geidai.Common` / `Geidai.Services` / `Geidai.Tests`（U2 以降で Foundation 等を追加）。既存 `Assembly-CSharp` は Auto-Referenced により新 asmdef を参照可能（新 asmdef→既存への逆参照はしない）。
- **`.meta` は Unity が生成**。手動生成しない（Unity 起動時にインポート）。
- **シーン変更は行わない**（Unity MCP 経由が規約 / US-TECH-05）。本ステージはスクリプト/アセット雛形と AsmDef の生成に限定。実シーンへの適用は別途 MCP で。
- **テストは生成のみ**（実行は Build & Test ステージ。ただし本ステージでも MCP `run_tests` でスモーク実行を試みる）。

### Unity MCP 活用方針（US-TECH-05）— Unity 純正 AI Assistant パッケージを使用
本ステージのコード生成・検証は **Unity 公式 AI Assistant パッケージ（`com.unity.ai.assistant`）が提供する「Unity MCP Server」** を活用する（サードパーティの "MCP for Unity" ブリッジは使用しない）。

- **採用理由**: Unity 純正・公式サポート、Unity Core Standards 準拠、性能面が有利（US-TECH-05 の「Unity 標準の MCP サーバー」＝公式）。
- **要件**: Unity 6.0 以降（本プロジェクト 6000.4.2f1＝Unity 6.4.2 で充足、6.0.66f2+ 推奨）／ `com.unity.ai.assistant` 導入 ／ Unity AI サブスク（Personal は無料トライアル、Pro/Enterprise 同梱。MCP は Unity AI クレジットを消費しない）／ Unity Cloud 連携。
- **仕組み**: Unity 本体が MCP サーバーになり、リレーバイナリ（`~/.unity/relay/`）経由で Cursor が MCP クライアントとして接続。接続後、Cursor に新 MCP サーバー（例: `unity-mcp`）として出現。
- **現状**: 未接続（未セットアップ）。現行環境の `user-*-unityMCP` は別系統（サードパーティ）で本方針では不使用。**Step 0 で公式サーバーをセットアップ・接続**してから生成・検証に進む。
**接続後に確認した公式ツール（`user-unity-mcp`）**
- `Unity_GetConsoleLogs` — コンソール（Error/Warning/Log）読み取り
- `Unity_RunCommand` — C# をコンパイル＆実行（`IRunCommand`）。AssetDatabase 更新・アセット/AsmDef 生成・ProjectSettings・パッケージ管理など万能
- `Unity_AssetGeneration_GenerateAsset` / `GetModels` — AI アセット生成（スプライト/画像/3D/音）
- `Unity_Camera_Capture` / `Unity_SceneView_Capture2DScene` / `CaptureMultiAngleSceneView` — 画面キャプチャ
- ※ サードパーティ版のような専用 `create_script`/`run_tests`/`manage_scene` は無い → 下記の使い分けで代替

**本ユニットでの使い分け**
- **スクリプト生成**: 本ツール（Write）で `Assets/` に `.cs`/`.asmdef` を直接生成 → `Unity_RunCommand` で `AssetDatabase.Refresh()` を呼びインポート/コンパイル誘発。
- **コンパイル確認**: 生成後に `Unity_GetConsoleLogs`（Error 0 を確認）。※コンパイルエラーがあると以降の型が使えない/ブリッジ不調のため必ず解消。
- **アセット生成**: `UITheme`（ScriptableObject）等の `.asset` は `Unity_RunCommand`（`ScriptableObject.CreateInstance`＋`AssetDatabase.CreateAsset`）で生成。UI 用のモチーフ画像/音は必要に応じ `Unity_AssetGeneration_GenerateAsset`（Sさん 調整前提のプレースホルダ）。
- **テスト実行**: 公式に専用ツールが無いため、`Unity_RunCommand`＋`TestRunnerApi` でのスモーク実行を試行（本実行は Build & Test ステージ）。
- **シーン/GameObject/プレハブ**: 実シーンへの適用は本ステージ対象外（U2 以降）。

### 生成先フォルダ構成（新規）
```
Assets/Scripts/
├── Common/         (Geidai.Common.asmdef)
│   ├── Models/     UserProfile, SoundClipMeta, SoundEffectSettingsData, AudioBuffer, SavedSound, SceneId
│   ├── Results/    Result, ResultCode
│   ├── Audio/      WavCodec, PitchMath
│   ├── Utils/      ValidationUtil, SafeLogger
│   └── UI/         ResponsiveCanvasConfigurator, SafeAreaFitter, ScreenRootBase, UITheme, ErrorPresenter
├── Services/       (Geidai.Services.asmdef → refs Geidai.Common)
│   ├── ServiceRegistry, AppManager
│   ├── Navigation/ INavigationService, NavigationService
│   ├── Storage/    IStorageService, StorageService(最小)
│   ├── Audio/      IAudioService(器)
│   └── Content/    IContentService, ContentService(器)
└── Tests/EditMode/ (Geidai.Tests.asmdef → refs Common/Services + FsCheck + nunit)
    WavCodecTests, PitchMathTests, SerializationTests
```

---

## 1. 対象ストーリー（U1 / トレース）
- US-TECH-01 端末横断のレスポンシブ表示 → `ResponsiveCanvasConfigurator`, `ScreenRootBase`
- US-TECH-02 SafeArea 対応 → `SafeAreaFitter`, `ScreenRootBase`
- US-TECH-04 型安全な画面遷移 → `SceneId`, `INavigationService`/`NavigationService`
- US-TECH-05 Unity MCP 経由のシーン操作（開発規約・本ステージはコード側のみ）
- US-TECH-07 UI ハンドオフ（前本→Sさん）→ `UITheme`（SO）＋差し替え余地
- 併せて U1 Common 基盤: NFR-07（Result/堅牢性）, NFR-09（WavCodec/PitchMath の PBT）, NFR-04/Security（ValidationUtil/SafeLogger）

## 2. 依存・インターフェース
- U1 は最下層。依存は `Services → Common` の一方向。UI基盤は Common に配置。
- Services は IF を公開し、U2 以降（Foundation/Rec/…）が参照。
- 外部 API/ネットワークなし（完全オフライン / NFR-02）。

---

## 実行ステップ（Part 2 でこの順に実行）

### Step 0: Unity 純正 AI Assistant（Unity MCP Server）セットアップ・接続（US-TECH-05）
**ユーザー操作（Unity 側）**
- [ ] Unity エディタで `com.unity.ai.assistant`（AI Assistant）パッケージを導入（Editor の AI ボタン or Package Manager）
- [ ] Unity AI サブスク（Personal は無料トライアル）を有効化し、プロジェクトを Unity Cloud に連携
- [ ] `Edit > Project Settings > AI > Unity MCP Server` で **ブリッジを Start**
- [ ] Integrations で Cursor を自動設定（または relay バイナリのパスを Cursor の MCP 設定に手動登録：`~/.unity/relay/relay_mac_arm64.app/Contents/MacOS/relay_mac_arm64 --mcp`）
- [ ] 初回接続時、Unity の Unity MCP Server 画面で **Pending Connection を承認**

**エージェント側（接続後）**
- [x] `GetMcpTools` で公式 Unity MCP サーバー（`user-unity-mcp`）のツール一覧・スキーマを取得
- [x] 接続確認＋ベースライン（`Unity_GetConsoleLogs`＝Error 0 / Warning 1[AI Assistant アカウントAPI 待ち]）
- [ ] FsCheck 導入方針の確定（NuGetForUnity or DLL 取り込み or UPM）。Step 1 の Tests asmdef と整合

**前提/注意**
- [x] コンパイルエラーが無いこと（ベースライン Error 0 を確認）
- [ ] 未接続のまま進める場合は §5 フォールバック（直接ファイル生成→後で公式 MCP 接続時に一括検証）
- _トレース: US-TECH-05 / NFR-10（変更管理）_

### Step 1: AsmDef とフォルダ基盤
- [ ] `Assets/Scripts/Common/Geidai.Common.asmdef`（autoReferenced=true）
- [ ] `Assets/Scripts/Services/Geidai.Services.asmdef`（references: Geidai.Common）
- [ ] `Assets/Scripts/Tests/EditMode/Geidai.Tests.asmdef`（Editor専用/testAssemblies、references: Common, Services；FsCheck/nunit）
- _トレース: NFR-08（モジュール分割）_

### Step 2: Common — ドメインモデル
- [ ] `Common/Models/SceneId.cs`（enum: Boot, Home, Register, Rec, Collection, Theme, Game1；Place は含めない/BR-15）
- [ ] `Common/Models/UserProfile.cs`（birthYear[1900..現在], nickname[1..8]；BR-01/02）
- [ ] `Common/Models/SoundClipMeta.cs`（id(GUID), displayName, createdAt, wavFileName 等）
- [ ] `Common/Models/SoundEffectSettingsData.cs`（pitchSemitones ±12, noiseLevel(4), timbre(3), reverb 0..1 等／JsonUtility 可能な素直構造）
- [ ] `Common/Models/AudioBuffer.cs`（44100Hz/mono/16bit/3s=132300 samples）
- [ ] `Common/Models/SavedSound.cs`（SoundClipMeta＋SoundEffectSettingsData の対）
- _トレース: Functional Design domain-entities.md / NFR-08_

### Step 3: Common — Result 型
- [ ] `Common/Results/ResultCode.cs`（enum: Ok, NotFound, Corrupted, IOError, ValidationError, Unknown）
- [ ] `Common/Results/Result.cs`（`Result` / `Result<T>`：IsSuccess, Code, Message, Value）
- _トレース: NFR-07 / nfr-design-patterns §1_

### Step 4: Common — 純粋関数（PBT対象）
- [ ] `Common/Audio/WavCodec.cs`（static Encode(float[]→byte[]) / Decode(byte[]→float[])；16bit PCM）
- [ ] `Common/Audio/PitchMath.cs`（static CentsToRatio / RatioToCents / SemitonesToCents 等）
- _トレース: NFR-09 / business-logic-model.md_

### Step 5: Common — ユーティリティ
- [ ] `Common/Utils/ValidationUtil.cs`（ValidateBirthYear, ValidateNickname → Result；BR-01〜03）
- [ ] `Common/Utils/SafeLogger.cs`（PII マスク付きログ、本番は詳細抑制）
- _トレース: Security/NFR-04 / nfr-design-patterns §3_

### Step 6: Common — UI 基盤
- [ ] `Common/UI/ResponsiveCanvasConfigurator.cs`（CanvasScaler=ScaleWithScreenSize, 参照1080×1920, Match0.5）
- [ ] `Common/UI/SafeAreaFitter.cs`（Screen.safeArea 追従、向き/解像度変更で再適用、差分間引き）
- [ ] `Common/UI/UITheme.cs`（ScriptableObject：配色/フォント/アイコン参照；Sさん調整点）
- [ ] `Common/UI/ScreenRootBase.cs`（abstract：ShowAsync→Configure→ApplySafeArea→固有初期化、OnBackPressed）
- [ ] `Common/UI/ErrorPresenter.cs`（トースト/バナー IF＋最小実装、アイコン＋平易文言、警告表示）
- [ ] （MCP）コンパイル確認: Common 生成後に `read_console`（Error 0 を確認、isCompiling=false 待ち）
- [ ] （MCP）`UITheme` の既定アセット作成: `manage_asset` で `Assets/Settings/UITheme_Default.asset` を生成（Sさん 調整の起点）
- _トレース: US-TECH-01/02/07 / NFR-11/12/05 / frontend-components.md_

### Step 7: Services — 器と登録
- [ ] `Services/ServiceRegistry.cs`（軽量サービスロケータ：Register<IF>/Resolve<IF>）
- [ ] `Services/AppManager.cs`（Bootstrap：サービス登録→profile読込→初回遷移；MonoBehaviour エントリ）
- _トレース: NFR-06/07/08 / logical-components.md §2.1_

### Step 8: Services — Navigation
- [ ] `Services/Navigation/INavigationService.cs`（GoTo(SceneId), GoBack）
- [ ] `Services/Navigation/NavigationService.cs`（SceneId→実シーン名マップ、Place 無効化/BR-15；ロードは SceneManager ラップ）
- _トレース: US-TECH-04 / FR-02 / BR-12〜15_

### Step 9: Services — Storage（U1 最小）
- [ ] `Services/Storage/IStorageService.cs`（LoadProfile/SaveProfile/ListSounds/LoadMeta 等 → Result）
- [ ] `Services/Storage/StorageService.cs`（persistentDataPath、基本保存＋対ファイル整合チェック/スキップ、JsonUtility；原子的置換は U4）
- _トレース: NFR-07 / BR-04〜07 / nfr-design-patterns §1.2_

### Step 10: Services — Audio / Content（器）
- [ ] `Services/Audio/IAudioService.cs`（録音/再生 IF；実装は U3。器のみ）
- [ ] `Services/Content/IContentService.cs`（コンテンツ取得 IF）
- [ ] `Services/Content/ContentService.cs`（器＋最小実装 or NotImplemented を Result で返す）
- [ ] （MCP）コンパイル確認: Services 生成後に `read_console`（Error 0、isCompiling=false 待ち）
- _トレース: application-design services.md / NFR-05_

### Step 11: Tests（生成・PBT）
- [ ] `Tests/EditMode/WavCodecTests.cs`（FsCheck：Encode→Decode ラウンドトリップ不変）
- [ ] `Tests/EditMode/PitchMathTests.cs`（FsCheck：Cents↔Ratio 逆変換、範囲不変）
- [ ] `Tests/EditMode/SerializationTests.cs`（JsonUtility：モデルのシリアライズ→デシリアライズ ラウンドトリップ）
- [ ] （MCP）`read_console` でテストアセンブリのコンパイル確認後、`run_tests`（EditMode）でスモーク実行を試行（本実行は Build & Test）
- _トレース: NFR-09_

### Step 12: コード生成サマリ（ドキュメント）
- [ ] `aidlc-docs/construction/u1-foundation/code/code-summary.md`（生成/変更ファイル一覧、名前空間、既知の TODO、U3/U4 での統合予定、Unity 取り込み手順、MCP 検証結果[read_console/run_tests]、次ユニットでの MCP シーン適用メモ）
- _注: サマリのみ aidlc-docs 配下。コードは Assets 配下。_

### Step 13: ストーリー完了マーク
- [ ] US-TECH-01/02/04/05/07 の U1 該当分を実装済みとしてマーク（frontend/services 生成完了時）

---

## 3. スコープ外（U1 では実施しない）
- 実シーン（Home.unity 等）への基盤コンポーネント適用（→ 別途 Unity MCP、U2 以降）。
- 録音実装の一本化・旧コード（RecorderWithEffects/Scean.cs）整理（→ U3/NFR-08）。
- StorageService の原子的置換・破損復旧の本実装（→ U4）。
- AudioService/ContentService の本実装（→ U3/U5/U6）。

## 4. 完了条件
- Step 0〜13 のチェックボックスが全て `[x]`。
- 新規コードが `Geidai.*` 名前空間で生成され、既存コードと衝突しない。
- （MCP 接続時）`read_console` でコンパイル Error 0、PBT の `run_tests` スモークがグリーン。
- PBT テストコードが生成済み（本実行は Build & Test）。
- code-summary.md が生成され、Unity 取り込み手順・MCP 検証結果が明記されている。

## 5. MCP 未接続時のフォールバック
Unity 未起動でも Part 2 を進められるよう、以下の順で対応する。
1. 本ツールで `.cs`/`.asmdef` を `Assets/` に直接生成（コードは完成）。
2. MCP 検証（read_console/run_tests/manage_asset）は **保留チェック** として残す。
3. ユーザーが Unity を起動後、MCP で一括検証（コンパイル確認・テスト・UITheme アセット作成）を実施し、チェックを完了。

---

## 承認のお願い
本プラン（全 13 ステップ）で U1 のコード生成を進めてよいか、ご確認ください。
- **Request Changes**: ステップ/対象ファイル/方針の修正を指定
- **Continue（承認）**: Part 2（コード生成）を開始
