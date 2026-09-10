# サウンドライブラリ構築 — 要件確認質問

**ワークストリーム**: サウンドライブラリ（属性設計・Editor登録・プレイヤー画面）  
**ブランチ**: `feature/sound-library-attributes`  
**作成**: 2026-08-29  
**企画の正**: Google Drive `プロジェクト概要.md`（2026-07-30）＋打ち合わせ記録 `20260818`  
**既存実装**: U7 `CuratedSoundDefinition`（id / displayName / category / description / clipRef / initiallyUnlocked）＋ `GeidaiLibrary` シーン

回答は各問の `[Answer]:` の後に **A/B/...**（必要なら補足）を記入してください。  
すべて埋まったらチャットで **done** と送ってください。

---

## 前提（現状のギャップ）

| 領域 | 現状 | 今回の狙い |
|---|---|---|
| 属性 | category 文字列のみ | 複数ゲーム（音合わせ／神経衰弱／音並べ等）で使える属性モデル |
| 登録 | SO を手編集 | WAV インポート → 属性設定の Editor 導線 |
| プレイヤー画面 | 一覧・試聴・ロック表示の骨組みあり | 11月展示向けに使えるライブラリ画面へ拡張 |
| ユーザー録音 | Collection に別系統 | 図鑑に載せるか／制作側音のみか要確認 |

---

## Question 1 — 属性モデルの方針

ゲーム横断で使う属性セットをどう決めますか？

A) **推奨セットを採用**（下記をベースに実装。細部は後から増やせる）  
　・識別: `id` / 図鑑ナンバー / 表示名 / 読み（ふりがな） / 説明 / 画像  
　・聴覚: 音色タグ（例: ベル・ドラム・環境音） / 音高（基準MIDI or 相対） / 強弱帯 / 長さ帯  
　・ゲーム用: ペアキー（神経衰弱） / ピッチシフト可否 / 難易度タグ  
　・運用: 初期アンロック / カテゴリ（図鑑の並び・絞り込み）

B) **最小セットから開始**（識別＋音色タグ＋画像＋初期アンロックのみ。音高・強弱はゲーム実装時に追加）

C) **企画側が確定した属性表を正とする**（回答に一覧を書く／別ファイルを指定）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 2 — 音高・ピッチの扱い

打ち合わせでは「音高のゲームではライブラリ音からピッチシフトでバリエーション生成」とあります。属性側の正はどれですか？

A) **基準ピッチ（例: MIDIノート or 基準周波数）を属性に持つ**。ゲームは実行時にシフトして出題（加工音は保存しない）

B) **基準クリップのみ登録**。ピッチ情報は属性に持たず、ゲーム側が相対シフトだけ行う

C) **あらかじめ複数ピッチ版クリップをカタログに登録**（属性で系列IDで束ねる）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 3 — コンテンツ担当の Editor ワークフロー

Unity Editor での登録体験の優先形はどれですか？

A) **推奨**: 専用 Editor ウィンドウ（WAV をドラッグ → 自動で AudioClip 生成 → 属性フォーム → カタログ SO に追加）

B) **Inspector 強化**: 既存 `CuratedSoundCatalog` SO のカスタム Inspector＋一括 WAV 取り込みボタン

C) **両方**（ウィンドウで新規登録、SO Inspector で編集・並べ替え）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 4 — プレイヤー向けライブラリ画面のスコープ（11月展示）

プレイヤー画面に含める範囲は？

A) **推奨（11月MVP）**: 一覧（ロック／解除）・試聴・名前／画像／説明表示・カテゴリ絞り込み。アンロックは既存 Progression を維持

B) **閲覧＋試聴のみ**（絞り込みなし。ロック表示は簡易）

C) **制作側音＋ユーザー録音を同一画面で扱う**（録音は Collection と統合表示）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 5 — 既存 U7 データとの関係

既存の `CuratedSoundDefinition` / カタログ SO / `GeidaiLibrary` をどう扱いますか？

A) **推奨**: 既存型を**後方互換で拡張**（新属性は任意フィールド。旧データは読める）

B) **新スキーマに置換**（移行ツール or 再登録前提。旧フィールドは廃止）

C) **新カタログ型を別 SO として追加**し、段階的に切替

X) Other (please describe after [Answer]: tag below)

[Answer]: B

---

## Question 6 — 図鑑の並び・開放の見せ方

A) **図鑑ナンバー順**固定＋カテゴリ／音色タグで絞り込み

B) **カテゴリ別セクション**（音色別まとめ）＋ナンバーは補助

C) **開放順／最近解除**を既定表示（図鑑順は切替）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 7 — 本ワークストリームの成果物境界

A) **推奨**: 属性モデル＋Editor登録ツール＋プレイヤー画面刷新＋既存 Create/Game が読むカタログ API の更新（ゲーム出題ロジック自体は別タスク）

B) **属性＋Editor のみ**（プレイヤー画面は現状維持の最小改修）

C) **属性＋Editor＋プレイヤー画面＋ゲーム①が属性を使う接続まで**（音合わせが音色タグで素材を選ぶ）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 8 — Extensions（本ワークストリーム）

既存プロジェクトでは Security / Resiliency / PBT が有効です。本ワークストリームでも同じにしますか？

A) Yes — 3拡張とも継続（Blocking / Full）。端末内のみ・PII非ログ・AtomicFile・属性／Unlock の決定的テストを維持（推奨）

B) No — 本ワークストリームでは拡張を緩和／スキップ（PoC 優先）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## 記入例

```text
## Question 1
[Answer]: A

## Question 2
[Answer]: A
...
```
