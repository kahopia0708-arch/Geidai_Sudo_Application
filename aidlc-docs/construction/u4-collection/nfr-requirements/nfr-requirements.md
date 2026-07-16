# U4 Persistence/Collection — NFR Requirements（非機能要件）

**ユニット**: U4 Persistence/Collection
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / NFR Requirements（Part 2）
**決定**: Q1=A（性能・数十〜数百件）/ Q2=A（原子的置換・破損スキップ・空フォールバック）/ Q3=A（PII 端末内のみ）/ Q4=A（純粋関数PBT＋統合テスト）/ Q5=A（`Geidai.Collection`＋`IStorageService` 拡張）/ Q6=A（Services 層 共有再生）
**対象**: NFR-04/05/06/07/08/09/11/12・RESILIENCY-01（US-COL-01〜04 / US-TECH-06）

> U1〜U3 の横断決定（プラットフォーム・レスポンシブ/SafeArea・オフライン・フェイルセーフ・セキュリティ既定・エンジン）は継承。本書は **U4 固有の受入可能値**を定義。詳細計測は Build & Test。

---

## 0. 継承する横断 NFR（U1〜U3 確定・再掲）
- NFR-01: iOS 15+ / Android 8.0(API26)+、縦横両対応。
- NFR-11: `ResponsiveCanvasConfigurator`（参照 1080×1920・Match=0.5・固定px排除）。
- NFR-12: `SafeAreaFitter`（`Screen.safeArea` 追従）。
- NFR-02: 完全オフライン（可用性/スケーラビリティ(サーバ)/DR は N/A）。
- NFR-07/SECURITY-15: 失敗は `Result` 化・クラッシュ/破損させない・フォールバック時警告。
- セキュリティ既定: PII 端末外送信禁止・ログ非出力（`SafeLogger`）・本番で詳細エラー非表示。

---

## 1. パフォーマンス・スケーラビリティ（NFR-06 / Q1=A）

| 項目 | 目標（受入可能値） | 備考 |
|---|---|---|
| 想定規模 | 個人利用 **数十〜数百件** | 上限厳密化は不要。多数時は仮想化/遅延読み |
| 一覧表示 | コレクションを開く→表示まで **体感即時（100件で目安 < 0.5s）** | meta 走査＋投影。サムネは遅延読み |
| スクロール | **60fps・最低 30fps を割らない** | 固定px排除・レイアウトグループ・サムネ遅延 |
| 視聴（再生開始） | タップ→発音 **体感即時（wav デコード込み 目安 < 0.3s）** | 3秒/44100/mono ≈ 264KB |
| 原子的書込 | メタ保存/写真差替/削除 **体感即時（目安 < 0.5s）** | temp→置換のコスト込み |
| フレーム | ターゲット 60fps／最低 30fps | 一覧描画・フィルタ適用時も維持 |

- **NFR-COL-P1**: 一覧読込は上記目標を満たし、件数増加時もスクロールが破綻しない（仮想化/遅延読みの適用は NFR Design）。
- **NFR-COL-P2**: フィルタ/検索の再適用は体感即時（純粋関数・O(n) 走査、n=数百で問題なし）。

## 2. 信頼性・堅牢性（NFR-07 / US-TECH-06 / RESILIENCY-01 / Q2=A）— U4 の主眼

- **NFR-COL-R1（原子性）**: profile / meta / wav / 写真の書込は「一時ファイルへ書込→成功後に本ファイルへ**原子的置換**」。**書込を中断/失敗させても既存データが無傷**。
- **NFR-COL-R2（破損スキップ）**: meta パース不可・対 wav 欠損は**安全にスキップ**し、他項目は正常表示（クラッシュしない）。
- **NFR-COL-R3（空フォールバック）**: 0 件・ディレクトリ無し・初期状態は**空状態**を表示（例外にしない）。
- **NFR-COL-R4（対整合）**: `SaveSound` は wav＋meta の対を整合保証（U3 の「失敗時 wav 削除」を原子的置換に強化）。削除は wav＋meta＋写真を一括（欠損無視）。
- **NFR-COL-R5（重要度）**: Rec/Collection = **Critical**（RESILIENCY-01）。
- **受入基準**:
  1. meta を故意に破損 → 一覧は他項目を正常表示（破損は読み飛ばし）。
  2. 書込を中断注入（例外/途中終了）→ 旧データが維持される（新も旧も壊れない）。
  3. 空/欠損 → 空状態（クラッシュ・例外画面にならない）。

## 3. ユーザビリティ（NFR-05）
- **NFR-COL-U1**: 削除は**確認ダイアログ**（既定＝いいえ）で誤操作防止。
- **NFR-COL-U2**: 空状態・検索0件は分かりやすい表示（子ども向け平易文言）。
- **NFR-COL-U3**: 失敗は `ErrorPresenter` で平易通知（生の例外を出さない）。
- **NFR-COL-U4**: タップ領域は十分な大きさ（一覧項目・再生/削除ボタン）。意匠は S さん（US-TECH-07）。

## 4. プライバシー（NFR-04 / SECURITY-03 / Q3=A）
- **NFR-COL-Priv1**: 写真・メモ・ニックネーム・音声・設定は**端末内（`persistentDataPath`）のみ**。**端末外送信なし**（`IPhotoPicker` もクラウドアップロードしない）。
- **NFR-COL-Priv2**: ログに PII（メモ・ニックネーム・写真実体）を出さない（`SafeLogger`）。
- **受入**: ネットワーク送信が無いこと・ログに PII が出ないことを確認。

## 5. テスト容易性（NFR-09 / PBT / Q4=A）
- **NFR-COL-T1（PBT）**: 絞込/検索 `Filter(items, query)` の不変条件（結果⊆入力・条件空→全件・冪等・AND 合成）。
- **NFR-COL-T2（PBT）**: メタ JSON 往復（`SavedSound` serialize↔deserialize、拡張フィールド欠損時は既定値＝後方互換）。
- **NFR-COL-T3（統合/EditMode）**: 原子的書込の性質（成功で新値・中断で旧値維持）、破損スキップ、削除（対ファイル）を故障注入で検証。
- 実行は Build & Test に集約可。

## 6. 保守性（NFR-08 / NFR-10 / Q5=A・Q6=A）
- **NFR-COL-M1**: 新規アセンブリ **`Geidai.Collection`**（`Collection → Services → Common` 一方向＋`UnityEngine.UI`）。
- **NFR-COL-M2**: `IStorageService` を**後方互換拡張**（`DeleteSound`／`SaveMeta`(or `UpdateSound`)、`SaveSound`/`SaveProfile` を原子的置換に強化・シグネチャ不変）。
- **NFR-COL-M3**: 写真取得は `IPhotoPicker` 抽象（Services 側 IF＋U4 スタブ、実機ピッカーはフォローアップ）。
- **NFR-COL-M4（共有再生）**: エフェクト適用付き再生を **Services 層の共有実装**へ寄せ、`IAudioService` を後方互換拡張（`Play(AudioBuffer, SoundEffectSettingsData)`）。Rec/Collection の双方が利用し、**`Collection→Rec` 依存を作らない**。録音側挙動は不変（後方互換）。
- **NFR-COL-M5**: 旧 `MySoundCollectionStorage`/`SoundSavePaths` は新方式へ集約（物理削除はシーン再配線と同時＝MCP フォローアップ）。

## 7. レスポンシブ / SafeArea（NFR-11/12）
- **NFR-COL-UI1**: 一覧は Anchor/レイアウトグループ・**固定px依存排除**（旧 `itemWidth 850px` 相当を相対化）。縦横両対応。
- **NFR-COL-UI2**: 画面ルートに `SafeAreaFitter`／`ResponsiveCanvasConfigurator` を適用。

---

## 8. 受入サマリ（Build & Test で検証）
1. 一覧/スクロール/視聴/書込が §1 の目標を満たす。
2. 故障注入（破損 meta・書込中断・空）で §2 受入 (1)(2)(3) を満たす。
3. ネットワーク送信なし・ログ PII なし（§4）。
4. `Filter`/メタ往復の PBT グリーン、原子性/破損/削除の統合テスト成功（§5）。
5. `Geidai.Collection` が一方向依存でコンパイル成功、`IStorageService`/`IAudioService` 拡張が後方互換（§6）。

## 9. トレース
NFR-06→§1 ／ NFR-07・US-TECH-06・RESILIENCY-01→§2 ／ NFR-05→§3 ／ NFR-04→§4 ／ NFR-09→§5 ／ NFR-08/10→§6 ／ NFR-11/12→§7。US-COL-01〜04 / US-TECH-06 を網羅。
