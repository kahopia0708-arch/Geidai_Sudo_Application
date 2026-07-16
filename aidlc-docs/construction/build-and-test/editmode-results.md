# EditMode 実行結果

**日時**: 2026-07-16T11:55:45+09:00  
**実行**: `Geidai/Tests/Run EditMode All`（`GeidaiTestRunner` / Unity MCP）  
**結果**: **Passed** — pass=85 / fail=0 / skip=0 / inconclusive=0

## 初回失敗→修正
| テスト | 原因 | 対応 |
|---|---|---|
| `AtomicFileTests.Invalid_Path_Fails_And_Preserves_Existing_Value` | 想定 IO 失敗時の `SafeLogger.Error` が LogAssert 未期待 | `LogAssert.Expect` 追加 |
| `NavigationRoutingTests.GoTo_Theme_IsMapped_NotNotFound` | EditMode で `LoadScene` 不可→Error ログ | `LogAssert.Expect` 追加 |
| `NavigationRoutingTests.GoTo_DoesNotThrow_OnMappedScene` | 同上（2回） | `LogAssert.Expect`×2 |

## 再実行方法
- Editor: `Geidai/Tests/Run EditMode All`
- CLI（Editor 閉じてから）: `unit-test-instructions.md` 方法B
- サマリ出力: `Logs/editmode-summary.txt`（gitignore）
