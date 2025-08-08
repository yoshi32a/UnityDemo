using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
#if VOXEL_USE_UNITASK
using Cysharp.Threading.Tasks;
#endif
#if VOXEL_USE_ZLOGGER
using ZLogger;
#endif

class VoxelWorld : MonoBehaviour
{
    public MaterialPalette palette;
    public float voxelSize = 0.5f;
    public int viewRadius = 2; // チャンク半径

    // ★ ここに任意のマテリアルを割り当てる（頂点カラー対応推奨）
    public Material defaultMaterial;

    Dictionary<int3, VoxelChunk> chunks = new();

    void Start()
    {
        Application.targetFrameRate = 60;
        // 原点に数チャンク用意（デモ）
        for (int z=-viewRadius; z<=viewRadius; z++)
        for (int x=-viewRadius; x<=viewRadius; x++)
        {
            CreateChunk(new int3(x,0,z));
        }

        // サンプル: 台地を生成
        foreach (var kv in chunks)
        {
            var ch = kv.Value;
            var n = VoxelConst.ChunkSize;
            for (int z=0; z<n; z++)
            for (int y=0; y<n; y++)
            for (int x=0; x<n; x++)
            {
                var worldY = y + ch.chunkCoord.y * n;
                byte den = (byte)(worldY < n/2 ? 1 : 0);
                ch.Set(x,y,z, new Voxel{ density=den, material=1}); // 1=土（パレットのIDに合わせて調整可）
            }
            ch.RebuildIfDirty();
        }
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
}
