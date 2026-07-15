# U3 Rec — NFR Requirements Plan（非機能要件・技術選定 計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U3 Rec（録音・加工・保存）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Requirements（Part 1: Planning）
**入力**: `../u3-rec/functional-design/*`、`../../inception/requirements/requirements.md`（NFR-01〜12）、U1/U2 NFR 成果物（`../u1-foundation/nfr-requirements/*`、`../u2-foundation/nfr-requirements/*`）
**対象NFR**: パフォーマンス(NFR-03/06)、ユーザビリティ(NFR-05)、信頼性/堅牢性(NFR-07)、プライバシー(NFR-04/SECURITY-15)、テスト容易性(NFR-09/PBT)、保守性(NFR-08/10)、レスポンシブ/SafeArea(NFR-11/12)

> 本ステージで U3 の**非機能目標の具体値**と**技術選定の差分**を確定する。
> **回答方法**: 各質問の `[Answer]:` に記号（例: `A`）。合う選択肢が無ければ「Other」。各質問に「(推奨)」あり。完了で「done」（または「全部推奨で」）。

---

## A. 実行チェックリスト（Part 2 で実行）
- [ ] `../u3-rec/nfr-requirements/nfr-requirements.md` を生成（U3 の NFR 目標・受入可能値）
- [ ] `../u3-rec/nfr-requirements/tech-stack-decisions.md` を生成（U3 の技術選定差分・根拠）
- [ ] 要件（NFR-01〜12 / SECURITY-15）・ストーリー（US-REC/US-TECH-03）とのトレース整合を確認

## B. 前提（U1/U2 で確定済み・U3 も踏襲。原則 再質問しない）
- **プラットフォーム**（NFR-01）: iOS 15+ / Android 8.0(API26)+、スマホ〜タブレット、縦横両対応。
- **レスポンシブ**（NFR-11）: `ResponsiveCanvasConfigurator`、参照解像度 1080×1920、Scale With Screen Size、Match=0.5。
- **SafeArea**（NFR-12）: `SafeAreaFitter`（`Screen.safeArea` 追従・向き/解像度変更で再計算）。
- **オフライン**（NFR-02）: 外部通信なし。可用性/スケーラビリティ/DR は N/A。
- **永続化/シリアライズ**（NFR-08）: `Application.persistentDataPath` ＋ Unity 標準 `JsonUtility`。WAV は `WavCodec`（16bit PCM）。
- **フェイルセーフ**（NFR-07/SECURITY-15）: 失敗は `Result`（理由コード）で表現し、クラッシュさせない。破損させない。フォールバック時は必ず警告。
- **セキュリティ既定**: PII・録音音声は端末外送信禁止・ログ非出力（`SafeLogger`）、本番ビルドで詳細エラー非表示（SECURITY-09）。
- **エンジン/言語**: Unity 6000.4.2f1 / URP / uGUI / C#。**シーン操作は公式 Unity AI Assistant（Unity MCP Server）**（US-TECH-05）。

> ※ 上記の変更が必要な場合のみ、該当質問で Other 指定してください。

## C. スコープ（U3 で確定する非機能の対象）
- 録音（3秒・自動停止）の**応答性**、加工プレビューの**リアルタイム体感**、保存の**レイテンシ・安全性**。
- マイク権限フェイルセーフ、保存失敗時のデータ非破壊。
- 録音音声のプライバシー（端末内のみ）。
- 旧→新加工設定の**換算ロジック**のテスト戦略。
- **スコープ外**: コレクション一覧/視聴/削除（U4）、永続化の原子的置換本実装（U4）、お題連携（U5）、ゲーム用ピッチ出題（U6）。

---

## D. NFR・技術選定に関する質問（Q1〜Q6）

## Question 1（パフォーマンス目標 / NFR-03・NFR-06）
U3 の性能目標は？（録音・加工プレビュー・保存が対象）

A) (推奨) **録音開始＝体感即時**（タップから収録開始が引っかからない）／**加工パラメータ変更→再生反映＝体感即時**（目安 < 0.1s・US-REC-02 AC2）／**リアルタイム再生中に音の途切れ（グリッチ）を出さない**／**保存（3秒モノラル WAV・小容量 ≈ 264KB）= 体感即時（目安 < 0.5s）**。ターゲット 60fps／最低 30fps を割らない。詳細計測は Build & Test。

B) より厳しく：加工反映 < 0.05s、保存 < 0.2s。

C) 具体数値は設定せず「体感で引っかからない」を定性目標にする。

D) Other（[Answer]: の後に記述）

[Answer]:

## Question 2（加工のリアルタイム反映方式・受入 / NFR-06）
加工プレビューの実現方式と受入基準は？（非破壊・Q3=A 前提）

A) (推奨) **Unity 標準の AudioSource ＋ AudioFilter 群**で再生時に加工を適用（ピッチは `AudioSource.pitch`＝`PitchMath` 換算、音色/リバーブ/ノイズ低減はフィルタ）。パラメータ変更は**再生中もできる限りライブ反映**（少なくとも次回再生で即反映）。**受入＝操作から反映が体感即時・バイパス切替で有無を比較でき・可聴グリッチが出ない**。DSP を自前実装せず標準フィルタに寄せることで軽量化・保守性を確保（US-TECH-03）。

B) 自前 DSP（`RecorderWithEffects` 系）でオフライン加工してから再生（表現力優先・コスト増）。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 3（信頼性・マイク権限フェイルセーフ / NFR-07・SECURITY-15）
録音・保存の失敗時のふるまいは？

A) (推奨) マイク**権限拒否／デバイス無し→録音せず**平易案内（`ErrorPresenter`）＋録音無効。**録音/再生/保存の例外は必ず捕捉し `Result` で表現、クラッシュ禁止**。保存失敗（I/O）→**データを破損させず**通知し、録音は保持して再試行可（原子的置換の堅牢化は U4）。対ファイル（wav＋meta）は**両方成立で成功**扱い。**受入＝権限拒否/保存失敗を注入してもクラッシュせず警告が出る**。

B) 権限は起動時に先回り要求してから録音可否を決める。以降は A と同じ。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 4（プライバシー / NFR-04・SECURITY-15）
録音音声・加工データの扱いは？

A) (推奨) 録音音声・WAV・加工設定は**端末内（`persistentDataPath/sounds`）のみ**に保存し、**端末外へ送信しない**。ログに音声データ/パス以外の PII を出さない（`SafeLogger`）。マイクは録音時のみ使用（常時録音しない）。**受入＝ネットワーク送信が無いこと・ログに PII が出ないことを確認**。

B) Other（[Answer]: の後に記述）

[Answer]:

## Question 5（テスト容易性 / NFR-09・PBT）
U3 の検証方針は？

A) (推奨) **既存 PBT を活用**：`WavCodec` ラウンドトリップ・`PitchMath` 逆変換は U1 で実装/検証済み。U3 は**新規の純粋換算関数（旧 cents→半音／ノイズ連続→4段／reverb 正規化）に軽量 PBT を追加**（境界・丸めの一貫性）。録音→加工→保存の**フローは PlayMode/統合テスト**で検証（3秒自動停止、保存で wav＋meta が対生成、権限拒否/保存失敗の安全処理）。実行は Build & Test に集約可。

B) 換算関数の PBT は行わず、PlayMode/統合テストと手動確認のみ。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 6（保守性・技術選定差分 / NFR-08・NFR-10・US-TECH-03）
U3 の実装配置・IF 拡張の方針は？

A) (推奨) **新規アセンブリ `Geidai.Rec`**（依存は `Geidai.Common`/`Geidai.Services`/`UnityEngine.UI` の一方向）。`IAudioService` 本実装と Rec コントローラ群を配置。**`IStorageService` に `SaveSound` を追加**（U3 最小実装・U4 で原子的置換に強化）。録音・加工は**標準 API（Microphone / AudioFilter）に一本化**し、`RecorderWithEffects`/`Scean` 等の重複を削除（参照除去・ビルド影響なし）。保存形式は新形式（`sounds/{id}`）へ統一。実シーン配線は Code Generation 以降で Unity MCP。

B) `Geidai.Rec` を作らず `Geidai.Services` 内に録音実装を置く（アセンブリ増やさない）。

C) Other（[Answer]: の後に記述）

[Answer]:

---

## E. 完了条件
- Q1〜Q6 に回答 → 曖昧回答は追質問 → nfr-requirements / tech-stack-decisions を生成 → 承認ゲート。
- U1/U2 の横断決定を踏襲し、U3 固有の差分（録音応答・リアルタイム加工・保存安全・換算テスト・`Geidai.Rec`/`SaveSound`）のみを明示する。
- 要件（NFR-01〜12 / SECURITY-15）とストーリー（US-REC/US-TECH-03）へのトレースが取れている。
