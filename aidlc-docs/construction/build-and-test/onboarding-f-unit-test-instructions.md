# Unit Test Execution — Fさん導入ドキュメント

C# の新規ユニットテストは無い。文書の単体検査で代替する。

## Checks

1. ガイドが参照する既存パスがディスク上に存在する（これから作る `GeidaiGame3` は除く）
2. `docs/Fさん向けガイド.md` に個人の連絡先（LINE、メールアドレス、電話）が無い
3. 担当表が `onboarding-f-requirements.md` §4 と一致する（③④・音作り＝Fさん、①②・Library＝前本）

## Execution (2026-08-19)

- 既存 `Assets/` 参照パス: すべて存在
- Settings 8 アセットのうちガイド記載 6 種: すべて存在
- PII: Fさんガイドはヒットなし。Sさんガイド既存の LINE 一文は本ワークストリームで変更していない
- 担当表: 要件と一致
