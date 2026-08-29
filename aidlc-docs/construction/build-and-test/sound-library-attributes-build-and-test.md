# Build and Test — サウンドライブラリ属性（U7a / U7b）

**ブランチ**: `feature/sound-library-attributes`  
**作成**: 2026-08-29  
**対象**: U7a Schema & Catalog API + U7b Editor & Library UI

既存の全体手順（`build-instructions.md` / `unit-test-instructions.md` 等）に加え、本ワークストリーム差分をここにまとめる。

---

## 1. ビルド（Unity）

### Prerequisites
- Unity `6000.4.2f1`（Unity 6 / URP）
- ブランチ `feature/sound-library-attributes` をチェックアウト
- 外部サービス・環境変数・シークレット不要（完全オフライン）

### 手順
1. Unity で本プロジェクトを開く
2. Console で **Error 0** を確認（スクリプト再コンパイル完了後）
3. （任意）シーンを最新ブートストラップで揃える:
   - メニュー **Geidai → Scenes → Build All Geidai Scenes**
   - または Library のみ: MCP / Roslyn で `GeidaiSceneBootstrap.BuildLibrary()`
4. 展示用: **Geidai → Build → Android Development APK** または iOS（既存手順どおり）

### 成功条件
- コンパイル Error 0
- `GeidaiLibrary` が Build Settings に含まれ、ホーム「おとずかん」から遷移できる

---

## 2. Unit Test（EditMode）

### 本ワーク追加・更新
| テスト | 件数目安 | 内容 |
|---|---|---|
| `CuratedSoundValidationTests` | 6 | Upsert 検証・CanRemove・LibraryQuery・PBT |
| `LibraryFilterOptionsTests` | 3 | カテゴリ／音色選択肢・選択維持 |
| `UnlockEvaluatorTests` | 5 | 新スキーマ Def ヘルパ追随 |

### 実行
- Editor: **Window → General → Test Runner → EditMode → Run All**
- またはメニュー **Geidai → Tests → Run EditMode All**
- フィルタ実行例: `LibraryFilterOptions` / `CuratedSoundValidation` / `UnlockEvaluator`

### 記録（2026-08-29 MCP）
- コンパイル: Error 0
- `LibraryFilterOptionsTests`: **3/3 PASS**
- `CuratedSoundValidationTests`: **6/6 PASS**
- `UnlockEvaluatorTests`: **5/5 PASS**（U7a 時点）

---

## 3. Integration / Play Mode

### Editor 登録（US-LIB-04）
1. **Geidai → Library → Curated Sound Catalog** を開く
2. Catalog / Timbre 既定 SO が選択されていることを確認
3. 「ついか」→ WAV 選択 → とりこむ → 必須属性入力 → ほぞん
4. ID／図鑑ナンバー重複時は保存されずメッセージが出る
5. 参照中タグの削除が拒否される

### プレイヤー図鑑（US-LIB-01）
1. Play → ホーム → **おとずかん**
2. 図鑑ナンバー順で一覧される
3. カテゴリ／音色ドロップダウンで絞り込める
4. 行タップで詳細（説明等）が表示される
5. 画像なし音は placeholder が表示される
6. ロック音は「きく」非活性。解除音は試聴できる
7. 「もどる」でホームへ

### Create 互換
1. おとつくり → カタログ音が一覧に出る（新スキーマ ValidItems）
2. クラッシュしない

---

## 4. Performance（軽量）

| 項目 | 目安 |
|---|---|
| LibraryQuery Sort+Filter（≤100件） | EditMode 16ms 未満（U7a NFR） |
| フィルタ変更〜一覧更新 | 体感即時（仮想化なし） |

計測は任意。展示端末で一覧スクロールのカクつきが無ければ十分。

---

## 5. Security / Resiliency（確認観点）
- 端末外送信なし
- 属性・ログに PII／説明全文を載せない
- UnlockState は既存 AtomicFile 方針を維持（本差分で変更なし）

---

## 6. 合否チェックリスト

- [x] C# コンパイル Error 0（2026-08-29）
- [x] `GeidaiLibrary` 再生成（BuildLibrary / Build All）
- [x] EditMode（本ワーク関連）PASS
- [ ] Play Mode: 図鑑フィルタ・詳細・試聴
- [ ] Editor Window: WAV→属性→保存／タグ CRUD
- [ ] Create からカタログ読取スモーク
- [ ] （任意）Android/iOS 展示ビルド

---

## 7. 成果物パス
- コード: `Assets/Scripts/Common/Library/`、`Assets/Scripts/Library/`、`Assets/Editor/CuratedSoundCatalog*`
- 設計: `aidlc-docs/construction/u7a-schema/`、`u7b-editor-library-ui/`
- 本ファイル: `aidlc-docs/construction/build-and-test/sound-library-attributes-build-and-test.md`
