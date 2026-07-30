# User Stories（ユーザーストーリー）

**プロジェクト**: 藝大 音響教育アプリ（「音」から始まる、耳のためのアプリケーション）
**作成**: 2026-07-15 / AI-DLC User Stories（Part 2）
**更新**: 2026-07-30 / フェーズC（音図鑑・アンロック・音づくり）差分
**方針（story-generation-plan.md 承認済み）**: Q1=ペルソナ（役割名） / Q2=ハイブリッド（エピック=モジュール＋配下ジャーニー順）/ Q3=Given/When/Then / Q4=中粒度 / Q5=将来はスタブ / Q6=技術イネーブラー化 / Q7=日本語 As a-I want-so that / Q8=フェーズタグ付与
**入力**: `../requirements/requirements.md`、`../reverse-engineering/plan-vs-implementation-gap.md`、`personas.md`

## 凡例
- **ペルソナ**: P1=こども/学習者「みみ」, P2=企画・デザイン, P3=基盤・統合, P4=音響実装, P5=ゲーム実装
- **フェーズ**: `[A]`=基盤 / `[B]`=最初のゲーム / `[C]`=次期MVP（音図鑑・音づくり） / `[将来]`=スコープ外（スタブ）
- **INVEST** 準拠。各ストーリーは受入基準（Given/When/Then）を持つ。
- **トレース**: 末尾に対応する FR/NFR を記載。
- **プライバシー**: 本ドキュメントは役割名で記述し、個人名・連絡先・個人予定を記載しない。

---

# EPIC-NAV: アプリ基盤・ナビゲーション `[A]`

## US-NAV-01 モジュールへの画面遷移
**P1 / P2**
こども/学習者として、メイン画面からホーム画面、そして各モジュール（Rec / コレクション / ゲーム選択 / weekly theme）へ移動したい。なぜなら、やりたいことにすぐたどり着きたいから。

**受入基準**
- Given メイン画面を開いている, When 「はじめる」等の起点を操作する, Then ホーム画面へ遷移する。
- Given ホーム画面, When 各モジュールのボタンをタップする, Then 対応するモジュール画面へ遷移する。
- Given いずれかのモジュール画面, When 「もどる/ホーム」を操作する, Then ホーム画面へ戻れる。
- Given 縦・横いずれの向き, When 画面遷移する, Then レイアウトが破綻せずボタンが操作可能である。

_トレース: FR-01 / NFR-11_
_実装状況（U2 Code Generation 2026-07-15）: コード実装済み（BootScreenController/StartupRouter/HomeScreenController/NavigationService＋BackToHomeButton）。実シーンの GameObject 配線は MCP フォローアップ（code-summary §5）。_

## US-NAV-02 迷わないホーム導線
**P1**
こども/学習者として、今どこにいて次に何ができるかが分かるホームがほしい。なぜなら、文字が読みにくくても迷わず操作したいから。

**受入基準**
- Given ホーム画面, When 表示する, Then 利用可能なモジュールがアイコン/モチーフ（カエル・おたまじゃくし・蓮）で識別できる。
- Given MVPでスコープ外のモジュール（共有/Place 等）, When ホームを表示する, Then それらは非表示または導線から除外されている。

_トレース: FR-01, FR-02 / NFR-05_
_実装状況（U2 Code Generation 2026-07-15）: コード実装済み（HomeScreenController＋HomeMenuConfig データ駆動／Place・テスト除外、モチーフはアイコンキーで識別）。見た目・シーン配線は Sさん/MCP フォローアップ。_

---

# EPIC-REG: 初回ユーザー登録 `[A]`

## US-REG-01 初回の簡易登録
**P1**
こども/学習者として、初回起動時に「生まれた年」と「ニックネーム」だけの簡単な登録をしたい。なぜなら、自分の音として記録を残したいから。

**受入基準**
- Given 初回起動, When アプリを開く, Then 生年・ニックネームの入力画面が表示される。
- Given 有効な入力, When 登録する, Then 端末ローカルに保存され、次回以降は登録画面をスキップする。
- Given 登録情報, When 保存する, Then その情報は端末外へ送信されない。

_トレース: FR-03 / NFR-02, NFR-04_
_実装状況（U2 Code Generation 2026-07-15）: コード実装済み（UserRegistrationScreenController[New]＋U1 ValidationUtil/StorageService、初回判定は StartupRouter、端末外送信なし）。実シーン配線は MCP フォローアップ。_

## US-REG-02 登録情報の編集と入力検証
**P1**
こども/学習者として、後からニックネームや年を直せて、入力ミスも防いでほしい。なぜなら、間違えても安心して使いたいから。

**受入基準**
- Given 登録済み, When 設定から編集する, Then 生年・ニックネームを更新できる。
- Given 不正な入力（年が範囲外、ニックネームが長すぎる/空）, When 保存しようとする, Then エラーを表示し保存を拒否する。
- Given 入力値, When 検証する, Then 妥当性（年の範囲・ニックネーム長）を満たす場合のみ確定する。

_トレース: FR-04 / SECURITY-05（入力検証）_
_実装状況（U2 Code Generation 2026-07-15）: コード実装済み（UserRegistrationScreenController[Edit]＝既存値ロード→検証→上書き保存、キャンセルで破棄）。実シーン配線は MCP フォローアップ。_

---

# EPIC-REC: Rec（録音・加工・保存） `[A]`

## US-REC-01 3秒録音
**P1**
こども/学習者として、マイクで自分の声や音を録りたい。なぜなら、その音で遊び・聴き分けをしたいから。

**受入基準**
- Given Rec画面, When 録音を開始する, Then **3秒間**録音し、自動的に停止する。
- Given 録音済み, When 再生する, Then 録音した音が再生される。
- Given マイク権限が未許可/拒否, When 録音しようとする, Then 権限を要求し、拒否時は安全に案内して録音を行わない（クラッシュしない）。

_トレース: FR-05 / NFR-03, NFR-06, SECURITY-15（フェイルセーフ）_

_実装状況（U3, 2026-07-15）: コード実装済み。`RecordingController`＋`RecAudioService`（Unity `Microphone`）で 3秒録音、`RecordingClock` で自動停止、`MicPermissionGate` で権限フェイルセーフ。実シーン配線は MCP フォローアップ（code-summary §6）。_

## US-REC-02 録音音の加工
**P1**
こども/学習者として、録った音にリバーブ・ノイズリダクション・ピッチ・音色の変化を付けたい。なぜなら、音が変わる面白さを楽しみたいから。

**受入基準**
- Given 録音済み, When 各加工（リバーブ / ノイズリダクション[0/弱/中/強] / ピッチ / 音色[なし/ロボット/コーラス系]）を調整する, Then 再生時に加工が反映される。
- Given 加工パラメータ, When 変更する, Then リアルタイムに近い体感で反映される（体感遅延が少ない）。
- Given 各加工, When バイパス（on/off）を切り替える, Then その加工の有無を比較できる（操作性が悪い場合は省略可）。

_トレース: FR-06 / NFR-06_

_実装状況（U3, 2026-07-15）: コード実装済み。`EffectPanelController`＋`EffectChain`（Unity 標準 AudioFilter）で非破壊プレビュー、`SoundEffectMapper`（純粋・PBT）で数値換算。UI 見た目/ラベルは Sさん調整（US-TECH-07）。実シーン配線は MCP フォローアップ。_

## US-REC-03 加工音の保存
**P1 / P3**
こども/学習者として、加工した音を保存して後から使いたい。なぜなら、自分の音コレクションを増やしたいから。

**受入基準**
- Given 加工済みの音, When 保存する, Then **WAVE（16bit PCM）**として端末ローカルに保存される。
- Given 保存, When 実行する, Then 加工設定（SoundEffectSettings）が音声と対で保存される。
- Given 保存処理中の失敗（I/Oエラー等）, When 発生する, Then データを破損させず安全に失敗を通知する。

_トレース: FR-08 / NFR-03, NFR-07, SECURITY-15_

_実装状況（U3, 2026-07-15）: コード実装済み。`SavePromptController`＋`IStorageService.SaveSound`／`StorageService`（wav→meta 対保存・失敗時 wav 削除）。WAV は既存 `WavCodec`（16bit PCM）。原子的置換・破損復旧の本実装は U4。EditMode `SaveSoundTests` で担保。_

---

# EPIC-COL: MySoundCollection（コレクション） `[A]`

## US-COL-01 保存音の一覧・視聴・削除
**P1**
こども/学習者として、保存した音を一覧で見て、聴いたり消したりしたい。なぜなら、集めた音を管理したいから。

**受入基準**
- Given 保存音がある, When コレクションを開く, Then 保存音が一覧表示される。
- Given 一覧の項目, When タップする, Then その音を再生できる。
- Given 一覧の項目, When 削除する, Then 確認のうえ該当の音声と設定・メタデータが削除される。

_実装状況（U4, 2026-07-15）: コード実装済み。新 `Geidai.Collection`（`CollectionScreenController`＋`SoundListView`/`SoundListItemView`）で一覧、`SoundDetailController` で視聴（共有 `IAudioService.Play(buffer, settings)` により保存エフェクトを非破壊再適用）と削除（`ConfirmDialog`→`IStorageService.DeleteSound`＝wav+meta+photo 一括・原子的）。実シーン配線と旧 `GoToSoundCollection`/`MySoundCollectionStorage` 差し替えは MCP フォローアップ（code-summary §6）。_

_トレース: FR-09 / NFR-05_

## US-COL-02 メタデータの拡張
**P1**
こども/学習者として、音に日付・タイトル・写真・メモ・ニックネームを付けたい。なぜなら、どんな音だったか思い出したいから。

**受入基準**
- Given 保存音, When メタデータを表示する, Then 日付・タイトル（デフォルトは日付）・写真（任意）・メモ（任意）・ニックネームが確認できる。
- Given メタデータ, When 編集する, Then タイトル/写真/メモを更新できる。
- Given 写真・メモ等の個人情報, When 保存する, Then 端末外へ送信されない。

_実装状況（U4, 2026-07-15）: コード実装済み。`SoundClipMeta` を後方互換で拡張（`title`/`photoFileName`/`memo`/`nickname`・旧 JSON も既定値で読める＝`SavedSoundJsonTests`）。`SoundDetailController` でタイトル/メモ編集（`IStorageService.SaveMeta`＝settings 保持・原子的置換）、写真は `IPhotoPicker`（U4 は `StubPhotoPicker`）→`SavePhoto`（拡張子検証・原子的コピー）→`SaveMeta`。写真/メモ/ニックネームは `persistentDataPath` 内のみ・ログ非出力（PII）。実機写真ピッカー本結線は MCP フォローアップ。_

_トレース: FR-10 / NFR-04_

## US-COL-03 絞り込みと検索
**P1**
こども/学習者として、月ごとに絞ったりキーワードで探したい。なぜなら、たくさん貯まっても目的の音を見つけたいから。

**受入基準**
- Given 複数月にまたがる保存音, When 月別で絞り込む, Then 対象月の音のみ表示される。
- Given キーワード, When 検索する, Then タイトル/メモ等に一致する音のみ表示される。
- Given 一致なし, When 検索する, Then 空状態が分かりやすく表示される。

_実装状況（U4, 2026-07-15）: コード実装済み。純粋関数 `Geidai.Common.Collection.CollectionFilter.Filter(items, query)`（月別＋キーワード[title/memo/nickname 部分一致・大小無視]・AND 合成・順序保持／PBT=`CollectionFilterTests`）。UI は `FilterSearchController`（月ドロップダウン＋検索入力→`CollectionQuery`）。空一致は `SoundListView` の空状態表示。MCP スモークで all=3/feb=2/neko=2/febTaro=1 を確認。_

_トレース: FR-11 / NFR-05_

## US-COL-04 端末での永続化と堅牢性
**P1 / P3**
こども/学習者として、アプリを閉じても集めた音が消えず、壊れないでほしい。なぜなら、大事なコレクションを失いたくないから。

**受入基準**
- Given 保存済みデータ, When アプリを再起動する, Then 音声・設定・メタデータが保持されている（Application.persistentDataPath 配下）。
- Given 一部ファイルが破損/欠損, When コレクションを読み込む, Then 破損項目を安全に読み飛ばし、他の項目は正常表示する（クラッシュしない）。
- Given 空/初期状態, When コレクションを開く, Then フォールバック表示（空状態）となる。

_実装状況（U4, 2026-07-15）: コード実装済み。`StorageService` の全書込（profile/meta/wav/写真）を `AtomicFile`（temp→原子的置換）へ統一し、`ListSoundsDetailed()` が破損 meta・対 wav 欠損を安全にスキップし空リストへフォールバック（`StorageCollectionTests`/`AtomicFileTests` で担保）。永続化は `Application.persistentDataPath` 配下。_

_トレース: FR-12 / NFR-07（データ堅牢性）_

---

# EPIC-THEME: weekly theme（お題） `[B]`

## US-THEME-01 週替わりお題の表示
**P1**
こども/学習者として、今週の「音のお題」を見たい。なぜなら、何を録ればいいかのきっかけがほしいから。

**受入基準**
- Given ホーム/お題画面, When 表示する, Then その週のお題（オノマトペ等）が表示される。
- Given 週が替わる, When 表示する, Then 対応するお題に切り替わる。

_トレース: FR-13 / NFR-05_

_実装状況（U5, 2026-07-16）: コード実装済み。`WeeklyThemeController`（再利用部品）＋`WeeklyThemeScreenController`（専用画面）が `IContentService.GetCurrentTheme()` で今週のお題を取得・表示。選択は純粋関数 `ThemeSelector.SelectIndex(date,count)`（`Geidai.Common.Content`・PBT）で決定的。空/無効カタログは `emptyState` フォールバック。既定 `ThemeCatalog`（`Assets/Settings/ThemeCatalog.asset`・13 オノマトペ移行済）を MCP 生成。実シーン配線・意匠は Sさん（US-TECH-07）＝MCP フォローアップ。_

## US-THEME-02 お題からRecへ
**P1**
こども/学習者として、お題をタップしたらそのまま録音に進みたい。なぜなら、思い立った流れで録りたいから。

**受入基準**
- Given お題が表示されている, When お題をタップする, Then Rec画面へ遷移する。
- Given お題から遷移したRec, When 表示する, Then どのお題に対する録音かが分かる（任意でお題を表示）。

_トレース: FR-13_

_実装状況（U5, 2026-07-16）: コード実装済み。お題タップ→`ThemeContext.Set(item)`（`Geidai.Services.Content`・実行時セッション・非永続）→`INavigationService.GoTo(Rec)`（失敗は `ErrorPresenter`）。Rec 側のお題ラベル表示は任意（`ThemeContext.Current` 参照・未設定でも通常録音）。Rec お題ラベルの実配置は MCP フォローアップ。_

## US-THEME-03 お題の差し替え可能な構成
**P2**
企画・運用者として、お題テキストを自分で用意した内容に差し替えたい。なぜなら、研究や季節に合わせて更新したいから。

**受入基準**
- Given お題データ, When 運用者が内容を差し替える, Then アプリの大きな作り直しなく反映できる構成である（データ/設定として分離）。
- Given 差し替え後, When 利用者が表示する, Then 更新後のお題が表示される。

_トレース: FR-14 / 前提（暫定・更新前提）_

_実装状況（U5, 2026-07-16）: コード実装済み。`ThemeCatalog`（ScriptableObject・`[CreateAssetMenu] Geidai/Theme Catalog`）の `items`（`ThemeItem`: text/reading/hint）を Sさん がインスペクタで追加/編集/並べ替え可能＝再ビルド不要。`ThemeItem.IsValid`（text 非空）で無効項目は選択対象外。旧 `WeeklyTextController`（Assembly-CSharp・固定配列）は当面残置し、シーン差し替え後に削除（BR-THEME-52・MCP フォローアップ）。_

---

# EPIC-GAME1: ミニゲーム ①音合わせ `[B]`

## US-GAME1-01 出題と解答（タップ確認・ドラッグ解答）
**P1**
こども/学習者として、お手本の音（カエル）と選択肢の音（おたまじゃくし）を聴き比べて、ドラッグで答えたい。なぜなら、聴き分けを遊びながら練習したいから。

**受入基準**
- Given ゲーム開始, When 出題される, Then お手本音（カエル）と複数の選択肢音（おたまじゃくし）が提示される。
- Given 選択肢, When タップする, Then その音を確認再生できる。
- Given 解答, When 対象へドラッグする, Then 解答として確定し、正誤が判定される。

_トレース: FR-15 / NFR-05_

_実装状況（U6, 2026-07-16）: コード実装済み。`SoundMatchGameController`（`Geidai.Game1`）が出題を提示し、`FrogTargetView`（お手本＝カエル）と `ChoiceItemView`（選択肢＝おたまじゃくし）を配線。タップで `IPitchVariationService.Play(cents)` 確認再生、uGUI ドラッグ＆ドロップでカエル領域に落とすと `SubmitAnswer(index)` により純粋判定（`Question.correctIndex` 比較）。領域外ドロップは元位置へ復帰。実シーン配線・意匠は Sさん（US-TECH-07）＝MCP フォローアップ。_

## US-GAME1-02 聞き分け対象の要素
**P1**
こども/学習者として、音色・音の高さ・強弱の違いで聴き分けたい。なぜなら、いろいろな「きく」観点を育てたいから。

**受入基準**
- Given 出題, When 生成する, Then 音色・音高・強弱のいずれか/組合せで差がある選択肢が提示される。
- Given 音の長さの扱い, When 設計する, Then 研究会での確定事項として調整可能な構成にする（暫定）。

_トレース: FR-16 / 前提_

_実装状況（U6, 2026-07-16）: 部分実装。今回は「音高（ピッチ）」の聴き分けを実装（`QuestionBuilder` が基準音からのセント差で選択肢を生成、`PitchVariationService` が再生時ピッチで発音）。音色・強弱・長さの本格 DSP は研究会後に拡張予定（スコープ外）。難易度はセント間隔（`DifficultyLevel.centsStep`）で段階化し `SoundMatchConfig` で調整可能な構成。_

## US-GAME1-03 正解演出
**P1**
こども/学習者として、正解したら嬉しい演出を見たい。なぜなら、続けたくなるから。

**受入基準**
- Given 正解, When 判定される, Then おたまじゃくし→カエルへ進化する演出が再生される。
- Given 不正解, When 判定される, Then 再挑戦できる（過度なペナルティなし）。

_トレース: FR-17 / NFR-05_

_実装状況（U6, 2026-07-16）: コード実装済み。`ResultEffectController` が正解時に `PlayCorrect()`（カエル成長スプライトを1段階進化・`growthStages`）、不正解時に `PlayRetry()`（無ペナルティで選択肢を元位置復帰・再挑戦）、終了時に `ShowResult(correct,total)` で結果サマリを提示。進行判定は Controller、演出は本コンポーネントに分離。アニメ/イラストは Sさん＝MCP フォローアップ。_

## US-GAME1-04 出題パラメータ設定
**P2 / P1**
企画・運用者として、出題数・選択肢数・難易度を設定したい。なぜなら、対象や狙いに合わせて調整したいから。

**受入基準**
- Given 設定, When 変更する, Then 出題数・選択肢数・難易度を指定できる。
- Given 難易度, When 表現する, Then ピッチ間隔（セント）で段階（かんたん/ふつう/むずかしい/とても難しい）を定義する。
- Given パラメータ, When 変更する, Then 出題に反映される。

_トレース: FR-18 / 前提（研究会後に細部確定）_

_実装状況（U6, 2026-07-16）: コード実装済み。`SoundMatchConfig`（ScriptableObject・`[CreateAssetMenu] Geidai/Sound Match Config`）で出題数（`questionCount`）・選択肢数（`choiceCount`）・難易度段階（`difficulties`＝label＋`centsStep`）・フォールバック素材（`fallbackClip`）を Sさん がインスペクタで調整可能＝再ビルド不要。異常値はアクセサでクランプ（questionCount≥1・choiceCount≥2・centsStep≥1）。既定アセット `Assets/Settings/SoundMatchConfig.asset`（かんたん200/ふつう100/むずかしい50/とても難しい20）を MCP 生成。_

## US-GAME1-05 ユーザーの保存音を使った出題
**P1 / P3**
こども/学習者として、自分の保存音がゲームのお題として出てきてほしい。なぜなら、自分の音で聴き分けるのが楽しいから。

**受入基準**
- Given ユーザーの保存音, When 出題を生成する, Then 保存音を素材にリアルタイムでピッチ加工したバリエーション（例: オリジナル/±10/±20セント）を作る。
- Given 生成した加工音, When 出題に使う, Then それらは保存されない（一時的）。
- Given 加工処理, When 実行する, Then モバイル端末で体感遅延の少ない実用的な処理時間に収まる。

_トレース: FR-19 / NFR-03, NFR-06_

_実装状況（U6, 2026-07-16）: コード実装済み。`SoundMatchGameController` が `IStorageService.ListSounds()`／`LoadSoundBuffer(id)` で保存音を素材に選択（seed で決定的・読込失敗は次候補・0件/全滅は `fallbackClip`→`Empty` フォールバック）。ピッチ加工は `PitchVariationService` が再生時 `AudioSource.pitch = PitchMath.CentsToRatio(cents)` を適用する軽量方式で、加工済み音声は生成・保存しない（非永続・低GC＝NFR-06）。基準 `AudioClip` はゲーム開始時に一度だけキャッシュし体感遅延を抑制。_

---

# EPIC-TECH: 技術イネーブラー（非機能・品質・保守） `[A]`

> **開発フロー・役割分担（UI）**: 実装フェーズの UI は、**基盤・統合担当が基本的な枠組み**（レイアウト骨格・機能構造・レスポンシブ/SafeArea・画面遷移）を作成し、**詳細な見た目調整は企画・デザイン担当**が行う。差し替え可能な素材/パラメータと柔軟なレイアウトで「調整余地」を残す。詳細は US-TECH-07 参照。

## US-TECH-01 端末横断のレスポンシブ表示
**P1 / P3**
利用者として、機種や画面サイズが違っても表示が崩れず使えてほしい。（実装者として、端末横断で破綻しないUIを担保したい。）なぜなら、多様なスマホ/タブレットで同じ体験を届けたいから。

**受入基準**
- Given 主要なアスペクト比（19.5:9〜4:3 等）・解像度, When 各画面を表示する, Then レイアウトが破綻せず操作要素が収まる。
- Given 全 Canvas, When 設定する, Then CanvasScaler = Scale With Screen Size を用い、縦・横 両対応の参照解像度・Match 方針が統一されている。
- Given 固定ピクセル依存（例: ScrollRectSnapLoop の itemWidth 850px）, When 見直す, Then 相対指定（Anchor/レイアウトグループ）へ置き換える。
- Given 向き変更（オートローテーション）, When 発生する, Then レイアウトが再構成され、操作要素が見切れない。

_トレース: NFR-11_

**U1 実装状況（基盤）**: ✅ 済 — `Geidai.Common.UI.ResponsiveCanvasConfigurator`（1080×1920/Match0.5）・`ScreenRootBase` 生成。実シーンへの付与は U2 以降（Unity MCP）。

## US-TECH-02 SafeArea への追従
**P1 / P3**
利用者として、ノッチや角丸でボタンが隠れないでほしい。（実装者として、SafeArea 追従を全画面で担保したい。）なぜなら、どの端末でも確実に操作できるようにしたいから。

**受入基準**
- Given ノッチ/パンチホール/ホームインジケータのある端末, When 各画面を表示する, Then 操作要素が Screen.safeArea 内に収まる。
- Given 各画面のルート, When 構成する, Then SafeArea 追従コンポーネントが適用されている（未実装のため新設）。
- Given 縦・横いずれの向き, When 切り替える, Then SafeArea が追従し操作要素がシステムUIに隠れない。

_トレース: NFR-12（ProjectSettings: androidRenderOutsideSafeArea=1 と整合）_

**U1 実装状況（基盤）**: ✅ 済 — `Geidai.Common.UI.SafeAreaFitter`（向き/解像度変更で再適用・差分間引き）生成。実シーンへの付与は U2 以降（Unity MCP）。

## US-TECH-03 録音実装の一本化
**P3**
実装者として、録音・加工の実装を VoiceRecordingSection に一本化したい。なぜなら、重複実装（RecorderWithEffects）を排し保守性を高めたいから。

**受入基準**
- Given 録音・加工機能, When 実装を統合する, Then VoiceRecordingSection（Unity標準AudioFilter）に一本化される。
- Given 重複/不要コード（RecorderWithEffects、Scean.cs 等）, When 整理する, Then 参照が除去され、ビルド・動作に影響がない。
- Given 統合後, When Rec の録音/加工/保存を実行する, Then 既存の受入基準（US-REC-01〜03）を満たす。

_トレース: FR-07, NFR-08_

_実装状況（U3, 2026-07-15）: コード実装済み（統合先は当初想定の `VoiceRecordingSection` ではなく、新 `Geidai.Rec` の `IAudioService` 実装＝`RecAudioService` に一本化。加工は Unity 標準 AudioFilter を `EffectChain` で適用）。重複 DSP の `RecorderWithEffects.cs` と空の `Scean.cs`（＋`.meta`）を削除（参照なしを確認済み・ビルド影響なし＝Error 0）。旧録音一式（`VoiceRecordingSection`/`WavUtility`/`MySoundCollectionStorage`/`SoundSavePaths`/`SoundEffectSettings`）は現行 Rec シーンが参照中のため、シーン再配線（MCP フォローアップ）と同時に物理削除予定。_

## US-TECH-04 Place導線の除外と遷移不具合の解消
**P1 / P3**
利用者として、使えない/未対応の画面に迷い込みたくない。（実装者として、画面遷移の不具合を解消したい。）なぜなら、MVPで安定した導線を提供したいから。

**受入基準**
- Given MVPスコープ, When ナビゲーションを構成する, Then 共有（Place）への導線が無効化/非表示になっている。
- Given 既存の遷移不具合（例: 遷移文字列 "place" と Place.unity の大文字小文字不一致）, When 修正する, Then 対象の遷移が正しく動作する、または導線から除外される。
- Given 各画面遷移, When 実行する, Then 存在しない/未対応シーンへの遷移でクラッシュしない。

_トレース: FR-02_

**U1 実装状況（基盤）**: ✅ 済 — `SceneId`（Place 除外）＋ `INavigationService`/`NavigationService`（未定義シーンは `NotFound` 返却でクラッシュ回避）生成。シーン登録の追加は U2/U5。

## US-TECH-05 Unity MCP 経由のシーン操作を規約化
**P3 / P2**
実装者として、シーン/GameObject/プレハブ操作を Unity 標準 MCP（unityMCP）経由で行いたい。なぜなら、再現性のある変更と軽量な変更管理を両立したいから。

**受入基準**
- Given シーン/GameObject/プレハブ/アセットの変更, When 実施する, Then unityMCP 経由の操作を基本とする（手作業編集と併用可）。
- Given 変更, When 行う, Then その内容を PR・変更メモに残す（Git ブランチ＋PR レビュー）。
- Given 変更後, When 検証する, Then コンソールのコンパイルエラーがないことを確認する。

_トレース: NFR-10（変更管理）/ technology-stack.md 開発規約_

**U1 実装状況（規約運用開始）**: ✅ 運用中 — Unity 公式 AI Assistant パッケージの Unity MCP Server（Cursor 上 `user-unity-mcp`）で U1 のコンパイル確認・アセット生成・スモーク検証を実施。実シーン操作は U2 以降で本格活用。

## US-TECH-06 ローカルデータの堅牢性
**P3**
実装者として、保存の原子性と破損時の安全処理を実装したい。なぜなら、利用者のコレクション（Critical データ）を失わせないため。

**受入基準**
- Given 保存処理, When 実行する, Then 原子的に書き込み（途中失敗で既存データを破損させない）。
- Given 破損/欠損ファイル, When 読み込む, Then 安全に読み飛ばし、フォールバックする（US-COL-04 と整合）。
- Given 重要度分類, When 定義する, Then Rec/Collection=Critical、ゲーム=High、weekly theme=Medium として扱う。

_実装状況（U4, 2026-07-15）: コード実装済み。`Geidai.Services.IO.AtomicFile`（一時ファイル→`File.Replace`/`Move` による原子的置換・例外時 tmp 破棄）を新設し、`StorageService` の profile/meta/wav/写真の全書込を集約。`SaveSound` は wav→meta 順で原子的＋対整合。読込は `ListSoundsDetailed()` が破損/欠損をスキップし空フォールバック。EditMode `AtomicFileTests`/`StorageCollectionTests`（Test Runner）で担保。_

_トレース: NFR-07（Resiliency R1）, RESILIENCY-01_

## US-TECH-07 UI 詳細調整のハンドオフ（基盤・統合→企画・デザイン）
**P3 / P2**
基盤・統合担当として、UI は基本的な枠組みまでを作り、詳細な見た目調整を企画・デザイン担当へ引き渡せるようにしたい。なぜなら、実装は技術に集中し、見た目の仕上げは企画/デザイン視点が担う分担にしたいから。

**受入基準**
- Given UI 実装, When 基盤・統合担当が枠組みを作る, Then レイアウト骨格・機能構造・レスポンシブ/SafeArea・画面遷移が動作する状態で提供される。
- Given 詳細な見た目調整（余白・配置・配色・アイコン/モチーフ配置・文言・素材差し替え）, When 企画・デザイン担当が調整する, Then コード改修を伴わず（または最小限で）調整できる調整余地（差し替え可能な素材/パラメータ・柔軟なレイアウト）が用意されている。
- Given お題テキスト・ゲームパラメータ等のコンテンツ, When 企画・デザイン担当が更新する, Then データ/設定として分離され差し替え可能である（US-THEME-03, US-GAME1-04 と整合）。
- Given 調整対象と手順, When ハンドオフする, Then 「基盤・統合＝枠組み / 企画・デザイン＝詳細調整」の分担と調整箇所が明確になっている。

_トレース: 要件 §7 UI開発フロー・役割分担 / NFR-05, NFR-11, NFR-12, NFR-15_

**U1 実装状況（基盤・調整余地）**: ✅ 済 — `UITheme`（ScriptableObject／既定アセット `Assets/Settings/UITheme_Default.asset`）＋ `ErrorPresenter` の差し替え可能フィールドを用意。コンテンツ差し替え（お題/パラメータ）は U5/U6。

## US-TECH-08 任意展示向け実機試用ビルド
**P3 / P2**
基盤・統合担当として、任意展示向けに音図鑑を含む実機インストール可能な試用ビルドを用意したい。なぜなら、ストア公開を待たずに体験を届けたいから。

**受入基準**
- Given 試用ビルド, When クリーンインストールする, Then 対象端末で起動し、完全オフラインで主要導線（録音・コレクション・ゲーム・音図鑑）を確認できる。
- Given ビルド成果物, When 受入確認する, Then ストア公開を必須条件に含めない。
- Given 確認記録, When 残す, Then 個人名・個人予定・端末識別子を含めず、端末クラスと合否のみを記録する（NFR-17）。

_トレース: NFR-16, NFR-17 / FR-20〜29（範囲はビルド時点の統合状況）_

## US-TECH-09 共同開発のモジュール境界
**P3 / P4 / P5 / P2**
チームとして、共通IFを先に合意し、役割別ワークストリームとゲーム縦割りで実装したい。なぜなら、衝突を減らし並行作業できるようにしたいから。

**受入基準**
- Given 次期機能, When 着手する, Then 基盤・統合／音響実装／企画・デザインの境界と、追加ゲーム1本単位の縦割りが文書化されている。
- Given 共通サービスの変更, When 提案する, Then feature ブランチ＋Pull Request でレビューし、破壊的変更は合意後に統合する。
- Given 計画・Issue・PR, When 記述する, Then 個人名・連絡先・個人予定を記載しない（NFR-17）。

_トレース: NFR-15, NFR-17 / 要件 §7_

> **テスト方針（NFR-09 / PBT）**: WAV エンコード/デコード、cents↔pitch 変換、設定JSONのシリアライズ、アンロック冪等、レシピ参照整合性等のプロパティベーステストは Construction フェーズで具体化する。ここではストーリー化しない。

---

# EPIC-LIBRARY: 音図鑑・アンロック `[C]`

## US-LIB-01 制作側音素材の閲覧・試聴
**P1 / P2**
こども/学習者として、制作側が用意した音素材を図鑑のように見て聴きたい。なぜなら、身の回りや珍しい音を発見したいから。

**受入基準**
- Given 音図鑑画面, When 開く, Then 安定した素材ID・表示名・分類を持つカタログが一覧できる（FR-20）。
- Given アンロック済み素材, When タップする, Then 試聴できる（FR-21）。
- Given 未解除素材, When 表示する, Then ロック状態が分かり、解除条件の概要が分かる（詳細文言は企画側で差し替え可能）。
- Given オフライン, When 操作する, Then サーバー通信なしで閲覧・試聴できる。

_トレース: FR-20, FR-21 / NFR-02, NFR-05, NFR-13_

## US-LIB-02 ゲームと録音課題によるアンロック
**P1**
こども/学習者として、ゲームをクリアしたり指定の音を録って保存したりすると、新しい音が使えるようになってほしい。なぜなら、進めるほど音の世界が広がるから。

**受入基準**
- Given ミニゲームのステージ／難易度クリア, When 達成する, Then 対応する素材が端末内でアンロックされる（FR-22）。
- Given 指定音の録音・保存課題, When 達成する, Then 対応する素材が端末内でアンロックされる（FR-22）。
- Given 同じ条件を再達成, When 発生する, Then 解除状態が重複・不整合なく維持される（FR-23・冪等）。
- Given アプリ再起動, When 音図鑑を開く, Then 解除済み素材が引き続き使える（FR-23）。
- Given アンロック, When 表現する, Then 経験値・カエルコイン・ライフ・課金を用いない（通貨ゲーミフィケーション除外）。

_トレース: FR-22, FR-23 / NFR-02, NFR-14_

## US-LIB-03 素材IDの共通参照
**P3 / P4 / P5**
実装担当として、音図鑑の素材を音づくりとミニゲームから同じ素材IDで参照したい。なぜなら、カタログと進行を一本化したいから。

**受入基準**
- Given カタログ上の素材ID, When 音づくりや対応ミニゲームが参照する, Then 同一IDで解決できる（FR-24）。
- Given 未知・欠損の素材ID, When 参照する, Then クラッシュせず安全に不足を示す（NFR-14）。

_トレース: FR-24 / NFR-14, NFR-15_

---

# EPIC-CREATE: 音を作る `[C]`

## US-CREATE-01 2音の組み合わせと試聴
**P1 / P4**
こども/学習者として、アンロックした音から2つを選び、重ねて聴きたい。なぜなら、組み合わせで新しい響きを作りたいから。

**受入基準**
- Given アンロック済み素材が2つ以上ある, When 2音を選ぶ, Then 重ねて試聴できる（FR-25）。
- Given 未解除素材のみ, When 音づくりを開く, Then 選択できない／解除を促す安全な状態になる。
- Given 2音再生, When 開始する, Then モバイルで体感遅延が少ない（NFR-13）。

_トレース: FR-25 / NFR-06, NFR-13_

## US-CREATE-02 加工パラメータの調整
**P1 / P4**
こども/学習者として、選んだ音に音量・ピッチ・リバーブ・音色を付けて変えたい。なぜなら、音が変わる面白さを楽しみたいから。

**受入基準**
- Given 2音を選択済み, When 音量・ピッチ・リバーブ・音色を調整する, Then 試聴に反映される（FR-26）。
- Given 加工範囲・適用単位, When 確定前, Then Functional Design で数値範囲を確定する（暫定・更新前提）。

_トレース: FR-26 / NFR-06_

## US-CREATE-03 レシピ保存と再編集
**P1 / P3**
こども/学習者として、作った組み合わせを後から開き直して直したい。なぜなら、気に入った音を育てたいから。

**受入基準**
- Given 調整済みの2音, When 保存する, Then 素材ID＋加工パラメータのレシピとして保存され、元の同梱音声は複製されない（FR-27）。
- Given 保存済みレシピ, When 開く, Then 再編集・再試聴できる（FR-27）。
- Given 参照素材が欠損・変更, When 開く, Then クラッシュせず不足を示し、再編集または削除を選べる（FR-29）。

_トレース: FR-27, FR-29 / NFR-07, NFR-14_

## US-CREATE-04 必要時のWAVE書き出し
**P1 / P4**
こども/学習者として、必要なときだけ完成音をファイルに書き出したい。なぜなら、普段は軽く保存し、必要なときだけ持ち出せるようにしたいから。

**受入基準**
- Given 保存済みレシピ, When 書き出しを選ぶ, Then WAVE（16bit PCM）として出力できる（FR-28）。
- Given 書き出し失敗, When 発生する, Then 不完全ファイルを残さず安全に通知する（FR-28）。

_トレース: FR-28 / NFR-03, NFR-07_

---

# EPIC-FUTURE: 将来エピック（今回スコープ外・スタブ） `[将来]`

> 見出し＋概要のみ。詳細受入基準は今回作成しない。優先度確定後に別途ストーリー化する。

- **FUT-01 ②音並べ〜⑧ゲーム**: ①音合わせ以外のミニゲーム群。仕様確定後に縦割りユニットとしてストーリー化。
- **FUT-02 ユーザー間共有（旧 Place）**: 第一弾では見送り。代替としてフェーズCの音図鑑／音づくりを採用。通信を伴う共有はオフライン方針との両立を確認したうえで将来再検討。
- **FUT-03 ゲーミフィケーション**: 経験値・カエルコイン・ライフ・課金。今回全除外。フェーズCのアンロックは通貨を持たないローカル達成として扱う。
- **FUT-04 ⑤テスト（能力変化計測）画面**: 「きく力」の変化を計測する画面。要否・指標は別途検討。
- **FUT-05 本格的な多レイヤー音づくり**: 3音以上の同時レイヤーや高度な組み合わせ。フェーズCの2音最小実装の後続。

_トレース: 要件 §4 スコープ外／後続_

---

# トレーサビリティ（Story → 要件）

| エピック | ストーリー | フェーズ | 対応 FR/NFR |
|---|---|:---:|---|
| NAV | US-NAV-01, 02 | A | FR-01, FR-02, NFR-05, NFR-11 |
| REG | US-REG-01, 02 | A | FR-03, FR-04, NFR-02, NFR-04, SEC-05 |
| REC | US-REC-01, 02, 03 | A | FR-05, FR-06, FR-08, NFR-03, NFR-06, NFR-07 |
| COL | US-COL-01〜04 | A | FR-09〜12, NFR-04, NFR-05, NFR-07 |
| THEME | US-THEME-01〜03 | B | FR-13, FR-14 |
| GAME1 | US-GAME1-01〜05 | B | FR-15〜19, NFR-03, NFR-06 |
| LIBRARY | US-LIB-01〜03 | C | FR-20〜24, NFR-02, NFR-13, NFR-14 |
| CREATE | US-CREATE-01〜04 | C | FR-25〜29, NFR-03, NFR-06, NFR-07, NFR-14 |
| TECH | US-TECH-01〜09 | A/C | FR-02, FR-07, NFR-05, NFR-07〜17, RESILIENCY-01, §7 |
| FUTURE | FUT-01〜05 | 将来 | §4 スコープ外 |

**INVEST 自己チェック**: 各ストーリーは独立して価値を持ち（Independent/Valuable）、受入基準で検証可能（Testable）、中粒度で見積り可能（Estimable/Small）、詳細は設計で調整可能（Negotiable）。音づくりの加工数値範囲・解除条件の具体表は Functional Design / コンテンツデータで確定（暫定・更新前提）。

<!-- U7U8_IMPL_STATUS -->
## 実装状況メモ（U7/U8 Code Generation 2026-07-30）
- US-LIB-01〜03 / US-CREATE-01〜04 / US-TECH-08: コード生成済み（シーン配線は MCP フォローアップ）
- モジュール: `Geidai.Library` / `Geidai.Create` + Progression/Unlock/Recipe 共通IF

