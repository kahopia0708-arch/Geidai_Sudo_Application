# U1 基盤 — NFR Design Patterns（NFRの実現パターン）

**ユニット**: U1 基盤
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Design（Part 2）
**決定**: Q1〜Q6＝すべて A（推奨）
**入力**: `../nfr-requirements/nfr-requirements.md`, `tech-stack-decisions.md`, `../functional-design/*`

> U1 の各 NFR を「どう実現するか」を設計パターンで定義。数値は NFR Requirements で確定済み。

---

## 1. Resilience（耐障害）パターン

### 1.1 エラー伝搬 = Result 型（Q1=A）
- 失敗し得る処理（保存/読込/検証）は **`Result<T>`（成功値 or 失敗理由コード＋メッセージ）** を返す。
- 呼び出し側は成否で分岐し、UI（トースト/バナー）へ反映。**致命的でない失敗はクラッシュさせない**（BR-16/19）。
- 例外は「想定外バグ」に限定。サービス境界で捕捉し Result に変換（生の例外を UI に漏らさない）。
- **失敗理由コード（enum）**: `NotFound` / `Corrupted` / `IOError` / `ValidationError` / `Unknown`。

### 1.2 破損/欠損データの扱い（Q2=A）
- 読込時に **対ファイル整合チェック**（`{id}.wav` と `{id}.meta.json` の対存在, BR-05）。
- 破損/欠損項目は **スキップして続行**。全滅させない（部分的に読める分は読む）。
- フォールバック発動時は **警告を1回通知**（BR-19）、過度なフォールバック禁止（BR-18）。
- **U1 は最小**（整合チェック＋スキップ）。原子的置換（temp→rename）・詳細復旧は **U4** で本実装。

## 2. Performance（性能）パターン（Q3=A）
- **同期API基本**、WAV 書込など重い処理のみ必要に応じ **非同期/コルーチン**（UIスレッドを止めない）。
- 画面遷移は軽量ロード（重量アセットは遷移先で遅延ロード）。目標 遷移<0.3s（NFR-06）。
- **GC 削減**: 録音/PCM バッファは使い回し（配列プール/再利用）。毎フレーム確保を避ける。
- **キャッシュ**: UITheme・共通 Prefab 参照はキャッシュ。頻繁な `GetComponent`/`Find` を避ける。
- フレームレート ターゲット60fps・最低30fps（NFR-06）。

## 3. Security（セキュリティ）パターン（Q4=A）
- **入力検証を検証ユーティリティに集約**（`ValidationUtil`, BR-01〜03 / SECURITY-05）。各画面はこれを呼ぶ。
- **PII 非送信・非ログ**: ネットワーク送信なし（NFR-02）。ログは **PII マスク付きラッパ**経由（生年/ニックネーム等を出さない）。
- **本番エラー秘匿**: リリースビルドは詳細エラー非表示（子ども向け平易文言のみ）。詳細は開発ビルド限定（SECURITY-09）。
- 保存データの暗号化は U1 では行わない（オフライン・端末ローカルのみ、PII 最小のため過剰と判断）。必要時は将来検討。

## 4. Responsive / SafeArea 適用パターン（Q5=A）
- **ScreenRoot Prefab テンプレート**に `ResponsiveCanvasConfigurator`＋`SafeAreaFitter` を組込済みにする。
- 全画面はこのテンプレートを継承/複製 → 個別実装漏れを防ぐ（テンプレート強制）。
- `ScreenRootBase` ライフサイクルで **Configure→ApplySafeArea を必ず実行**（`ShowAsync` 内）。
- **向き/解像度変更イベント**を購読して再適用（縦横両対応, NFR-11/12）。
- 数値: 参照 1080×1920 / Match=0.5（NFR Requirements 確定値）。

## 5. Scalability（スケーラビリティ）
- **ほぼ N/A**（単一端末・オフライン）。コレクション件数増への軽い配慮のみ（一覧は必要に応じ遅延/ページング、U4 で詳細化）。U1 では設計上の余地を残す程度。

## 6. Testability（テスト容易性 / PBT）パターン
- **純粋関数化**: `WavCodec`（encode/decode）・`PitchMath`（cents↔ratio）は静的・副作用なし → PBT 対象（NFR-09）。
- **I/O 抽象化**: `IStorageService` などインターフェースでモック可能に（§7 と連動）。
- FsCheck＋Unity Test Framework(EditMode)：WavCodec ラウンドトリップ / PitchMath 逆変換 / JSON ラウンドトリップ。

## 7. DI / サービス参照パターン（Q6=A）
- **軽量サービスロケータ/手動DI（インターフェース経由）**。`AppManager` が起動時にサービスを生成・登録。
- 各モジュールは**インターフェース**（`INavigationService`/`IStorageService`/…）に依存 → テストでモック差し替え。
- 本格 DI コンテナ（Zenject 等）は導入しない（オーバーヘッド回避・保守簡素）。
- MonoBehaviour 依存を最小化し、ロジックは POCO/静的へ寄せる（テスト容易性）。

## トレース
NFR-07→§1 / NFR-06→§2 / NFR-04・Security→§3 / NFR-11・NFR-12→§4 / NFR-02→§5 / NFR-09→§6 / NFR-08・保守→§7。
