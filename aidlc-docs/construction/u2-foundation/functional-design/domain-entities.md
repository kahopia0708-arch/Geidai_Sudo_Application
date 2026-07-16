# U2 Foundation — Domain Entities（ドメインモデル）

**ユニット**: U2 Foundation（起動・ホーム・登録・ナビ導線）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Functional Design（Part 2）
**方針**: Q1〜Q7＝すべて A（推奨）
**トレース**: US-NAV-01/02, US-REG-01/02 / FR-01/02/03/04, SECURITY-05

> 技術非依存のドメイン定義。永続化・UI 技術・シーン配線は NFR Design / Code Generation で扱う。
> **U1 再利用**: `UserProfile`（birthYear/nickname）、`SceneId`、`Result<T>`、`ValidationUtil`、`NavigationService`、`StorageService` は U1 で定義済み。本書は U2 固有の概念のみ新規定義する。

---

## 1. 再利用エンティティ（U1 由来）
| エンティティ | 所在 | U2 での用途 |
|---|---|---|
| `UserProfile` | `Geidai.Common.Models` | 登録・編集・初回判定の対象（birthYear:1900〜今年 / nickname:1〜8） |
| `SceneId` | `Geidai.Common.Models` | 遷移先識別（U2 で `Register` を運用開始、`GameSelect` を追加＝§4） |
| `Result` / `Result<T>` | `Geidai.Common.Results` | 保存/遷移/検証の成否伝搬 |

---

## 2. U2 固有エンティティ / 値オブジェクト

### 2.1 AppLaunchState（起動状態）— enum
アプリ起動時に、プロフィールの有無から導出する状態。
| 値 | 意味 | 導出条件 |
|---|---|---|
| `FirstLaunch` | 初回（未登録） | `StorageService.LoadProfile()` が `NotFound` |
| `Returning` | 登録済み | `LoadProfile()` が成功し `UserProfile` を取得 |

- 派生値であり永続化しない（`profile.json` の有無が唯一の真実源／BR）。

### 2.2 ModuleId（ホーム導線の対象）— enum
ホームから遷移可能な MVP モジュール。Place・テストは**含めない**（除外＝BR）。
| 値 | 表示（例） | モチーフ（NFR-05） | 遷移先 `SceneId` |
|---|---|---|---|
| `Rec` | ろくおん | カエル | `SceneId.Rec` |
| `Collection` | コレクション | 蓮 | `SceneId.Collection` |
| `GameSelect` | ゲーム | おたまじゃくし | `SceneId.GameSelect`（§4 で追加） |
| `WeeklyTheme` | こんしゅうのおだい | — | `SceneId.Theme` |
| `ProfileEdit` | せってい/プロフィール | — | `SceneId.Register`（編集モード） |

> `ProfileEdit` は「導線」だがモジュール画面ではなく登録シーンの編集モードを開く（Q4=A）。

### 2.3 HomeMenuItem（ホームメニュー項目）— 値オブジェクト
ホームに並ぶ 1 導線を表す。
| 属性 | 型 | 説明 |
|---|---|---|
| `moduleId` | `ModuleId` | 対象モジュール |
| `label` | string | 子ども向け平易表示（かな中心） |
| `iconKey` | string | `UITheme` のアイコン/モチーフ参照キー（Sさん 調整点／US-TECH-07） |
| `visible` | bool | 表示可否（MVP 外は false＝除外） |
| `enabled` | bool | 操作可否（MVP は全 true。将来「準備中」用に予約） |

- ホームメニューは `HomeMenuItem` のリスト（順序＝表示順）。内容は設定/データとして分離し差し替え可能（US-TECH-07 / NFR-05）。

### 2.4 RegistrationMode（登録画面モード）— enum
| 値 | 意味 | 遷移元 |
|---|---|---|
| `New` | 初回登録 | Boot（`FirstLaunch`） |
| `Edit` | 既存編集 | Home（`ProfileEdit`） |

### 2.5 RegistrationDraft（入力ドラフト）— 一時値オブジェクト
登録/編集フォームの未確定入力。確定時に `UserProfile` へ変換して保存。
| 属性 | 型 | 説明 |
|---|---|---|
| `birthYear` | int | ドロップダウン選択値（1900〜今年／Q5=A） |
| `nickname` | string | 前後空白除去して 1〜8 文字（U1 `ValidationUtil` で検証） |

- 一時オブジェクト（永続化しない）。検証 OK のときのみ `UserProfile` として `StorageService.SaveProfile` に渡す。

---

## 3. 関係（概念）
```
AppLaunchState ──(導出)── UserProfile 有無 ──(StorageService.LoadProfile)
Home ──保持── List<HomeMenuItem> ──(moduleId→SceneId)── NavigationService.GoTo
Registration ──(RegistrationMode)── RegistrationDraft ──(検証OK)── UserProfile ── SaveProfile
```
- テキスト説明: 起動状態は `UserProfile` の有無から導出。ホームは複数の `HomeMenuItem` を保持し、各項目の `moduleId` を `SceneId` に対応づけて `NavigationService` で遷移する。登録画面は `RegistrationMode` に応じ `RegistrationDraft` を編集し、検証通過時のみ `UserProfile` に変換して保存する。

---

## 4. U1 資産の拡張（U2 で反映）
U2 は U1 の `SceneId` / `NavigationService` マップを次のとおり拡張する（Code Generation で実装）。
- `SceneId` に **`GameSelect`**（ゲーム選択画面／既存 `game_Home` に対応）を追加。
- `NavigationService` のマップに **`Register`（登録シーン）** と **`GameSelect`** を登録。
- `Theme`（weekly theme 専用画面）は U5 でシーン整備するまで未登録のままとし、遷移要求時は `Result(NotFound)` で安全に扱う（Q7=A）。
- `Place` は `SceneId` に含めず（U1 既定）、ホーム導線からも除外（BR）。

> ※ 列挙拡張は Common への追記だが U1 契約の後方互換な拡張（既存値の意味は不変）。
