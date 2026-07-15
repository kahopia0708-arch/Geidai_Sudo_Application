# Requirements Clarification Questions（要件確認質問）

参照元（正の入力源）: `プロジェクト概要.md`（最終更新 2026-07-11）および `input/` 一次資料。
既存実装との差分: `aidlc-docs/inception/reverse-engineering/plan-vs-implementation-gap.md`。

以下は、企画の「未確定事項・相談ポイント」および企画と既存実装の差分から、要件確定に必要な項目です。
各質問の `[Answer]:` の後ろに選択肢の記号（A/B/C…）を記入してください。該当がなければ最後の「Other」を選び、内容を記述してください。
回答が終わったら「done」等でお知らせください。

**注記**: 本アプリは研究会後に細部を調整予定のため、「現時点の暫定方針」で構いません（後で更新可能）。

---

## セクション1：スコープ・プラットフォーム

## Question 1
対象プラットフォーム（OS）は何を想定しますか？

A) iOS のみ

B) Android のみ

C) iOS / Android 両方（MVP はどちらか片方から着手）

D) タブレット優先（iPad 等の大画面）

X) Other (please describe after [Answer]: tag below)

[Answer]: C

## Question 2
MVP（最初に「触ってもらえる」プロトタイプ）に含める範囲は？

A) 最小 — Rec（録音・加工・保存）＋ MySoundCollection（保存音の一覧・再生）

B) 標準 — 上記 ＋ ミニゲーム1〜2種 ＋ weekly theme（お題）

C) 拡張 — 上記 ＋ 共有(キュレーション) ＋ 初回ユーザー登録

X) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 3
プロトタイプで最初に実装するミニゲームは？（企画の最優先は ①音合わせ・②音並べ）

A) ①音合わせ のみ

B) ①音合わせ ＋ ②音並べ

C) ①音合わせ ＋ ②音並べ ＋ ③音の神経衰弱

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## セクション2：企画と既存実装の差分（要判断）

## Question 4
共有機能（Place）の扱いは？（企画は「初回見送り・制作側キュレーション案」だが、既存に Place シーンが実装済み）

A) MVP では無効化/非表示にする（企画の見送り方針に合わせる）

B) 制作側キュレーション案（ユーザーが公開→制作側が審査→全体配布）を今回の設計に含める

C) 既存の Place 実装をそのまま残して活かす

X) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 5
Rec の録音時間は？（企画の7/10仕様は「3秒固定」、既存実装は10秒/300秒）

A) 3秒固定（企画通り）

B) 可変（上限を設定。例: 10秒）

C) お題/ゲームごとに可変

X) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 6
録音・加工の実装が2系統併存（`RecorderWithEffects`＝カスタムDSP / `VoiceRecordingSection`＝Unity標準Filter）。統一方針は？

A) `VoiceRecordingSection`（Unity標準Filter）に統一

B) `RecorderWithEffects`（カスタムDSP）に統一

C) 新規に一本化（両者を統合して再設計）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 7
Rec の加工「音色エフェクト」の選択肢は？（企画: なし/ロボット/コーラス/他1、既存: Robot/Bitcrush/Distortion 等）

A) 企画に合わせる（ロボット/コーラス系を中心に）

B) 既存実装の選択肢を活かす（Robot/Bitcrush/Distortion 等）

C) 研究会後に確定（今回は仮置きで進める）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 8
ユーザーの保存音をゲーム出題に使う（リアルタイムでピッチ加工しバリエーション生成、加工音は非保存）方針は？

A) MVP から対応する（ユーザー録音音をゲームで使う）

B) MVP は制作側音のみ。ユーザー音連携は後続フェーズ

C) 未定（技術検証を先に行ってから判断）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## セクション3：機能の要否

## Question 9
ゲーミフィケーション（経験値・カエルコイン・ライフ・課金）は？（企画では削除可能性ありと明記）

A) MVP では全て除外

B) 経験値のみ導入

C) 一式導入（コイン・ライフ含む）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 10
コレクション（保存音）のメタデータ・検索は？（企画: 写真・メモ・ニックネーム＋月別/キーワード検索）

A) 企画通り拡張する（メタデータ＋検索）

B) メタデータ拡張のみ（検索は後回し）

C) 現状維持（表示名＋加工設定のみ）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 11
初回ユーザー登録（生年・ニックネームのローカル保存）は？

A) 企画通り MVP で実装

B) 後続フェーズで実装（MVP は省略）

C) 不要

X) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 12
データ保存・プライバシー方針は？（子ども利用の可能性あり）

A) 完全ローカルのみ（サーバー送信なし）

B) ローカル中心＋共有時のみ制作側へ送信（キュレーション用）

C) 未定

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## セクション4：拡張機能（AI-DLC Extensions）の適用可否

## Question 13
セキュリティ拡張ルールをこのプロジェクトに適用しますか？

A) Yes — すべての SECURITY ルールをブロッキング制約として適用（本番品質のアプリ向け推奨）

B) No — SECURITY ルールをスキップ（PoC・プロトタイプ・実験的プロジェクト向け）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 14
レジリエンシー（信頼性）ベースラインを適用しますか？（AWS Well-Architected 由来の設計時ベストプラクティス。ローカル完結アプリでは効果限定的）

A) Yes — レジリエンシーベースラインを設計指針として適用

B) No — スキップ（ローカル完結・プロトタイプ中心のため）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 15
プロパティベーステスト（PBT）ルールを適用しますか？（音声変換・WAVエンコード等のラウンドトリップ検証に有効）

A) Yes — 全 PBT ルールをブロッキング制約として適用

B) Partial — 純粋関数・シリアライズのラウンドトリップ（例: WAV/JSON、cents↔pitch変換）に限定して適用

C) No — PBT ルールをスキップ

X) Other (please describe after [Answer]: tag below)

[Answer]: A
