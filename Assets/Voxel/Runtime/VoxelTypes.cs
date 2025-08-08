using Unity.Mathematics;

// 1ボクセルあたりのデータ。密度は0=空気, 1=実体(簡易)。必要ならsbyteやfloat密度に。
public struct Voxel
{
    public byte density;  // 0 or 1（拡張: 0..255）
    public byte material; // 素材ID
}
// チャンクの寸法（正立方）
public static class VoxelConst
{
    public const int ChunkSize = 32; // 16/32/64など
    public const int MaxVertsPerChunk = ChunkSize * ChunkSize * ChunkSize * 6 * 6; // 最悪値（過剰）。必要に応じて縮小
}

// 素材パレット参照用
public struct VoxelMaterial
{
    public float4 baseColor;  // 表示用（URPのVertexColorに乗せるなど）
    public float hardness;    // 破壊コスト等
}
