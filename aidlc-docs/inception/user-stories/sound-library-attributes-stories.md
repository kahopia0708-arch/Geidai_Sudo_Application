# サウンドライブラリ属性 — User Stories（差分）

**作成**: 2026-08-29  
**入力**: `sound-library-attributes-requirements.md`（承認済み）  
**既存**: `stories.md` の US-LIB-01〜03（U7 実装済み）を前提に**追加・更新**する  
**ブランチ**: `feature/sound-library-attributes`

---

## ペルソナ（差分）

| ID | 役割 | 本ワークでの関心 |
|---|---|---|
| P1 | こども/学習者 | 図鑑ナンバー順・絞り込み・試聴 |
| P2 | 企画・デザイン担当 | Editor ウィンドウで WAV→属性登録（コード不要） |
| P3 | 基盤・統合担当 | 新スキーマ・カタログ API・Create/Game 参照更新 |

（詳細ペルソナは既存 `personas.md` を正とする）

---

## US-LIB-01（更新）制作側音素材の閲覧・試聴

**P1** — 既存ストーリーを新スキーマ／11月MVPに合わせて拡張。

**受入基準（追加・置換）**
- Given 音図鑑画面, When 開く, Then **図鑑ナンバー昇順**で一覧できる（FR-LIB-ATTR-04）。
- Given カタログ, When 表示する, Then 各音は id・図鑑ナンバー・表示名・画像（なければプレースホルダー）・説明・ロック状態を持つ（FR-LIB-ATTR-01）。
- Given カテゴリまたは音色タグ, When 絞り込む, Then 該当音のみ表示される。
- Given アンロック済み, When 試聴する, Then 再生できる。未解除はロックが分かり試聴不可（または既存方針どおり制限）。
- Given オフライン, When 操作する, Then サーバー通信なし。

_トレース: FR-LIB-ATTR-01, 04 / NFR-01〜03_

---

## US-LIB-04 コンテンツ担当による Editor 登録

**P2**
企画・デザイン担当として、Unity Editor の専用ウィンドウで WAV を取り込み属性を設定してカタログに追加したい。なぜなら、コードを触らずに展示用の音を増やしたいから。

**受入基準**
- Given Editor ウィンドウ, When WAV を指定する, Then AudioClip が規約フォルダにインポートされる（FR-LIB-ATTR-03）。
- Given 属性フォーム, When 必須項目を満たして保存する, Then 新スキーマのカタログ SO に追加／更新される。
- Given ID 重複または図鑑ナンバー重複または必須欠落, When 保存しようとする, Then 保存されずエラーが分かる（NFR-06）。
- Given 登録済み音, When プレイヤー図鑑を開く, Then 一覧に反映される（再生ビルド後）。

_トレース: FR-LIB-ATTR-01, 03 / NFR-06_

---

## US-LIB-05 ゲーム横断属性の保持と参照

**P3 / P1**
実装・ゲーム担当として、音色・基準ピッチ・ペアキー等の属性をカタログから読みたい。なぜなら、複数ミニゲームが出題素材を同じ正から選びたいから。

**受入基準**
- Given 新スキーマ定義, When カタログ API が返す, Then timbreTags・basePitchMidi・allowPitchShift・pairKey・category 等を取得できる（FR-LIB-ATTR-02）。
- Given 旧最小定義のみの資産, When 移行後, Then 新スキーマ再登録済みカタログのみが有効（Q5=B。旧フィールド依存を廃止）。
- Given Create または既存参照, When 素材 ID で解決する, Then クラッシュせず解決または不足表示（既存 US-LIB-03 と整合）。
- Given ピッチシフト, When ゲームが出題する, Then 基準ピッチ属性を正とし、加工音は保存しない（出題ロジック本体はスコープ外）。

_トレース: FR-LIB-ATTR-01, 02 / US-LIB-03_

---

## 明示的に本差分に含めないもの

- US-LIB-02（アンロック条件ロジック）— 既存維持
- US-CREATE-* — Create はカタログ読取更新のみ
- ゲーム①出題が音色タグで選ぶ接続 — スコープ外（要件 Q7=A）

---

## トレーサビリティ

| ストーリー | 要件 |
|---|---|
| US-LIB-01（更新） | FR-LIB-ATTR-01, 04 |
| US-LIB-04 | FR-LIB-ATTR-03 |
| US-LIB-05 | FR-LIB-ATTR-01, 02 |
| （既存）US-LIB-02/03 | 維持 |
