# U2 Foundation — Business Rules（業務ルール・制約）

**ユニット**: U2 Foundation（起動・ホーム・登録・ナビ導線）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Functional Design（Part 2）
**方針**: Q1〜Q7＝すべて A（推奨）
**トレース**: US-NAV-01/02, US-REG-01/02 / FR-01/02/03/04, SECURITY-05

> BR 番号は U2 ローカル（U2-BRxx）。U1 の業務ルール（検証範囲・遷移安全）を再利用/延長する。

---

## 1. 初回起動・遷移
- **U2-BR-01**: 起動時の状態は `profile.json` の有無で判定する（`LoadProfile`＝`NotFound` なら `FirstLaunch`、成功なら `Returning`）。派生状態は永続化しない。
- **U2-BR-02**: Boot 画面は明示的なタップ起点（「はじめる」）を経てから遷移する（子ども配慮／Q2=A）。自動遷移はしない。
- **U2-BR-03**: `FirstLaunch` は登録（New）へ、`Returning` はホームへ遷移する。
- **U2-BR-04**: プロフィール読込が `Corrupted`/`IOError` の場合はクラッシュせず平易通知し、安全側として登録（New）へ誘導する。**破損を正常と誤認させる過度な自動処理はせず、フォールバック時は警告を出す**（U1 Q6 方針と整合）。

## 2. 入力検証（登録・編集）
- **U2-BR-05**: 生年は 1900〜今年（未来年不可）。UI はドロップダウン選択（Q5=A）。検証は U1 `ValidationUtil.ValidateBirthYear` を用いる。
- **U2-BR-06**: ニックネームは前後空白除去して 1〜8 文字（空不可）。検証は U1 `ValidationUtil.ValidateNickname` を用いる。
- **U2-BR-07**: 検証はすべて通過した場合のみ確定（保存）する。1 つでも失敗したら保存せず、該当項目を平易通知する（`ErrorPresenter`）。
- **U2-BR-08**: 保存は `UserProfile` として `StorageService.SaveProfile` で行い、**端末外へ送信しない**（NFR-04）。ログにも生年・ニックネーム（PII）を出さない（U1 `SafeLogger` 方針）。
- **U2-BR-09**: 編集（Edit）は既存値をフォーム初期値とし、確定時に上書き保存。キャンセルは変更破棄でホームへ戻る。

## 3. ホーム導線・除外
- **U2-BR-10**: ホームは可視な `HomeMenuItem` のみ描画する。MVP の可視導線＝Rec / コレクション / ゲーム選択 / weekly theme ＋ 設定/プロフィール編集（Q3=A）。
- **U2-BR-11**: **共有（Place）・テストはホーム導線から除外**（非表示）。`SceneId` に Place を含めない（U1 既定）。既存 `GoToPlace` 等の Place 導線は削除する（Q6=A）。
- **U2-BR-12**: 各導線はアイコン/モチーフ（カエル・おたまじゃくし・蓮）で識別できる（NFR-05）。ラベル・アイコン・並び順は `UITheme`/データとして分離し Sさん が調整可能（US-TECH-07）。

## 4. 遷移の安全性
- **U2-BR-13**: 画面遷移は必ず `NavigationService` 経由で行う（直接 `SceneManager` 呼び出しをコントローラで行わない）。
- **U2-BR-14**: 未定義/未整備シーンへの遷移要求は `Result(NotFound)` を返し、クラッシュせず平易通知する（例: weekly theme 専用画面が未整備の間は「準備中」通知）。US-TECH-04 と整合。
- **U2-BR-15**: モジュール画面からの「もどる/ホーム」はホームへ遷移する。ホームでの端末バック（Android）は**終了確認**を挟む（誤操作防止／Q7=A）。
- **U2-BR-16**: 既存の per-button 遷移スクリプト（`SceneSwitcher`/`GoTo*`/`ReturnHomeButton`）は U2 のコントローラ＋`NavigationService` 方式へ統一し、置き換え/除去する（Q6=A）。実シーンの配線は Code Generation 以降で Unity MCP により実施。

## 5. トレーサビリティ
| ルール | ストーリー | 要件 |
|---|---|---|
| U2-BR-01〜04 | US-NAV-01, US-REG-01 | FR-01, FR-03 |
| U2-BR-05〜09 | US-REG-01, US-REG-02 | FR-03, FR-04, SECURITY-05 |
| U2-BR-10〜12 | US-NAV-02 | FR-01, FR-02, NFR-05 |
| U2-BR-13〜16 | US-NAV-01, US-TECH-04 | FR-02 |
