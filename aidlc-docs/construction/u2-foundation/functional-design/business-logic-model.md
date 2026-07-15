# U2 Foundation — Business Logic Model（業務ロジック・データフロー）

**ユニット**: U2 Foundation（起動・ホーム・登録・ナビ導線）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Functional Design（Part 2）
**方針**: Q1〜Q7＝すべて A（推奨）
**トレース**: US-NAV-01/02, US-REG-01/02 / FR-01/02/03/04, SECURITY-05

> 技術非依存のふるまい定義。U1 のサービス契約（`NavigationService`/`StorageService`/`ErrorPresenter`/`ValidationUtil`）を利用する。

---

## 1. コントローラ責務（ふるまい）
| コントローラ | 継承 | 責務 |
|---|---|---|
| `BootScreenController` | `ScreenRootBase` | 起動起点の提示（「はじめる」）→ 起動状態判定 → 登録 or ホームへ遷移 |
| `HomeScreenController` | `ScreenRootBase` | ホームメニュー（`HomeMenuItem` 群）の提示と各モジュールへの遷移、設定/編集入口 |
| `UserRegistrationScreenController` | `ScreenRootBase` | 登録（New）/編集（Edit）の入力・検証・保存・遷移 |

---

## 2. フロー: アプリ起動（US-NAV-01 / FR-01）
```
起動 → Boot 表示 → ユーザーが「はじめる」タップ
   → StorageService.LoadProfile()
       ├─ NotFound        → AppLaunchState=FirstLaunch → NavigationService.GoTo(Register[New])
       ├─ 成功(profile)    → AppLaunchState=Returning   → NavigationService.GoTo(Home)
       └─ Corrupted/IOError → ErrorPresenter で平易通知 → 既定は登録(New)へ誘導（安全側）
```
- テキスト説明: 起動後、Boot 画面で明示的なタップ起点（子ども配慮／Q2=A）を経てからプロフィールを読み込む。未登録なら登録（New）、登録済みならホームへ。読み込みが破損/IO 失敗のときはクラッシュせず通知し、安全側として登録フローへ誘導する（過度な自動リセットはせず、破損時は警告する＝BR）。
- **データフロー**: 入力=なし／参照=`profile.json`（`StorageService`）／出力=遷移（`SceneId`）。

## 3. フロー: 初回登録（US-REG-01 / FR-03）
```
Register[New] 表示 → RegistrationDraft 入力（birthYear 選択, nickname 入力）
   → 確定操作 → ValidationUtil.ValidateBirthYear + ValidateNickname
       ├─ いずれか失敗 → ErrorPresenter で該当項目を平易通知（保存しない）
       └─ すべて成功  → UserProfile 生成 → StorageService.SaveProfile
             ├─ 成功    → NavigationService.GoTo(Home)
             └─ 失敗    → ErrorPresenter 通知（フォームに留まる、再試行可）
```
- テキスト説明: 入力を検証し、通過時のみ `UserProfile` を保存してホームへ。保存内容は端末外へ送信しない（NFR-04）。次回起動以降は `Returning` となり登録はスキップされる（US-REG-01）。
- **データフロー**: 入力=`RegistrationDraft`／出力=`profile.json` 更新＋遷移。

## 4. フロー: 登録情報の編集（US-REG-02 / FR-04）
```
Home → ProfileEdit 選択 → NavigationService.GoTo(Register[Edit])
   → 既存 UserProfile を LoadProfile で読み、Draft に初期表示
   → 変更 → 確定 → 検証（同上）
       ├─ 失敗 → 通知（保存しない）
       └─ 成功 → SaveProfile(上書き) → Home へ戻る（またはキャンセルで戻る）
```
- テキスト説明: 編集モードは既存値をフォームに初期表示し、検証通過時のみ上書き保存する。キャンセル時は変更を破棄してホームへ戻る。

## 5. フロー: ホーム導線（US-NAV-01/02 / FR-01/02）
```
Home 表示 → 可視な HomeMenuItem 群を描画（Rec/Collection/GameSelect/WeeklyTheme + 設定）
   → 項目タップ → moduleId→SceneId 解決 → NavigationService.GoTo(SceneId)
       ├─ 成功       → 対象モジュール画面へ
       └─ NotFound   → ErrorPresenter で「準備中」等を平易通知（クラッシュしない／Q7=A）
モジュール画面の「もどる/ホーム」→ NavigationService.GoTo(Home)
Home で端末バック → 終了確認（誤操作防止／Q7=A）
```
- テキスト説明: ホームは可視項目のみ描画（Place・テストは除外）。遷移は必ず `NavigationService` を通し、未整備シーンは `NotFound` として安全に通知する。モジュールからは共通の「もどる/ホーム」でホームへ戻り、ホームでの端末バックは終了確認を挟む。

---

## 6. サービス連携（U1 契約の利用）
| 呼び出し | 目的 | 失敗時 |
|---|---|---|
| `StorageService.LoadProfile()` | 起動判定・編集初期値 | `NotFound`→登録、`Corrupted/IOError`→通知＋安全誘導 |
| `StorageService.SaveProfile(profile)` | 登録/編集確定 | `IOError`→通知・フォーム維持（再試行） |
| `ValidationUtil.ValidateBirthYear/Nickname` | 入力検証 | `ValidationError`→該当項目を通知 |
| `NavigationService.GoTo(SceneId)` | 画面遷移 | `NotFound`→通知（クラッシュ回避） |
| `NavigationService.GoBack()` | 戻る導線（任意） | `NotFound`→ホームへフォールバック |
| `ErrorPresenter.ShowError/ShowWarning` | 平易通知 | — |

- U2 は新規サービスを増やさず、U1 の器を利用する（NFR-08）。

---

## 7. 非目標（U2 では行わない）
- 各モジュール（Rec/Collection/Theme/Game）の内部ロジック（各担当ユニット）。
- 永続化の原子的置換・破損復旧の本実装（U4）。
- 認証・ネットワーク（完全オフライン／NFR-02）。
