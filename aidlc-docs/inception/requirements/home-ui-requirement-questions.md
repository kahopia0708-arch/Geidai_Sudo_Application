# ホーム画面 UI 整備 — 要件確認質問

**ワークストリーム**: ホーム／メイン画面デザイン適用（2026-08-28）  
**参照**: 打ち合わせ資料スクショ（4ボタン＋プロフィールパネル）、`GeidaiHome.unity`、`HomeMenuConfig_Default.asset`

打ち合わせデザインに沿って UI を整備します。実装前に、導線とデータ表示方針を確定したいです。  
各問の `[Answer]:` に **A / B / C …** または **Other** の説明を記入してください。

---

## Question 1
**「おとあつめ」ボタンの遷移先**はどれにしますか？（現状ホームには ろくおん / コレクション / お題 が別ボタン）

A) **コレクション**（`GeidaiCollection`）のみ。録音・お題はホームからは外し、別導線（後述 Q2）で触れる

B) **新しいハブ画面**をこのブランチで追加（録音＋コレクション＋お題への入口を1画面に集約）

C) **録音**（`GeidaiRec`）を直接開く（「あつめる」＝録音体験を前面に）

D) Other (please describe after [Answer]: tag below)

[Answer]:A

---

## Question 2
ホームから外す項目（現状: ろくおん / こんしゅうのおだい / せってい）へユーザーが行く**代替導線**は？

A) **今回はホームから非表示のみ**（各シーンは Build Settings に残し、開発・既存画面からのみ到達可）

B) **プロフィールパネル**から設定（ニックネーム編集＝既存 Register 編集モード）へ。録音・お題は「おとあつめ」内 or コレクション内ボタンで後続 PR

C) **おとあつめ（コレクション）画面**に録音・お題へのサブボタンをこのブランチで追加

D) Other (please describe after [Answer]: tag below)

[Answer]:Aだが、**プロフィールパネル**から設定（ニックネーム編集＝既存 Register 編集モード）へ

---

## Question 3
右上プロフィールバッジ＋「◯◯ のプロフィール」パネルの**数値表示**は今回どこまで実装しますか？

A) **ラベルのみ**（スクショどおり文言配置。数値は「—」や空でプレースホルダー）

B) **実データのうち既にあるものだけ**（ニックネーム、`ListSounds()` の件数、図鑑アンロック数など。ポイントは未実装のためプレースホルダー）

C) **ポイント／次の音まで**も含め、簡易ルールで計算して表示（ルールを Other で指定）

D) Other (please describe after [Answer]: tag below)

[Answer]:A

---

## Question 4
プロフィールバッジ内の**黄色い区切りプログレスバー**の意味は？

A) **図鑑アンロック進捗**（解除済み / 全カタログ件数）

B) **収集音数に対する目標**（例: 次のアンロックまであと N 音 — 文言と連動）

C) **今回は装飾のみ**（固定セグメント表示。ロジックは後続）

D) Other (please describe after [Answer]: tag below)

[Answer]:C

---

## Question 5
**メイン画面（Boot）**も今回のデザイン対象に含めますか？

A) **ホーム（`GeidaiHome`）のみ**。Boot は起動→Home 遷移のまま（見た目変更なし）

B) **Boot も背景色など最低限**をホームと揃える（ロゴ等はプレースホルダー可）

C) Other (please describe after [Answer]: tag below)

[Answer]:Boot画面は不要、ホームからスタートする。プロフィール未設定時は自動的にプロフィール登録へ

---

## Question 6
**プレースホルダー画像**の置き場所・差し替え方針は？

A) `Assets/Art/Home/Placeholders/` に PNG を置き、`HomeMenuConfig` の iconKey で参照。Sさんは後から Sprite 差し替え

B) `Assets/Settings/` に Home 用 Icon Atlas / SO を追加し、Sさん向けガイドに追記予定

C) Other (please describe after [Answer]: tag below)

[Answer]:A

---

## Question 7
**「おとあそび」**の遷移先は？

A) 既存 **ゲーム選択**（`game_Home` / GameSelect）のまま

B) 新デザイン用に **Geidai 配下のゲーム選択シーン**へ切替（別 PR 想定なら A）

C) Other (please describe after [Answer]: tag below)

[Answer]:B

