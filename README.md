# Unity ボクセルデモ

Minecraftスタイルのボクセル地形システムを実装したUnityデモプロジェクトです。リアルタイムでの地形編集が可能で、効率的なメッシュ生成により高パフォーマンスを実現しています。

## 機能

- 🎮 **リアルタイム地形編集**: マウス操作でボクセルの追加・削除が可能
- 🎨 **マテリアルシステム**: 複数のマテリアルタイプに対応（土、石、草など）
- ⚡ **最適化されたレンダリング**: グリーディメッシングアルゴリズムによるポリゴン数削減
- 🏗️ **チャンクベース管理**: 効率的なメモリ使用とスケーラビリティ

## 動作環境

- **Unity バージョン**: 6000.1.10f1
- **レンダーパイプライン**: Universal Render Pipeline (URP) 17.1.0
- **プラットフォーム**: PC/Mac/Linux Standalone
- **入力システム**: Unity Input System

## セットアップ

1. リポジトリをクローン
```bash
git clone git@github.com:yoshi32a/UnityDemo.git
cd UnityDemo
```

2. Unity Hubで開く
   - Unity 6000.1.10f1がインストールされていることを確認
   - プロジェクトをUnity Hubから開く

3. プレイモードで実行
   - Unity Editorで`Assets/Scenes/VoxelSampleScene.unity`を開く
   - プレイボタンを押して実行

## 操作方法

| 操作 | アクション |
|------|-----------|
| **左クリック** | ボクセルを削除 |
| **右クリック** | ボクセルを追加 |
| **数字キー 1-6** | マテリアル切り替え |
| **マウス移動** | 視点操作 |

## プロジェクト構造

```
Assets/
├── Voxel/                    # ボクセルシステムのコア
│   ├── Runtime/              # ランタイムスクリプト
│   │   ├── VoxelWorld.cs     # ワールド管理
│   │   ├── VoxelChunk.cs     # チャンク管理
│   │   ├── GreedyMesher.cs   # メッシュ生成
│   │   ├── VoxelBrush.cs     # 編集ツール
│   │   ├── VoxelTypes.cs     # データ構造
│   │   └── MaterialPallet.cs # マテリアル定義
│   ├── CustomLitShader.shadergraph  # カスタムシェーダー
│   └── VoxelMaterialPallet.asset    # マテリアル設定
├── Scenes/                   # シーンファイル
│   └── VoxelSampleScene.unity
└── Settings/                 # URP設定
```

## 技術詳細

### チャンクシステム
- 各チャンクは32×32×32のボクセルで構成
- 動的なチャンク生成と管理
- メモリ効率を考慮したNativeArray使用

### グリーディメッシング
- 同一マテリアルの隣接面を結合
- ドローコール削減による描画パフォーマンス向上
- 頂点カラーによるマテリアル表現

### パフォーマンス最適化
- Unity Job SystemとBurst Compilerの活用
- 共有マテリアルによるメモリ使用量削減
- 必要時のみメッシュコライダー更新

## カスタマイズ

### チャンクサイズの変更
`Assets/Voxel/Runtime/VoxelTypes.cs`の`ChunkSize`定数を編集：
```csharp
public const int ChunkSize = 32; // 16, 64などに変更可能
```

### マテリアルの追加
1. `Assets/Voxel/VoxelMaterialPallet.asset`を選択
2. Inspectorで新しいエントリを追加
3. 色と硬度を設定

### ワールド生成のカスタマイズ
`VoxelWorld.cs`の`Start()`メソッドを編集して、ノイズベースの地形生成などを実装可能

## トラブルシューティング

| 問題 | 解決方法 |
|------|----------|
| マテリアルが紫色になる | CustomLitShader.shadergraphを再インポート |
| 操作が効かない | VoxelBrushコンポーネントとCamera.mainの存在を確認 |
| パフォーマンスが低い | VoxelWorld.viewRadiusを小さくする |
| メモリ不足警告 | ChunkSizeを小さくするか、遠方チャンクのアンロードを実装 |

## 今後の拡張予定

- [ ] セーブ/ロード機能の実装
- [ ] ノイズベースの地形生成
- [ ] LODシステムの導入
- [ ] マルチプレイヤー対応
- [ ] より多様なブロックタイプ
- [ ] 水や流体のシミュレーション

## ライセンス

このプロジェクトは学習・デモンストレーション目的で作成されています。

## 開発者

- **作成者**: yoshi32a
- **連絡先**: yoshi32.spl@gmail.com
- **GitHub**: [@yoshi32a](https://github.com/yoshi32a)

## 貢献

バグ報告や機能提案は[Issues](https://github.com/yoshi32a/UnityDemo/issues)からお願いします。

プルリクエストも歓迎します！

---

*このプロジェクトはUnity 6とUniversal Render Pipelineの学習を目的として作成されました。*