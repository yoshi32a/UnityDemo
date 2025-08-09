# 技術スタック

## Unity環境
- **Unityバージョン**: 6000.1.10f1
- **エディターバージョン**: 6000.1.10f1 (3c681a6c22ff)
- **レンダーパイプライン**: Universal Render Pipeline (URP) 17.1.0
- **プラットフォーム**: PC/Mac/Linux Standalone

## 主要パッケージ
- **com.unity.render-pipelines.universal**: 17.1.0 (URP)
- **com.unity.inputsystem**: 1.14.0 (新Input System)
- **com.unity.ugui**: 2.0.0 (UI システム)
- **com.unity.test-framework**: 1.5.1 (テストフレームワーク)
- **com.unity.ai.navigation**: 2.0.8 (ナビゲーション)

## 技術要素
- **プログラミング言語**: C#
- **レンダリング**: Universal Render Pipeline + カスタムシェーダー
- **入力システム**: Unity Input System
- **メッシュ生成**: グリーディメッシングアルゴリズム
- **パフォーマンス最適化**: Unity Job System + Burst Compiler (対応準備済み)
- **メモリ管理**: NativeArray (Persistentアロケータ)

## プロジェクト設定
- **カラースペース**: Linear
- **グラフィックスAPI**: DirectX 11/Vulkan (Windows), Metal (Mac), OpenGL (Linux)
- **フレームレート**: 60 FPS (Application.targetFrameRate)