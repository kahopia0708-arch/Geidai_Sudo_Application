# Application Design Plan（アプリケーション設計 計画）

**プロジェクト**: 藝大 須藤さんアプリ
**作成**: 2026-07-15 / AI-DLC Application Design（Part 1: Planning）
**入力**: `../requirements/requirements.md`、`../user-stories/stories.md`、`../reverse-engineering/*`、`execution-plan.md`
**目的**: 高レベルのコンポーネント特定・責務・インターフェース（メソッド署名）・サービス層・依存関係を定義する。詳細な業務ロジックは Construction の Functional Design（ユニットごと）で扱う。

> **回答方法**: 各質問の `[Answer]:` の後ろに記号（例: `A`）を記入してください。合う選択肢が無ければ「Other」を選び内容を記述してください。各質問に「(推奨)」案を付けています。すべて回答したら「done」等でお知らせください。全て推奨で良ければ「全部推奨で」でも構いません。

---

## A. 実行チェックリスト（Part 2 で実行）
- [ ] `components.md` を生成（コンポーネント定義・高レベル責務・インターフェース）
- [ ] `component-methods.md` を生成（メソッド署名・入出力型・高レベル目的。詳細業務ルールは Functional Design）
- [ ] `services.md` を生成（サービス定義・責務・オーケストレーション）
- [ ] `component-dependency.md` を生成（依存マトリクス・通信パターン・データフロー）
- [ ] `application-design.md` を生成（上記を統合したサマリ）
- [ ] 設計の完全性・整合性を検証（要件 FR/NFR・ストーリーとのトレース）

## B. 必須成果物（Mandatory Artifacts）
`aidlc-docs/inception/application-design/` 配下に:
- `components.md` / `component-methods.md` / `services.md` / `component-dependency.md` / `application-design.md`

## C. 設計アプローチ（前提）
- 既存の Unity シーン分割型アーキテクチャを踏襲しつつ、責務の分離とデータ駆動化（Sさん の UI/コンテンツ調整のため）を強化。
- Units（U1 Foundation/UI基盤 → U2 Rec → U3 Persistence/Collection → U4 weekly theme → U5 Game①）に対応するコンポーネント/サービス群を定義。
- 完全オフライン・ローカル永続化。外部API/サーバーなし。

---

## D. 設計に関する質問（Q1〜Q7）

## Question 1
コンポーネント/モジュール構成と Assembly Definition の方針は？

A) (推奨) 機能モジュール単位（Foundation / Rec / Collection / Theme / Game1 / Common）に分割し、各モジュールに Assembly Definition を整備して依存関係を明示（保守性・ビルド時間・テスト分離に有利）

B) 現状の単一 Assembly-CSharp のまま、名前空間だけで論理分割

C) レイヤ単位（UI / Domain / Data）で分割

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 2
サービス層（オーケストレーション）の実現方式は？

A) (推奨) 軽量 Manager（必要最小限のシングルトン）＋ ScriptableObject 設定の併用（例: AppManager / NavigationService / StorageService / AudioService）。Unity 標準に沿い学習コスト低

B) 純粋なシングルトン Manager 中心（ScriptableObject は使わない）

C) DI コンテナ導入（VContainer / Zenject 等）でテスト容易性を最大化

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 3
ローカル永続化（録音WAV＋設定JSON＋メタデータ＋ユーザー登録）の設計は？

A) (推奨) 単一の StorageService に集約し、保存の原子性・破損フォールバック（NFR-07）を一元管理。各モジュールは StorageService 経由で読み書き

B) モジュールごとに保存処理を分散（結合は緩いが堅牢性/一貫性の担保が分散）

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 4
画面遷移（ナビゲーション）の方式は？（現状は文字列指定で大文字小文字不一致の不具合あり）

A) (推奨) NavigationService を設け、シーン識別を enum/定数で型安全化（FR-02 の不具合解消、Place は無効化/非表示）

B) 既存の文字列ベースを維持しつつ定数化のみ（最小変更）

C) Additive Scene / パネル切替方式へ再設計（遷移を軽量化）

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 5
音声処理アーキテクチャは？（録音・加工の一本化＋ゲームのリアルタイム加工）

A) (推奨) AudioService に集約。録音/加工は VoiceRecordingSection（Unity標準AudioFilter）に一本化（FR-07）、ゲーム出題のリアルタイムピッチ加工は PitchVariationService として分離（生成音は非保存 / FR-19）

B) Rec と Game で音声処理を別々に実装（共通化しない）

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 6
コンテンツのデータ駆動化（Sさん が調整する範囲）の方式は？（US-TECH-07 準拠）

A) (推奨) weekly theme のお題、①音合わせのパラメータ/難易度（セント）、UI 文言/素材参照を ScriptableObject（一部 JSON）で外部化し、コード改修なしで差し替え可能に

B) JSON ファイルで外部化（ScriptableObject は使わない）

C) 当面はコード内定義とし、将来外部化（今回は最小限）

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 7
UI 基盤コンポーネント（枠組み提供とハンドオフ）の方針は？（前本=枠組み / Sさん=詳細調整）

A) (推奨) 共通 UI 基盤を用意（SafeAreaFitter、レスポンシブ Canvas 設定、画面ルートの Prefab テンプレート）。前本が枠組み Prefab を提供し、Sさん が Prefab/ScriptableObject 上で詳細調整

B) 各画面で個別に SafeArea/レスポンシブを実装（共通基盤は作らない）

D) Other (please describe after [Answer]: tag below)

[Answer]: 
