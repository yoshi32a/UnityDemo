# コードベース構造

## プロジェクトルート構造
```
/mnt/c/Users/myunp/workspace/Demo/
├── Assets/                          # Unity アセットルート
│   ├── Voxel/                      # ボクセルシステムのコア
│   ├── Scenes/                     # シーンファイル
│   ├── Settings/                   # URP設定・プロファイル
│   ├── Resources/                  # リソースファイル
│   └── UI Toolkit/                 # UI要素
├── ProjectSettings/                # Unity プロジェクト設定
├── Packages/                      # Package Manager管理
├── CLAUDE.md                      # 開発指針ドキュメント
├── README.md                      # プロジェクト説明
└── .editorconfig                  # コードスタイル設定
```

## Voxelシステム構造 (`Assets/Voxel/`)
```
Assets/Voxel/
├── Runtime/                       # ランタイムスクリプト
│   ├── VoxelWorld.cs             # ワールド管理・チャンクシステム
│   ├── VoxelChunk.cs             # 個別チャンク管理
│   ├── GreedyMesher.cs           # メッシュ生成アルゴリズム
│   ├── SmoothMesher.cs           # スムースメッシング（別実装）
│   ├── VoxelBrush.cs             # 地形編集ツール
│   ├── VoxelTypes.cs             # データ構造定義
│   ├── MaterialPallet.cs          # マテリアル定義
│   ├── PlayerController.cs        # プレイヤー制御
│   ├── TerrainGenerator.cs        # プロシージャル地形生成
│   ├── ResourceInventory.cs       # インベントリシステム
│   ├── ConstructionManager.cs     # 建築システム
│   ├── DailyTaskManager.cs        # デイリータスク
│   ├── TimeSystem.cs             # 時間システム
│   └── VoxelGameUIToolkit.cs     # UIシステム
├── Editor/                       # エディター拡張
│   └── VoxelGameSetup.cs         # セットアップウィザード
├── Resources/                    # マテリアルアセット
│   ├── 土.asset                  # 各種マテリアル設定
│   ├── 石材.asset
│   └── [その他マテリアル]
├── UI/                          # UI定義ファイル
│   ├── VoxelGameUI.uxml         # UI構造定義
│   └── VoxelGameUI.uss          # UIスタイル定義
├── CustomLitShader.shadergraph   # カスタムシェーダー
└── VoxelMaterialPalette.asset   # メインマテリアルパレット
```

## シーン構造
- **VoxelSampleScene.unity**: メインゲームシーン
- **VoxelSample2.unity**: テスト用シーン

## 設定ファイル (`Assets/Settings/`)
- **DefaultVolumeProfile.asset**: URP ボリューム設定
- **PC_RPAsset.asset**: PC用レンダーパイプライン設定
- **Mobile_RPAsset.asset**: モバイル用レンダーパイプライン設定
- **UniversalRenderPipelineGlobalSettings.asset**: グローバルURP設定