# U3 Rec — Business Rules（業務ルール）

**ユニット**: U3 Rec（録音・加工・保存）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Functional Design（Part 2）
**決定**: Q1〜Q7＝すべて A（推奨）
**対応**: US-REC-01/02/03, US-TECH-03 / FR-05〜08 / NFR-03/06/08 / SECURITY-15

> 本書は**技術非依存の業務ルール**。具体数値（フィルタ値・レンジ換算・許容遅延の実測基準）は NFR Requirements/Design で確定する。

---

## 1. 録音ルール（US-REC-01 / FR-05 / NFR-03）

- **BR-REC-01**: 録音は**3秒固定**で、経過後**自動停止**する（手動停止を主操作としない）。
- **BR-REC-02**: 録音フォーマットは **44100Hz / モノラル / 16bit PCM**（`AudioBuffer`＝132300サンプルに一致）。
- **BR-REC-03**: 録り直しは新規録音で**上書き**し、直前の未保存録音は破棄する（破棄前に §5 の離脱ルールが必要な文脈では確認する）。

## 2. マイク権限・フェイルセーフ（US-REC-01 AC3 / SECURITY-15）

- **BR-REC-10**: 録音開始時に権限状態を確認する。未確認なら OS へ要求する。
- **BR-REC-11**: 権限**拒否**または**デバイス無し**の場合は録音を行わず、子ども向けに平易な案内を表示し、録音操作を無効化する。
- **BR-REC-12**: 録音・保存・再生の**いかなる失敗もクラッシュさせず** `Result`（理由コード）で表現する。
- **BR-REC-13**: 録音音声・加工結果は**端末外へ送信しない**（NFR-04）。

## 3. 加工ルール（US-REC-02 / FR-06 / NFR-06）

- **BR-REC-20**: 加工の**正準モデルは `SoundEffectSettingsData`**（ピッチ=半音 ±12 / ノイズ低減=None/Low/Medium/High / 音色=Original/Soft/Hard / リバーブ=0〜1）。
- **BR-REC-21**: 加工は**非破壊**。`AudioBuffer` は不変とし、設定は再生時に再適用する。
- **BR-REC-22**: 各加工は**バイパス（on/off）**を持ち、off の加工は再生へ反映しない（有無の比較を可能にする）。全体一括 on/off も可能とする。
- **BR-REC-23**: 加工パラメータは定義レンジ内にクランプする（範囲外入力は境界へ丸める）。
- **BR-REC-24**: ピッチは半音を基準とし、内部の再生ピッチ換算は `PitchMath`（U1 純粋関数）を用いる。
- **BR-REC-25**: 音色（TimbreType）は内部プリセット（lowpass/highpass/distortion 等の組）へ写像する。UI 表記「なし/ロボット/コーラス系」は TimbreType（Original/Hard/Soft）へマッピングし、表記は S さん調整可とする。

## 4. 旧設定との整合（US-TECH-03 / データ統一）

- **BR-REC-04**: 旧 `SoundEffectSettings`（cents 等）から `SoundEffectSettingsData` への換算では、**100 セント = 1 半音**とし、境界は最寄りへ丸める。
- **BR-REC-05**: 旧 `echo`/`distortion`/`lowPass`/`highPass` は保存モデルに独立フィールドを持たせず、**音色プリセットに内包**する（MVP 簡素化）。
- **BR-REC-06**: ノイズ低減の連続値（0〜1）は 4 段（None/Low/Medium/High）へ離散化する。

## 5. 保存ルール（US-REC-03 / FR-08 / NFR-07 / SECURITY-15）

- **BR-REC-30**: 保存は **WAVE（16bit PCM）** を `sounds/{id}.wav`、加工設定＋メタを `sounds/{id}.meta.json`（`SavedSound`）として、**必ず対で**書き込む（BR-05 と整合）。
- **BR-REC-31**: `id` は GUID（`SoundClipMeta.CreateNew`）で採番する（衝突しない一意名）。
- **BR-REC-32**: タイトルは任意入力。未入力時は既定名（日時等）を用いる。タイトルの検証（長さ等）は U1 検証方針に準ずる（過度な制約は課さない）。
- **BR-REC-33**: 保存失敗（I/O 等）時は**データを破損させず**、`Result(IOError)` として平易に通知する。原子的置換・破損フォールバックの**堅牢化は U4**（U3 は最小実装）。
- **BR-REC-34**: 保存先は**新形式（`sounds/`）に統一**し、旧 `MySoundCollection` 形式は使用しない。旧データの移行は U4 or 対象外。

## 6. 離脱・遷移ルール（Q7=A / US-TECH-04 整合）

- **BR-REC-40**: 未保存の録音がある状態での画面離脱（もどる/ホーム/端末バック）は**破棄確認**を行う（`ConfirmDialog` 再利用）。
- **BR-REC-41**: 遷移先が未対応/不存在の場合、`NavigationService` は `Result(NotFound)` を返し、クラッシュせず `ErrorPresenter` で通知する。

## 7. 実装一本化ルール（US-TECH-03 / FR-07 / NFR-08）

- **BR-REC-50**: 録音・加工は `IAudioService` 本実装（＋Rec コントローラ群）に**一本化**する。
- **BR-REC-51**: 重複/不要実装（`RecorderWithEffects` 等）は参照除去のうえ削除し、**ビルド・動作に影響を出さない**。
- **BR-REC-52**: 統合後も既存受入基準（US-REC-01〜03）を満たすこと。

---

## 8. トレーサビリティ

| ルール | ストーリー | 要件 |
|---|---|---|
| BR-REC-01〜03, 10〜13 | US-REC-01 | FR-05 / NFR-03 / SECURITY-15 |
| BR-REC-20〜25 | US-REC-02 | FR-06 / NFR-06 |
| BR-REC-04〜06 | US-TECH-03 | FR-07 |
| BR-REC-30〜34 | US-REC-03 | FR-08 / NFR-07 / SECURITY-15 |
| BR-REC-40〜41 | US-REC-*, US-TECH-04 | SECURITY-15 |
| BR-REC-50〜52 | US-TECH-03 | FR-07 / NFR-08 |
