# Code Generation Plan — onboarding-f-guide

**ユニット**: onboarding-f-guide（ドキュメントのみ）  
**作成**: 2026-08-19  
**入力**: `aidlc-docs/inception/requirements/onboarding-f-requirements.md`  
**ブランチ**: `feature/onboarding-f-guide`  
**原則**: C# / シーン / ScriptableObject は変更しない。企画本文は Drive を正とし複製しない。個人の連絡先は書かない。

本計画が Code Generation の正。各ステップ完了時に同じ作業内で `[x]` にする。

---

## ユニット文脈

| 項目 | 内容 |
|---|---|
| 目的 | Fさんが担当シーンに着手できる導入ドキュメント |
| 依存 | 既存 README、`docs/Sさん向けガイド.md`、実装パス（参照のみ） |
| トレース | FR-ONB-01〜08 / NFR-ONB-01〜05 |
| Stories | User Stories SKIP。受入は要件の成功基準 |

---

## 実行ステップ

- [x] **Step 0** 参照パスがリポジトリに存在することを確認する（下記「参照パス」）
- [x] **Step 1** `docs/Fさん向けガイド.md` を新規作成する（導入編＋リファレンス編）
  - 導入編: Unity 6000.4.2f1、Play、担当表（打ち合わせ 2026-08-18）、Git/PR、触ってよい／合意が必要／触らない
  - 見た目の最短再掲と Sさんガイドへのリンク
  - リファレンス: アセンブリ境界、SceneId / ModuleId / ModuleRouter、共通サービス、Settings SO、Editor メニュー注意
  - 音楽理論対応表（PitchMath、SoundMatchConfig、EffectChain、Recipe、IPitchVariationService、ウィレムスは Drive リンクのみ）
  - 新ゲーム追加手順（Game1 を型: asmdef → ScreenRootBase → SceneId 末尾追加 → ゲーム選択接続 → Build Settings → Progression は前本合意 → EditMode）
  - 未確定は未確定と書く（サウンドレスキューの音律、評価テスト、かな／漢字）
- [x] **Step 2** `README.md` を更新する
  - 役割表に Fさん（ゲーム実装／音楽理論設計）とガイドリンク
  - ドキュメント案内に Fさんガイド
  - シーン表に Library / Create と、Fさん担当の今後のゲームを注記（未実装シーンはパスを捏造しない）
- [x] **Step 3** `docs/Sさん向けガイド.md` に Fさんガイドへのリンクを 1 箇所追加する
- [x] **Step 4** `aidlc-docs/construction/onboarding-f-guide/code/code-summary.md` に生成サマリを書く
- [x] **Step 5** 自己検査: パス実在、PII なし、Drive 本文の複製なし、担当表が要件 §4 と一致

---

## 参照パス（ガイドに書く実在パス）

| 用途 | パス |
|---|---|
| 起動 | `Assets/Main画面.unity` |
| ホーム | `Assets/Scenes/Geidai/GeidaiHome.unity` |
| 録音 | `Assets/Scenes/Geidai/GeidaiRec.unity` |
| ①音合わせ | `Assets/Scenes/Geidai/GeidaiGame1.unity` |
| 音作り | `Assets/Scenes/Geidai/GeidaiCreate.unity` |
| 音図鑑 | `Assets/Scenes/Geidai/GeidaiLibrary.unity` |
| ゲーム選択 | `Assets/game_Home.unity` |
| SceneId | `Assets/Scripts/Common/Models/SceneId.cs` |
| ModuleId / Router | `Assets/Scripts/Foundation/ModuleId.cs`, `ModuleRouter.cs` |
| Pitch | `Assets/Scripts/Common/Audio/PitchMath.cs`, `SoundEffectMapper.cs` |
| 音合わせ出題 | `Assets/Scripts/Common/Game/QuestionBuilder.cs`, `Assets/Settings/SoundMatchConfig.asset` |
| ピッチシフト | `Assets/Scripts/Services/Audio/IPitchVariationService.cs` |
| エフェクト | `Assets/Scripts/Services/Audio/EffectChain.cs` |
| レシピ | `Assets/Scripts/Common/Create/SoundRecipe.cs`, `RecipeTimbreKind.cs` |
| 進行 | `Assets/Scripts/Services/Progression/IProgressionService.cs` |
| Game1 型 | `Assets/Scripts/Game1/Geidai.Game1.asmdef` |
| Create 型 | `Assets/Scripts/Create/Geidai.Create.asmdef` |

③音並べ・④サウンドレスキューのシーンファイルは未作成。ガイドでは「これから作る」と書き、存在しないパスを書かない。

---

## 明示的にやらないこと

- Unity コード・シーン・アセットの変更
- 打ち合わせ PDF のリポジトリ追加
- メールアドレスなど登録仕様の実装指示をガイドの主内容にすること
- User Stories / 新ユニット設計書の作成
