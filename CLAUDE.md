# CLAUDE.md

このファイルは、Claude Code (claude.ai/code) がこのリポジトリのコードを扱う際のガイダンスを提供します。

## プロジェクト概要

Unity 6000.1.10f1を使用したボクセルデモプロジェクトです。Universal Render Pipeline (URP 17.1.0)を採用し、Minecraftスタイルのボクセル地形システムとリアルタイム編集機能を実装しています。

## プロジェクト構造

### コアシステム

#### ボクセルシステム (`Assets/Voxel/Runtime/`)
チャンクベースの地形生成とレンダリングを実装するプロジェクトの中核システム：

- **VoxelWorld.cs**: チャンクを辞書(`Dictionary<int3, VoxelChunk>`)で管理するメインワールドコントローラー。チャンク生成、ブラシ適用、ワールド座標からチャンク座標への変換を処理。

- **VoxelChunk.cs**: 個別チャンク管理（デフォルト32x32x32ボクセル）。パフォーマンス向上のためUnityのJob SystemとBurstコンパイルを使用。ボクセルデータ用の永続的NativeArraysを管理し、ダーティフラグでメッシュ再構築をトリガー。

- **GreedyMesher.cs**: 同一マテリアルの隣接面をマージしてポリゴン数を削減するグリーディメッシングアルゴリズムを実装。マテリアルパレットに基づいて頂点カラーを生成。

- **VoxelBrush.cs**: 地形編集用の入力ハンドラー。左クリックでボクセル削除（密度-1）、右クリックでボクセル追加（密度+1）。数字キー1-6でマテリアル切り替え。

- **VoxelTypes.cs**: `Voxel`構造体（密度+マテリアルID）と定数を含むコアデータ構造。

- **MaterialPallet.cs**: 色とプロパティを持つボクセルマテリアルを定義するScriptableObject。

### アーキテクチャノート

1. **チャンクベースシステム**: ワールドは32x32x32ボクセルのチャンクに分割。各チャンクは独自のGameObjectにMeshFilter、MeshRenderer、MeshColliderを持つ。

2. **メモリ管理**: ボクセルデータにPersistentアロケータ付きUnityのNativeArraysを使用。OnDestroyで適切に破棄。

3. **レンダリング**: カスタムシェーダー(`CustomLitShader.shadergraph`)で頂点カラーをサポート。カスタムマテリアル未割り当て時は標準のURP/HDRP Litシェーダーにフォールバック。

4. **パフォーマンス**: 
   - グリーディメッシングでポリゴン数削減
   - Burstコンパイル対応（属性付与済み）
   - メッシュコライダーはチャンク再構築時のみ更新
   - マテリアルインスタンスの増殖を防ぐため共有マテリアルを使用

## 開発コマンド

### Unity Editor操作
- **プレイモード**: Unity Editorでプレイモードに入りボクセル操作をテスト
- **シーン**: メインシーンは`Assets/Scenes/VoxelSampleScene.unity`

### ビルド設定
- **ターゲットプラットフォーム**: PC/Mac/Linux Standalone向けに設定
- **グラフィックスAPI**: URPでリニアカラースペース
- **入力システム**: Unityの新Input Systemパッケージを使用

### ボクセルシステムのテスト
1. Unity Editorでプレイモードに入る
2. マウスで地形を狙う
3. 左クリックでボクセル削除
4. 右クリックでボクセル追加
5. 数字キー1-6でマテリアル切り替え

## 重要な技術的考慮事項

### パフォーマンス最適化ポイント
- チャンクサイズ（VoxelConst.ChunkSize）はメモリ使用量と再構築パフォーマンスに影響
- グリーディメッシャーはドローコールを削減するが、再構築時のCPU使用量は増加
- プロダクション環境では遠方チャンク用のLODシステムを検討

### 拡張ポイント
- **Voxel構造体**: 現在はbyte密度（0/1）を使用。より滑らかな地形のためsbyte/floatへ拡張可能
- **マテリアルシステム**: MaterialPaletteにテクスチャインデックス、法線マップなどを追加可能
- **ワールド生成**: 現在はフラット地形を生成。VoxelWorld.Start()にノイズベース生成を追加
- **永続化**: セーブ/ロードシステム未実装。ワールド永続化のためチャンクシリアライゼーションを実装

### Input System設定
プロジェクトはUnityの新Input Systemを使用。アクションは`Assets/InputSystem_Actions.inputactions`で定義。Project SettingsでInput Systemパッケージが正しく設定されていることを確認。

### シェーダー要件
デフォルトマテリアルは適切なマテリアル表示のため頂点カラーをサポートする必要がある。付属のCustomLitShader.shadergraphはこの目的で設定済み。

## よくある問題と解決策

1. **マテリアルが紫色になる**: シェーダーコンパイルの問題。CustomLitShader.shadergraphを再インポート
2. **プレイモードで操作できない**: VoxelBrushコンポーネントがアクティブでCamera.mainが存在することを確認
3. **パフォーマンス問題**: VoxelWorld.viewRadiusでチャンク表示半径を削減
4. **メモリ警告**: VoxelConst.ChunkSizeを削減するか、遠方チャンクのアンロードを実装

## Git設定
- mainブランチでリポジトリ初期化済み
- Unity専用.gitignore設定済み
- ユーザー: yoshi32a (yoshi32.spl@gmail.com)
- リモート: git@github.com:yoshi32a/UnityDemo.git