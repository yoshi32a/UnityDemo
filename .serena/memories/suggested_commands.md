# 開発コマンド

## Unity Editor操作

### プロジェクト開始
```bash
# リポジトリクローン
git clone git@github.com:yoshi32a/UnityDemo.git
cd Demo

# Unity Hubからプロジェクトを開く（Unity 6000.1.10f1必須）
```

### シーン操作
- **メインシーン**: `Assets/Scenes/VoxelSampleScene.unity`を開く
- **テストシーン**: `Assets/Scenes/VoxelSample2.unity`を開く
- **プレイモード**: Unity Editor の ▶️ Play ボタンまたは Ctrl+P (Cmd+P)

### ゲーム操作（プレイモード中）
```
W/A/S/D     - 前後左右移動
マウス移動    - 視点操作（一人称視点）
Space       - ジャンプ  
Left Shift  - ダッシュ（高速移動）
ESC         - マウスカーソル表示切替

左クリック    - ブロック破壊
右クリック    - ブロック配置
数字キー1-9   - マテリアル切り替え

Tab         - インベントリ表示/非表示
F1          - ヘルプ表示切替
```

## デバッグ・トラブルシューティング

### Unity Editor
```
Window → General → Console     - エラーログ確認
Window → Analysis → Profiler   - パフォーマンス確認
Edit → Project Settings        - プロジェクト設定確認
```

### よく使うデバッグ設定
- **Application.targetFrameRate = 60**: FPS設定（VoxelWorld.Start()）
- **VoxelWorld.viewRadius**: チャンク表示範囲調整（パフォーマンス調整）
- **VoxelConst.ChunkSize**: チャンクサイズ変更（メモリ使用量調整）

## Git操作
```bash
git status                    # ファイル状態確認
git add .                     # 全変更をステージング
git commit -m "コミットメッセージ"   # コミット作成  
git push origin main          # リモートにプッシュ
git pull origin main          # リモートから取得
```

## Linux システムコマンド
```bash
ls -la                       # ファイル一覧表示
cd [directory]               # ディレクトリ移動
find . -name "*.cs"          # C#ファイル検索
grep -r "VoxelWorld" .       # テキスト検索
```

## パフォーマンス最適化
- **チャンク数削減**: VoxelWorld.viewRadius を 1-2 に設定
- **チャンクサイズ調整**: VoxelTypes.cs の ChunkSize を 16 または 32 に設定
- **メッシュコライダー最適化**: 必要なチャンクのみ更新
- **マテリアル共有**: デフォルトマテリアルを適切に設定