# Fさん導入ドキュメント — 要件確認質問

**ワークストリーム**: Fさん向け導入ドキュメント  
**作成**: 2026-08-19  
**入力**: 「新しいメンバーFさんが参加。ゲーム開発・音楽理論の設計が得意。シーン単位で前本と分担。Fさん向け導入ドキュメントを作りたい。」

各質問の `[Answer]:` のあとに字母（A, B, …）を書いてください。選択肢に無い場合は最後の **X) Other** を選び、同じ行に説明を足してください。  
全部埋めたらチャットで完了と知らせてください。

---

## Question 1
ドキュメントの置き場所はどこにしますか。

A) `docs/Fさん向けガイド.md`（既存の `docs/Sさん向けガイド.md` と同列。推奨）

B) ルート `README.md` に導入節を足し、独立ファイルは作らない

C) `aidlc-docs/` 配下（実装記録側。Fさん向けの入口としてはやや奥まる）

X) Other (please describe after [Answer]: tag below)

[Answer]:A

## Question 2
Fさんの想定スキルに合わせた書き方はどれですか。

A) Unity / C# の経験はある前提。リポジトリ固有の約束事・シーン所有・共通IFを短く書く

B) Unity はこれから。環境構築（Hub、バージョン、Play、テスト）から丁寧に書く

C) 導入編（最短で動かす）とリファレンス編（アーキテクチャ・音楽理論対応）の二部構成

X) Other (please describe after [Answer]: tag below)

[Answer]:AとC

## Question 3
初版に含める範囲はどれですか。

A) 環境構築、役割分担、シーン一覧、Git/PR、触ってよい範囲／触らない共通基盤

B) A に加え、音響パイプラインと音楽理論の実装対応（セント難易度、ピッチ、ウィレムスの位置づけ）

C) B に加え、新しいゲームシーンを追加する手順（asmdef / Navigation / Build Settings）

X) Other (please describe after [Answer]: tag below)

[Answer]:C

## Question 4
前本と F さんのシーン分担表はどう扱いますか。

A) 「シーン単位で分担する」型だけ書き、担当列は未定のまま残す

B) 暫定案を入れる（前本＝基盤・Home/Rec/Collection/Library/Create、Fさん＝ゲームシーンと音響設計）。後で更新する前提

C) この質問への回答で担当を決めて、初版から担当名を固定する

X) Other (please describe after [Answer]: tag below)

[Answer]:/Users/maemoto/Downloads/20260818打ち合わせ記録.pdf に記載あり

## Question 5
Question 4 で C を選んだ場合のみ、Fさんが初版から持つシーンはどれですか。（A/B の場合は空欄で構いません）

A) 未実装ゲーム（②音並べ以降）のみ。①音合わせは前本の既存実装を維持

B) ゲーム選択＋未実装ゲーム。①音合わせの改修も含む

C) ゲーム全般に加え、音図鑑／音づくりの音響まわりも持つ

X) Other (please describe after [Answer]: tag below)

[Answer]:/Users/maemoto/Downloads/20260818打ち合わせ記録.pdf から検討

## Question 6
音楽理論・教材設計の書き方はどれですか。

A) 既存仕様の読み方案内に留める（難易度セント表、各ゲームの聴き分け目的、企画ドキュメントへのリンク）

B) 実装との対応表まで書く（例: セント → `AudioSource.pitch`、`SoundMatchConfig`、Recipe のピッチ／リバーブ／音色）

C) 音楽理論は別ファイルにし、導入ドキュメントではリンクのみ

X) Other (please describe after [Answer]: tag below)

[Answer]:B

## Question 7
既存の Sさん向けガイドとの関係はどれですか。

A) 見た目・お題・イラストは Sさんガイドへ委譲し、Fさんガイドは実装・シーン所有・音響／ゲーム設計に集中

B) Fさんガイドにも見た目調整の要点を短く再掲する

X) Other (please describe after [Answer]: tag below)

[Answer]:B

## Question 8
ルート `README.md` の役割表は更新しますか。

A) Fさん（ゲーム実装／音楽理論設計）を追加し、ガイドへリンクする

B) ガイドのみ作成し、README は当面変えない

X) Other (please describe after [Answer]: tag below)

[Answer]:A

## Question 9
個人情報・連絡手段の扱い（企画ドキュメントの既存方針）はどうしますか。

A) 役割名と手順のみ。個人の連絡先・所属詳細・私的な予定は書かない（推奨・既存方針）

B) リポジトリ内に連絡手段を書く

X) Other (please describe after [Answer]: tag below)

[Answer]:A

## Question 10
今回のドキュメント作業に、既存の拡張（Security / Resiliency / PBT）をどう適用しますか。

A) 現行のまま維持する（すべて有効）。ドキュメント作業なので適用対象はほぼ N/A

B) 今回ワークストリームでは拡張を適用しない

X) Other (please describe after [Answer]: tag below)

[Answer]:A
