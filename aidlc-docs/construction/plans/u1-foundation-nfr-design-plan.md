# U1 基盤 — NFR Design Plan（計画）

**ユニット**: U1 基盤
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Design（Part 1: 計画）
**入力**: `../u1-foundation/nfr-requirements/nfr-requirements.md`, `tech-stack-decisions.md`, `../u1-foundation/functional-design/*`, `../../inception/application-design/component-dependency.md`

> 目的: U1 の NFR（レスポンシブ/SafeArea/性能/堅牢性/PBT/保守/プライバシー）を**設計パターン**と**論理コンポーネント**へ落とし込む。数値は NFR Requirements で確定済み。ここでは「どう実現するか（パターン）」を決める。

---

## A. 実行チェックリスト（Part 2 で実行）
- [x] `../u1-foundation/nfr-design/nfr-design-patterns.md` を生成（各NFRの実現パターン）
- [x] `../u1-foundation/nfr-design/logical-components.md` を生成（論理コンポーネント・責務・連携）
- [x] NFR Requirements / Functional Design / Application Design とのトレース整合を確認

## B. カテゴリ適用性（このユニットでの判断）
- **Resilience（耐障害）**: 適用（ローカルI/Oの失敗・破損データ耐性）。※ネットワーク再試行系は N/A。
- **Scalability（スケーラビリティ）**: ほぼ N/A（単一端末・オフライン）。データ件数増（コレクション）への軽い配慮のみ。
- **Performance（性能）**: 適用（起動/遷移/保存/フレームレート）。
- **Security（セキュリティ）**: 適用（PII のローカル限定・入力検証・本番エラー秘匿）。
- **Logical Components（論理部品）**: 適用（Result型・例外境界・SafeArea/Responsive・エラー通知・DIの器）。

---

## C. 明確化のための質問（回答を [Answer]: に記入してください）

### Question 1（Resilience パターン — Result 型 vs 例外）
StorageService など失敗し得る処理のエラー伝搬パターンは？

A) (推奨) **Result 型（成功/失敗＋理由）を返す**設計。呼び出し側が分岐しUI表示。致命的でない失敗はクラッシュさせない（BR-16/19 と整合）
B) 例外を投げ、上位の共通ハンドラ（try/catch 境界）で捕捉
C) 両用（下位は例外→サービス境界でResultに変換）

[Answer]: A

### Question 2（Resilience パターン — 破損/欠損データの扱い）
ローカルデータ（profile.json / meta.json / wav）の破損・欠損時の設計方針は？

A) (推奨) **読み込み時に対ファイル整合チェック（BR-05）＋破損項目はスキップして続行**、警告を1回通知（BR-19）。U1は最小、原子的置換はU4
B) 破損検知時に既定値へ自動リセット（プロファイルは初回登録画面へ）
C) A＋簡易バックアップ（前回正常値の保持）を U1 から入れる

[Answer]: A

### Question 3（Performance パターン）
性能目標（遷移<0.3s / 起動 数秒 / 保存<0.5s / 60fps）を満たす設計パターンは？

A) (推奨) **同期API基本＋WAV書込など重い処理のみ必要に応じ非同期/コルーチン**、遷移は軽量ロード、GC削減（録音バッファ等は使い回し/配列プール）、UITheme/Prefab はキャッシュ
B) すべて同期でシンプルに（重い処理も同期、まず動作優先）
C) すべて async/await ベースで統一

[Answer]: A

### Question 4（Security パターン）
PII 保護・入力検証・エラー秘匿の設計パターンは？

A) (推奨) **入力検証を検証ユーティリティに集約（BR-01〜03）**、PII はローカルのみ・ログ出力禁止（ログラッパで PII マスク）、本番ビルドは詳細エラー非表示（開発ビルドのみ詳細）
B) 検証は各画面で個別実装、ログ方針は運用ルールで担保
C) A＋保存データの難読化/簡易暗号化まで U1 で行う

[Answer]: A

### Question 5（Logical Components — レスポンシブ/SafeArea の適用パターン）
SafeArea/Responsive を各画面へ行き渡らせる設計は？

A) (推奨) **ScreenRoot Prefab テンプレートに ResponsiveCanvasConfigurator＋SafeAreaFitter を組込済みにし、全画面がこれを継承/複製**。ScreenRootBase ライフサイクルで Configure→ApplySafeArea を必ず実行。向き/解像度変更イベントで再適用
B) 各画面が個別に SafeArea/Responsive を付与（テンプレート強制なし）
C) グローバルな1コンポーネントが全 Canvas を走査して一括適用

[Answer]: A

### Question 6（Logical Components — DI/サービス参照とテスト容易性）
Services（App/Navigation/Storage/Audio/Content）の参照解決とテスト容易性の設計は？

A) (推奨) **軽量サービスロケータ/手動DI（インターフェース経由）＋ AppManager が起動時に生成・登録**。純粋関数（WavCodec/PitchMath）は静的で副作用なし→PBT対象。I/O はインターフェースでモック可能
B) 各サービスを MonoBehaviour シングルトン（従来型）で参照
C) 本格DIコンテナ（Zenject 等）を導入

[Answer]: A

---

## D. 完了条件
- Q1〜Q6 に回答 → 矛盾チェック → nfr-design-patterns.md / logical-components.md を生成 → 承認ゲート。
