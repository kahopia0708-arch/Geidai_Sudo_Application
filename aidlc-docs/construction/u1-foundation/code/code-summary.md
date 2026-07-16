# U1 基盤 — Code Generation Summary

**ユニット**: U1 基盤（Common ＋ Services器 ＋ UI基盤）
**フェーズ**: AI-DLC CONSTRUCTION / Code Generation（Part 2 実行結果）
**プロジェクト種別**: Brownfield（Unity 6000.4.2f1 / URP / uGUI / C#）
**MCP**: Unity 公式 AI Assistant パッケージ（`com.unity.ai.assistant` 2.9.0-pre.2）の Unity MCP Server（Cursor 上 `user-unity-mcp`）

> コードは `Assets/` 配下、本サマリのみ `aidlc-docs/` 配下（AI-DLC 規約）。

---

## 1. 生成方針（実施結果）
- **Brownfield 非破壊**: 既存スクリプト（`SoundEffectSettings.cs` / `WavUtility.cs` / `MySoundCollectionStorage.cs` 等）は変更せず。新規コードは **`Geidai.*` 名前空間**＋新規フォルダに分離し、既存 `Assembly-CSharp` とのクラス名衝突を回避。
- **AsmDef 分割**: `Geidai.Common` ←（一方向）← `Geidai.Services`、テストは `Geidai.Tests`（Editor 専用）。
- **`.meta` は Unity が自動生成**（コミット済み）。

## 2. 生成ファイル一覧

### Geidai.Common（`Assets/Scripts/Common/` — asmdef refs: `UnityEngine.UI`）
| ファイル | 種別 | 役割 | トレース |
|---|---|---|---|
| `Geidai.Common.asmdef` | asmdef | 最下層アセンブリ | NFR-08 |
| `Models/SceneId.cs` | enum | 論理シーン識別子（Place 除外） | BR-12〜15 / US-TECH-04 |
| `Models/UserProfile.cs` | model | 生年(1900..現在)/ニックネーム(1..8) | BR-01/02 |
| `Models/SoundClipMeta.cs` | model | 保存音メタ（GUID/表示名/作成日時/wav名） | BR-04/05 |
| `Models/SoundEffectSettingsData.cs` | model+enum | pitch±12 / NoiseLevel(4) / TimbreType(3) / reverb 0..1 | domain-entities |
| `Models/AudioBuffer.cs` | model | 44100Hz/mono/16bit/3s=132300 | NFR-03/06 |
| `Models/SavedSound.cs` | model | メタ＋設定の対 | BR-05 |
| `Results/ResultCode.cs` | enum | 失敗理由コード | NFR-07 |
| `Results/Result.cs` | struct | `Result` / `Result<T>` | NFR-07 / nfr-design §1 |
| `Audio/WavCodec.cs` | pure | 16bit PCM WAV Encode/Decode | NFR-09 |
| `Audio/PitchMath.cs` | pure | cents↔ratio↔semitone 変換 | NFR-09 |
| `Utils/ValidationUtil.cs` | util | 入力検証→Result | BR-01〜03 / SECURITY-05 |
| `Utils/SafeLogger.cs` | util | PII マスクログ・本番抑制 | NFR-04 / Security |
| `UI/ResponsiveCanvasConfigurator.cs` | MB | CanvasScaler 統一（1080×1920/Match0.5） | US-TECH-01 / NFR-11 |
| `UI/SafeAreaFitter.cs` | MB | SafeArea 追従（向き/解像度で再適用） | US-TECH-02 / NFR-12 |
| `UI/UITheme.cs` | SO | 配色/フォント/アイコン（Sさん調整点） | US-TECH-07 / NFR-05 |
| `UI/ScreenRootBase.cs` | MB(abstract) | 画面基底（Show→Configure→SafeArea、BackRequested） | US-TECH-01/02 |
| `UI/ErrorPresenter.cs` | MB | 子ども向けエラー/警告バナー | BR-16/19 |

### Geidai.Services（`Assets/Scripts/Services/` — asmdef refs: `Geidai.Common`）
| ファイル | 種別 | 役割 | トレース |
|---|---|---|---|
| `Geidai.Services.asmdef` | asmdef | サービス層 | NFR-08 |
| `ServiceRegistry.cs` | static | 軽量サービスロケータ | NFR-08 / logical-components §7 |
| `AppManager.cs` | MB | Bootstrap（登録→初回遷移判定） | logical-components §2.1 |
| `Navigation/INavigationService.cs` | IF | 型安全遷移契約 | US-TECH-04 |
| `Navigation/NavigationService.cs` | impl | SceneId→シーン名マップ＋履歴 | FR-02 / BR-12〜15 |
| `Storage/IStorageService.cs` | IF | 永続化契約 | NFR-07 |
| `Storage/StorageService.cs` | impl(最小) | profile 保存/読込・対ファイル整合スキップ | BR-04〜07 |
| `Audio/IAudioService.cs` | IF | 録音/再生契約（実装 U3） | services.md |
| `Content/IContentService.cs` | IF | コンテンツ取得契約 | NFR-05 |
| `Content/ContentService.cs` | impl(器) | NotImplemented を Result 返却 | NFR-05 |

### Geidai.Tests（`Assets/Scripts/Tests/EditMode/` — Editor 専用 / overrideReferences）
| ファイル | 種別 | 役割 | トレース |
|---|---|---|---|
| `Geidai.Tests.asmdef` | asmdef | nunit + FsCheck + FSharp.Core | NFR-09 |
| `WavCodecTests.cs` | PBT | Encode→Decode ラウンドトリップ・フォーマット一致 | NFR-09 |
| `PitchMathTests.cs` | PBT | cents↔ratio 逆変換・semitone 線形 | NFR-09 |
| `SerializationTests.cs` | PBT | SoundEffectSettingsData / UserProfile JSON 往復 | NFR-09 |

### アセット / パッケージ
- `Assets/Settings/UITheme_Default.asset`（MCP `Unity_RunCommand` で生成）
- `Packages/manifest.json`：`com.github-glitchenzo.nugetforunity`（UPM git）追加
- `Assets/packages.config`：`FsCheck 2.16.6` / `FSharp.Core 4.7.2`
- `Assets/Packages/`：FsCheck / FSharp.Core DLL（コミット済み＝復元不要で即ビルド可）
- `Assets/NuGet.config`：NuGetForUnity 設定

## 3. MCP 検証結果（公式 Unity MCP Server）
- **接続**: `user-unity-mcp`（serverStatus=ready）。ツール: `Unity_GetConsoleLogs` / `Unity_RunCommand` / `Unity_AssetGeneration_*` / `Unity_*Capture*`。
- **コンパイル**: 本体・テストアセンブリともに `Unity_GetConsoleLogs` で **Error 0**（残警告は AI Assistant アカウント API 待ちのみ、当方コード無関係）。
- **純粋関数スモーク**（`Unity_RunCommand`）: WavCodec ラウンドトリップ maxErr=1.53e-05、PitchMath 700cents 逆変換一致。
- **プロパティ同期スモーク**（各 500 ケース）: WavCodec **PASS**（maxErr=0）/ PitchMath **PASS**（maxDiff≈1.8e-12）/ Serialization **PASS**。
- **UITheme アセット生成**: `Assets/Settings/UITheme_Default.asset` 作成成功。
- **正式 NUnit/FsCheck 実行**: Build & Test ステージで実施（TestRunnerApi は非同期で、MCP 一発実行だとドメインリロードによりコールバック結果を取得できないため、当ステージでは同期スモークで代替検証）。

## 4. 既知の TODO / 後続ユニットでの統合
- **実シーン適用**（Home.unity 等への `ScreenRootBase`/`ResponsiveCanvasConfigurator`/`SafeAreaFitter` 付与）→ U2 以降で Unity MCP 経由。
- **NavigationService のシーン登録**: `Register`/`Theme` はシーン未整備のため未登録（`NotFound` 返却）。U2/U5 で追加。
- **録音実装の一本化**（`IAudioService` 実装、既存 `RecorderWithEffects`/`VoiceRecordingSection` の整理）→ U3 / NFR-08。
- **StorageService の原子的置換・破損復旧の本実装**→ U4。
- **ContentService 本実装**（お題/ゲームパラメータ）→ U5/U6。
- **`AppManager.navigateOnStart`** は U1 では既定 false（シーン未整備のため）。エントリシーン整備時に有効化。

## 5. Unity 取り込み手順（新規クローン時）
1. Unity 6000.4.2f1 で本プロジェクトを開く（`Assets/Packages/` の DLL はコミット済みのため NuGet 復元不要）。
2. 初回インポートでコンパイル → `Geidai.Common` / `Geidai.Services` / `Geidai.Tests` が生成される。
3. Test Runner（Window > General > Test Runner）→ EditMode で `Geidai.Tests` を実行（PBT）。
