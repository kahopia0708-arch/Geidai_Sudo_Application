# U3 Rec — Tech Stack Decisions（技術選定・差分）

**ユニット**: U3 Rec（録音・加工・保存）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Requirements（Part 2）
**決定**: Q1〜Q6＝すべて A（推奨）

> U1/U2 の技術選定を土台に、U3 固有の差分と根拠を明示する。数値パラメータの最終確定・実装詳細は NFR Design / Code Generation で行う。

---

## 1. 録音（Recording）
| 項目 | 決定 | 根拠 |
|---|---|---|
| 録音 API | Unity **`Microphone`**（標準） | 追加依存なし・オフライン・クロスプラットフォーム。既存 `VoiceRecordingSection` も採用済み。 |
| フォーマット | 44100Hz / モノラル / 16bit PCM（3秒固定） | NFR-03・`AudioBuffer`（132300サンプル）と一致。 |
| 権限 | 録音開始時に要求（拒否/無しは安全案内） | SECURITY-15 / Q3=A。iOS: マイク使用許諾（Info.plist）、Android: RECORD_AUDIO。 |
| バッファ受領 | `IAudioService.StopRecording()` → `Result<AudioBuffer>` | 一本化した契約（US-TECH-03）。 |

## 2. 加工（Effects）
| 項目 | 決定 | 根拠 |
|---|---|---|
| 方式 | Unity 標準 **AudioSource ＋ AudioFilter 群**（非破壊・再生時適用） | Q2=A。自前 DSP を排し軽量・保守性（US-TECH-03）。 |
| ピッチ | `AudioSource.pitch`（`PitchMath` で半音↔比率） | U1 純粋関数を再利用。 |
| 音色 | `AudioLowPassFilter`/`AudioHighPassFilter`/`AudioDistortionFilter` プリセット（TimbreType） | 既存プリセット思想を踏襲。具体値は NFR Design。 |
| リバーブ | `AudioReverbFilter`（reverb 0〜1 を内部レベルへ換算） | 標準フィルタ。 |
| ノイズ低減 | フィルタ組合せで 4 段（NoiseLevel） | 連続値を離散化（BR-REC-06）。 |
| バイパス | 各 EffectKind on/off＋全体一括 | US-REC-02 AC3。 |

## 3. 保存（Persistence）
| 項目 | 決定 | 根拠 |
|---|---|---|
| WAV 生成 | U1 **`WavCodec`**（16bit PCM エンコード） | 既存純粋関数（PBT 済）を再利用。 |
| 保存 IF | **`IStorageService.SaveSound(SavedSound, AudioBuffer)` を新規追加** | Q5/Q6=A。U1 IF に保存メソッドが無かったギャップを解消。U3 最小実装・U4 で原子的置換に強化。 |
| 保存形式 | `persistentDataPath/sounds/{id}.wav` ＋ `{id}.meta.json`（`SavedSound`） | U1 形式へ統一。旧 `MySoundCollection` 形式は廃止。 |
| シリアライズ | Unity 標準 `JsonUtility` | NFR-08・U1 踏襲。 |

## 4. モジュール構成（Assembly）
| 項目 | 決定 | 根拠 |
|---|---|---|
| 新規アセンブリ | **`Geidai.Rec`**（`Geidai.Common`/`Geidai.Services`/`UnityEngine.UI` 参照・一方向） | NFR-08。モジュール境界の明確化・循環依存回避。 |
| 配置 | `IAudioService` 本実装＋Rec コントローラ群（RecScreen/Recording/EffectPanel/SavePrompt） | Q6=A。 |
| テスト | `Geidai.Tests` に U3 テスト追加（`Geidai.Rec` 参照） | 既存テストアセンブリを拡張。 |

## 5. 削除・整理（US-TECH-03）
| 対象 | 措置 | 備考 |
|---|---|---|
| `RecorderWithEffects.cs` | 削除（参照除去） | 重複 DSP 実装。 |
| `Scean.cs`（該当時） | 削除/整理 | 不要スクリプト。 |
| 旧 `MySoundCollectionStorage`/`SoundSavePaths`/グローバル `SoundEffectSettings` | 新形式へ移行後に整理 | Rec の保存経路を `StorageService` に統一。Collection 側（U4）と整合を取ってから最終削除。 |
| 実シーン配線 | Unity MCP で差し替え（Code Generation 以降） | US-TECH-05。 |

## 6. テスト技術（NFR-09 / PBT）
| 項目 | 決定 |
|---|---|
| PBT フレームワーク | FsCheck ＋ Unity Test Framework（EditMode）※U1 踏襲 |
| U3 追加 PBT | cents→半音／ノイズ連続→4段／reverb 正規化 の純粋換算 |
| PlayMode/統合 | 録音3秒自動停止・保存 対生成・権限/保存失敗の安全処理 |

## 7. 未確定（NFR Design / Code Generation で確定）
- 各 AudioFilter の具体パラメータ（cutoff/distortion/reverb レベル、ノイズ4段の値）。
- リアルタイム反映の実装（再生中ライブ更新のコルーチン/更新戴点、GC 削減の具体策）。
- `SaveSound` の I/O 実装詳細（書込順序・失敗時ロールバックの最小形）。
- iOS/Android 権限要求 UX の実装（プラットフォーム分岐）。
