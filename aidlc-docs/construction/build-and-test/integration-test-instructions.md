# Integration Test Instructions（統合テスト手順）

**プロジェクト**: 藝大 須藤さんアプリ
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / Build and Test

## Purpose（目的）
ユニット間（アセンブリ間）およびサービス連携が正しく協調することを検証する。本アプリはオフライン単体アプリのため、統合テストは **(a) サービス層の EditMode/PlayMode 結合テスト** と **(b) 実シーンでの手動 E2E** の二層で行う。

> ⚠️ 多くの結合は**実シーン配線が前提**（コントローラ⇄プレハブ⇄サービス）。実シーン作成/配線は各ユニット code-summary の「残タスク（MCP フォローアップ）」であり、シーン整備後に本手順の手動シナリオを実施する。サービス層のロジック結合は EditMode/PlayMode で先行検証可能。

---

## 依存関係マップ（統合の観点）
```
Foundation(Boot/Home) ──GoTo──> Rec / Collection / Theme / Game1   （すべて INavigationService 経由）
Rec ──SaveSound──> StorageService ──(wav+meta 原子的書込)
Collection ──ListSounds/LoadSoundBuffer/DeleteSound/SaveMeta──> StorageService
Collection ──Play(buffer,settings)──> AudioService(共有・EffectChain)
Theme ──ThemeContext.Set──> (Rec が Current 参照) ──> Rec
Game1 ──ListSounds/LoadSoundBuffer──> StorageService ──> PitchVariationService(再生時ピッチ)
全モジュール ──Resolve──> ServiceRegistry（AppManager で登録）
```

---

## Test Scenarios（シナリオ）

### Scenario 1: 起動 → ホーム → 各モジュール遷移（Foundation ↔ Navigation）
- **説明**: Boot が初回/既存を判定し、Home メニュー（データ駆動）から各モジュールへ安全に遷移する。
- **セットアップ**: `AppManager` がサービス登録済みの起動シーン。`HomeMenuConfig` アセット割当。
- **手順**: 起動 → 初回はユーザー登録、既存はホーム → 各メニュー（Rec/Collection/Theme/Game1）をタップ → 戻るでホーム復帰。
- **期待**: 遷移失敗時は `ErrorPresenter` が平易文言表示。未登録シーンでもクラッシュしない（`Result` で失敗返却）。
- **後片付け**: なし（永続はプロファイルのみ）。

### Scenario 2: 録音 → 保存 → コレクション表示/再生（Rec → Storage → Collection）
- **説明**: 3秒録音・エフェクト付与・保存した音が、Collection 一覧に現れ、保存エフェクトを再適用して再生できる。
- **セットアップ**: マイク許可済みの実機/エディタ。空の `persistentDataPath`。
- **手順**: Rec で録音（3秒自動停止）→ エフェクト調整 → 保存 → Home 経由で Collection → 一覧に新規項目 → 項目再生。
- **期待**: `sounds/{id}.wav` ＋ `{id}.meta.json` が**原子的**に対で作成。Collection 一覧に反映。再生は共有 `AudioService`＋`EffectChain` で保存設定を非破壊適用。保存途中失敗時に wav/meta の片割れが残らない。
- **後片付け**: `persistentDataPath/sounds/` を削除。

### Scenario 3: メタ編集・写真・削除の整合（Collection ↔ Storage）
- **説明**: タイトル/メモ/ニックネーム編集、写真取り込み（U4 はスタブ）、削除が全関連ファイルに整合的に反映される。
- **手順**: 項目編集→保存（`SaveMeta` 原子的置換・settings 保持）→ 写真設定（`SavePhoto`）→ 削除確認ダイアログ→削除。
- **期待**: メタのみ更新で wav 不変。削除で `wav+meta+photo` を一括削除。破損/欠損メタは一覧で安全にスキップ（空フォールバック）。
- **後片付け**: `sounds/` 削除。

### Scenario 4: お題 → Rec 連携（Theme → Rec）
- **説明**: 今週のお題を表示し、タップで `ThemeContext` に設定して Rec へ遷移。お題未設定でも通常録音できる。
- **手順**: Theme 画面表示（`ThemeCatalog` 割当）→ お題タップ → Rec 遷移（お題ラベル任意表示）→ 録音。
- **期待**: 週選択は決定的（`ThemeSelector`）。空/無効カタログは `emptyState`。`ThemeContext` は非永続（アプリ再起動で消える）。
- **後片付け**: なし。

### Scenario 5: ①音合わせ 素材選択・出題・解答（Game1 → Storage → PitchVariation）
- **説明**: 保存音を素材に出題し、タップ確認・ドラッグ解答・判定・演出・進行が成立する。0 件時は fallback。
- **セットアップ**: Scenario 2 で保存音を数件用意（または `SoundMatchConfig.fallbackClip` 設定）。
- **手順**: Game1 開始 → お手本/選択肢提示 → 選択肢タップで確認再生 → カエルへドラッグ → 判定（正解=進化/不正解=やり直し）→ 全問終了で結果サマリ。
- **期待**: 出題は正解ちょうど1つ・不正解は `centsStep` 以上離れる。加工音は**非保存**（再生時ピッチ）。保存音 0 件は fallback→Empty（`ErrorPresenter` 警告）。領域外ドロップは元位置復帰。
- **後片付け**: なし（ゲーム状態は非永続）。

### Scenario 6: サービス登録・解決の一貫性（ServiceRegistry / AppManager）
- **説明**: 各 Bootstrap（Rec/Theme/Game1）が必要サービスを解決/未登録なら登録し、シーンをまたいで単一インスタンスを共有する。
- **手順**: 起動 → 複数モジュールを行き来 → `AudioService`/`PitchVariationService` の常駐リグ（`DontDestroyOnLoad`）が重複生成されないこと。
- **期待**: サービスは単一。クロスシーン再生が継続。

---

## Setup Integration Test Environment（環境準備）

### 1. サービス起動
外部サービスなし（オフライン）。エディタ Play または実機起動のみ。
```bash
# PlayMode 結合テストを CLI で実行する場合（PlayMode テストを追加した場合）
"/Applications/Unity/Hub/Editor/6000.4.2f1/Unity.app/Contents/MacOS/Unity" \
  -runTests -batchmode -projectPath "$(pwd)" \
  -testPlatform PlayMode \
  -testResults ./Logs/playmode-results.xml -logFile ./Logs/playmode.log
```

### 2. エンドポイント設定
不要（ネットワーク境界なし）。データ境界は `Application.persistentDataPath` のみ。

## Run Integration Tests（実行）
1. 実シーン整備後、上記 Scenario 1〜6 を手動 E2E で実施（実機・縦横両向き・複数解像度）。
2. サービス層の自動結合は EditMode（`StorageCollectionTests`/`AtomicFileTests`/`SaveSoundTests` が Storage 結合を、`ContentServiceThemeTests` が Content 結合をカバー）で先行検証。
3. ログは `Unity_GetConsoleLogs`（MCP）または Editor Console / `./Logs/*.log` で確認。

### Cleanup（後片付け）
```bash
# 端末/エディタの永続データをリセット（テスト間の独立性確保）
# エディタ persistentDataPath 例（macOS）:
rm -rf "$HOME/Library/Application Support/DefaultCompany/"*  # 実 Company/Product 名に合わせて調整
```

---

## 合否基準
- Scenario 1〜6 が縦横両向き・代表解像度（スマホ/タブレット）で破綻なく完了。
- 保存/削除/編集でファイル片割れ・破損残存がない（原子性）。
- 遷移/権限/空データの各失敗が**クラッシュせず**平易な通知に落ちる。
