# U4 Persistence/Collection — Domain Entities（ドメインモデル）

**ユニット**: U4 Persistence/Collection
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Functional Design（Part 2）
**決定**: Q1=A（新形式のみ）/ Q2=A（`SoundClipMeta` 後方互換拡張）/ Q3=A（原子的書込・破損スキップ・空フォールバック）/ Q4=A（写真=ローカル参照＋ピッカー抽象）/ Q5=A（純粋な絞込・検索）/ Q6=A（確認削除）/ Q7=A（1画面・保存エフェクト再適用）
**技術非依存**: 具体 API（`File.Replace`/temp+rename）・スクロール仮想化・写真ピッカー実装・シーン配線は NFR Design / Code Generation。

> U1（`SoundClipMeta`/`SoundEffectSettingsData`/`SavedSound`/`AudioBuffer`）を再利用し、FR-10 のメタ拡張を **後方互換**で追記する。旧 `MySoundCollection` 形式は対象外（Q1=A）。

---

## 1. 再利用する既存モデル（U1・不変 or 後方互換拡張）

| モデル | 所在 | U4 での役割 | 変更 |
|---|---|---|---|
| `SavedSound` | `Geidai.Common.Models` | 一覧/詳細の単位（meta＋settings の対） | 不変 |
| `SoundEffectSettingsData` | `Geidai.Common.Models` | 保存エフェクト（視聴時に再適用） | 不変 |
| `AudioBuffer` | `Geidai.Common.Models` | 視聴時の生サンプル（wav→デコード） | 不変 |
| `SoundClipMeta` | `Geidai.Common.Models` | メタ情報 | **後方互換拡張**（下記 §2） |
| `Result` / `Result<T>` / `ResultCode` | `Geidai.Common.Results` | 全 I/O の成否伝搬 | 不変（必要なら理由コード追加を NFR Design で検討） |

---

## 2. `SoundClipMeta` 後方互換拡張（Q2=A / FR-10）

既存フィールド（`id` / `displayName` / `createdAtIso` / `wavFileName`）は**不変**。以下を**追記**する。`JsonUtility` は欠損フィールドを既定値で読むため、旧 JSON も安全に読める。

| フィールド | 型 | 既定 | 説明 |
|---|---|---|---|
| `id` | string | — | GUID（既存・不変）。ファイル名の基底。 |
| `displayName` | string | "" | 内部識別（既存・不変）。**表示には使わない**（表示は `title`）。 |
| `createdAtIso` | string | "" | 作成日時 ISO 8601（既存・不変）。**月別絞込・日付表示の源**。 |
| `wavFileName` | string | "" | 対 wav ファイル名（既存・不変）。 |
| `title` | string | "" | **表示名**（FR-10）。空なら **作成日付を表示**（BR-COL-11）。 |
| `photoFileName` | string | "" | 任意の写真ファイル名（`sounds/{id}.photo.*`）。空＝写真なし。 |
| `memo` | string | "" | 任意メモ（FR-10）。 |
| `nickname` | string | "" | 保存時にプロフィールのニックネームを写す（FR-10）。 |

- **不変条件**: `id` は非空・GUID。`title` は表示用（未設定時は日付にフォールバック）。`photoFileName` は空か、`sounds/` 配下の実在ファイル名。
- **PII**: `photoFileName`/`memo`/`nickname` は個人情報 → 端末外送信なし・ログ出力なし（NFR-04 / SECURITY-03）。

---

## 3. U4 で新設する値・ビューモデル（技術非依存）

### 3.1 `CollectionQuery`（絞込・検索条件 / Q5=A）
一覧に適用するフィルタ条件（純粋関数の入力）。

| フィールド | 型 | 説明 |
|---|---|---|
| `yearMonth` | string?（"YYYY-MM"） | 月別絞込。null/空＝全月。 |
| `keyword` | string | キーワード（title/memo/nickname 部分一致）。空＝検索なし。 |

- **合成規則**: `yearMonth` と `keyword` は **AND**（両方満たす項目のみ）。
- **正規化**: `keyword` は trim ＋ 大文字小文字/全半角を実用的に無視して比較（BR-COL-30）。

### 3.2 `SoundListItem`（一覧項目のビュー投影・任意）
一覧描画に必要な最小情報（`SavedSound` からの投影。実装で必要なら用いる）。

| フィールド | 型 | 由来 |
|---|---|---|
| `id` | string | `meta.id` |
| `displayTitle` | string | `meta.title`（空なら日付） |
| `createdAtIso` | string | `meta.createdAtIso` |
| `hasPhoto` | bool | `meta.photoFileName` 非空 |

### 3.3 `LoadOutcome`（破損を含む読込結果の表現 / Q3=A）
一覧読込は「有効項目リスト」と「読み飛ばした件数」を返す（破損スキップの可視化）。

| フィールド | 型 | 説明 |
|---|---|---|
| `items` | List&lt;SavedSound&gt; | 有効に読めた項目（破損・欠損は除外） |
| `skippedCount` | int | 破損/対欠損で読み飛ばした件数（ログ/デバッグ用途） |

> ※ 実装表現（新クラス化するか `Result<List<SavedSound>>` に留めるか）は NFR Design/Code Generation で確定。機能設計上は「破損を安全に除外し、他を返す」ことを規定。

---

## 4. 写真エンティティ（Q4=A / US-COL-02）
- 写真は**独立バイナリファイル** `sounds/{id}.photo.<ext>`（jpg/png）として保存し、`meta.photoFileName` で対応付け。
- **取得手段は抽象**（`IPhotoPicker` 相当）: 「選ぶ→一時パス取得→`sounds/` へ原子的コピー→`photoFileName` 更新」。U4 は枠組み＋スタブ、実機ピッカーは MCP/プラグイン フォローアップ。
- 写真の**差し替え/削除**でファイルも更新/削除（削除時は音と対で削除＝BR-COL-40）。

---

## 5. 永続化レイアウト（Q1=A / FR-12）
```
Application.persistentDataPath/
├── profile.json                 (U1: UserProfile／原子的書込へ強化)
└── sounds/
    ├── {id}.wav                 (生 16bit PCM / WavCodec)
    ├── {id}.meta.json           (SavedSound = SoundClipMeta＋SoundEffectSettingsData)
    └── {id}.photo.jpg|png       (任意・写真)
```
- **対の原則**: `{id}.wav` と `{id}.meta.json` は対。**meta 基準**で一覧を作り、対 wav 欠損はスキップ（BR-COL-21）。写真は任意（欠損しても項目は有効）。
- 旧 `MySoundCollection/` は U4 対象外（Q1=A）。

---

## 6. トレース
- FR-09→`SavedSound` 一覧・視聴・削除 ／ FR-10→`SoundClipMeta` 拡張（title/photo/memo/nickname）／ FR-11→`CollectionQuery`（月別・検索）／ FR-12→§5 レイアウト。
- NFR-04→写真/メモ/ニックネームの PII 非送信 ／ NFR-07・US-TECH-06→§3.3 `LoadOutcome`（破損スキップ）・原子的書込 ／ RESILIENCY-01→Collection=Critical。
