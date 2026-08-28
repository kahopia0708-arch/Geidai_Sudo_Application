# Sさん向けガイド — デザイン・コンテンツの変え方

このアプリでは、**見た目と教材コンテンツはできるだけコードを触らずに変えられる**ようにしてあります。  
（分担: 前本＝動く骨組み／Sさん＝見た目・お題・パラメータ・イラスト）

困ったら LINE で前本へ。C# の編集が必要な変更は前本側の作業になります。

---

## 1. 役割の境界（やること／やらないこと）

### やってほしいこと（コード不要）

- シーン上のボタン・文字の**位置・大きさ・色・文言**
- 背景・アイコン・カエル／おたまじゃくしなどの**画像差し替え**
- ホームメニューの**並び・表示/非表示・ラベル**
- **今週のお題**テキストの追加・編集
- ①音合わせの**出題数・選択肢数・難易度の目安**
- Prefab（部品）の見た目調整

### 前本に任せてほしいこと

- 録音・保存・遷移など「動く仕組み」の変更
- 新しい画面・新しいゲームの追加
- ビルドエラー・実機で動かない不具合の修正
- `Geidai/Scenes/Build All Geidai Scenes` の実行（シーンを一から作り直す操作）

---

## 2. Unity の開き方（最短）

1. Unity Hub でこのプロジェクトを開く（バージョン **6000.4.2f1**）  
2. Project ウィンドウで次を覚えておく  

| 場所 | 中身 |
|---|---|
| `Assets/Scenes/Geidai/` | 各画面のシーン |
| `Assets/Settings/` | **お題・ホームメニュー・ゲーム設定・UITheme**（ここがメイン） |
| `Assets/Prefabs/Geidai/` | 使い回し部品（例: ホームメニューボタン） |

3. シーンをダブルクリック → Hierarchy でオブジェクトを選ぶ → Inspector で調整  
4. 調整したら **Ctrl/Cmd + S** でシーン保存、またはアセット保存  

Play で動作確認 → 問題なければ Git にコミット（または変更を前本に共有）。

---

## 3. 画面ごとの見た目調整

### 3.1 どのシーンを開くか

| 画面 | シーンファイル |
|---|---|
| ホーム | `Assets/Scenes/Geidai/GeidaiHome.unity` |
| ゲーム選択 | `Assets/Scenes/Geidai/GeidaiGameSelect.unity` |
| 設定（ニックネーム等） | `GeidaiRegister.unity` |
| 録音 | `GeidaiRec.unity` |
| コレクション | `GeidaiCollection.unity` |
| 今週のお題 | `GeidaiTheme.unity` |
| ①音合わせ | `GeidaiGame1.unity` |
| ゲーム選択（旧・参照用） | `Assets/game_Home.unity` |

### 3.2 よく触るもの（Inspector）

- **Text** … 文言・フォントサイズ・色・配置  
- **Image** … 色、Sprite（画像）の差し替え  
- **Button** … 子の Label 文言、ボタン画像の色  
- **Rect Transform** … 位置・サイズ（アンカーを崩しすぎない）  

縦横どちらでも見えるよう、極端な座標（画面外）には置かないでください。SafeArea 内（画面の内側）に収めるのが安全です。

### 3.3 色・モチーフの共通設定（UITheme）

1. `Assets/Settings/UITheme_Default.asset` を開く  
2. Primary / Secondary / Background / Text などの色を変更  
3. カエル・おたまじゃくし・蓮の Sprite を差し替え  

> いまの骨組みシーンは、すべての色が Theme に自動連動しているわけではありません。  
> **まずは各シーン上の Image/Text を直接変える**のが確実です。UITheme は「共通の色・アイコン置き場」として使ってください。

---

## 4. コンテンツの差し替え（再ビルド不要）

ここは **アセットを編集するだけ**でアプリの中身が変わります。

### 4.1 ホームのメニュー

**ファイル**: `Assets/Settings/HomeMenuConfig_Default.asset`

| 項目 | 意味 |
|---|---|
| Label | ボタンに出る文字 |
| Order | 並び（小さいほど上） |
| Visible | オフにすると非表示 |
| iconKey | `Assets/Settings/HomeMenuIconCatalog_Default.asset` のキー（空＝アイコンなし） |
| Module / 遷移先 | どの画面へ行くか（変えると導線が変わります。迷ったら前本へ） |

**ホーム用プレースホルダー画像**: `Assets/Art/Home/Placeholders/`（PNG を Sprite にして差し替え。`HomeMenuIconCatalog_Default` の Sprite 参照を更新）

### 4.2 今週のお題

**ファイル**: `Assets/Settings/ThemeCatalog.asset`

各要素（Theme Item）:

| 項目 | 意味 |
|---|---|
| Text | お題本文（オノマトペなど）※空だと無効 |
| Reading | 読み（任意） |
| Hint | ヒント（任意） |
| Id | 管理用ID（空でも可） |

- リストの**順番**が週の割り当てに使われます  
- 行の追加・削除・並べ替えは Inspector のリスト操作で OK  

新規カタログを作る場合: メニュー `Assets > Create > Geidai > Theme Catalog`  
（使うにはシーン側の割り当て変更が必要なので、基本は既定アセットを編集）

### 4.3 ①音合わせのパラメータ

**ファイル**: `Assets/Settings/SoundMatchConfig.asset`

| 項目 | 意味 |
|---|---|
| Question Count | 1ゲームの出題数 |
| Choice Count | 1問の選択肢数（2以上） |
| Difficulties | 難易度名とピッチ間隔（セント）。数字が小さいほど難しい |
| Fallback Clip | 保存音が0件のときに使うお手本音（任意） |

カエルの成長スプライトなどは `GeidaiGame1` シーン内の `ResultEffect` 周りで差し替えます。

---

## 5. 画像・イラストの入れ方

1. 画像ファイル（PNG 推奨・透過可）を `Assets/` 配下に置く（例: `Assets/Art/` を新規作成してOK）  
2. 画像を選ぶ → Inspector で Texture Type を **Sprite (2D and UI)** → Apply  
3. シーンの Image コンポーネントの Source Image にドラッグ  

アイコン用は小さめ、背景用は大きめの解像度を用意するときれいです。

---

## 6. 絶対に注意すること

### シーン再生成で見た目が消える

メニュー **`Geidai/Scenes/Build All Geidai Scenes`** は、前本が骨組みを作り直すための機能です。  
**実行すると、シーン上の見た目調整が上書きされます。**  
Sさんが日常的に押す必要はありません。押す前に必ず前本に確認してください。

### スクリプト（C#）を書き換えない

`Assets/Scripts/` 内の `.cs` はロジックです。見た目調整のために編集しないでください。

### コントローラの参照を外さない

Hierarchy に `*Screen` / `*Controller` という名前のオブジェクトがあります。  
Inspector の参照欄（List View、Button など）を空にすると動かなくなります。  
「見た目の子オブジェクト」だけ動かして、参照付きコンポーネントは残してください。

---

## 7. おすすめの作業の流れ

1. 変えたい画面のシーンを開く  
2. Play で今の動きを確認  
3. Play を止めてから配置・色・文言を調整  
4. もう一度 Play で確認（縦・横の両方）  
5. お題やメニュー文言は `Assets/Settings/` のアセットも合わせて確認  
6. 保存 → Git にコミット、または変更内容を前本に共有  

---

## 8. よくある質問

**Q. ボタンの文字だけ変えたい**  
A. そのボタンの子にある `Label`（Text）を選んで文言を変更。

**Q. お題を増やしたい**  
A. `ThemeCatalog.asset` の Items に要素を追加。Text を必ず入れる。

**Q. ホームにボタンを増やしたい／消したい**  
A. `HomeMenuConfig_Default.asset` で Visible や項目追加。遷移先が分からなければ前本へ。

**Q. 実機で確認したい**  
A. ビルドは前本側で用意していることが多いです。見た目だけの確認なら Editor の Play＋Game ビューの解像度切替でも十分です。

**Q. うまく動かなくなった**  
A. 直前に触ったシーンを保存せず戻す／Git で差分を確認。解決しなければ前本に「どのシーンの何を変えたか」を伝えてください。

---

## 9. 関連リンク

- リポジトリ概要: [README.md](../README.md)  
- ゲーム実装・シーン所有: [Fさん向けガイド.md](Fさん向けガイド.md)  
- 実機確認チェックリスト: `aidlc-docs/construction/build-and-test/device-verification-checklist.md`  
- ハンドオフ方針（ストーリー）: `aidlc-docs/inception/user-stories/stories.md` の US-TECH-07  
- 企画の正: Google Drive の `プロジェクト概要.md`（リポジトリ外）
