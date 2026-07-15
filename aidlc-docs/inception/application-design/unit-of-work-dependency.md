# Unit of Work Dependency（ユニット依存・実装順序）

**プロジェクト**: 藝大 須藤さんアプリ
**作成**: 2026-07-15 / AI-DLC Units Generation（Part 2）

---

## 1. 依存マトリクス（→ = 依存する）

| From \ To | U1 基盤 | U2 Foundation | U3 Rec | U4 Persist/Collection | U5 Theme | U6 Game1 |
|---|:---:|:---:|:---:|:---:|:---:|:---:|
| U1 基盤 | - | | | | | |
| U2 Foundation | → | - | | | | |
| U3 Rec | → | | - | | | |
| U4 Persist/Collection | → | | →(保存物) | - | | |
| U5 Theme | → | →(導線) | →(Rec遷移) | | - | |
| U6 Game1 | → | | | →(保存音取得) | | - |

- すべて **U1 基盤** に依存（UI基盤・サービス器・Common）。循環なし。
- U2 は U1 のみ。U3 は U1 のみ（保存は U1 の StorageService器を利用）。
- U4 は U3 の保存物を扱い、StorageService を堅牢化。U6 は U4 経由で保存音を取得。

## 2. 実装順序（Q3=A / 依存順・逐次）
```
U1 基盤 → U2 Foundation → U3 Rec → U4 Persistence/Collection → U5 weekly theme → U6 Game①音合わせ
```

## 3. StorageService の段階的強化（重要な調整点）
- **U1**: `StorageService` の IF＋**最小実装**（基本的な保存/読込）。→ U3 の保存が可能に。
- **U4**: 原子的保存・破損フォールバック・空フォールバックの**堅牢性本実装**（US-TECH-06 / US-COL-04 / NFR-07）。
- 保存フォーマット（`{id}.wav` ＋ `{id}.meta.json`）は U1 で確定し、U3/U4/U6 で共有。

## 4. コーディネーションポイント
- **保存フォーマット確定（U1）**: Rec/Collection/Game で共有するため最初に固定。
- **サービス IF 確定（U1）**: Navigation/Audio/Storage/Content/Pitch の署名を U1 で確定し、以降のユニットは IF に対して実装。
- **UI ハンドオフ（各ユニット完了時）**: 前本が枠組みを提供 → Sさん が Prefab/ScriptableObject/UITheme 上で詳細調整（US-TECH-07）。
- **テストチェックポイント**: U1（WavCodec/PitchMath の PBT）、U3（録音一本化の回帰）、U4（破損フォールバック）、U6（リアルタイム加工の実機性能）。

## 5. リスク/留意点
- **AsmDef 段階移行**: 既存 `Assembly-CSharp` からモジュール分割を U1 で開始。移行中のコンパイル整合に注意（Unity MCP でシーン/参照を確認）。
- **U3→U4 の保存互換**: U3 で保存した WAV/メタを U4 の堅牢化後も読めるよう、フォーマットは U1 で確定。
- **フェーズ境界**: U1〜U4＝フェーズA、U5〜U6＝フェーズB。中間報告（2026-11）に向けフェーズA優先。
