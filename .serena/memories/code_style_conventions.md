# コードスタイルと規約

## コーディング規約
`.editorconfig`ファイルでプロジェクト全体のコードスタイルを定義。

### 命名規則
- **private フィールド**: camelCase（接頭辞なし）
  - 例: `velocity`, `isJumping`, `playerCamera`
- **public フィールド**: camelCase
  - 例: `voxelSize`, `viewRadius`, `useProceduralTerrain`
- **クラス名**: PascalCase
  - 例: `VoxelWorld`, `PlayerController`, `GreedyMesher`
- **メソッド名**: PascalCase
  - 例: `CreateChunk()`, `RebuildIfDirty()`, `GenerateChunk()`

### コード品質ルール
- **修飾子**: private修飾子を暗黙的にする
- **this修飾子**: インスタンスメンバーでthisを削除
- **波括弧**: すべての単一ステートメントに波括弧必須
  - if-else、for、foreach、while、do-while、using、lock、fixed文

### ファイル構造規約
- **using文**: ファイル先頭に配置
- **コメント**: 日本語コメント使用（プロジェクト特徴）
- **フィールド順序**: public → protected → private
- **メソッド順序**: Unity Messages → public → private

### 属性とヘッダー使用
```csharp
[Header("移動設定")]
public float walkSpeed = 5f;
public float sprintSpeed = 8f;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
```

### Unity固有の規約
- **MonoBehaviour**: Unity Messagesの適切な使用 (Start, Update, OnDestroy など)
- **ScriptableObject**: 設定データはScriptableObjectで管理
- **NativeArray**: パフォーマンスが必要な箇所では適切にDispose