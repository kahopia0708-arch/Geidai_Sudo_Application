# Story Generation Plan（ユーザーストーリー生成計画）

**プロジェクト**: 藝大 須藤さんアプリ
**作成**: 2026-07-15 / AI-DLC User Stories（Part 1: Planning）
**入力**: `../requirements/requirements.md`、`../reverse-engineering/plan-vs-implementation-gap.md`、`user-stories-assessment.md`
**役割**: プロダクトオーナーとして、要件（FR/NFR）を利用者視点のストーリー＋受入基準に変換する。

> このファイルの目的は「どう作るか（方法・方針）」の合意です。実際の `stories.md` / `personas.md` は、下記の質問（Q1〜Q8）にご回答・ご承認いただいた後に Part 2 で生成します。
> **回答方法**: 各質問の `[Answer]:` の後ろに選択肢の記号（例: `A`）を記入してください。どれも合わない場合は最後の「Other」を選び、内容を書いてください。各質問には「(推奨)」を付けた案があります。すべて回答したら「done」等でお知らせください。

---

## A. 実行チェックリスト（Part 2 で実行）

- [ ] 承認済みアプローチ・フォーマットに基づき `personas.md` を生成（ペルソナ archetype と特性、目標、利用文脈）
- [ ] `stories.md` を生成（INVEST 準拠のユーザーストーリー）
  - [ ] フェーズA（基盤）: ナビゲーション / 初回ユーザー登録 / Rec / MySoundCollection
  - [ ] フェーズB（最初のゲーム）: weekly theme / ①音合わせ（ユーザー音の出題含む）
  - [ ] 技術・品質系（レスポンシブ/SafeArea、録音実装一本化、Place 導線除外 等）の扱いを Q6 の回答方針で反映
  - [ ] スコープ外（②〜⑧, Place共有, ゲーミフィケーション）を将来エピックのスタブとして Q5 の回答方針で反映
- [ ] 各ストーリーに受入基準（Q3 のフォーマット）を付与
- [ ] 各ストーリーが INVEST（Independent, Negotiable, Valuable, Estimable, Small, Testable）を満たすことを確認
- [ ] ペルソナ ↔ ストーリーの対応表を作成
- [ ] 要件トレーサビリティ（Story ↔ FR/NFR）を付与
- [ ] `aidlc-state.md` を更新し、生成成果物を保存

## B. 必須成果物（Mandatory Artifacts）
- `aidlc-docs/inception/user-stories/stories.md`
- `aidlc-docs/inception/user-stories/personas.md`

## C. ストーリー分割アプローチの選択肢（トレードオフ）
- **モジュール/機能ベース**: Rec・Collection・weekly theme・①音合わせ 等、要件のモジュール単位でグルーピング。要件との対応が明快で、Unity のシーン/モジュール構成とも整合。
- **ユーザージャーニーベース**: 「録音してコレクションに貯め、お題で録り、ゲームで自分の音を聴き分ける」という体験の流れで並べる。体験価値が伝わりやすい。
- **ペルソナベース**: 利用者像ごとにニーズをまとめる。多様な利用者の差が大きい場合に有効。
- **エピックベース（階層）**: 大きなエピック配下にストーリーをぶら下げる。将来スコープ（②〜⑧等）の見通しに有効。
- **ハイブリッド**: 例）エピック=モジュール、その配下にジャーニー順のストーリー、を併用（→ Q2）。

---

## D. 計画に関する質問（Q1〜Q8）

## Question 1
formalize（正式定義）するペルソナの構成はどれにしますか？（`personas.md` に載せる利用者像）

A) (推奨) 3ペルソナ: ①子ども/学習者（プレイヤー）、②企画・コンテンツ運用者（Sさん想定：お題や音素材を用意し、初心者制作者としての視点も持つ）、③実装・技術担当（前本：技術/保守の観点）

B) 2ペルソナ: ①エンドユーザー（子ども・学習者）、②コンテンツ運用者（Sさん）

C) 4ペルソナ: ①子ども、②大人の音楽学習者、③企画・運用者（Sさん）、④音楽教育者

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 2
ストーリーの分割・整理アプローチはどれを主軸にしますか？（C章の選択肢参照）

A) (推奨) ハイブリッド: エピック＝モジュール（Rec / Collection / weekly theme / ①音合わせ / 基盤ナビ）、配下はユーザージャーニー順に並べる

B) モジュール/機能ベースのみ

C) ユーザージャーニーベースのみ

D) ペルソナベース

E) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 3
受入基準（Acceptance Criteria）のフォーマットはどれにしますか？

A) (推奨) Given/When/Then（前提/操作/期待結果）形式

B) チェックリスト形式（箇条書きの満たすべき条件）

C) 両方併用（重要ストーリーは Given/When/Then、軽微なものはチェックリスト）

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 4
ストーリーの粒度（サイズ）はどの程度にしますか？

A) (推奨) 中粒度: 1機能=数ストーリー（例: Rec は「録音する」「加工する」「保存する」に分割）。INVEST の Small を保ちつつ数を過剰にしない

B) 粗粒度: 1機能=1ストーリー（数は少なく、受入基準を厚めに）

C) 細粒度: 操作単位まで細かく分割（数は多いが個々は最小）

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 5
スコープ外の項目（②音並べ〜⑧ゲーム、③共有/Place、ゲーミフィケーション、⑤テスト画面）は `stories.md` でどう扱いますか？

A) (推奨) 将来エピックの「スタブ」として見出し＋一言のみ記載（詳細な受入基準は書かない）。MVPスコープの明確化と将来展望の両立

B) 完全に除外（MVPスコープのストーリーのみ記載）

C) 将来エピックも詳細ストーリーまで記載

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 6
技術・品質系の要件（NFR-11 レスポンシブUI / NFR-12 SafeArea / 録音実装の一本化 FR-07 / Place 導線除外 FR-02 / Unity MCP 規約）はストーリーとしてどう表現しますか？

A) (推奨) 「技術イネーブラー・ストーリー」として明示的にストーリー化（例: 開発者/利用者として〜、受入基準に端末横断表示・SafeArea 追従等を記載）

B) ストーリー化せず、関連する機能ストーリーの受入基準・制約として埋め込む

C) 別セクション「非機能・品質ストーリー」を設けてまとめて記載

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 7
ストーリーの記述テンプレート・言語を確認します。

A) (推奨) 日本語・「〜として、〜したい、なぜなら〜」形式（As a / I want / so that の日本語版）＋ INVEST 準拠

B) 日本語・「〜として、〜できる」形式（目的句は任意）

C) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 8
各ストーリーに MVP フェーズ区分（フェーズA基盤 / フェーズB最初のゲーム / 将来）のタグを付けますか？（優先度・スプリント計画ではなく、要件のスコープ区分の可視化）

A) (推奨) 付ける（フェーズA / フェーズB / 将来 のラベルのみ。細かな優先順位付けは Workflow Planning 以降で行う）

B) 付けない（スコープ区分は章立てで表現）

C) Other (please describe after [Answer]: tag below)

[Answer]: 
