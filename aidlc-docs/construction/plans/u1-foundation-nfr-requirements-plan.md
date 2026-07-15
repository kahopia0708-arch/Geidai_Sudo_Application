# U1 基盤 — NFR Requirements Plan（非機能要件・技術選定 計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U1 基盤（UI基盤 ＋ Services器 ＋ Common）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Requirements（Part 1: Planning）
**入力**: `../u1-foundation/functional-design/*`、`../../inception/requirements/requirements.md`（NFR-01〜12）
**対象NFR**: パフォーマンス(NFR-06)、プライバシー/セキュリティ(NFR-04)、レスポンシブ(NFR-11)、SafeArea(NFR-12)、堅牢性(NFR-07)、テスト容易性(NFR-09/PBT)、保守性(NFR-08)、プラットフォーム(NFR-01)

> 本ステージで U1 の**非機能目標の具体値**と**技術選定**を確定する（Functional Design の技術非依存設計に対する「品質・技術」面）。
> **回答方法**: 各質問の `[Answer]:` に記号（例: `A`）。合う選択肢が無ければ「Other」。各質問に「(推奨)」あり。完了で「done」（または「全部推奨で」）。

---

## A. 実行チェックリスト（Part 2 で実行）
- [ ] `../u1-foundation/nfr-requirements/nfr-requirements.md` を生成（U1 の NFR 目標・受入可能値）
- [ ] `../u1-foundation/nfr-requirements/tech-stack-decisions.md` を生成（技術選定・根拠）
- [ ] 要件（NFR-01〜12）とのトレース整合を確認

## B. 前提（確定済み・要件由来）
- 完全オフライン・ローカルのみ（NFR-02）。サーバー/クラウド/外部通信なし → 可用性/スケーラビリティ/DR はアプリ内・端末内の範囲。
- Unity 6000.4.2f1 / URP / uGUI / C#。録音は VoiceRecordingSection 一本化。

---

## C. NFR・技術選定に関する質問（Q1〜Q7）

## Question 1
対象プラットフォーム/最小OSバージョン（NFR-01）は？（MVP は片方から着手可）

A) (推奨) iOS 15+ / Android 8.0(API 26)+ を目標レンジ（実機検証は入手可能な端末で実施、MVPは片方から）

B) iOS 16+ / Android 10(API 29)+（比較的新しめに絞る）

C) 現時点では未確定（Unity 既定に従い、Build & Test で確定）

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 2
UI レスポンシブの技術数値（NFR-11 / CanvasScaler）は？（両向き対応）

A) (推奨) 参照解像度=1080×1920、CanvasScaler=Scale With Screen Size、Match=0.5（縦横中間）。タブレット等の広アスペクトは Anchor＋レイアウトグループで吸収

B) 参照解像度=1920×1080（横基準）、Match=0.5

C) 縦横で参照解像度/ Match を切替（縦=1080×1920/Match1、横=1920×1080/Match0）

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 3
SafeArea の実装方式（NFR-12）は？

A) (推奨) 各画面ルートに SafeAreaFitter を付与し、`Screen.safeArea` から RectTransform のアンカーを毎表示＋解像度/向き変更時に再計算（`androidRenderOutsideSafeArea=1` と整合）

B) Canvas 全体を safeArea 内に固定（外側は背景色で塗る）

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 4
パフォーマンス目標（NFR-06。U1 は起動/遷移/保存が対象）は？

A) (推奨) 画面遷移=体感即時（目安 <0.3s）、起動=数秒以内、保存（WAV書込）=体感即時（目安 <0.5s）。ターゲット 60fps、最低 30fps を割らない

B) 具体数値は設定せず「体感で引っかからない」を定性目標にする

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 5
データ堅牢性の実装方針（NFR-07。U1 は最小、U4 で本実装）は？

A) (推奨) U1 では基本保存＋対ファイル整合チェック（BR-05）。原子的置換（temp→rename）・詳細フォールバックは U4 で本実装。フォールバック時は必ず警告（BR-19）

B) U1 の段階で原子的置換まで実装してしまう（U4 は Collection UI に専念）

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 6
PBT（プロパティベーステスト / NFR-09）のフレームワークは？

A) (推奨) FsCheck（.NET）を採用し、Unity Test Framework（EditMode）から実行。対象=WavCodec ラウンドトリップ、PitchMath 逆変換、設定JSON シリアライズ

B) CsCheck を採用

C) 当面は通常のユニットテスト（NUnit）で代表ケース＋境界値、PBT は後日導入

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 7
保守性・シリアライズ等の技術選定（NFR-08）は？

A) (推奨) Assembly Definition でモジュール分割、設定/メタの永続化は Unity 標準 `JsonUtility`（不足時のみ軽量JSONライブラリ検討）、コーディング規約＋PRレビュー（NFR-10）

B) 永続化に Newtonsoft.Json（com.unity.nuget.newtonsoft-json）を採用

D) Other (please describe after [Answer]: tag below)

[Answer]: 
