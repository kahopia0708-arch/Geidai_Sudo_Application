# Unity 6.6 アップグレードメモ

## UAC0020（`com.unity.ai.assistant`）

Unity 6.6 から **UAC アナライザ**が `Assembly.Load(byte[])` をエラーにする。  
古い `com.unity.ai.assistant`（例: 2.9.0-pre.2）の `DynamicAssemblyBuilder.cs` が該当。

### 対応（本リポジトリ）

- `Packages/manifest.json` を **`com.unity.ai.assistant`: `2.21.0-pre.2`** に更新済み
- Unity を開き直すと Package Manager が再解決する（初回はダウンロードに時間がかかる）

### まだエラーが出る場合

1. Unity を終了
2. `Library/PackageCache/com.unity.ai.assistant@*` を削除（または `Library` ごと再生成）
3. プロジェクトを再オープン

### MCP を使わない場合

`manifest.json` から `com.unity.ai.assistant` 行を**削除**してよい（ゲーム本体・iOS ビルドには不要）。

## 参照

- [Incompatible .NET API (6000.6)](https://docs.unity.com/en-us/engine/6000.6/manual/programming-environment/overview-of-dot-net-in-unity/dotnet-incompatible-api)
- Unity Discussions: AI Assistant incompatible with Unity 6.6 alpha（UAC0020）
