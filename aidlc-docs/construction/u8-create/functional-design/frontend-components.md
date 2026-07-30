# U8 Sound Create — Frontend Components

**作成**: 2026-07-30

## 構成
- **CreateScreenController** (: ScreenRootBase)
- **RecipeLayerPicker**（スロットA/B）
- **RecipeEffectPanel**（volume/pitch/reverb/timbre）
- **RecipeListController**（保存一覧・開く・削除）
- **RecipeExportController**（書き出し確認）
- ConfirmDialog / ErrorPresenter

## 状態
Idle → Picking → Previewing → Editing → Saving / Exporting → Ready / Error

## ハンドオフ（企画・デザイン）
- スライダー見た目、スロット表現、書き出し文言
- 音づくり画面のモチーフ配置

## 非対象
- 3音以上、DAW級編集、ユーザー間共有
