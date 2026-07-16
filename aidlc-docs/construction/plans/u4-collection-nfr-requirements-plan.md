# U4 Persistence/Collection — NFR Requirements Plan（非機能要件・技術選定 計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U4 Persistence/Collection（永続化本実装・コレクション）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Requirements（Part 1: Planning）
**入力**: `../u4-collection/functional-design/*`、`../../inception/requirements/requirements.md`（NFR-01〜12 / RESILIENCY-01）、U1/U2/U3 NFR 成果物（`../u1-foundation|u2-foundation|u3-rec/nfr-requirements/*`）
**対象NFR**: パフォーマンス/スケーラビリティ(NFR-06)、信頼性/堅牢性(NFR-07・US-TECH-06=U4 主眼)、ユーザビリティ(NFR-05)、プライバシー(NFR-04)、テスト容易性(NFR-09/PBT)、保守性(NFR-08/10)、レスポンシブ/SafeArea(NFR-11/12)

> 本ステージで U4 の**非機能目標の具体値**と**技術選定の差分**を確定する。
> **回答方法**: 各質問の `[Answer]:` に記号（例: `A`）。合う選択肢が無ければ「Other」。各質問に「(推奨)」あり。完了で「done」（または「全部推奨で」）。

---

## A. 実行チェックリスト（Part 2 で実行）
- [x] `../u4-collection/nfr-requirements/nfr-requirements.md` を生成（U4 の NFR 目標・受入可能値）
- [x] `../u4-collection/nfr-requirements/tech-stack-decisions.md` を生成（U4 の技術選定差分・根拠）
- [x] 要件（NFR-04/05/06/07/08/09/11/12 / RESILIENCY-01）・ストーリー（US-COL/US-TECH-06）とのトレース整合を確認

> **回答**: Q1〜Q6＝すべて A（推奨）。矛盾なし。Part 2 実行済み（2026-07-15）。

## B. 前提（U1〜U3 で確定済み・U4 も踏襲。原則 再質問しない）
- **プラットフォーム**（NFR-01）: iOS 15+ / Android 8.0(API26)+、スマホ〜タブレット、縦横両対応。
- **レスポンシブ**（NFR-11）: `ResponsiveCanvasConfigurator`、参照解像度 1080×1920、Scale With Screen Size、Match=0.5。固定 px 依存を排除。
- **SafeArea**（NFR-12）: `SafeAreaFitter`（`Screen.safeArea` 追従・向き/解像度変更で再計算）。
- **オフライン**（NFR-02）: 外部通信なし。可用性/スケーラビリティ(サーバ)/DR は N/A。
- **永続化/シリアライズ**（NFR-08）: `Application.persistentDataPath` ＋ `JsonUtility`。WAV は `WavCodec`（16bit PCM）。
- **フェイルセーフ**（NFR-07/SECURITY-15）: 失敗は `Result`（理由コード）で表現、クラッシュさせない・破損させない・フォールバック時は警告。
- **セキュリティ既定**: PII は端末外送信禁止・ログ非出力（`SafeLogger`）、本番ビルドで詳細エラー非表示（SECURITY-09）。
- **エンジン/言語**: Unity 6000.4.2f1 / URP / uGUI / C#。**シーン操作は公式 Unity AI Assistant（Unity MCP Server）**（US-TECH-05）。
- **UI ハンドオフ**（US-TECH-07）: 枠組みは前本、意匠は S さん。

> ※ 上記の変更が必要な場合のみ、該当質問で Other 指定してください。

## C. スコープ（U4 で確定する非機能の対象）
- コレクション**一覧読込・スクロール・視聴**の応答性、**原子的書込**のコスト、**メタ編集/削除**の安全性。
- **堅牢性（U4 の主眼）**: 原子的置換・破損スキップ・空フォールバック（US-TECH-06 / NFR-07）。
- 写真/メモ/ニックネームの**プライバシー**（端末内のみ）。
- 純粋関数（絞込/検索）とメタ往復・原子的書込の**テスト戦略**。
- **保守性**: 新 `Geidai.Collection` アセンブリ、`IStorageService` 拡張、`IPhotoPicker` 抽象、視聴の再生忠実度の配置。
- **スコープ外**: 新規録音/加工（U3 済）、お題（U5）、ゲーム出題（U6）、旧 `MySoundCollection` 移行（対象外）、クラウド/共有（Place 除外）。

---

## D. NFR・技術選定に関する質問（Q1〜Q6）

## Question 1（パフォーマンス・スケーラビリティ目標 / NFR-06）
U4 の性能・規模目標は？（一覧読込・スクロール・視聴・原子的書込が対象）

A) (推奨) 個人利用のコレクション規模を想定（**数十〜数百件**）。**一覧を開いてから表示まで体感即時**（目安：100 件程度で < 0.5s）／**スクロールは 60fps・最低 30fps を割らない**（サムネ遅延読み・固定px排除）／**視聴の再生開始＝体感即時**（wav デコード込みで目安 < 0.3s）／**原子的書込（meta/写真/削除）= 体感即時**（目安 < 0.5s）。件数が非常に多い場合はスクロール仮想化/遅延読みで対応（実装詳細は NFR Design）。詳細計測は Build & Test。

B) より厳しく：一覧 < 0.2s、視聴開始 < 0.1s、書込 < 0.2s。

C) 具体数値は設定せず「体感で引っかからない」を定性目標にする。

D) Other（[Answer]: の後に記述）

[Answer]:A

## Question 2（信頼性・堅牢性の受入基準 / NFR-07・US-TECH-06 — U4 の主眼）
永続化の原子性・破損耐性の受入基準は？

A) (推奨) **原子的書込**＝「一時ファイルへ書込→成功後に本ファイルへ原子的置換」を profile / meta / wav / 写真に適用し、**書込を中断/失敗させても既存データが無傷**であること。**破損/欠損**（meta パース不可・対 wav 欠損）は**安全にスキップ**し他項目は正常表示。**空/初期**はフォールバック（空状態）。全失敗は `Result` で通知しクラッシュしない。**受入＝ (1) meta を故意に破損→一覧が他項目を正常表示 (2) 書込を中断注入→旧データ維持 (3) 空/欠損→空状態**。重要度＝Collection=Critical（RESILIENCY-01）。

B) 原子性は meta/profile のみ（小容量テキスト）。wav/写真は単純書込。破損スキップ・空フォールバックは A と同じ。

C) Other（[Answer]: の後に記述）

[Answer]:A

## Question 3（プライバシー / NFR-04・SECURITY-03）
拡張メタ（写真/メモ/ニックネーム）と音声の扱いは？

A) (推奨) 写真・メモ・ニックネーム・音声・設定は**端末内（`persistentDataPath`）のみ**に保存し、**端末外へ送信しない**。写真取得（`IPhotoPicker`）もクラウドアップロードを行わない。ログに PII（写真パス実体・メモ・ニックネーム）を出さない（`SafeLogger`）。**受入＝ネットワーク送信が無いこと・ログに PII が出ないことを確認**。

B) Other（[Answer]: の後に記述）

[Answer]:A

## Question 4（テスト容易性 / NFR-09・PBT）
U4 の検証方針は？

A) (推奨) **純粋関数に PBT**：絞込/検索 `Filter(items, query)`（AND 合成・部分一致・月導出／不変条件＝結果は入力の部分集合・条件空なら全件・冪等）と、**メタ JSON 往復**（`SavedSound` serialize↔deserialize・拡張フィールドの後方互換＝欠損時既定値）を PBT。**原子的書込の性質**（成功で新値・中断で旧値維持）と **破損スキップ/削除** は EditMode/統合テストで検証（故障注入）。実行は Build & Test に集約可。

B) PBT は行わず、統合テストと手動確認のみ。

C) Other（[Answer]: の後に記述）

[Answer]:A

## Question 5（保守性・アセンブリ/IF 拡張 / NFR-08・NFR-10）
U4 の実装配置と `IStorageService` 拡張の方針は？

A) (推奨) **新規アセンブリ `Geidai.Collection`**（依存は `Geidai.Common`/`Geidai.Services`/`UnityEngine.UI` の一方向）にコレクション画面/コントローラを配置。**`IStorageService` を後方互換拡張**：`DeleteSound(id)`／`SaveMeta(SoundClipMeta)`（または `UpdateSound`）を追加し、既存 `SaveSound`/`SaveProfile` を**原子的置換へ強化**（シグネチャ不変）。写真取得は **`IPhotoPicker` 抽象**（`Geidai.Services` 側 IF＋U4 スタブ実装、実機ピッカーはフォローアップ）。旧 `MySoundCollectionStorage`/`SoundSavePaths` は新方式へ集約（物理削除はシーン再配線と同時）。

B) `Geidai.Collection` を作らず `Geidai.Foundation` などに相乗り（アセンブリ増やさない）。

C) Other（[Answer]: の後に記述）

[Answer]:A

## Question 6（視聴の再生忠実度・技術配置 / Q7=A の技術決定）
コレクションの「視聴」で保存エフェクトを再適用するための配置は？（Collection→Rec の依存を作らない）

A) (推奨) **共有再生を Services 層に用意**：`IAudioService` を**後方互換拡張**（例: `Play(AudioBuffer, SoundEffectSettingsData)` オーバーロード）し、エフェクト適用（`EffectChain` 相当）を **`Geidai.Services.Audio`（または共有 Audio モジュール）へ配置**して Rec/Collection の双方が利用。これにより **Collection は `Geidai.Services` のみ依存**（Rec 非依存）。U3 の `RecAudioService`/`EffectChain` は共有実装へ寄せる（後方互換・録音側の挙動は不変）。ServiceRegistry で実装解決。

B) コレクションの視聴は**生 WAV をそのまま再生**（エフェクト非適用・簡素）。エフェクト再適用は将来。

C) 共有せず、Collection から U3 `EffectChain` を参照（`Geidai.Collection → Geidai.Rec` 依存を許可）。

D) Other（[Answer]: の後に記述）

[Answer]:A

---

## E. 完了条件
- Q1〜Q6 に回答 → 曖昧回答は追質問 → nfr-requirements / tech-stack-decisions を生成 → 承認ゲート。
- U1〜U3 の横断決定を踏襲し、U4 固有の差分（原子的置換の受入・一覧/視聴性能・写真プライバシー・純粋関数PBT・`Geidai.Collection`/`IStorageService` 拡張・共有再生配置）のみを明示する。
- 要件（NFR-04〜12 / RESILIENCY-01）とストーリー（US-COL/US-TECH-06）へのトレースが取れている。
