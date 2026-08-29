# U7b NFR Requirements Plan — Editor & Library UI

**ユニット**: U7b  
**作成**: 2026-08-29  
**前提**: U7a NFR 継承（オフライン・PII・Unlock AtomicFile・Query 16ms）。Infrastructure SKIP

## チェックリスト
- [x] 質問回答（Q1〜Q4 = A）
- [x] nfr-requirements / tech-stack-decisions 生成
- [x] 承認ゲート

---

## Question 1 — プレイヤー一覧の体感性能

A) **推奨**: 100 件以下でフィルタ変更〜一覧更新は体感即時（目安 1 フレーム相当／特別な仮想化なし）。ScrollRect 既存踏襲

B) 仮想化リスト（大量件数向け）を本ユニットで導入

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 2 — Editor ウィンドウの実装スタック

A) **推奨**: 既存プロジェクト同様 **IMGUI（EditorGUILayout）** の EditorWindow。追加パッケージなし

B) UI Toolkit（UXML/USS）で Editor Window を新規構築

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 3 — テスト範囲（U7b）

A) **推奨**: EditMode でフィルタ選択肢生成・placeholder 解決・画面用純粋ヘルパを検証。EditorWindow 本体は手動（メニュー操作）＋必要なら最小 Editor テスト。Play Mode はシーン配線後のスモーク

B) EditorWindow の自動テストまで必須（UI 操作シミュレーション）

C) コード生成後の手動確認のみ（新規自動テストなし）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 4 — Editor 保存失敗時の扱い

A) **推奨**: Validation 失敗時は保存せず、ウィンドウ内に日本語メッセージ。例外時は Dialog＋ログ（PII・説明全文なし）

B) 失敗時も仮保存し、Inspector で後修正

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

記入後 **done** と送ってください。
