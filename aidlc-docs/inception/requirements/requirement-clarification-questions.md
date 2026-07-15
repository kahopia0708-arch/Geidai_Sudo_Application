# Requirements Clarification（明確化質問）

回答（`requirement-verification-questions.md`）を分析した結果、**MVPスコープに矛盾**が見つかりました。要件確定のため、以下を明確化してください。
各 `[Answer]:` に記号を記入し、終わったら「done」等でお知らせください。

---

## 矛盾1：MVPにゲーム（①音合わせ）を含めるか

- **Q2 = A**「最小 — Rec＋MySoundCollection のみ」→ ゲームは MVP に含まない
- **Q3 = A**「最初に実装するミニゲームは ①音合わせ」＋ **Q8 = A**「MVP からユーザー録音音をゲームで使う」→ ゲームを MVP に含む

これらは矛盾します（ゲームを MVP に入れるか入れないか）。

参考: `プロジェクト概要.md` では Phase 1（基盤=Rec/Collection、9〜10月）と Phase 2（プロトタイプ=ミニゲーム、秋冬・中間報告11月）が分かれています。

### Clarification Question 1
「触ってもらえるプロトタイプ（MVP）」の範囲をどう定義しますか？

A) 段階型（推奨）— まず Rec＋Collection（Phase1基盤）を完成させ、続けて同じ計画内で ①音合わせ＋ユーザー音連携（Phase2）まで設計する。実装/コミットは基盤→ゲームの順。

B) 基盤のみ — MVP は Rec＋Collection のみ。ゲームは今回の計画スコープ外（別途あらためて計画）。

C) 一体型 — 最初から Rec＋Collection＋①音合わせ＋ユーザー音連携をひとまとまりの MVP として設計する。

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## 矛盾2：初回ユーザー登録をMVPに含めるか

- **Q2 = A**「最小 — Rec＋MySoundCollection のみ」→ ユーザー登録は含まない
- **Q11 = A**「初回ユーザー登録を企画通り MVP で実装」→ ユーザー登録を含む

### Clarification Question 2
初回ユーザー登録（生年・ニックネームのローカル保存）はいつ実装しますか？

A) MVP（基盤フェーズ）に含める（Q11 を優先）

B) 後続フェーズに回す（Q2 の「最小」を優先。まず録音・保存体験に集中）

X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## 確認：その他の回答（矛盾なし・このまま採用予定）

以下は矛盾がないため、この方針で `requirements.md` に反映します（訂正があれば併せてご記入ください）:

- 対象OS: **iOS/Android 両方**（MVPは片方から）
- 共有(Place): **MVPでは無効化/非表示**（既存Placeシーンは非導線化。文字列不一致バグも解消）
- Rec録音: **3秒固定**
- 録音実装: **VoiceRecordingSection（Unity標準Filter）に統一**（RecorderWithEffects は整理）
- 音色エフェクト: **企画準拠（ロボット/コーラス系）**
- ゲーミフィケーション（経験値/コイン/ライフ/課金）: **全除外**
- コレクション: **企画通り拡張（写真・メモ・ニックネーム＋月別/キーワード検索）**
- データ/プライバシー: **完全ローカルのみ（サーバー送信なし）**
- 拡張機能: **Security=適用 / Resiliency=適用 / PBT=全適用**

[訂正があれば Answer]: 
