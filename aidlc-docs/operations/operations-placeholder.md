# Operations（運用）— PLACEHOLDER

**プロジェクト**: 藝大 須藤さんアプリ（「おと」 / `jp.geidai.sudo.oto`）  
**作成**: 2026-07-30 / AI-DLC OPERATIONS  
**ブランチ**: `feature/sound-library-planning`  
**ステータス**: **PLACEHOLDER**（AI-DLC 標準。将来の配布・監視・保守ワークフロー用）

---

## 1. 現時点の位置づけ

本アプリは**完全オフライン**の Unity モバイルアプリのため、クラウドのデプロイ／監視／インシデント対応は対象外。  
Construction（Build and Test）までが現行ワークフローの実質終端。Operations は以下を将来拡張する枠として残す。

| 将来項目 | 本プロジェクトでの読み替え |
|---|---|
| Deployment | 展示用／検証用の実機インストールビルド（APK / IPA） |
| Monitoring | 端末ログ・クラッシュ報告（現状は Editor Console / 実機手動） |
| Incident response | 展示中の不具合切り分け手順（未整備） |
| Maintenance | カタログ差し替え・ルール表更新・バージョン上げ |
| Production readiness | シーン配線・全件テスト・サイズ計測・Progression 配線の完了 |

---

## 2. 配布・展示の当面方針（確定済み要件のメモ）

- **オフラインのみ**。ユーザー間共有・サーバー同期なし。
- **任意展示**（2026-11-20〜12-02）: 音図鑑入りのインストール可能ビルドを目標（任意）。
- **必須展示**（2027-03-19〜22）: 別途確認。
- ビルド手順の正: `aidlc-docs/construction/build-and-test/build-instructions.md`  
- Phase C 差分・サイズ計測: `.../phase-c-u7-u8-addendum.md`

個人名・個人予定は運用メモに書かない（役割名とプロジェクトマイルストーンのみ）。

---

## 3. Construction 残タスク（運用着手前の推奨）

Operations を本格化する前に、Build and Test Phase C の未了を優先する。

1. MCP: `GeidaiLibrary` / `GeidaiCreate` シーン作成・Home 導線・Build Settings  
2. EditMode 全件（21 ファイル）再実行  
3. 本番カタログ 50〜100 音＋ビルドサイズ計測  
4. Game1 / Rec → `IProgressionService` 通知の本番配線  
5. （任意）Profiler・ストア署名

---

## 4. 役割ハンドオフ（運用時）

| 役割 | 運用寄りの作業 |
|---|---|
| 企画・デザイン | カタログ／解除表／見た目・文言の差し替え（コード不要）。手順: `docs/Sさん向けガイド.md` |
| 実装（基盤・統合） | ビルド・シーン配線・不具合修正・バージョン上げ |
| 実装（音声・音作り） | 素材品質・DSP／書き出し品質。音作りシーンは Fさん所有（2026-08-18） |
| 実装（ゲーム縦割り） | ③音並べ・④サウンドレスキューは Fさん。クリアイベントの Progression 契約は前本と合意。手順: `docs/Fさん向けガイド.md` |

---

## 5. プレースホルダ完了条件（本ステージ）

- [x] Operations が PLACEHOLDER であることと、オフライン／展示ビルドへの読み替えを文書化  
- [x] Construction 残タスクと参照先を明示  
- [ ] （将来）配布チェックリスト・展示当日ランブック・バージョン管理ルールの本文化

---

## 6. AI-DLC ワークフロー終端

Inception → Construction（U1〜U8 ＋ Build and Test）→ **Operations（本 PLACEHOLDER）** まで到達。  
追加のソフトウェア開発要求があれば、Workspace Detection から適応的に再開する。

---

## 7. ワークストリーム: Fさん導入ドキュメント（2026-08-19）

- ブランチ: `feature/onboarding-f-guide`
- Construction 完了: `docs/Fさん向けガイド.md`、README 役割表、Sさんガイド相互リンク
- 本ワークストリームにクラウドデプロイは無い。Operations は PLACEHOLDER のまま終端
- 配布・展示は §2 の既存方針。Fさんの着手は導入ガイドを正とする

