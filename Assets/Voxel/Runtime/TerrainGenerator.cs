using Unity.Mathematics;
using UnityEngine;

public static class TerrainGenerator
{
    // 地形生成パラメータ
    public struct TerrainSettings
    {
        public float noiseScale;       // ノイズのスケール
        public float heightScale;       // 高さのスケール
        public int baseHeight;          // 基準高さ
        public float caveThreshold;    // 洞窟生成の閾値
        public int seaLevel;           // 海面レベル
        
        public static TerrainSettings Default => new TerrainSettings
        {
            noiseScale = 0.05f,
            heightScale = 20f,
            baseHeight = 16,
            caveThreshold = 0.4f,
            seaLevel = 12
        };
    }
    
    // バイオームタイプ
    public enum BiomeType
    {
        Plains,     // 平原
        Desert,     // 砂漠
        Forest,     // 森林
        Mountains,  // 山岳
        Ocean      // 海洋
    }
    
    // チャンクのボクセルデータを生成
    public static void GenerateChunk(VoxelChunk chunk, TerrainSettings settings, int seed)
    {
        var n = VoxelConst.ChunkSize;
        var chunkWorldPos = chunk.chunkCoord * n;
        
        // シード値でランダムを初期化
        var random = new Unity.Mathematics.Random((uint)(seed + chunk.chunkCoord.GetHashCode()));
        
        for (int z = 0; z < n; z++)
        for (int x = 0; x < n; x++)
        {
            var worldX = chunkWorldPos.x + x;
            var worldZ = chunkWorldPos.z + z;
            
            // 高さマップを生成
            float height = GenerateHeight(worldX, worldZ, settings);
            
            // バイオームを決定
            var biome = GetBiome(worldX, worldZ, height, settings);
            
            for (int y = 0; y < n; y++)
            {
                var worldY = chunkWorldPos.y + y;
                var voxel = GenerateVoxel(worldX, worldY, worldZ, height, biome, settings, random);
                chunk.Set(x, y, z, voxel);
            }
        }
    }
    
    // 高さを生成
    static float GenerateHeight(int x, int z, TerrainSettings settings)
    {
        float height = 0;
        
        // 複数のオクターブを重ねて自然な地形を生成
        float amplitude = 1f;
        float frequency = 1f;
        
        for (int i = 0; i < 4; i++)
        {
            float noiseValue = noise.snoise(new float2(
                x * settings.noiseScale * frequency,
                z * settings.noiseScale * frequency
            ));
            height += noiseValue * amplitude;
            
            amplitude *= 0.5f;
            frequency *= 2f;
        }
        
        return settings.baseHeight + height * settings.heightScale;
    }
    
    // バイオームを決定
    static BiomeType GetBiome(int x, int z, float height, TerrainSettings settings)
    {
        // 温度と湿度のノイズマップ
        float temperature = noise.snoise(new float2(x * 0.01f, z * 0.01f));
        float humidity = noise.snoise(new float2(x * 0.01f + 1000, z * 0.01f + 1000));
        
        // 高さベースの基本バイオーム
        if (height < settings.seaLevel - 5)
            return BiomeType.Ocean;
        
        if (height > settings.baseHeight + 15)
            return BiomeType.Mountains;
        
        // 温度と湿度でバイオームを決定
        if (temperature > 0.3f && humidity < -0.2f)
            return BiomeType.Desert;
        
        if (humidity > 0.2f)
            return BiomeType.Forest;
        
        return BiomeType.Plains;
    }
    
    // ボクセルを生成
    static Voxel GenerateVoxel(int x, int y, int z, float surfaceHeight, BiomeType biome, TerrainSettings settings, Unity.Mathematics.Random random)
    {
        // 空気
        if (y > surfaceHeight)
            return new Voxel { density = 0, material = 0 };
        
        // 洞窟生成
        float caveNoise = Generate3DNoise(x, y, z, 0.05f);
        if (caveNoise > settings.caveThreshold && y < surfaceHeight - 5)
            return new Voxel { density = 0, material = 0 };
        
        // 地表のマテリアルを決定
        byte material = GetMaterial(y, surfaceHeight, biome, settings);
        
        return new Voxel { density = 1, material = material };
    }
    
    // マテリアルIDを取得
    static byte GetMaterial(int y, float surfaceHeight, BiomeType biome, TerrainSettings settings)
    {
        float depth = surfaceHeight - y;
        
        // バイオーム別の表層マテリアル
        if (depth < 1)
        {
            switch (biome)
            {
                case BiomeType.Desert:
                    return 4; // 砂
                case BiomeType.Forest:
                case BiomeType.Plains:
                    return 2; // 草
                case BiomeType.Mountains:
                    return y > 30 ? 5 : 1; // 雪または石
                case BiomeType.Ocean:
                    return 4; // 砂（海底）
            }
        }
        
        // 中層
        if (depth < 4)
        {
            switch (biome)
            {
                case BiomeType.Desert:
                    return 4; // 砂
                default:
                    return 1; // 土
            }
        }
        
        // 深層
        return 3; // 石
    }
    
    // 3Dノイズ生成（洞窟用）
    static float Generate3DNoise(int x, int y, int z, float scale)
    {
        return noise.snoise(new float3(x * scale, y * scale, z * scale));
    }
    
    // 構造物生成（木、建物など）
    public static void GenerateStructures(VoxelWorld world, int3 chunkCoord, BiomeType biome, int seed)
    {
        var random = new Unity.Mathematics.Random((uint)(seed + chunkCoord.GetHashCode() * 2));
        
        switch (biome)
        {
            case BiomeType.Forest:
                GenerateTrees(world, chunkCoord, random, 5);
                break;
            case BiomeType.Plains:
                GenerateTrees(world, chunkCoord, random, 1);
                break;
        }
    }
    
    // 木を生成
    static void GenerateTrees(VoxelWorld world, int3 chunkCoord, Unity.Mathematics.Random random, int treeCount)
    {
        var chunkSize = VoxelConst.ChunkSize;
        var worldBase = chunkCoord * chunkSize;
        
        for (int i = 0; i < treeCount; i++)
        {
            int localX = random.NextInt(4, chunkSize - 4);
            int localZ = random.NextInt(4, chunkSize - 4);
            
            // 地表の高さを探す
            for (int y = chunkSize - 1; y >= 0; y--)
            {
                var worldPos = new Vector3(
                    worldBase.x + localX,
                    worldBase.y + y,
                    worldBase.z + localZ
                );
                
                if (world.TryGetVoxelAt(worldPos, out Voxel voxel) && voxel.density > 0)
                {
                    // 木を生成
                    GenerateTree(world, new Vector3(worldPos.x, worldPos.y + 1, worldPos.z), random);
                    break;
                }
            }
        }
    }
    
    // 単体の木を生成
    static void GenerateTree(VoxelWorld world, Vector3 basePos, Unity.Mathematics.Random random)
    {
        int height = random.NextInt(4, 7);
        byte woodMaterial = 6;  // 木材
        byte leafMaterial = 7;  // 葉
        
        // 幹
        for (int y = 0; y < height; y++)
        {
            world.SetVoxelAt(basePos + Vector3.up * y, new Voxel { density = 1, material = woodMaterial });
        }
        
        // 葉（球状）
        int leafRadius = 2;
        var leafCenter = basePos + Vector3.up * (height - 1);
        
        for (int x = -leafRadius; x <= leafRadius; x++)
        for (int y = -leafRadius; y <= leafRadius; y++)
        for (int z = -leafRadius; z <= leafRadius; z++)
        {
            if (x * x + y * y + z * z <= leafRadius * leafRadius)
            {
                var pos = leafCenter + new Vector3(x, y, z);
                world.SetVoxelAt(pos, new Voxel { density = 1, material = leafMaterial });
            }
        }
    }
}