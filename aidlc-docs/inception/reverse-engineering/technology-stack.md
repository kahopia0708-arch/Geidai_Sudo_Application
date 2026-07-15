# Technology Stack

## Programming Languages
- **C#** — Unity スクリプト（`Assets/Scripts/`）

## Engine / Framework
- **Unity** 6000.4.2f1（Unity 6）
- **Universal Render Pipeline (URP)** 17.4.0
- **uGUI (com.unity.ugui)** 2.0.0 — UI（Button/Slider/Dropdown/InputField/ScrollRect）
- **TextMesh Pro** — テキスト表示（`TMPro`）
- **Input System (com.unity.inputsystem)** 1.19.0
- **Visual Scripting** 1.9.11（導入済み・利用有無は未確認）
- **Timeline** 1.8.12
- **2D パッケージ群**（animation/aseprite/psdimporter/sprite/spriteshape/tilemap 等）

## Audio（標準モジュール）
- `com.unity.modules.audio` — `Microphone`, `AudioSource`, `AudioClip`, `AudioLowPassFilter`/`HighPassFilter`/`ReverbFilter`/`EchoFilter`/`DistortionFilter`
- カスタムDSP: `OnAudioFilterRead`（`RecorderWithEffects`）

## Persistence
- **ファイルベース**: `System.IO` によるWAV/JSON書き込み、`JsonUtility`（`SoundEffectSettings`のシリアライズ）
- 保存先: `Application.persistentDataPath/MySoundCollection`

## Build Tools
- **Unity Editor** 6000.4.2f1（ビルド）
- **IDE**: Rider（`com.unity.ide.rider` 3.0.39）/ Visual Studio（`2.0.27`）

## Testing Tools
- **Unity Test Framework** 1.6.0（導入済み・**テストコード未整備**）

## Version Control / Collab
- Git（本リポジトリ）
- `com.unity.collab-proxy` 2.12.4

## 開発規約 / ツール方針（Development Conventions）
- **Unity MCP（unityMCP）**: Unity のシーン／GameObject／プレハブ／アセット操作、コンソール読み取り、テスト実行は Unity 標準 MCP サーバー経由で行う。AI/自動化からのシーン変更は MCP を通し、変更内容は PR・変更メモに残す（NFR-10 変更管理と整合）。手作業のシーン編集と併用可。
- **UI レスポンシブ方針（NFR-11/NFR-12）**: 全 Canvas で CanvasScaler = Scale With Screen Size を採用。縦・横 両対応。SafeArea 追従コンポーネントを各画面ルートに新設。参照解像度・Match 値は両対応向けに設計段階で統一（現状は 1920×1080・Match 0.5 の横基準）。固定ピクセルレイアウト（`ScrollRectSnapLoop` の itemWidth 等）は相対指定へ見直し。
- 関連 ProjectSettings: `defaultScreenOrientation: 4`（AutoRotation・4方向許可）、`androidRenderOutsideSafeArea: 1`。

## 未確定（NFR/Construction で決定予定）
- 対象OS（iOS / Android / 両方）、最小OSバージョン
- ビルドターゲット・解像度/向き（縦/横）方針
