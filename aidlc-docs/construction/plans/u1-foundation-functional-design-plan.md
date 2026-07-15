# U1 基盤 — Functional Design Plan（機能設計 計画）

**プロジェクト**: 藝大 須藤さんアプリ
**ユニット**: U1 基盤（UI基盤 ＋ Services器 ＋ Common）
**作成**: 2026-07-15 / AI-DLC CONSTRUCTION / Functional Design（Part 1: Planning）
**入力**: `../../inception/application-design/unit-of-work.md`、`unit-of-work-story-map.md`、`../../inception/application-design/component-methods.md`、`../../inception/requirements/requirements.md`
**含むストーリー**: US-TECH-01, US-TECH-02, US-TECH-04, US-TECH-05, US-TECH-07

> 本ステージは**技術非依存の業務ロジック/ドメインモデル/業務ルール**を詳細化する。UI の技術パラメータ（CanvasScaler の参照解像度・Match 値、SafeArea 実装方式）は **NFR Design** で扱う。
> **回答方法**: 各質問の `[Answer]:` に記号（例: `A`）を記入してください。合う選択肢が無ければ「Other」。各質問に「(推奨)」案あり。完了で「done」（または「全部推奨で」）。

---

## A. 実行チェックリスト（Part 2 で実行）
- [ ] `../u1-foundation/functional-design/domain-entities.md` を生成（Common のドメインモデル定義）
- [ ] `../u1-foundation/functional-design/business-logic-model.md` を生成（サービス器の振る舞い・データフロー）
- [ ] `../u1-foundation/functional-design/business-rules.md` を生成（検証・制約・エラー方針）
- [ ] `../u1-foundation/functional-design/frontend-components.md` を生成（UI基盤の構造・ライフサイクル・ハンドオフ）
- [ ] 要件（FR/NFR）・ストーリーとのトレース整合を確認

## B. スコープ（U1 で確定する対象）
- **ドメインモデル**: UserProfile / SoundClipMeta / SoundEffectSettings / SavedSound / AudioBuffer
- **純粋関数**: WavCodec（encode/decode）、PitchMath（cents↔ratio）
- **サービス器**: AppManager / NavigationService / StorageService(最小) / AudioService(器) / ContentService(器)
- **UI基盤**: ScreenRootBase / SafeAreaFitter / ResponsiveCanvasConfigurator / UITheme（振る舞い・ライフサイクルのみ。数値パラメータは NFR Design）

---

## C. 設計に関する質問（Q1〜Q6）

## Question 1
UserProfile の検証ルール（業務ルール）は？

A) (推奨) 生まれた年=1900〜今年（未来年は不可）、ニックネーム=前後空白除去して1〜12文字（空不可、絵文字許容）。不正時は保存拒否＋その場で通知

B) 生まれた年=ドロップダウン選択（未来年を選べない）、ニックネーム=1〜8文字

C) 子ども配慮で生年の代わりに「年齢帯」を選択（未就学/小学生/中学生以上）

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 2
保存の ID 採番とファイルレイアウトは？

A) (推奨) id=GUID。`sounds/{guid}.wav` ＋ `sounds/{guid}.meta.json`（メタ＝SoundClipMeta＋SoundEffectSettings）、`profile.json` 単一。片方欠損時は該当項目を読み飛ばし

B) id=タイムスタンプ（yyyyMMdd_HHmmss、衝突時連番）

C) メタを単一インデックス（collection.json）に集約し、WAV はファイル参照

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 3
SoundEffectSettings の保持値・範囲・既定は？

A) (推奨) ピッチ=±12半音（既定0）／ノイズ低減=0・弱・中・強（既定0）／音色=なし・ロボット・コーラス系（既定なし）／リバーブ=0〜1（既定0）／各エフェクトのバイパス既定=off

B) ピッチをセント（±1200cents）で保持（内部一貫のため）

C) 具体値は研究会後に確定するため、当面は上記を「暫定既定」として実装（差し替え可能に）

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 4
録音/WAV 仕様（WavCodec の前提）は？

A) (推奨) 44100Hz・モノラル・16bit・3秒固定（=132,300サンプル）。ステレオ入力はモノラル化して保存

B) 48000Hz・モノラル・16bit・3秒固定

C) 端末デフォルトのサンプルレートに追従し、メタに実サンプルレートを記録

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 5
SceneId 列挙と初期遷移フローは？

A) (推奨) SceneId = { Main, Home, Rec, Collection, WeeklyTheme, SoundMatchGame }（Place は含めない）。初回=Main→登録→Home、登録済み=Main→Home。登録は Home 内フローで表示

B) 登録を専用 SceneId（Registration）として分離し、初回=Main→Registration→Home

D) Other (please describe after [Answer]: tag below)

[Answer]: 

## Question 6
エラー/失敗時のユーザー提示方針（子ども配慮 / SECURITY-15）は？

A) (推奨) 失敗はアイコン＋短い平易な文言（＋必要なら音）で通知。詳細/PII は内部ログにも出さない。致命的でない失敗は自動フォールバック（例: 破損項目は読み飛ばし）

B) テキストダイアログ主体で明示的に通知

D) Other (please describe after [Answer]: tag below)

[Answer]: 
