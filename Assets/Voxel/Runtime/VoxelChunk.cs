using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class VoxelChunk : MonoBehaviour
{
    public MaterialPalette palette;
    public int3 chunkCoord; // ワールド→チャンク座標
    public float voxelSize = 0.5f;

    NativeArray<Voxel> voxels; // length: ChunkSize^3
    bool isInitialized;
    bool dirty;

    Mesh mesh;
    MeshFilter mf;
    MeshCollider mc;
    
    // TerrainGeneratorからアクセス可能にする
    public NativeArray<Voxel> Voxels => voxels;

    public void Init(int3 coord, MaterialPalette pal, float voxel)
    {
        if (isInitialized) return;
        chunkCoord = coord;
        palette = pal;
        voxelSize = voxel;

        var count = VoxelConst.ChunkSize * VoxelConst.ChunkSize * VoxelConst.ChunkSize;
        voxels = new NativeArray<Voxel>(count, Allocator.Persistent, NativeArrayOptions.ClearMemory);

        mf = GetComponent<MeshFilter>();
        mc = GetComponent<MeshCollider>();
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // 大きめ対応
        mf.sharedMesh = mesh;

        isInitialized = true;
        dirty = true;
    }

    void OnDestroy()
    {
        if (voxels.IsCreated) voxels.Dispose();
        if (mesh != null) Destroy(mesh);
    }

    public Voxel Get(int x, int y, int z)
    {
        var n = VoxelConst.ChunkSize;
        return voxels[x + n*(y + n*z)];
    }

    public void Set(int x, int y, int z, Voxel v)
    {
        var n = VoxelConst.ChunkSize;
        voxels[x + n*(y + n*z)] = v;
        dirty = true;
    }

    public void Fill(Voxel v)
    {
        for (int i=0;i<voxels.Length;i++) voxels[i]=v;
        dirty = true;
    }

    public Bounds GetBounds()
    {
        var s = VoxelConst.ChunkSize * voxelSize;
        return new Bounds(transform.position + new Vector3(s*0.5f, s*0.5f, s*0.5f), new Vector3(s,s,s));
    }

    public void RebuildIfDirty()
    {
        if (!dirty) return;
        dirty = false;
        GreedyMesher.BuildMesh(this, ref mesh);
        mc.sharedMesh = null; // 再アサインで更新
        mc.sharedMesh = mesh;
    }

    // ワールド→ローカルボクセル座標
    public bool WorldToLocalVoxel(Vector3 world, out int3 v)
    {
        var local = world - transform.position;
        int x = (int)math.floor(local.x / voxelSize);
        int y = (int)math.floor(local.y / voxelSize);
        int z = (int)math.floor(local.z / voxelSize);
        var n = VoxelConst.ChunkSize;
        v = new int3(x,y,z);
        return x>=0 && y>=0 && z>=0 && x<n && y<n && z<n;
    }

    public ref NativeArray<Voxel> Voxels => ref voxels;
}
