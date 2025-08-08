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

### 1. リポジトリをクローン
```bash
git clone git@github.com:yoshi32a/UnityDemo.git
cd UnityDemo
```

### 2. Unity Hubで開く
- Unity 6000.1.10f1がインストールされていることを確認
- プロジェクトをUnity Hubから開く

### 3. シーンセットアップ
Unity Editorで`Assets/Scenes/VoxelSampleScene.unity`を開き、以下のゲームオブジェクトを設定：

#### プレイヤーオブジェクトの作成
1. **Hierarchy** で右クリック → **Create Empty** → 名前を「Player」に変更
2. **Player** を選択して **Inspector** で：
   - **Add Component** → **Character Controller**
   - **Add Component** → **Player Controller**

#### VoxelWorldオブジェクトの作成
1. **Hierarchy** で右クリック → **Create Empty** → 名前を「VoxelWorld」に変更
2. **VoxelWorld** を選択して **Inspector** で：
   - **Add Component** → **Voxel World**
   - **Use Procedural Terrain** にチェック
   - **World Seed** を任意の数値に設定（地形生成用）

#### VoxelBrushオブジェクトの作成
1. **Hierarchy** で右クリック → **Create Empty** → 名前を「VoxelBrush」に変更
2. **VoxelBrush** を選択して **Inspector** で：
   - **Add Component** → **Voxel Brush**
   - **World** に VoxelWorld オブジェクトをドラッグ
   - **Player** に Player オブジェクトをドラッグ
   - **Use First Person** にチェック

#### ゲームUIの作成
1. **Hierarchy** で右クリック → **Create Empty** → 名前を「GameUI」に変更
2. **GameUI** を選択して **Inspector** で：
   - **Add Component** → **Voxel Game UI**
   - **Voxel Brush** に VoxelBrush オブジェクトをドラッグ
   - **Voxel World** に VoxelWorld オブジェクトをドラッグ

#### マテリアルパレットの設定
1. **Project** で右クリック → **Create** → **Voxel** → **Material Palette**
2. 名前を「VoxelMaterialPalette」に変更
3. **Inspector** でマテリアルエントリーを追加：
   - エントリー 1: 土（茶色 #8B4513）
   - エントリー 2: 草（緑色 #228B22）
   - エントリー 3: 石（灰色 #696969）
   - エントリー 4: 砂（黄色 #F4A460）
   - エントリー 5: 雪（白色 #FFFAFA）
   - エントリー 6: 木材（茶色 #DEB887）
   - エントリー 7: 葉（濃緑 #006400）
4. **VoxelWorld** の **Palette** フィールドにドラッグ

### 4. プレイモードで実行
- Unity Editorの **▶️ Play** ボタンをクリック
- **Game** ウィンドウをクリックしてフォーカスを当てる

## 操作方法

### 基本操作
| 操作 | アクション |
|------|-----------|
| **W/A/S/D** | 前後左右移動 |
| **マウス移動** | 視点操作（一人称視点） |
| **Space** | ジャンプ |
| **Left Shift** | ダッシュ（高速移動） |
| **ESC** | マウスカーソル表示切替 |

### ブロック編集
| 操作 | アクション |
|------|-----------|
| **左クリック** | ブロック破壊 |
| **右クリック** | ブロック配置 |
| **数字キー 1-9** | マテリアル切り替え |

### UI操作
| 操作 | アクション |
|------|-----------|
| **Tab** | インベントリ表示/非表示 |
| **F1** | ヘルプ表示切替 |

### マテリアル一覧
1. **土** - 基本的な建築ブロック
2. **草** - 地表の装飾
3. **石** - 堅固な建築材料
4. **砂** - 砂漠バイオーム用
5. **雪** - 山岳バイオーム用
6. **木材** - 木の幹部分
7. **葉** - 木の葉部分
8. **レンガ** - 装飾用建築材料
9. **ガラス** - 透明建築材料（将来実装予定）

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

### よくある問題と解決方法

| 問題 | 原因 | 解決方法 |
|------|------|----------|
| **ゲームが開始されない** | オブジェクトの設定不備 | 上記のセットアップ手順を再確認 |
| **マウス操作ができない** | フォーカスが当たっていない | Gameウィンドウをクリック、ESCキーでカーソル状態を確認 |
| **移動できない** | CharacterControllerが未設定 | PlayerオブジェクトにCharacterControllerを追加 |
| **ブロックが配置/破壊できない** | VoxelBrushの参照エラー | VoxelBrushのWorldとPlayerフィールドを正しく設定 |
| **マテリアルが紫色になる** | シェーダーエラー | CustomLitShader.shadergraphを再インポート |
| **地形が生成されない** | マテリアルパレット未設定 | VoxelMaterialPaletteを作成してVoxelWorldに設定 |
| **パフォーマンスが低い** | チャンク数が多すぎる | VoxelWorld.viewRadiusを2以下に設定 |
| **メモリ不足警告** | チャンクサイズが大きい | VoxelConst.ChunkSizeを16に変更 |
| **UIが表示されない** | Canvas未作成 | VoxelGameUIが自動でCanvasを作成するまで待機 |

### デバッグのヒント

1. **Console ウィンドウを確認**
   - Unity Editor → Window → General → Console
   - エラーメッセージがある場合は内容を確認

2. **Inspector での設定確認**
   - 各コンポーネントのフィールドがすべて設定されているか
   - null参照エラーが出ていないか

3. **プレイモード中の状態確認**
   - Sceneビューでプレイヤーの位置を確認
   - Gameビューでフォーカスが当たっているか確認

4. **パフォーマンス確認**
   - Window → Analysis → Profiler
   - FPSが30以下の場合はviewRadiusを削減

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