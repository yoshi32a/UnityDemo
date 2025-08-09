# 開発ガイドライン

## タスク完了時の必須チェック

### 1. 動作テスト
- Unity Editorでプレイモードに入る
- Game ウィンドウをクリックしてフォーカス確認
- 基本操作（W/A/S/D、マウス、クリック）をテスト
- エラーが Console に出ていないか確認

### 2. パフォーマンス確認
- Profiler (Window → Analysis → Profiler) でCPU使用率確認
- FPSが安定して30以上あることを確認
- メモリ使用量が異常に高くないか確認

### 3. コードスタイル確認
- `.editorconfig` の規約に従っているか確認
- private フィールドの命名がcamelCaseか確認
- 波括弧がすべての制御文についているか確認
- 日本語コメントが適切に記述されているか確認

## 座標系とワールド変換

### Unity座標系
- **右手座標系**: X右、Y上、Z前方
- **ワールド座標**: Unity空間での絶対座標
- **ローカル座標**: 各チャンクの相対座標

### ボクセル座標変換
```csharp
// ワールド座標からチャンク座標への変換
int3 chunkPos = VoxelWorld.WorldToChunk(worldPos);

// ワールド座標からローカルボクセル座標への変換  
int3 localVoxelPos = VoxelWorld.WorldToLocalVoxel(worldPos);
```

## 重要な設計パターン

### 1. チャンクベース管理
- 各チャンクは32×32×32のボクセルで構成
- Dictionary<int3, VoxelChunk>でチャンクを管理
- 必要時のみメッシュ再構築（ダーティフラグ使用）

### 2. メモリ管理
```csharp
// NativeArrayは必ずDisposeする
if (voxelData.IsCreated)
    voxelData.Dispose();
```

### 3. Unity固有の注意点
- **MonoBehaviour**: Start/Update/OnDestroyの適切な使用
- **ScriptableObject**: 設定データの永続化
- **RequireComponent**: 依存コンポーネントの明示
- **Header**: Inspectorでの見やすい分類

## コードレビューチェックリスト
- [ ] null参照チェックが適切に実装されている
- [ ] メモリリークの原因となるNativeArrayが適切にDispose される
- [ ] パフォーマンスに影響するUpdate処理が最小限に抑えられている
- [ ] Unity Messagesが適切なタイミングで呼ばれている
- [ ] 日本語コメントで処理の意図が明確に記述されている

## バグ修正時の着眼点
- **座標変換エラー**: ワールド座標とローカル座標の混同
- **メッシュ更新問題**: ダーティフラグの設定漏れ
- **入力システム**: 新Input Systemの設定不備
- **マテリアル問題**: パレットの設定やシェーダーコンパイルエラー
- **パフォーマンス**: チャンク数やメッシュ更新頻度の最適化