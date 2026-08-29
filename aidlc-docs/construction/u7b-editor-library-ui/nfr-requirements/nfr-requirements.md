# U7b — NFR Requirements

**ユニット**: U7b Editor & Library UI  
**作成**: 2026-08-29  
**回答**: Q1〜Q4 = 全 A  
**継承**: U7a NFR（オフライン・PII・Unlock AtomicFile・Query 16ms）  
**Infrastructure Design**: SKIP（オフライン・クラウド無し）

| ID | 内容 |
|---|---|
| NFR-U7B-01 | カタログ 100 件以下でフィルタ変更〜一覧再構築は体感即時。仮想化リストは導入しない（Q1=A）。ScrollRect 既存踏襲 |
| NFR-U7B-02 | 完全オフライン。端末外送信なし（ワークストリーム NFR-01） |
| NFR-U7B-03 | 属性・カタログ・Editor ログに PII を置かない。説明全文をログしない |
| NFR-U7B-04 | UnlockState は既存 AtomicFile＋破損時空フォールバックを維持（変更なし） |
| NFR-U7B-05 | Editor 保存は Validation 成功時のみ。失敗時は SO 非 Dirty・ウィンドウ内日本語メッセージ。例外は Dialog＋安全ログ（Q4=A） |
| NFR-U7B-06 | EditMode: フィルタ選択肢生成・placeholder 解決・画面用純粋ヘルパ。EditorWindow 本体は手動確認（必要なら最小 Editor テスト）。シーン配線後は Play Mode スモーク（Q3=A） |
| NFR-U7B-07 | プレイヤー UI は `HomeUiTheme`／既存 uGUI。Editor は IMGUI EditorWindow（Q2=A） |
| NFR-U7B-08 | WAV インポート失敗時は clipRef 未設定のまま保存拒否（必須 clip） |
| NFR-U7B-09 | 一覧・詳細・フィルタ操作でサーバー通信・課金・外部 API なし |

## Extension
| Extension | 適用 |
|---|---|
| Security | NFR-U7B-02/03/09 |
| Resiliency | NFR-U7B-04/05/08 |
| PBT | フィルタ選択肢・ヘルパの決定性を EditMode で担保（必要なら軽量 PBT）。Editor GUI 本体は対象外 |
