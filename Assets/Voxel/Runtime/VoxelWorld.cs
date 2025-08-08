using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
#if VOXEL_USE_UNITASK
using Cysharp.Threading.Tasks;
#endif
#if VOXEL_USE_ZLOGGER
using ZLogger;
#endif

public class VoxelWorld : MonoBehaviour
{
    public MaterialPalette palette;
    public float voxelSize = 0.5f;
    public int viewRadius = 1; // チャンク半径

    // ★ ここに任意のマテリアルを割り当てる（頂点カラー対応推奨）
    public Material defaultMaterial;
    
    [Header("地形生成設定")]
    public bool useProceduralTerrain = true;
    public int worldSeed = 12345;
    public TerrainGenerator.TerrainSettings terrainSettings = TerrainGenerator.TerrainSettings.Default;

    Dictionary<int3, VoxelChunk> chunks = new();

    void Start()
    {
        Application.targetFrameRate = 60;
        
        // チャンクを生成
        for (int y = -1; y <= 1; y++)  // 高さ方向を縮小
        for (int z = -viewRadius; z <= viewRadius; z++)
        for (int x = -viewRadius; x <= viewRadius; x++)
        {
            CreateChunk(new int3(x, y, z));
        }

        if (useProceduralTerrain)
        {
            // プロシージャル地形生成
            var chunkList = new List<KeyValuePair<int3, VoxelChunk>>(chunks);
            foreach (var kv in chunkList)
            {
                var ch = kv.Value;
                TerrainGenerator.GenerateChunk(ch, terrainSettings, worldSeed);
                ch.RebuildIfDirty();
            }
            
            // 構造物（木など）を生成
            var surfaceChunks = new List<KeyValuePair<int3, VoxelChunk>>();
            foreach (var kv in chunks)
            {
                if (kv.Key.y == 0) // 地表レベルのチャンクのみ
                {
                    surfaceChunks.Add(kv);
                }
            }
            
            foreach (var kv in surfaceChunks)
            {
                var biome = DetermineBiomeForChunk(kv.Key);
                TerrainGenerator.GenerateStructures(this, kv.Key, biome, worldSeed);
            }
        }
        else
        {
            // 従来のフラット地形生成
            var chunkList = new List<KeyValuePair<int3, VoxelChunk>>(chunks);
            foreach (var kv in chunkList)
            {
                var ch = kv.Value;
                var n = VoxelConst.ChunkSize;
                for (int z=0; z<n; z++)
                for (int y=0; y<n; y++)
                for (int x=0; x<n; x++)
                {
                    var worldY = y + ch.chunkCoord.y * n;
                    byte den = (byte)(worldY < n/2 ? 1 : 0);
                    ch.Set(x,y,z, new Voxel{ density=den, material=1});
                }
                ch.RebuildIfDirty();
            }
        }
    }
    
    TerrainGenerator.BiomeType DetermineBiomeForChunk(int3 chunkCoord)
    {
        var n = VoxelConst.ChunkSize;
        var worldX = chunkCoord.x * n + n/2;
        var worldZ = chunkCoord.z * n + n/2;
        
        float height = 0;
        float amplitude = 1f;
        float frequency = 1f;
        
        for (int i = 0; i < 4; i++)
        {
            float noiseValue = noise.snoise(new float2(
                worldX * terrainSettings.noiseScale * frequency,
                worldZ * terrainSettings.noiseScale * frequency
            ));
            height += noiseValue * amplitude;
            amplitude *= 0.5f;
            frequency *= 2f;
        }
        
        height = terrainSettings.baseHeight + height * terrainSettings.heightScale;
        
        // 温度と湿度
        float temperature = noise.snoise(new float2(worldX * 0.01f, worldZ * 0.01f));
        float humidity = noise.snoise(new float2(worldX * 0.01f + 1000, worldZ * 0.01f + 1000));
        
        if (height < terrainSettings.seaLevel - 5)
            return TerrainGenerator.BiomeType.Ocean;
        if (height > terrainSettings.baseHeight + 15)
            return TerrainGenerator.BiomeType.Mountains;
        if (temperature > 0.3f && humidity < -0.2f)
            return TerrainGenerator.BiomeType.Desert;
        if (humidity > 0.2f)
            return TerrainGenerator.BiomeType.Forest;
        
        return TerrainGenerator.BiomeType.Plains;
    }

    VoxelChunk CreateChunk(int3 coord)
    {
        if (chunks.ContainsKey(coord)) return chunks[coord];

        var go = new GameObject($"Chunk_{coord.x}_{coord.y}_{coord.z}");
        go.transform.SetParent(transform);
        var size = VoxelConst.ChunkSize * voxelSize;
        go.transform.position = new Vector3(coord.x*size, coord.y*size, coord.z*size);

        var ch = go.AddComponent<VoxelChunk>();
        var mr = go.GetComponent<MeshRenderer>();

        // ★ ここでInspector指定のマテリアルを使用（nullならフォールバックを作成）
        if (defaultMaterial != null)
        {
            mr.sharedMaterial = defaultMaterial; // shared にすることでチャンクごとのインスタンス増殖を回避
        }
        else
        {
            // Fallback: パイプラインに応じて適当なLitを探す
            var shader =
                Shader.Find("Universal Render Pipeline/Lit") ??
                Shader.Find("HDRP/Lit") ??
                Shader.Find("Standard");
            var fallback = new Material(shader) { enableInstancing = true };
            mr.sharedMaterial = fallback;
        }

        ch.Init(coord, palette, voxelSize);

        chunks.Add(coord, ch);
        return ch;
    }

    public bool TryGetChunk(int3 coord, out VoxelChunk c) => chunks.TryGetValue(coord, out c);

    public bool TryWorldToChunk(Vector3 world, out int3 chunk, out VoxelChunk c)
    {
        float size = VoxelConst.ChunkSize * voxelSize;
        int cx = Mathf.FloorToInt(world.x / size);
        int cy = Mathf.FloorToInt(world.y / size);
        int cz = Mathf.FloorToInt(world.z / size);
        chunk = new int3(cx,cy,cz);
        return chunks.TryGetValue(chunk, out c);
    }

    public void ApplyBrush(Vector3 worldPos, float radius, sbyte deltaDensity, byte material)
    {
        // 球ブラシ：半径内を埋める/削る（deltaDensity: +1埋める, -1削る）
        int3 ccoord; VoxelChunk ch;
        if (!TryWorldToChunk(worldPos, out ccoord, out ch)) return;

        var n = VoxelConst.ChunkSize;
        var r2 = radius*radius;

        // 近傍チャンクも巻き込む
        for (int dz=-1; dz<=1; dz++)
        for (int dy=-1; dy<=1; dy++)
        for (int dx=-1; dx<=1; dx++)
        {
            var cc = new int3(ccoord.x+dx, ccoord.y+dy, ccoord.z+dz);
            if (!chunks.TryGetValue(cc, out var cch)) continue;

            // ワールド→ローカルボクセルで領域走査
            var basePos = cch.transform.position;
            for (int z=0; z<n; z++)
            for (int y=0; y<n; y++)
            for (int x=0; x<n; x++)
            {
                var center = basePos + new Vector3((x+0.5f)*cch.voxelSize, (y+0.5f)*cch.voxelSize, (z+0.5f)*cch.voxelSize);
                var d2 = (center - worldPos).sqrMagnitude;
                if (d2 > r2) continue;

                var v = cch.Get(x,y,z);
                int den = v.density + deltaDensity;
                den = Mathf.Clamp(den, 0, 1);
                v.density = (byte)den;
                if (material!=255) v.material = material;
                cch.Set(x,y,z,v);
            }
            cch.RebuildIfDirty();
        }
    }
    
    // 指定位置のボクセルを取得
    public bool TryGetVoxelAt(Vector3 worldPos, out Voxel voxel)
    {
        voxel = default;
        
        if (!TryWorldToChunk(worldPos, out var chunkCoord, out var chunk))
            return false;
        
        var localPos = worldPos - chunk.transform.position;
        var n = VoxelConst.ChunkSize;
        
        int x = Mathf.FloorToInt(localPos.x / voxelSize);
        int y = Mathf.FloorToInt(localPos.y / voxelSize);
        int z = Mathf.FloorToInt(localPos.z / voxelSize);
        
        if (x < 0 || x >= n || y < 0 || y >= n || z < 0 || z >= n)
            return false;
        
        voxel = chunk.Get(x, y, z);
        return true;
    }
    
    // 指定位置にボクセルを設定
    public void SetVoxelAt(Vector3 worldPos, Voxel voxel)
    {
        if (!TryWorldToChunk(worldPos, out var chunkCoord, out var chunk))
        {
            // チャンクが存在しない場合は作成
            chunk = CreateChunk(chunkCoord);
        }
        
        var localPos = worldPos - chunk.transform.position;
        var n = VoxelConst.ChunkSize;
        
        int x = Mathf.FloorToInt(localPos.x / voxelSize);
        int y = Mathf.FloorToInt(localPos.y / voxelSize);
        int z = Mathf.FloorToInt(localPos.z / voxelSize);
        
        if (x >= 0 && x < n && y >= 0 && y < n && z >= 0 && z < n)
        {
            chunk.Set(x, y, z, voxel);
            chunk.RebuildIfDirty();
        }
    }
}
