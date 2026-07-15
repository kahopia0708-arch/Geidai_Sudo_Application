# Unit of Work Plan（ユニット分解 計画）

**プロジェクト**: 藝大 須藤さんアプリ
**作成**: 2026-07-15 / AI-DLC Units Generation（Part 1: Planning）
**入力**: `../requirements/requirements.md`、`../user-stories/stories.md`、`../application-design/*`、`execution-plan.md`
**目的**: システムを開発しやすい「ユニット（ストーリーの論理グルーピング）」へ分解する。本プロジェクトは**単一 Unity アプリ（モジュール構成のモノリス）**のため、各ユニット＝論理モジュール（独立デプロイ単位ではない）。

> **回答方法**: 各質問の `[Answer]:` に記号（例: `A`）を記入してください。合う選択肢が無ければ「Other」を選び記述してください。各質問に「(推奨)」案を付けています。回答完了で「done」（または「全部推奨で」）とお知らせください。

---

## A. 実行チェックリスト（Part 2 で実行）
- [ ] `../application-design/unit-of-work.md` を生成（ユニット定義・責務・含むストーリー/コンポーネント）
- [ ] `../application-design/unit-of-work-dependency.md` を生成（ユニット依存マトリクス・実装順序）
- [ ] `../application-design/unit-of-work-story-map.md` を生成（ストーリー→ユニットの割当・網羅確認）
- [ ] ユニット境界・依存の妥当性検証
- [ ] 全ストーリー（US-*）がいずれかのユニットに割り当て済みであることを確認

## B. 想定ユニット（application-design / execution-plan の前提）
- **U1 Foundation/UI基盤**: 起動・ホーム・ユーザー登録・ナビゲーション＋共通UI基盤（SafeArea/レスポンシブ）＋横断サービスの器（App/Navigation/Storage/Audio/Content の枠）
- **U2 Rec**: 録音・加工・保存（AudioService 一本化）
- **U3 Persistence/Collection**: 永続化の本実装＋コレクション（一覧/検索/メタ/堅牢性）
- **U4 weekly theme**: お題表示・Rec導線・差し替え構成
- **U5 Game①音合わせ**: 出題・セント難易度・ユーザー音のリアルタイム加工出題・演出

---

## C. 分解に関する質問（Q1〜Q5）

## Question 1
ユニット境界（Story Grouping）はどうしますか？

A) (推奨) 上記 5 ユニット（U1〜U5）を採用（機能モジュール単位。要件のフェーズA/Bとも整合）

B) 6 ユニットに分割: U1 から「UI基盤（SafeArea/レスポンシブ/共通Prefab）」を独立ユニットとして分離

C) 統合: U2 Rec と U3 Persistence/Collection を 1 ユニットに統合（保存周りを一体で実装）

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 2
横断的な Common / Services（StorageService 等）の所属（Dependencies）はどうしますか？

A) (推奨) Common/Services の「器（インターフェースと最小実装）」は U1 で先行整備し、各機能ユニットで肉付け（例: StorageService は U1 で枠、U3 で堅牢性の本実装）

B) 独立した基盤ユニット U0（Common/Services 専用）を新設して最初に完成させる

C) 各サービスを利用側ユニットで都度実装（U1 に器を置かない）

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 3
実装順序（依存順）はどうしますか？

A) (推奨) U1 → U2 → U3 → U4 → U5（依存順・逐次）

B) U1 → U3（永続化先行）→ U2 → U4 → U5

C) 別の順序（Other に記述）

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 4
チーム/オーナーシップと作業モデル（Team Alignment）はどうしますか？

A) (推奨) 単独開発（前本）＋UI詳細調整は Sさん。ユニットは逐次実装（並行前提にしない）。各ユニット完了時に Sさん へUIハンドオフ

B) 複数人での並行作業を前提にユニットを分割（インターフェース契約を厳密化）

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 5
デプロイ/ドメイン境界（Technical / Business Domain）の確認です。

A) (推奨) 単一 Unity アプリ（モジュール構成のモノリス）。ユニット＝論理モジュールで、独立デプロイはしない。ドメイン境界は「録音」「保存/コレクション」「お題」「ゲーム」で分離

B) 一部を将来的に独立パッケージ化する前提で、より疎結合な境界にする

D) Other (please describe after [Answer]: tag below)

[Answer]: 
