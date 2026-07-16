# U3 Rec — NFR Requirements（非機能要件・受入値）

**ユニット**: U3 Rec（録音・加工・保存）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Requirements（Part 2）
**決定**: Q1〜Q6＝すべて A（推奨）
**入力**: `../functional-design/*`、`../../../inception/requirements/requirements.md`（NFR-01〜12 / SECURITY-15）、U1/U2 NFR 成果物

> U1/U2 の横断決定（プラットフォーム/レスポンシブ/SafeArea/オフライン/フェイルセーフ/PII/Unity MCP）を**踏襲**し、U3 固有の非機能目標と受入可能値を定義する。

---

## 1. 継承（U1/U2 で確定・U3 も踏襲）
- **プラットフォーム**（NFR-01）: iOS 15+ / Android 8.0(API26)+、スマホ〜タブレット、縦横両対応。
- **レスポンシブ**（NFR-11）: `ResponsiveCanvasConfigurator`（参照 1080×1920 / Match=0.5）。
- **SafeArea**（NFR-12）: `SafeAreaFitter`（`Screen.safeArea` 追従・向き/解像度変更で再計算）。
- **オフライン**（NFR-02）: 外部通信なし。可用性/スケーラビリティ/DR は N/A。
- **フェイルセーフ**（NFR-07/SECURITY-15）: 失敗は `Result`（理由コード）、クラッシュ/破損させない、フォールバック時は必ず警告。

## 2. パフォーマンス（NFR-03・NFR-06 / Q1=A）
- **録音開始**: 体感即時（タップから収録開始が引っかからない）。
- **録音長**: 3秒固定・自動停止（NFR-03 / 44100Hz・モノラル・132300サンプル）。
- **加工パラメータ反映**: **体感即時（目安 < 0.1s）**（US-REC-02 AC2）。再生中もできる限りライブ反映（最低限、次回再生で即反映）。
- **リアルタイム再生**: **可聴グリッチ（音の途切れ/ノイズ）を出さない**。
- **保存**: 3秒モノラル WAV（16bit PCM ≈ 264KB）の書込＋メタ JSON = **体感即時（目安 < 0.5s）**。
- **フレームレート**: ターゲット 60fps、最低 30fps を割らない。
- **受入**: 上記目安を主要端末で満たす（詳細計測は Build & Test）。

## 3. 加工のリアルタイム反映方式（NFR-06 / Q2=A）
- **方式**: Unity 標準 **AudioSource ＋ AudioFilter 群**で再生時に加工適用。自前 DSP は用いない（US-TECH-03・軽量化/保守性）。
  - ピッチ: `AudioSource.pitch`（`PitchMath` で半音↔比率換算）。
  - 音色（TimbreType）: lowpass/highpass/distortion プリセット。
  - リバーブ: `AudioReverbFilter`。ノイズ低減: フィルタ組合せ（4段）。
- **非破壊**: `AudioBuffer` は不変。設定は再生系へ反映（保存 WAV は生録音）。
- **バイパス**: 各加工 on/off＋全体一括 on/off。
- **受入**: 操作から反映が体感即時・バイパスで有無比較可・可聴グリッチ無し。

## 4. 信頼性・堅牢性（NFR-07 / SECURITY-15 / Q3=A）
- **マイク権限**: 拒否／デバイス無し→録音せず平易案内（`ErrorPresenter`）＋録音無効。
- **例外**: 録音/再生/保存の例外は必ず捕捉し `Result` で表現（クラッシュ禁止）。
- **保存失敗（I/O）**: データを破損させず通知、録音は保持して再試行可。原子的置換の堅牢化は U4。
- **対保存**: `sounds/{id}.wav` と `{id}.meta.json` は**両方成立で成功**扱い（片方欠損は不整合として扱う）。
- **受入**: 権限拒否/保存失敗を注入してもクラッシュせず、警告が出る。

## 5. プライバシー/セキュリティ（NFR-04 / SECURITY-15 / Q4=A）
- 録音音声・WAV・加工設定は**端末内（`persistentDataPath/sounds`）のみ**。**端末外送信禁止**。
- ログに PII/音声パス以外を出さない（`SafeLogger`）。本番ビルドで詳細エラー非表示（SECURITY-09）。
- マイクは**録音時のみ**使用（常時録音しない）。
- **受入**: ネットワーク送信が無いこと・ログに PII が出ないことを確認。

## 6. テスト容易性（NFR-09 / PBT / Q5=A）
- **既存 PBT 活用**: `WavCodec` ラウンドトリップ・`PitchMath` 逆変換は U1 で検証済み（再利用）。
- **U3 追加 PBT（軽量）**: 新規純粋換算関数の境界・丸めの一貫性
  - 旧 `cents` → `pitchSemitones`（100 セント=1 半音・丸め）。
  - ノイズ低減 連続(0〜1) → `NoiseLevel`（4段離散化）。
  - `reverbLevel`(mB) → `reverb`(0〜1) 正規化。
- **PlayMode/統合テスト**: 3秒自動停止、保存で wav＋meta が対生成、権限拒否/保存失敗の安全処理、加工バイパスの反映。
- **受入**: 追加 PBT がグリーン、フロー統合テストがグリーン（実行は Build & Test に集約可）。

## 7. 保守性（NFR-08 / NFR-10 / US-TECH-03 / Q6=A）
- **新規アセンブリ `Geidai.Rec`**（依存: `Geidai.Common`/`Geidai.Services`/`UnityEngine.UI` の一方向）。
- `IAudioService` 本実装＋Rec コントローラ群を配置。
- **`IStorageService` に `SaveSound` を追加**（U3 最小実装・U4 で原子的置換に強化）。
- 録音・加工は**標準 API（Microphone / AudioFilter）に一本化**、`RecorderWithEffects`/`Scean` 等の重複を削除（参照除去・ビルド影響なし）。保存形式は新形式（`sounds/{id}`）へ統一。
- Git ブランチ＋PR＋変更メモ（NFR-10）。実シーン配線は Unity MCP（US-TECH-05）。
- **受入**: モジュール境界でビルドが通り、循環依存が無い。旧重複が参照除去されている。

## 8. ユーザビリティ（NFR-05）
- 録音・再生・保存は**大きめタップ領域**（子ども/タブレット配慮）。加工は色＋アイコン＋かなラベル併用。
- 失敗はアイコン＋平易文言（`ErrorPresenter`）。見た目詳細は S さんが `UITheme`/Prefab で調整（US-TECH-07）。
- **受入**: Rec 画面テンプレート＋UITheme 差し替えで見た目が変わること。

## 9. 可用性/スケーラビリティ/DR
- **N/A（ローカル・オフライン）**: サーバー無し・端末内完結（NFR-02）。RESILIENCY のクラウド系ルールは N/A。

## トレース
NFR-01→§1 / NFR-11→§1 / NFR-12→§1 / NFR-03→§2 / NFR-06→§2,§3 / NFR-07→§4 / NFR-04・SECURITY-15→§5 / NFR-09→§6 / NFR-08・NFR-10→§7 / NFR-05→§8 / NFR-02→§9。
US-REC-01→§2,§4 / US-REC-02→§3 / US-REC-03→§4,§5 / US-TECH-03→§3,§7。
