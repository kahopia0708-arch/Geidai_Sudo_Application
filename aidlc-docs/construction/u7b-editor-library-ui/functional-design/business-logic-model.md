# U7b — Business Logic Model

**ユニット**: U7b  
**作成**: 2026-08-29

## 1. Editor — 登録／更新フロー

```text
[Open Window]
    → 対象 Catalog / TimbreTagCatalog を選択（既定 Settings 資産）
    → 音一覧を ValidItems 相当で表示
    → [追加] or 行選択
         → SoundDraft をフォームにロード（追加時は空＋allowPitchShift=true）
    → [WAV 指定]
         → Assets/Audio/Library/{id}.{ext} へ配置 → clipRef 設定
    → [保存]
         → ValidateForUpsert
         → OK: Catalog 更新 + AssetDatabase.SaveAssets
         → NG: エラー表示、ディスク変更なし
```

## 2. Editor — 音色語彙 CRUD

```text
[語彙パネル]
    → ValidTags 一覧
    → 追加／編集 → ValidateTagUpsert → SO 更新
    → 削除 → CanRemoveTag
         → false: 「使われているタグは消せない」
         → true: 削除して保存
```

## 3. Player — 図鑑画面フロー

```text
[OnShow / Reload]
    → Progression.Reload + EnsureCatalogs
    → ValidItems → SortAndFilter(category, timbre)
    → UnlockEvaluator.Project → ListView
    → フィルタ変更 → 再 SortAndFilter（カタログ再読込は任意で軽量再適用）
    → 行選択 → selectedId → DetailPanel 更新
    → 試聴 → unlocked かつ clip ありのみ（ボタン非活性でガード）
    → もどる → Home
```

## 4. データ依存

| 入力 | 処理 | 出力 |
|---|---|---|
| WAV ファイル | Editor Import | AudioClip under `Audio/Library` |
| SoundDraft | Validation + Upsert | CuratedSoundCatalog SO |
| TimbreDraft | Tag validation | TimbreTagCatalog SO |
| Catalog + Unlock + Filters | Query + Project | LibraryRow / Detail |
| Placeholder asset | UI bind | Image when imageRef null |

## 5. エラー方針

| 状況 | ユーザー向け |
|---|---|
| 必須欠落・重複 | Editor ダイアログ／ヘルプボックス |
| タグ参照削除 | 削除拒否メッセージ |
| カタログ未設定（Play） | 既存どおり「おとのずかんが ないよ」系 |
| 空フィルタ結果 | Empty 状態（0件メッセージ） |
