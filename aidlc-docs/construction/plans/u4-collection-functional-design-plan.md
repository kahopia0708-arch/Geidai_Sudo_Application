# U4 Persistence/Collection — Functional Design Plan（機能設計 計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U4 Persistence/Collection（永続化本実装・コレクション）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Functional Design（Part 1: Planning）
**入力**: `../../inception/application-design/unit-of-work.md`（§U4）、`unit-of-work-story-map.md`、`../../inception/requirements/requirements.md`（FR-09〜12 / NFR-04/05/07）、`../../inception/user-stories/stories.md`（EPIC-COL / US-TECH-06）、U1/U3 成果物（`Assets/Scripts/Common|Services|Rec`）、既存コレクション実装（`Assets/Scripts/MySoundCollectionStorage.cs`・`SoundSavePaths.cs`）
**含むストーリー**: US-COL-01, US-COL-02, US-COL-03, US-COL-04, US-TECH-06（対応要件: FR-09/10/11/12, NFR-04/05/07, RESILIENCY-01）

> 本ステージは**技術非依存の業務ロジック/ドメイン/業務ルール/画面構造**を詳細化する。原子的書込の具体API（`File.Replace`/temp+rename）・スクロール仮想化・写真ピッカー実装・実シーン配線などの技術は **NFR Design / Code Generation（Unity MCP）** で扱う。
> **回答方法**: 各質問の `[Answer]:` に記号（例: `A`）を記入してください。合う選択肢が無ければ「Other」。各質問に「(推奨)」案あり。完了で「done」（または「全部推奨で」）。

---

## A. 実行チェックリスト（Part 2 で実行）
- [ ] `../u4-collection/functional-design/domain-entities.md`（U4 で扱うモデル：拡張メタ（日付/タイトル/写真/メモ/ニックネーム）、一覧項目、絞込/検索条件、破損項目の表現。U1 `SoundClipMeta`/`SoundEffectSettingsData`/`SavedSound`/`AudioBuffer` を再利用・後方互換拡張）
- [ ] `../u4-collection/functional-design/business-logic-model.md`（一覧読込→視聴→削除、メタ編集、月別絞込/キーワード検索、原子的保存・破損フォールバックのふるまいとデータフロー）
- [ ] `../u4-collection/functional-design/business-rules.md`（原子性・破損スキップ・空フォールバック、削除確認、メタ検証、検索一致規則、重要度分類=Critical）
- [ ] `../u4-collection/functional-design/frontend-components.md`（Collection 画面：一覧/詳細・編集/絞込・検索/空状態の構造・状態・操作フロー・S さんハンドオフ点）
- [ ] 要件（FR-09〜12 / NFR-04/05/07 / RESILIENCY-01）・ストーリー（US-COL/US-TECH-06）とのトレース整合確認

## B. スコープ（U4 で確定する対象）
- **永続化本実装（`StorageService` 強化）**: 原子的書込（途中失敗で既存を壊さない）、破損/欠損の安全な読み飛ばし、空/初期状態のフォールバック。対象＝profile / メタ / wav /（写真）。
- **コレクション（ふるまい）**: 一覧表示・タップ視聴・削除（確認付）／メタ表示・編集（タイトル/写真/メモ）／月別絞込・キーワード検索／空状態。
- **メタデータ拡張**: 日付・タイトル（既定＝日付）・写真（任意）・メモ（任意）・ニックネーム。U1 `SoundClipMeta` を後方互換拡張。
- **画面コントローラ（U1 基盤の上に）**: `CollectionScreenController`（U1 `ScreenRootBase` 継承）/ `SoundListView` / `SoundDetailController` / `FilterSearchController`（unit-of-work §U4）。
- **U1/U3 依存の利用**: `IStorageService`（＋強化）、`SavedSound`/`SoundClipMeta`/`SoundEffectSettingsData`/`AudioBuffer`、`WavCodec`、`IAudioService`（保存音の再生）、`ErrorPresenter`/`ConfirmDialog`、`NavigationService`。
- **スコープ外**: 新規録音/加工（U3 済）、お題（U5）、ゲーム出題（U6）、クラウド/共有（Place＝MVP除外）、旧 `MySoundCollection` 形式データの移行（Q1 で判断）。

## C. 既存実装（brownfield）との関係（要判断の背景）
- **新形式（U1/U3）**: `persistentDataPath/sounds/{id}.wav` ＋ `{id}.meta.json`（`SavedSound` = `SoundClipMeta`＋`SoundEffectSettingsData`）。`StorageService.ListSounds/LoadSound/SaveSound` が対応。破損メタ・対 wav 欠損は既に「スキップ」実装あり（U1）。**ただし原子的置換・写真・メタ更新（保存）系は未実装**。
- **旧形式（brownfield）**: `persistentDataPath/MySoundCollection/sound_YYYYMMDD_HHMMSS.wav` ＋ `.json`（グローバル名前空間 `SoundEffectSettings`）。`MySoundCollectionStorage`/`SoundSavePaths`/`WavUtility` に依存。U3 で新形式に統一済み（旧保存経路は使用しない）。→ 旧データ取り込みの要否は Q1。
- **メタモデル差**: U1 `SoundClipMeta` は `id / displayName / createdAtIso / wavFileName` のみ。FR-10 の拡張（タイトル/写真/メモ/ニックネーム）分が不足 → Q2 で後方互換拡張方針を確定。
- **保存音の再生**: U3 は録音バッファを非破壊保存（wav=生／設定は別）。コレクションでの視聴時に「保存エフェクトを再適用して聴く」か「生を聴く」かは Q7。

---

## D. 設計に関する質問（Q1〜Q7）

## Question 1（コレクションのデータソース／対象形式）
一覧・視聴・管理の対象データは？（新形式 `sounds/{id}` と 旧 `MySoundCollection` の扱い）

A) (推奨) **新形式（`sounds/{id}.wav` ＋ `{id}.meta.json` = `SavedSound`）のみを対象**。旧 `MySoundCollection` 形式データの移行/取り込みは**対象外**（U3 で保存経路は新形式に統一済み・実データはまだ本番運用前）。旧 `MySoundCollectionStorage`/`SoundSavePaths` は新方式へ集約（物理削除はシーン再配線と同時＝MCP フォローアップ）。

B) 新形式を主としつつ、**起動時に旧 `MySoundCollection` を新形式へ一度だけ移行**（マイグレーション）して一覧に含める。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 2（メタデータ拡張モデル：後方互換）
FR-10 のメタ拡張（日付/タイトル/写真/メモ/ニックネーム）をどう表現する？（U1 `SoundClipMeta`＝id/displayName/createdAtIso/wavFileName）

A) (推奨) **`SoundClipMeta` を後方互換で拡張**：`title`（既定＝作成日付。未設定時は日付を表示）／`photoFileName`（任意・`sounds/{id}.photo.*` を参照）／`memo`（任意）／`nickname`（登録プロフィールのニックネームを保存時に写す）を**追記**。既存フィールド（id/createdAtIso/wavFileName/displayName）は不変で、旧 JSON も `JsonUtility` で欠損=既定値として読める。`displayName` は内部識別、`title` が表示名。

B) `SoundClipMeta` は変えず、拡張分を**別ファイル**（`{id}.extra.json`）に分離して保存する。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 3（永続化の堅牢化方式：原子性・破損・空）
NFR-07 / US-TECH-06 のデータ堅牢性はどう実装する？（技術非依存のふるまいとして）

A) (推奨) **原子的書込**：新規/更新は「一時ファイルへ書込→完了後に本ファイルへ原子的置換」で行い、途中失敗でも既存データを壊さない（対象＝profile / meta / wav / 写真）。**破損/欠損**は読込時に安全にスキップし他項目は正常表示（U1 の ListSounds スキップを全経路へ徹底）。**空/初期**はフォールバック（空状態）。すべて失敗は `Result`（理由コード）で通知しクラッシュしない。重要度＝Rec/Collection=Critical（RESILIENCY-01）。

B) 原子性は meta/profile のみ（テキスト小容量）に適用し、wav/写真は単純書込のまま（コスト優先）。破損スキップ・空フォールバックは A と同じ。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 4（写真の扱い：取得と保存・プライバシー）
US-COL-02 の「写真（任意）」はどう扱う？（NFR-04：端末外へ送信しない）

A) (推奨) **ローカルファイル参照**：写真は `sounds/{id}.photo.jpg`（または png）として端末ローカルに保存し、`meta.photoFileName` で参照。**取得手段（カメラ/ギャラリー）は抽象インターフェース（例: `IPhotoPicker`）として定義**し、U4 では**枠組み＋スタブ**（表示・削除・差し替えのフローは実装、実機のネイティブピッカー結線は MCP/プラグイン フォローアップ）。写真は**端末外へ送信しない**・ログに出さない。写真なしでも成立。

B) U4 では写真は**メタ項目として定義＋表示のみ**（撮影/選択の取得フローは U4 スコープ外・将来）。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 5（絞り込み・検索モデル）
US-COL-03 の月別絞込・キーワード検索の規則は？

A) (推奨) **月別絞込**＝`createdAtIso` から年月（YYYY-MM）を導出し一致で絞る。**キーワード検索**＝`title`／`memo`／`nickname` を対象に**部分一致・大文字小文字/全半角を実用的に無視**（trim・前方後方一致でなく含む）。絞込と検索は**AND 合成**。判定は**純粋関数**（`SavedSound` リスト→フィルタ済みリスト）としてテスト容易化（PBT/単体候補）。一致なしは**空状態**を分かりやすく表示。

B) 月別絞込のみ（検索は将来）。または検索は `title` のみ対象。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 6（削除のふるまい）
US-COL-01 の削除はどう扱う？

A) (推奨) **確認ダイアログ（`ConfirmDialog`・既定＝いいえ）→ 承諾で該当の wav＋meta＋写真を一括削除**。欠損ファイルは無視（ベストエフォート）。削除は原子性に準じ、途中失敗は `Result` で通知し一覧は安全に再読込。削除後は一覧・絞込結果を即時更新。

B) 確認なしで即削除（誤操作リスクあり）＋アンドゥ（一定時間の取り消し）を用意。

C) Other（[Answer]: の後に記述）

[Answer]:

## Question 7（Collection 画面構成＋再生忠実度）
コレクション画面の構成と「視聴」の音は？

A) (推奨) **1画面**に「一覧（スクロール・レスポンシブ／固定px依存を排除）＋絞込/検索バー＋詳細・編集パネル＋空状態」を配置。項目タップで**視聴**、詳細でタイトル/写真/メモ編集・削除。**視聴は保存エフェクト（`SoundEffectSettingsData`）を非破壊で再適用して再生**（録音時と同じ聴こえ）。※再生の技術配置（共有再生サービス／`IAudioService` 拡張／U3 EffectChain の共有化）は **NFR Design** で確定。見た目（配色/アイコン/レイアウト）は S さんハンドオフ（US-TECH-07）。

B) 一覧画面と詳細画面を分割（一覧→タップ→詳細/編集）。視聴は**生 WAV をそのまま再生**（エフェクト非適用・簡素）。

C) Other（[Answer]: の後に記述）

[Answer]:

---

## E. 完了条件
- Q1〜Q7 に回答 → 矛盾チェック（曖昧回答は追質問）→ domain-entities / business-logic-model / business-rules / frontend-components を生成 → 承認ゲート。
- 生成物は技術非依存（原子的置換 API・スクロール仮想化・写真ピッカー実装・シーン配線は NFR Design / Code Generation で扱う）。
- 既存実装との差分（新形式一本化・メタ後方互換拡張・原子性/破損フォールバックの本実装・旧経路集約）が設計に反映されている。
