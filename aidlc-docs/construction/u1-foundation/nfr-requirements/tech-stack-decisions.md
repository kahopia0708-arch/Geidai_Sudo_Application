# U1 基盤 — Tech Stack Decisions（技術選定・根拠）

**ユニット**: U1 基盤
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Requirements（Part 2）
**決定**: Q1〜Q7＝すべて A（推奨）

> U1 で確定する技術選定。以降のユニット（U2〜U6）も原則これに従う（横断基盤のため）。

---

## 1. エンジン / 言語
- **Unity 6000.4.2f1 / URP / uGUI / C#**（既存踏襲）。
- **根拠**: 既存 Brownfield 実装・チームの習熟・モバイル対応。

## 2. モジュール分割
- **Assembly Definition**: `Geidai.Common` / `Geidai.Services` / `Geidai.Foundation` / `Geidai.Rec` / `Geidai.Collection` / `Geidai.Theme` / `Geidai.Game1` / `Geidai.Tests`。
- **根拠**: 保守性・ビルド時間・テスト分離（NFR-08）。依存は一方向（モジュール→Services→Common）。
- **留意**: 既存 `Assembly-CSharp` からの段階移行を U1 で開始。

## 3. 永続化 / シリアライズ
- **保存先**: `Application.persistentDataPath`（`profile.json` / `sounds/{id}.wav` / `sounds/{id}.meta.json`）。
- **シリアライザ**: Unity 標準 **`JsonUtility`**。
- **根拠**: 追加依存なし・軽量（Q7=A）。
- **留意/リスク**: `JsonUtility` は Dictionary・null 許容・多態に弱い。ドメインモデル（SoundClipMeta/SoundEffectSettings/UserProfile）は **JsonUtility でシリアライズ可能な素直な構造**で設計する。列挙は int/文字列で保持。不足が判明した場合のみ軽量 JSON ライブラリ（例: Newtonsoft）採用を再検討（その場合は本書を更新）。

## 4. 音声処理
- **録音/加工**: `VoiceRecordingSection`（Unity 標準 AudioFilter）に一本化（FR-07 / US-TECH-03）。重複 `RecorderWithEffects` は整理（U3）。
- **ゲーム用リアルタイム加工**: `PitchVariationService`（U6、非保存）。
- **WAV**: 44100Hz・モノラル・16bit・3秒固定（`WavCodec`）。
- **根拠**: 標準機能でモバイル性能を確保（NFR-06）、実装一本化で保守性向上。

## 5. UI 基盤
- **CanvasScaler**: Scale With Screen Size、参照解像度 1080×1920、Match=0.5（Q2=A）。
- **SafeArea**: `SafeAreaFitter`（`Screen.safeArea` 追従、再計算）（Q3=A）。
- **見た目データ**: `UITheme`（ScriptableObject）で Sさん が調整（US-TECH-07）。
- **根拠**: 端末横断・両向き対応（NFR-11/12）、コード非依存な UI 調整。

## 6. テスト
- **PBT**: **FsCheck** ＋ **Unity Test Framework（EditMode）**（Q6=A）。
- **対象**: WavCodec ラウンドトリップ、PitchMath 逆変換、設定/メタ JSON ラウンドトリップ（NFR-09）。
- **通常テスト**: NUnit（Unity Test Framework）で境界値・代表ケース。
- **根拠**: 純粋関数の不変条件検証に PBT が有効。

## 7. シーン操作 / 変更管理
- **Unity MCP（unityMCP）**経由でシーン/GameObject/プレハブ操作（US-TECH-05）。
- **Git ブランチ＋PR レビュー＋変更メモ**（NFR-10）。
- **根拠**: 再現性・軽量な変更管理。

## 8. プラットフォーム / ビルド
- **対象OS**: iOS 15+ / Android 8.0(API 26)+（Q1=A）。
- **ネットワーク**: 使用しない（完全オフライン / NFR-02）。
- **署名/ストア設定**: Build and Test 段階で扱う（インフラ設計は SKIP）。

## 9. 追加パッケージ（想定）
| 目的 | パッケージ | 状態 |
|---|---|---|
| PBT | FsCheck（NuGet/DLL 取り込み or UPM） | 導入予定（Code Generation 時） |
| テスト実行 | com.unity.test-framework | 既存/確認 |
| （不足時のみ）JSON | com.unity.nuget.newtonsoft-json | 保留（JsonUtilityで不足時） |

## トレース
Unity/AsmDef→NFR-08 / JsonUtility→NFR-08 / VoiceRecordingSection・WavCodec→FR-07/NFR-03/06 / UI基盤→NFR-11/12 / FsCheck→NFR-09 / MCP・Git→NFR-10 / 対象OS・オフライン→NFR-01/02。
