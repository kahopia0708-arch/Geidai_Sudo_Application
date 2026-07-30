# Unit of Work Dependency（ユニット依存・実装順序）

**プロジェクト**: 藝大 音響教育アプリ
**作成**: 2026-07-15 / AI-DLC Units Generation（Part 2）
**更新**: 2026-07-30 / U7・U8 追加

---

## 1. 依存マトリクス（→ = 依存する）

| From \ To | U1 | U2 | U3 | U4 | U5 | U6 | U7 | U8 |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| U1 | - | | | | | | | |
| U2 | → | - | | | | | | |
| U3 | → | | - | | | | | |
| U4 | → | | → | - | | | | |
| U5 | → | → | → | | - | | | |
| U6 | → | | | → | | - | | |
| U7 | → | | | → | | | - | |
| U8 | → | | | → | | | → | - |

- 循環なし。U7/U8 は Rec/Collection UI に直接依存しない。
- U3/U6 → Progression（U1）はイベント通知のみ（逆依存なし）。

## 2. 実装順序
```
[完了] U1 → U2 → U3 → U4 → U5 → U6
[フェーズC] U1/U4 IF拡張 → U7 Library → U8 Create → 導線/展示ビルド
```

## 3. Storage / Progression の段階
- **既存**: ユーザー録音 WAV＋meta、プロファイル。
- **フェーズC**: `unlock-state.json`、`recipes/{id}.json`、任意 `exports/`。
- UnlockEvaluator は純粋関数として U1 Common に置き、U7/U8/ゲームから共有。

## 4. コーディネーションポイント
- **共通契約先決め**: CuratedSoundId / UnlockRule / SoundRecipe / Progression イベント。
- **UIハンドオフ**: 基盤・統合が枠組み → 企画・デザインが詳細調整（US-TECH-07）。
- **音響境界**: Create のDSPは合意済み AudioService IF 内（US-TECH-09）。
- **テスト**: Unlock 冪等 PBT、Recipe JSON 往復、2音再生の実機性能、50〜100音の容量測定。

## 5. リスク/留意点
- 解除条件表の未確定はデータ差し替え可能な UnlockRulesCatalog で吸収。
- 加工パラメータ範囲は U8 Functional Design で確定。
- 個人情報・個人予定は計画・Issue・試用記録に載せない（NFR-17）。
