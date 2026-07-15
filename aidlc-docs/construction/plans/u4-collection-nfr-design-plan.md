# U4 Persistence/Collection — NFR Design Plan（計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U4 Persistence/Collection（永続化・コレクション）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Design（Part 1: 計画）
**入力**: `../u4-collection/nfr-requirements/nfr-requirements.md`, `tech-stack-decisions.md`, `../u4-collection/functional-design/*`, U1〜U3 NFR Design 成果物（`../u1-foundation/nfr-design/*`・`../u2-foundation/nfr-design/*`・`../u3-rec/nfr-design/*`）

> 目的: U4 の NFR（性能=一覧/スクロール/視聴・堅牢性=原子的置換/破損スキップ/空フォールバック・プライバシー・テスト容易性・保守性/共有再生）を**設計パターン**と**論理コンポーネント**へ落とし込む。数値は NFR Requirements で確定済み。ここでは「どう実現するか（パターン）」を決める。
> **回答方法**: 各質問の `[Answer]:` に記号（例: `A`）。合う選択肢が無ければ「Other」。各質問に「(推奨)」あり。完了で「done」（または「全部推奨で」）。

---

## A. 実行チェックリスト（Part 2 で実行）
- [ ] `../u4-collection/nfr-design/nfr-design-patterns.md` を生成（各 NFR の実現パターン）
- [ ] `../u4-collection/nfr-design/logical-components.md` を生成（論理コンポーネント・責務・連携）
- [ ] NFR Requirements / Functional Design とのトレース整合を確認

## B. カテゴリ適用性（このユニットでの判断）
- **Resilience（耐障害）**: 適用（**U4 の主眼**＝原子的置換・破損スキップ・空フォールバック・対ファイル整合）。ネットワーク再試行系は N/A。
- **Performance（性能）**: 適用（一覧読込・スクロール fps・サムネ遅延読み・視聴再生開始・書込レイテンシ・GC）。
- **Scalability（スケーラビリティ）**: 限定適用（単一端末・オフラインだが件数増に対する一覧の仮想化/遅延読み）。サーバスケールは N/A。
- **Security（セキュリティ）**: 適用（写真/メモ/ニックネーム PII の端末内限定・非ログ）。U1 パターン踏襲。
- **Logical Components（論理部品）**: 適用（AtomicFile ヘルパー・`IStorageService` 拡張・`CollectionFilter`・共有 Audio 再生・`IPhotoPicker`・コレクション画面コントローラ群）。

## B-2. U1〜U3 から継承する設計パターン（再質問しない・前提）
- **エラー伝搬**: `Result<T>`（成功/失敗＋理由コード）。致命的でない失敗はクラッシュさせない。
- **UI 基盤**: `ScreenRootBase` ＋ `ResponsiveCanvasConfigurator` ＋ `SafeAreaFitter`（表示時/向き変更で再適用）。固定px依存排除。
- **通知**: `ErrorPresenter`（子ども向けバナー）。**確認ダイアログ**: `ConfirmDialog`（再利用・既定=いいえ）。
- **DI**: 軽量サービスロケータ（`ServiceRegistry`）＋インターフェース（`IStorageService`/`IAudioService`/`INavigationService`）。
- **性能/GC**: 同期API基本・重い処理のみ非同期/コルーチン、バッファ再利用、参照キャッシュ。
- **セキュリティ**: PII/音声/写真は端末外送信なし、`SafeLogger` で非ログ、本番で詳細エラー非表示。
- **テスト**: 純粋関数化＋I/O 抽象化（インターフェース）で PBT/モック可能に。
- **保存/WAV**: `Application.persistentDataPath` ＋ `JsonUtility` ＋ `WavCodec`（16bit PCM）。

> ※ 上記の変更が必要な場合のみ、該当質問で Other 指定してください。

---

## C. 明確化のための質問（Q1〜Q6）

### Question 1（Resilience — 原子的置換の実装パターン）※U4 の主眼
profile / meta / wav / 写真の「壊さない書込」の共通パターンは？

A) (推奨) **`AtomicFile` ヘルパー（静的・`Geidai.Services` 内）に集約**：`WriteAllBytesAtomic(path, bytes)` / `WriteAllTextAtomic(path, text)` を提供し、内部は「`{path}.tmp` へ全内容書込→flush/close→本ファイルへ**原子的置換**（既存あり=`File.Replace`／新規=`File.Move(overwrite)`・プラットフォーム差を吸収）→例外時は `.tmp` を破棄」。`StorageService` の profile/meta/wav/写真の全書込を本ヘルパー経由に統一。**受入＝書込を中断注入しても本ファイルは旧内容のまま無傷**（NFR-COL-R1）。

B) 各書込箇所で個別に temp→置換を実装（ヘルパー集約なし・最短だが重複）。

C) Other（[Answer]: の後に記述）

[Answer]:

### Question 2（Resilience — 読込の破損スキップ／空フォールバック）
一覧読込の「破損は飛ばす・空は空状態」の集約パターンは？

A) (推奨) **`StorageService.ListSounds()` に読込集約**：`sounds/*.meta.json` を走査し、各 meta を try/catch で読む→**パース不可/`meta==null`/対 wav 欠損はスキップ**（`skippedCount++`）、有効のみ収集。返却は `Result<List<SavedSound>>`＋（任意で `LoadOutcome{ items, skippedCount }`）。**どの段階の例外も最悪は空リストへフォールバック**（クラッシュしない）。コントローラは 0 件で `Empty` 状態へ。**受入＝meta 故意破損で一覧が他項目を正常表示・全滅時も空状態**（NFR-COL-R2/R3）。

B) コントローラ側で各 meta を個別に読み、失敗を都度ハンドリング（集約なし）。

C) Other（[Answer]: の後に記述）

[Answer]:

### Question 3（Performance/Scalability — 一覧描画・スクロール・サムネ）
数十〜数百件の一覧を体感即時＆60fps で出す実現パターンは？

A) (推奨) **段階適用**：まず**レイアウトグループ＋相対サイズ（固定px排除）**で構築し、サムネ（写真）は**遅延読み（表示時に非同期ロード・プレースホルダ）**＋`Texture2D` の適切な破棄で GC を抑制。件数が多い場合に備え **`SoundListView` を仮想化可能な抽象**（可視範囲のみ生成）にしておき、数百件で fps 低下が見えたら仮想化を有効化（実装は Code Generation 判断）。メタ投影（表示用 VM）を一度作りキャッシュ。**受入＝100件で表示<0.5s・スクロール60fps/最低30fps**（NFR-COL-P1）。

B) 全項目を一括生成（仮想化なし）。数百件で重ければ後日対応。

C) Other（[Answer]: の後に記述）

[Answer]:

### Question 4（Performance/Maintainability — 共有再生・エフェクト再適用の配置）
視聴で保存エフェクトを再適用する実現パターンは？（Collection→Rec 依存を作らない／NFR-COL-M4）

A) (推奨) **共有 Audio を Services 層へ**：`IAudioService` を後方互換拡張（`Result Play(AudioBuffer buffer, SoundEffectSettingsData settings)` 追加・既存 `Play(AudioBuffer)` 不変）。エフェクト適用（U3 `EffectChain` 相当）を **`Geidai.Services.Audio` の共有実装へ寄せ**、Rec/Collection の双方が `ServiceRegistry.Resolve<IAudioService>()` で利用。**シーンをまたぐ再生の可用性**は、サービス実装が**自前 AudioSource（必要時に生成/確保）**を持つ形で担保（Collection シーンでも発音可）。U3 の `RecAudioService` は共有実装へ移設/委譲（**録音側の挙動は不変**）。ServiceRegistry で実装解決。

B) コレクションの視聴は**生 WAV 再生**（エフェクト非適用）。再適用は将来（簡素）。

C) 共有せず `Geidai.Collection → Geidai.Rec` 依存を許可して `EffectChain` を直接参照。

D) Other（[Answer]: の後に記述）

[Answer]:

### Question 5（Testability/Security — 純粋フィルタと写真 I/O 抽象）
絞込/検索の純粋関数化と写真取得の抽象パターンは？

A) (推奨) **`CollectionFilter.Filter(items, query)` を純粋関数**（副作用なし）として配置＝**PBT 対象**（不変条件: 結果⊆入力・条件空→全件・冪等・AND 合成・月導出は `createdAtIso`→`YYYY-MM`・検索は `title/memo/nickname` 正規化部分一致[大小無視]）。写真取得は **`IPhotoPicker`（`Geidai.Services` IF）＋U4 スタブ実装**：選択→一時パス→`sounds/{id}.photo.<ext>` へ **`AtomicFile` で原子的コピー**→`photoFileName` 更新→meta 保存。**クラウド送信なし・PII 非ログ**（`SafeLogger`）。実機ネイティブピッカーはフォローアップ。

B) フィルタはコントローラ内にインライン（純粋分離なし）／写真は直接ファイル操作（抽象なし）。

C) Other（[Answer]: の後に記述）

[Answer]:

### Question 6（DI/Logical Components — Geidai.Collection 構成と IStorageService 拡張）
コレクションの論理コンポーネント構成と永続化 IF 拡張の設計は？

A) (推奨) **新規 `Geidai.Collection`**（`Collection → Services → Common`＋`UnityEngine.UI` 一方向）に **`CollectionScreenController`（`ScreenRootBase` 継承・状態統括）**＋`SoundListView`／`SoundDetailController`（編集）／`FilterSearchController`（`CollectionQuery` 保持・純粋 `Filter` 適用）を配置。ロジックは POCO/静的へ寄せ MonoBehaviour 依存最小化。`IStorageService` を**後方互換拡張**：`Result DeleteSound(string id)`（wav+meta+photo 一括・欠損無視）／`Result SaveMeta(SoundClipMeta)`（メタのみ原子的置換）を追加し、既存 `SaveSound`/`SaveProfile`/`SaveMeta` を **`AtomicFile` 経由の原子的置換へ統一**（シグネチャ不変）。削除確認=`ConfirmDialog`、失敗通知=`ErrorPresenter`、遷移=`NavigationService` を再利用。旧 `MySoundCollectionStorage`/`SoundSavePaths` は新方式へ集約（物理削除はシーン再配線と同時＝MCP フォローアップ）。

B) `Geidai.Collection` を作らず既存アセンブリに相乗り／`IStorageService` 拡張は最小（`DeleteSound` のみ）。

C) Other（[Answer]: の後に記述）

[Answer]:

---

## D. 完了条件
- Q1〜Q6 に回答 → 矛盾チェック（曖昧回答は追質問）→ nfr-design-patterns.md / logical-components.md を生成 → 承認ゲート。
- U1〜U3 の設計パターンを踏襲し、U4 固有の論理部品（`AtomicFile`・`IStorageService` 拡張・`CollectionFilter`・共有 Audio 再生・`IPhotoPicker`・コレクション画面コントローラ群）を明確化する。
- NFR Requirements（NFR-COL-P/R/U/Priv/T/M/UI）・Functional Design（§2〜§8）へのトレースが取れている。
