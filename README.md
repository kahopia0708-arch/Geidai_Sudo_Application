# おと（藝大 須藤さんアプリ）

「音」から始まる、耳のためのアプリケーション（東京藝大アートDX 学内公募研究）。  
録音 → 加工 → 自分の音で「聴き分ける」ミニゲーム（①音合わせ）までを、**完全オフライン**の Unity モバイルアプリとして実装しています。

| 項目 | 内容 |
|---|---|
| 表示名 | `おと` |
| Package ID | `jp.geidai.sudo.oto` |
| エンジン | Unity `6000.4.2f1`（Unity 6 / URP） |
| 対象 | iOS 15+ / Android API 25+（ARM64） |
| ブランチ | `feature/project-foundation`（Construction 一式） |

---

## 役割分担

| 担当 | やること |
|---|---|
| **Sさん** | 企画・音素材・**画面の見た目調整**・お題/パラメータ差し替え・試用フィードバック |
| **前本** | 実装ロジック・画面の骨組み・ナビ/録音/保存・ビルド環境・GitHub |

見た目の変え方は **[docs/Sさん向けガイド.md](docs/Sさん向けガイド.md)** を参照してください。

---

## まず触る（開発者）

1. Unity Hub で本リポジトリを開き、**Unity 6000.4.2f1** で起動する  
2. Build Settings で `Main画面` が先頭であることを確認  
3. Play → ホーム（`GeidaiHome`）から各機能へ遷移できることを確認  

### 主なシーン

| シーン | 役割 |
|---|---|
| `Assets/Main画面.unity` | 起動（Boot） |
| `Assets/Scenes/Geidai/GeidaiHome.unity` | ホーム |
| `GeidaiRegister` / `GeidaiRec` / `GeidaiCollection` / `GeidaiTheme` / `GeidaiGame1` | 設定・録音・コレクション・今週のお題・①音合わせ |
| `Assets/game_Home.unity` | ゲーム選択（既存 UI → Game1 へ） |

### メニュー（Editor）

| メニュー | 用途 |
|---|---|
| `Geidai/Scenes/Build All Geidai Scenes` | シーン骨組みの再生成（**見た目は上書きされる**） |
| `Geidai/Build/Android Development APK` | Android 開発用 APK（`Builds/Android/Oto.apk`） |
| `Geidai/Build/iOS Xcode Project` | iOS Xcode プロジェクト（`Builds/iOS/`） |
| `Geidai/Tests/Run EditMode All` | EditMode テスト（結果: `Logs/editmode-summary.txt`） |

---

## ドキュメント案内

| 場所 | 内容 |
|---|---|
| [docs/Sさん向けガイド.md](docs/Sさん向けガイド.md) | **デザイン変更・お題/設定の差し替え手順**（Sさん向け） |
| `aidlc-docs/` | AI-DLC の要件・設計・Construction 記録（実装者向け） |
| `aidlc-docs/construction/build-and-test/` | ビルド・テスト・実機確認チェックリスト |
| 企画の正（Google Drive） | `プロジェクト概要.md` および `input/`（リポジトリには複製しない） |

---

## 現状ステータス（要約）

- Construction（U1〜U6）コード生成完了  
- Geidai 実シーン配線・導線ホットフィックス完了  
- EditMode **85 Pass / 0 Fail**  
- iOS / Android 実機: 録音・再生 OK、縦横レイアウト崩れなし（2026-07-16）  
- 意匠の仕上げ・イラスト差し替えは Sさん 作業（本ガイド参照）  
- 性能計測（Profiler）・ストア署名は任意フォローアップ  

詳細は `aidlc-docs/aidlc-state.md` と `aidlc-docs/construction/build-and-test/device-verification-checklist.md`。

---

## 注意

- **オフライン専用**（サーバー通信・課金・Place 共有は MVP 対象外）  
- `Geidai/Scenes/Build All Geidai Scenes` は骨組み再生成用。Sさんがシーン上で直した見た目が消えます  
- 企画・仕様の最新は Google Drive の資料を正とする（`.cursor/rules/project-reference.mdc`）
