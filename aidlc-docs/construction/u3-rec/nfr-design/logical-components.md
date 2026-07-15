# U3 Rec — Logical Components（論理コンポーネント）

**ユニット**: U3 Rec（録音・加工・保存）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Design（Part 2）
**決定**: Q1〜Q6＝すべて A（推奨）

> NFR Design パターン（`nfr-design-patterns.md`）を実現する論理コンポーネントの責務・連携・配置を定義する。API 署名の最終形・具体数値は Code Generation で確定。
> 配置アセンブリ: **`Geidai.Rec`**（新規・`Geidai.Common`/`Geidai.Services`/`UnityEngine.UI` 一方向依存）。既存 `Geidai.Common`/`Geidai.Services` は再利用・一部拡張。

---

## 1. コンポーネント一覧

| # | コンポーネント | 種別 | 配置 | 責務 |
|---|---|---|---|---|
| 1 | `RecScreenController` | MonoBehaviour（`ScreenRootBase` 継承） | Geidai.Rec | 画面ライフサイクル・`RecordingState` 統括・サブコントローラ調停・離脱時の破棄確認（`OnBackPressed`）。 |
| 2 | `RecordingController` | MonoBehaviour/部品 | Geidai.Rec | `MicPermissionGate`→録音開始、`RecordingClock` で3秒自動停止、`IAudioService` 経由で `AudioBuffer` 受領、残り時間表示連携。 |
| 3 | `EffectPanelController` | MonoBehaviour/部品 | Geidai.Rec | 加工 UI を `SoundEffectSettingsData` にバインドし、変更で `EffectChain.Apply` を呼ぶ（非破壊・バイパス）。 |
| 4 | `SavePromptController` | MonoBehaviour/部品 | Geidai.Rec | タイトル入力→確認→`IStorageService.SaveSound` 呼出→結果通知（`ErrorPresenter`）。 |
| 5 | `EffectChain` | POCO/部品（AudioSource＋Filter 束ね） | Geidai.Rec | `Apply(SoundEffectSettingsData)` で再生系（pitch/各 AudioFilter）を一括再構成。参照キャッシュ・GC 抑制。 |
| 6 | `RecordingClock` | POCO/コルーチン | Geidai.Rec | 3.0s 経過計測・カウントダウン供給・自動停止トリガ。 |
| 7 | `MicPermissionGate` | 静的/POCO | Geidai.Rec | 権限確認/要求（iOS/Android/デバイス有無）→ `MicPermissionStatus`。プラットフォーム分岐を内包。 |
| 8 | `SoundEffectMapper` | 静的・純粋関数 | Geidai.Rec（or Common） | 旧↔新設定変換・cents→半音・ノイズ4段離散化・reverb 正規化。**PBT 対象**。 |
| 9 | `IAudioService` 実装（例 `RecAudioService`） | POCO/サービス | Geidai.Rec | 録音/再生/停止の本実装（U1 IF を満たす）。`ServiceRegistry` 登録。 |

### 再利用（既存・変更なし〜軽微）
| コンポーネント | 配置 | U3 での利用 |
|---|---|---|
| `AudioBuffer` / `SoundEffectSettingsData` / `SavedSound` / `SoundClipMeta` | Geidai.Common.Models | 録音バッファ・加工設定・保存集約。 |
| `WavCodec` / `PitchMath` | Geidai.Common.Audio | WAV エンコード・ピッチ換算（U1 PBT 済）。 |
| `Result` / `Result<T>` / `ResultCode` | Geidai.Common.Results | 成否表現。 |
| `SafeLogger` | Geidai.Common.Utils | 非ログ（PII/内容を出さない）。 |
| `ScreenRootBase` / `ResponsiveCanvasConfigurator` / `SafeAreaFitter` / `UITheme` / `ErrorPresenter` / `ConfirmDialog` | Geidai.Common.UI | UI 基盤・通知・破棄確認。 |
| `INavigationService`/`NavigationService` | Geidai.Services.Navigation | ホーム等への安全遷移。 |
| `ServiceRegistry` / `AppManager` | Geidai.Services | サービス登録・DI。 |

### 拡張（既存 IF/実装に追加）
| 対象 | 追加内容 | 備考 |
|---|---|---|
| `IStorageService` | `Result SaveSound(SavedSound sound, AudioBuffer buffer)` | U3 で契約追加（Q5/Q6）。 |
| `StorageService` | `SaveSound` の U3 最小実装（wav→meta・失敗時 wav 削除） | 原子的置換は U4。 |

### 削除（US-TECH-03）
| 対象 | 措置 |
|---|---|
| `RecorderWithEffects.cs` / `Scean.cs`（該当時） | 参照除去のうえ削除。 |
| 旧 `MySoundCollectionStorage`/`SoundSavePaths`/グローバル `SoundEffectSettings` | 新形式移行後に整理（Collection=U4 と整合後に最終削除）。 |

---

## 2. 連携（録音→加工→保存の流れ）

```
[RecScreenController]
   ├─ RecordingController
   │     ├─ MicPermissionGate.Check/Request() → MicPermissionStatus
   │     ├─ IAudioService.StartRecording()
   │     ├─ RecordingClock(3.0s) → IAudioService.StopRecording() → Result<AudioBuffer>
   │     └─ (buffer を保持・hasUnsavedRecording=true)
   │
   ├─ EffectPanelController
   │     └─ (UI変更) → SoundEffectSettingsData 更新 → EffectChain.Apply(settings)
   │                                                     └─ AudioSource.pitch / AudioFilter 群
   │
   └─ SavePromptController
         ├─ SoundClipMeta.CreateNew(title) → SavedSound(meta, settings)
         ├─ IStorageService.SaveSound(savedSound, buffer)
         │       └─ WavCodec.Encode → {id}.wav / JsonUtility → {id}.meta.json
         └─ Result → 成功「保存できたよ」/ 失敗 ErrorPresenter(IOError)

離脱時: RecScreenController.OnBackPressed → (未保存あり) ConfirmDialog → NavigationService.GoTo(Home)
```

---

## 3. 依存関係（一方向）
```
Geidai.Rec  ──▶ Geidai.Services ──▶ Geidai.Common
   │                                     ▲
   └──────────────▶ UnityEngine.UI       │（Common は他に依存しない）
Geidai.Tests ──▶ Geidai.Rec / Services / Common
```
- 循環依存なし。ロジック（`SoundEffectMapper`・`RecordingClock`）は POCO/静的でテスト容易。

---

## 4. テスト観点（NFR-09）
| 対象 | 種別 | 内容 |
|---|---|---|
| `SoundEffectMapper` | PBT（EditMode） | cents→半音・ノイズ4段・reverb 正規化の境界/丸め/往復一貫性。 |
| `WavCodec`/`PitchMath` | PBT（U1 既存） | ラウンドトリップ・逆変換（再利用）。 |
| 録音→保存フロー | PlayMode/統合 | 3秒自動停止、`SaveSound` で wav＋meta 対生成、権限拒否/保存失敗の安全処理、バイパス反映。 |
| `SaveSound` 失敗クリーンアップ | 統合 | meta 失敗注入時に wav が残らない。 |

## 5. トレース
| コンポーネント | パターン(§) | ストーリー/要件 |
|---|---|---|
| EffectChain / EffectPanelController | §1 | US-REC-02 / NFR-06 |
| RecordingClock / RecordingController / IAudioService 実装 | §2 | US-REC-01 / NFR-03 |
| MicPermissionGate | §3 | US-REC-01(AC3) / SECURITY-15 |
| SaveSound 経路 / SavePromptController | §4 | US-REC-03 / NFR-07 |
| SoundEffectMapper | §5 | US-TECH-03 / NFR-09 |
| DI/一本化/削除 | §7 | US-TECH-03 / NFR-08 |
