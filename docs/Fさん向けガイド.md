# Fさん向けガイド — ゲーム実装・音楽理論設計

ゲーム開発と音楽理論の設計を担当する人向けの導入です。Unity / C# の一般知識は前提に、このリポジトリ固有の約束事だけを書きます。

見た目・お題・イラストの詳細は **[Sさん向けガイド.md](Sさん向けガイド.md)**。  
企画・仕様の正は Google Drive の `プロジェクト概要.md`（リポジトリには複製しない）。

---

# 導入編

## 1. 最短で動かす

1. Unity Hub でこのリポジトリを開き、**Unity 6000.4.2f1** で起動する  
2. File > Build Settings で先頭が `Assets/Main画面.unity` であることを確認する  
3. Play → ホーム（`GeidaiHome`）から録音・ゲーム選択・音図鑑・音づくりへ辿れることを確認する  
4. テスト: メニュー `Geidai/Tests/Run EditMode All`（結果は `Logs/editmode-summary.txt`）

日常的に **`Geidai/Scenes/Build All Geidai Scenes` は実行しない**。シーン上の見た目が消えます。

## 2. いまの担当（2026-08-18 打ち合わせ）

役割ニックネーム: **前本**（基盤・統合）、**Sさん**（企画・デザイン）、**Fさん**（ゲーム実装・音楽理論）。連絡先はここには書かない。

| 画面 | 担当 | 目安 | リポジトリ上の状態 |
|---|---|---|---|
| Boot / 登録 / ホーム / ユーザー情報 | 前本 | 11月中心 | シーンあり |
| 録音 Rec | 前本 | 11月（訂正あり） | `GeidaiRec` |
| ゲーム選択 | 前本 | ひとまず 11月 | `Assets/game_Home.unity` |
| ①音合わせ | 前本 | 11月（音色のみ可） | `GeidaiGame1` |
| ②音の神経衰弱 | 前本 | 11月（音色のみ可） | シーン未作成 |
| **③音並べ** | **Fさん** | **11月（高い順・低い順まで可）** | **未作成。最初の実装入口** |
| **④サウンドレスキュー** | **Fさん** | **3月** | **未作成。音階・音律は未確定** |
| **音作り** | **Fさん** | **3月** | `GeidaiCreate` あり。拡張・改修 |
| サウンドライブラリ | 前本 | 11月（一部可）・優先度高 | `GeidaiLibrary` |

展示の目安: 任意 2026-11-20 頃〜12-02（未確定）、必須 2027-03-19〜21（実機インストール可なら配信不要）。

④は打ち合わせ上「音を聞いて声を出す」ゲーム。企画 PDF のコインバードとは別物として扱う。

## 3. Git

1. `main` から feature ブランチを切る  
2. 担当モジュールだけを変える  
3. Pull Request で統合する  
4. `Geidai.Services.*` / `Geidai.Common.*` の公開 IF を変える前に前本と合意する  

## 4. 触ってよい／合意が必要／触らない

**主に変えてよい**

- 担当シーン（③、④、音作り）とその asmdef 配下
- 担当ゲームの純粋ロジックと EditMode テスト
- 担当モジュール内の難易度・音律パラメータ（または合意済み ScriptableObject）

**前本と先に合意する**

- `Geidai.Services.*` と `Geidai.Common.*` の公開 IF
- `SceneId` / `ModuleId` / `NavigationService` のマップ / Home メニュー
- `IProgressionService.NotifyGameCleared(gameKey)` のキー
- Build Settings・パッケージ依存

**日常的にやらない**

- `Geidai/Scenes/Build All Geidai Scenes`
- 登録・Home・Rec・Library・①② の本番改修
- 個人の連絡先や企画 PDF 本文をリポジトリへ置くこと

## 5. 見た目（最短）

シーン上の Text / Image / Rect Transform と `Assets/Settings/` のアセットで変える。Sprite は Texture Type を Sprite (2D and UI) にする。コントローラの参照欄は空にしない。詳細は [Sさん向けガイド](Sさん向けガイド.md)。

---

# リファレンス編

## 6. アーキテクチャ

完全オフライン。サーバーなし。データは `Application.persistentDataPath`。

依存は一方向:

```
画面モジュール (Game1 / Create / Library / Rec / ...)
        |
        v
Geidai.Services  (Audio / Storage / Navigation / Progression / Content)
        |
        v
Geidai.Common    (Models / Audio math / Recipe / Unlock / Game 純粋ロジック)
```

画面どうし（例: Create → Rec、Game3 → Collection）は参照しない。保存音が必要なら `IStorageService`、発音は `IAudioService` または `IPitchVariationService`。

### シーンと ID

| SceneId | Unity シーン名 | ファイル |
|---|---|---|
| Boot | `Main画面` | `Assets/Main画面.unity` |
| Home | `GeidaiHome` | `Assets/Scenes/Geidai/GeidaiHome.unity` |
| Register | `GeidaiRegister` | 同フォルダ |
| Rec | `GeidaiRec` | 同 |
| Collection | `GeidaiCollection` | 同 |
| Theme | `GeidaiTheme` | 同 |
| Game1 | `GeidaiGame1` | 同 |
| GameSelect | `game_Home` | `Assets/game_Home.unity` |
| Library | `GeidaiLibrary` | `Assets/Scenes/Geidai/GeidaiLibrary.unity` |
| Create | `GeidaiCreate` | `Assets/Scenes/Geidai/GeidaiCreate.unity` |

マップの実装:

- 列挙: `Assets/Scripts/Common/Models/SceneId.cs`（既存値の順序は変えず、末尾に追加）
- ホーム導線: `Assets/Scripts/Foundation/ModuleId.cs` → `ModuleRouter.cs`
- 実シーン名: `Assets/Scripts/Services/Navigation/NavigationService.cs` の `SceneMap`

遷移: `INavigationService.GoTo(SceneId)` / `GoBack()`。画面コントローラは `ScreenRootBase` を継承する（①の例: `SoundMatchGameController`）。

### 共通サービス

起動時に `AppManager` が `ServiceRegistry` へ登録する。

| 契約 | 用途 |
|---|---|
| `IAudioService` | 録音、再生、同梱クリップ試聴、2レイヤー再生、WAVE 書き出し |
| `IPitchVariationService` | 出題用の再生時ピッチ（セント）。加工音は保存しない |
| `IStorageService` | ユーザー録音・メタ・レシピ・解除状態 |
| `INavigationService` | シーン遷移 |
| `IProgressionService` | `NotifyGameCleared` / `NotifyRecordingChallenge` |
| `IContentService` | お題・キュレーションカタログ |

失敗は例外で落とさず `Result` / `Result<T>`。ログは `SafeLogger`。

### Settings（データ駆動）

| アセット | 中身 |
|---|---|
| `Assets/Settings/HomeMenuConfig_Default.asset` | ホームボタン |
| `Assets/Settings/ThemeCatalog.asset` | 今週のお題 |
| `Assets/Settings/SoundMatchConfig.asset` | ①の出題数・選択肢・セント段階 |
| `Assets/Settings/CuratedSoundCatalog_Default.asset` | 音図鑑の同梱音 |
| `Assets/Settings/UnlockRulesCatalog_Default.asset` | 解除条件 |
| `Assets/Settings/UITheme_Default.asset` | 色・モチーフ置き場 |

## 7. 音楽理論 ↔ 実装

ウィレムス（聴く → 図で表す）の説明本文は Drive を正とする。コードに教材文章は埋め込まない。

### 単位

| 概念 | コード | 換算 |
|---|---|---|
| 1 半音 | `PitchMath.CentsPerSemitone` = 100 | 100 セント = 1 半音 |
| 1 オクターブ | `PitchMath.CentsPerOctave` = 1200 | |
| セント → 周波数比 | `PitchMath.CentsToRatio` | `AudioSource.pitch` に渡す値 |
| 半音 → 比 | `PitchMath.SemitonesToRatio` | Rec / 音作りのピッチ |

ファイル: `Assets/Scripts/Common/Audio/PitchMath.cs`  
UI 正規化（リバーブ 0〜1 など）: `Assets/Scripts/Common/Audio/SoundEffectMapper.cs`

### ①音合わせ（既存実装例・前本所有）

難易度は **選択肢間の最小ピッチ間隔（セント）**。既定 SO:

| ラベル | centsStep（コード既定） |
|---|---|
| かんたん | 200 |
| ふつう | 100 |
| むずかしい | 50 |
| とても難しい | 20 |

企画 PDF の候補（300/400/500 など）と SO の値が違うことがある。変える場所は `SoundMatchConfig.asset` と `QuestionBuilder`（純粋・PBT あり）。

発音: `IPitchVariationService.Play(cents)` → 内部で `AudioSource.pitch = CentsToRatio(cents)`。加工済み WAV は作らない。

打ち合わせ「ライブラリ音からピッチシフトして出題」もこの経路。素材取得は `IStorageService`（ユーザー音）またはカタログクリップ。Game モジュールから Collection アセンブリは参照しない。

### 音作り（Fさん・3月）

レシピはクリップを複製せず **素材 ID + パラメータ**。

| パラメータ | 型 | 範囲 | 再生 |
|---|---|---|---|
| volume | float | 0〜1 | `AudioSource.volume` |
| pitchSemitones | int | -12〜+12 | `EffectChain` → `pitch` |
| reverb | float | 0〜1 | `AudioReverbFilter`（room / decay / level） |
| timbre | `RecipeTimbreKind` | None / Robot / Chorus | LPF/HPF/Distortion プリセット |

型: `SoundRecipe` / `SoundRecipeLayer` / `RecipeClamp`  
再生: `IAudioService.PlayLayers`  
音色の内部対応: Robot → Hard、Chorus → Soft、None → Original（`AudioService` → `EffectChain`）

### Rec の加工（前本所有・参照用）

同じ `EffectChain`。ノイズ低減は Rec 側。音作り MVP では使わない。

## 8. 新しいゲームシーンの追加（③をこの型で）

型にする既存物: `Assets/Scripts/Game1/` と `Geidai.Game1.asmdef`。

1. `Assets/Scripts/Game3/` を作り、`Geidai.Game3.asmdef`  
   - references: `Geidai.Common`, `Geidai.Services`, `UnityEngine.UI`  
   - Rec / Collection / Library / Create は参照しない  
2. 純粋ロジック（並べ替え判定など）は `Geidai.Common.Game` か Game3 内の静的クラスに置き、EditMode で試す  
3. `ScreenRootBase` のコントローラ + Bootstrap（サービス解決は `ServiceRegistry`）  
4. シーン `Assets/Scenes/Geidai/GeidaiGame3.unity` を作る（Build All に頼らない）  
5. `SceneId` の **末尾** に `Game3` を追加  
6. `NavigationService.SceneMap` に `{ SceneId.Game3, "GeidaiGame3" }`  
7. File > Build Settings にシーンを追加  
8. ゲーム選択（`game_Home`）から `GoTo(SceneId.Game3)`。選択 UI の所有は前本なので、カード追加は合意する  
9. クリアで図鑑を開けるなら `IProgressionService.NotifyGameCleared("game3")` のキーを UnlockRules と揃えて合意する  
10. テストは `Assets/Scripts/Tests/EditMode/`。`Geidai.Tests.asmdef` が Game3 を要すれば references に足す。純粋関数は FsCheck（PBT）対象になり得る  

④サウンドレスキューも同じ型。マイク入力が必要なら `IAudioService` の録音 API を使う。音階・音律は未確定。

音作りの改修は既存 `Geidai.Create` と `GeidaiCreate` シーンを拡張する。新規 asmdef は不要。

## 9. 未確定（決定済みにしない）

- ④で使う音階・音律
- 評価用テスト画面の有無と指標
- 年齢別のかな表記・読み上げ（3月は漢字まででも可、という打ち合わせ上の案）
- 音図鑑の分類（音色別 / 種類別 / 開放順）— 所有は前本＋企画
- 登録項目の追加（メール等）— 所有は前本。本ガイドの実装範囲外

## 10. 関連

- [README.md](../README.md)
- [Sさん向けガイド.md](Sさん向けガイド.md)
- ビルド・実機: `aidlc-docs/construction/build-and-test/`
- 要件（このガイドの範囲）: `aidlc-docs/inception/requirements/onboarding-f-requirements.md`
