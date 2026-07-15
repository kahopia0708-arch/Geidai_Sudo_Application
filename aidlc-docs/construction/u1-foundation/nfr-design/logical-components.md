# U1 基盤 — Logical Components（NFRを支える論理コンポーネント）

**ユニット**: U1 基盤
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Design（Part 2）
**入力**: `nfr-design-patterns.md`, `../functional-design/*`, `../../inception/application-design/component-dependency.md`

> NFR を実現するために U1 で用意する論理コンポーネント（横断部品）と、その責務・連携。

---

## 1. 論理コンポーネント一覧

| コンポーネント | 種別 | 責務 | 支えるNFR |
|---|---|---|---|
| `Result<T>` / `ResultCode` | 型（Common） | 成功/失敗＋理由の伝搬。例外を UI に漏らさない | NFR-07 |
| `ServiceRegistry` | 器（Services） | 軽量サービスロケータ。IF→実装の登録/解決 | NFR-08 |
| `AppManager` | サービス | 起動オーケストレーション。サービス生成・登録・初回遷移 | NFR-06/07/08 |
| `IStorageService` / `StorageService`(最小) | サービス | ローカル永続化 IF＋最小実装（整合チェック/スキップ） | NFR-07 |
| `INavigationService` / `NavigationService` | サービス | enum(SceneId) 型安全遷移・GoBack | NFR-06 |
| `IContentService` / `ContentService`(器) | サービス | ScriptableObject/JSON コンテンツ取得の IF | NFR-05/08 |
| `IAudioService`(器) | サービス IF | 録音/再生 IF（実装は U3） | — |
| `ValidationUtil` | ユーティリティ(Common) | 入力検証集約（生年/ニックネーム, BR-01〜03） | Security/NFR-05 |
| `SafeLogger` | ユーティリティ(Common) | PII マスク付きログラッパ。本番は詳細抑制 | Security/NFR-04 |
| `WavCodec` | 純粋関数(Common) | WAV encode/decode（副作用なし）→PBT | NFR-09 |
| `PitchMath` | 純粋関数(Common) | cents↔ratio 変換（副作用なし）→PBT | NFR-09 |
| `ResponsiveCanvasConfigurator` | UI部品(Common) | CanvasScaler 統一設定（1080×1920/Match0.5） | NFR-11 |
| `SafeAreaFitter` | UI部品(Common) | `Screen.safeArea` 追従・向き/解像度変更で再適用 | NFR-12 |
| `ScreenRootBase` | UI基底(Common) | 表示/遷移/Responsive/SafeArea をライフサイクルで強制 | NFR-11/12 |
| `UITheme` | ScriptableObject | 配色/フォント/アイコンの一元管理（Sさん調整点） | NFR-05/US-TECH-07 |
| `ErrorPresenter`（トースト/バナー） | UI部品(Common) | アイコン＋平易文言でエラー/警告提示（BR-16/19） | NFR-05/07 |

## 2. 連携（主要フロー）

### 2.1 起動（AppManager オーケストレーション）
1. `AppManager.Bootstrap()` → `ServiceRegistry` に各サービス（IF→実装）を登録。
2. `IStorageService` で `profile.json` 読込（整合チェック）。破損→`ErrorPresenter` 警告＋登録画面へ。
3. `INavigationService.GoTo(SceneId.Home or Register)`（初回判定, BR-13）。

### 2.2 保存（Result 型 + 性能）
1. 呼び出し元 → `IStorageService.Save(...)`。
2. 重い WAV 書込は必要に応じ非同期。結果は `Result<T>`。
3. 失敗 → `ErrorPresenter` に理由コードで通知（クラッシュさせない）。

### 2.3 画面表示（Responsive/SafeArea 強制）
1. `ScreenRootBase.ShowAsync()` → `ResponsiveCanvasConfigurator.Configure()` → `SafeAreaFitter.Apply()` → 画面固有初期化。
2. 向き/解像度変更イベント → `SafeAreaFitter.Reapply()`。

## 3. モジュール配置（AsmDef 境界 / NFR-08）
```
Geidai.Common    : Result, ValidationUtil, SafeLogger, WavCodec, PitchMath,
                   ResponsiveCanvasConfigurator, SafeAreaFitter, ScreenRootBase,
                   UITheme, ErrorPresenter, ドメインモデル, SceneId
Geidai.Services  : ServiceRegistry, AppManager, StorageService(最小),
                   NavigationService, ContentService(器), IAudioService(IF)
Geidai.Tests     : FsCheck + UTF（WavCodec/PitchMath/JSON の PBT）
```
- 依存方向: モジュール → Services → Common（一方向・循環なし, Application Design と整合）。
- インターフェースは Common または Services 側に置き、実装差し替え/モックを可能に。

## 4. リスク / 留意
- `ServiceRegistry` は軽量ゆえ登録漏れに注意 → `AppManager.Bootstrap` に登録を集約し1箇所管理。
- `JsonUtility` 制約（Dictionary/null 弱い）→ モデルは素直な構造で設計（tech-stack-decisions.md §3）。
- SafeArea 再適用の過剰呼び出しで性能低下しないよう、変更検知（前回値と差分）で間引く。

## トレース
Result/Storage→NFR-07 / AppManager・Navigation→NFR-06 / ValidationUtil・SafeLogger→Security・NFR-04 / WavCodec・PitchMath→NFR-09 / Responsive・SafeArea・ScreenRootBase→NFR-11/12 / UITheme→US-TECH-07 / AsmDef→NFR-08。
