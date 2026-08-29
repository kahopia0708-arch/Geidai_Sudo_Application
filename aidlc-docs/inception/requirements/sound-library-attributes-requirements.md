# サウンドライブラリ構築 — 要件

**プロジェクト**: 藝大 音響教育アプリ（「音」）  
**ワークストリーム**: 属性設計・Editor登録・プレイヤー画面  
**作成**: 2026-08-29  
**入力**: `sound-library-attributes-questions.md`（回答済み）＋ `プロジェクト概要.md` ＋打ち合わせ記録 20260818  
**ブランチ**: `feature/sound-library-attributes`  
**企画の正**: Google Drive。本ファイルは本ワークストリーム範囲のみを定義する。

---

## 1. Intent Analysis

| 項目 | 内容 |
|---|---|
| User request | 複数ゲーム要件を満たす属性付きサウンドライブラリ。コンテンツ担当が Unity Editor で WAV→属性登録。プレイヤー用ライブラリ画面も実装 |
| Request type | Enhancement（既存 U7 を新スキーマへ置換しつつ機能拡張） |
| Scope | 属性モデル・Editor 登録ツール・プレイヤー画面・カタログ API。ゲーム出題ロジック自体は対象外 |
| Complexity | Complex（属性設計・Editor UX・画面・データ移行） |
| Requirements depth | Comprehensive |

---

## 2. 回答の確定

| Q | 回答 | 確定内容 |
|---|---|---|
| 1 | A | **推奨属性セット**を採用（識別／聴覚／ゲーム用／運用） |
| 2 | A | **基準ピッチ属性を持つ**。ゲームは実行時ピッチシフト（加工音は非保存） |
| 3 | A | **専用 Editor ウィンドウ**（WAV ドラッグ → AudioClip → 属性フォーム → カタログ追加） |
| 4 | A | **11月MVP**: 一覧（ロック／解除）・試聴・名前／画像／説明・カテゴリ絞り込み。Progression 維持 |
| 5 | B | **新スキーマに置換**（再登録前提。旧フィールドは廃止方向） |
| 6 | A | **図鑑ナンバー順**固定＋カテゴリ／音色タグで絞り込み |
| 7 | A | 属性＋Editor＋プレイヤー画面＋Create/Game が読むカタログ API 更新。出題ロジックは別タスク |
| 8 | A | Security / Resiliency / PBT を継続 |

矛盾なし。Q5=B により、既存サンプルカタログは新スキーマで再登録する。

---

## 3. 機能要件

### FR-LIB-ATTR-01 属性モデル（新スキーマ）

1音の定義は次を含む（必須／任意を実装時に明確化。必須は Editor で未入力時に保存不可）。

| グループ | フィールド | 必須 | 説明 |
|---|---|---|---|
| 識別 | `id` | 必須 | 安定 ID（カタログ・Unlock・レシピ参照） |
| 識別 | `encyclopediaNumber` | 必須 | 図鑑ナンバー（表示・既定ソート） |
| 識別 | `displayName` | 必須 | 表示名 |
| 識別 | `reading` | 任意 | ふりがな／読み |
| 識別 | `description` | 任意 | 説明文 |
| 識別 | `image` | 任意 | 図鑑用画像（Sprite）。未設定時はプレースホルダー |
| 聴覚 | `timbreTags` | 必須（1つ以上） | 音色タグ（ベル・ドラム・環境音など。列挙 or タグリスト） |
| 聴覚 | `basePitchMidi` | 任意※ | 基準ピッチ（MIDI）。ピッチ系ゲーム用。未設定時はそのゲームで除外 or 相対のみ |
| 聴覚 | `loudnessBand` | 任意 | 強弱帯（例: Soft / Mid / Loud） |
| 聴覚 | `durationBand` | 任意 | 長さ帯（例: Short / Mid / Long） |
| ゲーム | `pairKey` | 任意 | 神経衰弱用ペアキー |
| ゲーム | `allowPitchShift` | 必須 | 実行時ピッチシフト可否（既定 true） |
| ゲーム | `difficultyTags` | 任意 | 難易度タグ |
| 運用 | `category` | 必須 | 図鑑カテゴリ（絞り込み用） |
| 運用 | `initiallyUnlocked` | 必須 | 初期解除 |
| 音声 | `clipRef` | 必須 | 同梱 AudioClip |

※ Q2=A: 基準ピッチは属性の正。ピッチシフト生成自体はゲーム側（本ワークストリーム外）。

**スキーマ置換（Q5=B）**:
- 旧 `CuratedSoundDefinition` の最小フィールドのみの資産は**新スキーマで再登録**する
- 旧 SO／定義は移行完了後に廃止（または読み取り非対応）
- `id` の安定性を優先し、Unlock／レシピが参照する ID 方針を Functional Design で確定する

### FR-LIB-ATTR-02 カタログ API

- Create / Game 等が読む公開 API（既存 `IContentService` / Catalog SO）を**新スキーマ対応**に更新
- クエリ例: カテゴリ、timbreTags、図鑑ナンバー範囲、アンロック状態投影は Library UI 側
- ゲーム出題ロジック（選択肢生成）は**本スコープ外**（API が属性を返すところまで）

### FR-LIB-ATTR-03 Editor 登録ツール

Unity メニューから開く専用ウィンドウ:

1. WAV（または対応音声）をドラッグ／選択
2. プロジェクト内へ AudioClip としてインポート（配置フォルダ規約あり）
3. 属性フォーム入力（上表）
4. 対象 `CuratedSoundCatalog`（新スキーマ）へ追加／更新
5. 検証: 必須欠落・ID 重複・図鑑ナンバー重複を保存前に拒否

コンテンツ担当はコード編集なしで登録できること。

### FR-LIB-ATTR-04 プレイヤー向けライブラリ画面（11月MVP）

- 既定並び: **図鑑ナンバー昇順**
- 絞り込み: **カテゴリ** および／または **音色タグ**
- 各行／詳細: ロック状態、名前、画像、説明、試聴
- ロック中は試聴不可または制限（既存 U7 方針を踏襲し FD で確定）
- アンロック状態は既存 `IProgressionService` / `UnlockState` を維持
- ユーザー録音（Collection）は**同一画面に含めない**（Q4=A）

### FR-LIB-ATTR-05 ホーム／ナビ

- 既存ホーム「おとずかん」→ `GeidaiLibrary` 導線を維持
- 画面意匠はホーム基調（`HomeUiTheme`）に揃える

---

## 4. 非機能要件

| ID | 内容 |
|---|---|
| NFR-01 | 完全オフライン。端末外送信なし |
| NFR-02 | PII 非ログ（属性・カタログに個人情報を置かない） |
| NFR-03 | カタログ件数 50〜100 音を想定。一覧スクロールは体感即時 |
| NFR-04 | UnlockState は AtomicFile。破損時は空フォールバック（既存 Resiliency） |
| NFR-05 | 属性バリデーション・Unlock 評価は決定的テスト可能（PBT／EditMode） |
| NFR-06 | Editor 操作で必須欠落・重複 ID を防ぐ |

---

## 5. スコープ外（明示）

- ゲーム①〜④の出題ロジック変更・ピッチシフト実装本体
- ユーザー録音を図鑑へ統合
- WAVE 書き出し／共有／サーバー
- 漢字・かな年齢別表記の自動切替（検討中事項。任意なら後続）
- 読み上げ機能

---

## 6. Extension Compliance（本ワークストリーム）

| Extension | Enabled | 適用 |
|---|---|---|
| Security Baseline | Yes | 端末内・PII非ログ・共有なし |
| Resiliency Baseline | Yes | AtomicFile・破損フォールバック |
| Property-Based Testing | Yes | 属性検証・Unlock／JSON 往復 |

---

## 7. 成功基準

1. コンテンツ担当が Editor ウィンドウだけで WAV→属性→カタログ登録できる
2. プレイヤーが図鑑ナンバー順で一覧し、カテゴリ／音色で絞り込み、解除音を試聴できる
3. Create／既存サービスが新スキーマのカタログを読める
4. 旧最小定義に依存しない（再登録済みカタログで動作）

---

## 承認ゲート

**Requirements Analysis 完了。この要件で次（User Stories / Workflow Planning）に進めてよいですか？**

- 変更がある場合はその内容を指示してください
- 問題なければ **OK** と返信してください
