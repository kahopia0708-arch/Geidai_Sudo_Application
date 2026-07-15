# U1 基盤 — Frontend Components（UI基盤の構造・ライフサイクル・ハンドオフ）

**ユニット**: U1 基盤
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Functional Design（Part 2）
**対象**: 共通 UI 基盤（ScreenRootBase / SafeAreaFitter / ResponsiveCanvasConfigurator / UITheme / 画面ルート Prefab テンプレート）
**注**: CanvasScaler の参照解像度・Match 値、SafeArea の具体実装方式（値・タイミング）は **NFR Design** で確定。ここでは構造・状態・相互作用・ハンドオフ点を定義。

---

## 1. コンポーネント階層（UI基盤テンプレート）
```
ScreenRoot (Prefab テンプレート)
├── Canvas (ResponsiveCanvasConfigurator 適用)
│   └── SafeAreaRoot (SafeAreaFitter 適用)
│       ├── Header (任意: タイトル/戻る)
│       ├── Content (各画面が差し込む領域)
│       └── Footer (任意)
└── (UITheme を参照して配色/フォント/アイコンを適用)
```

### テキスト代替
- 画面ルート Prefab は Canvas（レスポンシブ設定）→ SafeAreaRoot（SafeArea 追従）→ Header/Content/Footer の順で構成。各画面は Content に自身のUIを差し込む。配色・フォント・アイコンは UITheme を参照。

## 2. コンポーネント定義

### 2.1 ScreenRootBase（抽象）
- **役割**: 全画面コントローラの基底。表示/非表示・戻る・SafeArea/レスポンシブ組込を共通化。
- **状態（State）**: `IsVisible`（表示中か）。
- **ライフサイクル**: `ShowAsync()` → `ConfigureResponsive()` → `ApplySafeArea()` → 画面固有初期化。`HideAsync()` で後始末。`OnBackPressed()` は既定で `NavigationService.GoBack()`。
- **相互作用**: NavigationService（遷移）、UITheme（見た目適用）。

### 2.2 SafeAreaFitter（MonoBehaviour）
- **役割**: `Screen.safeArea` に RectTransform を追従（縦横両対応、向き変更に再適用）。
- **入力**: 現在の safeArea・画面向き。**出力**: アンカー/オフセット調整。
- **再計算タイミング**: 表示時＋解像度/向き変更時（詳細は NFR Design）。

### 2.3 ResponsiveCanvasConfigurator
- **役割**: CanvasScaler を Scale With Screen Size に統一設定（縦横両対応）。
- **入力**: 対象 Canvas。**出力**: 統一設定の適用。**数値（参照解像度/Match）は NFR Design**。

### 2.4 UITheme（ScriptableObject）
- **役割**: 配色・フォント・アイコン/モチーフ参照（カエル/おたまじゃくし/蓮）の一元管理。
- **ハンドオフ**: Sさん が UITheme アセットを編集して見た目を調整（コード改修不要 / US-TECH-07）。

## 3. ユーザー操作フロー（基盤レベル）
- **戻る操作**: 各画面の戻る/システムバック → `ScreenRootBase.OnBackPressed()` → `NavigationService.GoBack()`。
- **向き変更**: 端末回転 → SafeAreaFitter/Responsive が再適用され、操作要素が見切れない（NFR-11/12, US-TECH-01/02）。
- **エラー通知**: 共通のトースト/バナー（アイコン＋平易文言）で提示（BR-16）。フォールバック時は警告表示（BR-19）。

## 4. フォーム/入力（登録は U2 で画面実装、基盤は検証支援のみ）
- U1 は検証ユーティリティ（BR-01〜03）と入力UI部品の枠組みを提供。実際の登録画面は U2。
- 年の選択式（ドロップダウン）UI 部品の枠を用意（Q1=B）。

## 5. ハンドオフ点（前本→Sさん / US-TECH-07）
- **前本が提供**: ScreenRoot Prefab テンプレート、SafeArea/レスポンシブ組込、UITheme の枠、Content 差し込み構造、動作する空画面。
- **Sさん が調整**: UITheme（配色/フォント/アイコン）、各 Prefab 上の余白/配置/素材/文言。
- **原則**: 見た目調整はコード改修を伴わない（または最小限）。調整箇所を Prefab/ScriptableObject として明示。

## 6. API 連携
- 外部 API/バックエンドなし（完全オフライン / NFR-02）。UI はローカルサービス（Navigation/Storage/Content/Audio）のみを利用。

## トレース
ScreenRootBase/SafeAreaFitter/ResponsiveCanvasConfigurator→US-TECH-01/02, NFR-11/12 ／ UITheme・ハンドオフ→US-TECH-07, §7 ／ 戻る/遷移→US-NAV, US-TECH-04。
