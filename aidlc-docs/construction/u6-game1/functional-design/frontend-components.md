# U6 Game①音合わせ — Frontend Components（UI 構成・ハンドオフ）

**ユニット**: U6 Game①音合わせ
**作成**: 2026-07-16 / AI-DLC CONSTRUCTION / Functional Design（Part 2）
**決定**: Q5=A（タップ確認＋uGUI ドラッグ）/ Q6=A（カエル進化・再挑戦）/ Q7=A（新 Geidai.Game1）

> 前本さん が基本 UI 枠組み → Sさん が意匠/演出を調整（US-TECH-07）。レスポンシブ/SafeArea（U1 基盤）踏襲。

---

## 1. コンポーネント一覧

| コンポーネント | 種別 | 役割 | 配置 |
|---|---|---|---|
| `SoundMatchGameController` | `ScreenRootBase` サブクラス | ①音合わせの統括（開始→出題→判定→演出→進行） | `Geidai.Game1` |
| `ChoiceItemView` | MonoBehaviour | 選択肢（おたまじゃくし）1件。タップ確認・ドラッグ解答 | `Geidai.Game1` |
| `FrogTargetView` | MonoBehaviour | お手本（カエル）＝タップ確認＋ドロップ領域 | `Geidai.Game1` |
| `ResultEffectController` | MonoBehaviour | 正解演出（進化）・不正解の再挑戦・結果まとめ | `Geidai.Game1` |
| `BackToHomeButton`（既存 U2） | MonoBehaviour | ホームへ戻る | シーン配置 |
| `ErrorPresenter`（既存 U1） | MonoBehaviour | 再生/取得エラー通知 | 画面共通 |

---

## 2. SoundMatchGameController（統括 / `ScreenRootBase`）
- **依存**: `IStorageService`（保存音）、`PitchVariationService`＋`IAudioService`（発音）、`ContentService` or インスペクタ（`SoundMatchConfig`）、`INavigationService`（戻る）。`ServiceRegistry` 解決。
- **状態**:

| 状態 | 表示 | 遷移 |
|---|---|---|
| Loading | 準備中（素材選択・出題生成） | 完了で Playing / Empty |
| Empty | 素材なしフォールバック（ろくおんしてね） | ホーム誘導 |
| Playing | お手本＋選択肢・操作受付 | 解答→Judging |
| Judging | 判定・演出 | 正解→次問 or Result／不正解→Playing |
| Result | 正解数まとめ | もう一度 / ホーム |

- **役割**: `StartGame()`、`NextQuestion()`、`OnAnswer(choiceIndex)`、`OnBackPressed()`→ホーム。

## 3. ChoiceItemView（選択肢／おたまじゃくし）
- **UI 参照**: ルート `RectTransform`、アイコン `Image`、`Button`（タップ確認）、ドラッグ用ハンドラ。
- **操作**: タップ→`Play(baseBuffer, cents)`（確認）。ドラッグ（`IBeginDragHandler`/`IDragHandler`/`IEndDragHandler`）→お手本ドロップ領域で確定。領域外は元位置へ復帰。
- **データ**: 割り当てられた `ChoiceSpec`（cents）と自身の index を保持。
- **ハンドオフ（Sさん）**: おたまじゃくし絵柄・サイズ・当たり判定・ドラッグの視覚フィードバック。

## 4. FrogTargetView（お手本／カエル・ドロップ領域）
- **UI 参照**: カエル `Image`、タップ `Button`（お手本再生）、ドロップ判定領域（`RectTransform`）。
- **操作**: タップ→`Play(baseBuffer, targetCents)`。選択肢がここにドロップされたら `SoundMatchGameController.OnAnswer(index)`。
- **ハンドオフ（Sさん）**: カエル絵柄・進化アニメの土台。

## 5. ResultEffectController（演出・結果）
- **役割**: `PlayCorrect()`（おたまじゃくし→カエル進化＝アニメ/パーティクル/効果音フック）、`PlayRetry()`（やさしい再挑戦）、`ShowResult(correct, total)`。
- **分離**: 進行ロジック（次問/終了）は Controller。ここは見せ方のみ。
- **ハンドオフ（Sさん）**: 進化アニメ・効果音・褒め文言・結果画面の意匠。

## 6. レスポンシブ / SafeArea / アクセシビリティ
- U1 の `ResponsiveCanvasConfigurator`/`SafeAreaFitter` 配下（縦横両対応・NFR-11/12）。
- 大きく分かりやすい絵/文字（NFR-05）。操作は指で扱いやすいサイズ。

## 7. MCP フォローアップ（シーン配線・Code Generation 後）
- 既定 `SoundMatchConfig` アセット生成（暫定セント段階）。
- Game1 シーン作成/更新：`SoundMatchGameController`＋`FrogTargetView`＋`ChoiceItemView`（プレハブ）＋`ResultEffectController` 配置、サービス解決。
- 既存ゲーム選択 UI（`GameListUI`/`StartGameButton`）から①音合わせへ `NavigationService.GoTo(Game1)` で接続。
- Build Settings に Game1 シーン登録（未登録なら）。
- 演出アニメ・イラスト差し込み（Sさん）。

## 8. トレース
US-GAME1-01→ChoiceItemView/FrogTargetView（タップ/ドラッグ）・Controller 判定 ／ US-GAME1-02→出題要素（ピッチ主軸） ／ US-GAME1-03→ResultEffectController ／ US-GAME1-04→SoundMatchConfig 反映 ／ US-GAME1-05→保存音素材・非保存加工。US-TECH-07→各ハンドオフ点。
