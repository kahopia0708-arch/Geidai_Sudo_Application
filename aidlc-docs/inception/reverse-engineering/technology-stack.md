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

## 未確定（NFR/Construction で決定予定）
- 対象OS（iOS / Android / 両方）、最小OSバージョン
- ビルドターゲット・解像度/向き（縦/横）方針
