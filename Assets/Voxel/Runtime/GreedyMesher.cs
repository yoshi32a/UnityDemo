using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

static class GreedyMesher
{
    // 近傍が空気の面だけを張る（Minecraft系の基本）。
    // “同一素材”の面を貫通統合してポリゴン数を削減（Greedy）
    public static void BuildMesh(VoxelChunk chunk, ref Mesh mesh)
    {
        var n = VoxelConst.ChunkSize;
        var size = chunk.voxelSize;
        var vox = chunk.Voxels;

        var vertices = new List<Vector3>(8192);
        var colors   = new List<Color>(8192);
        var triangles= new List<int>(16384);

        // 3軸
        int3[] axes = { new int3(1,0,0), new int3(0,1,0), new int3(0,0,1) };
        for (int axis=0; axis<3; axis++)
        {
            int u = (axis+1)%3; // 面内1軸
            int v = (axis+2)%3; // 面内2軸

            // 正面/背面の2枚
            for (int dir = -1; dir <= 1; dir+=2)
            {
                for (int w=0; w<=n; w++)
                {
                    // mask: n*n, 0空気/ >0 同一素材ID
                    byte[,] idMask = new byte[n,n];

                    for (int j=0;j<n;j++)
                    for (int i=0;i<n;i++)
                    {
                        int3 a = int3.zero;
                        a[axis] = math.clamp(w-1,0,n-1);
                        a[u] = i; a[v] = j;

                        int3 b = int3.zero;
                        b[axis] = math.clamp(w,0,n-1);
                        b[u] = i; b[v] = j;

                        bool insideA = w>0;
                        bool insideB = w<n;

                        byte idA = 0, idB = 0; byte denA=0, denB=0;
                        if (insideA)
                        {
                            var va = vox[a.x + n*(a.y + n*a.z)];
                            denA = va.density; idA = va.material;
                        }
                        if (insideB)
                        {
                            var vb = vox[b.x + n*(b.y + n*b.z)];
                            denB = vb.density; idB = vb.material;
                        }

                        // 片側が実体、片側が空気の境界だけ面を張る
                        bool face = (denA>0) != (denB>0);
                        if (!face) { idMask[i,j]=0; continue; }

                        // 面側の素材IDを採用
                        byte id = denA>0 ? idA : idB;
                        idMask[i,j] = (byte)math.max(1,id); // 0は空気扱いにするため1..に寄せる
                    }

                    // Greedyで長方形にまとめてQuad生成
                    bool[,] used = new bool[n,n];
                    for (int j=0;j<n;j++)
                    for (int i=0;i<n;i++)
                    {
                        if (used[i,j] || idMask[i,j]==0) continue;
                        byte id0 = idMask[i,j];

                        int wLen=1;
                        while (i+wLen<n && !used[i+wLen,j] && idMask[i+wLen,j]==id0) wLen++;
                        int hLen=1; bool can=true;
                        while (j+hLen<n && can)
                        {
                            for (int k=0;k<wLen;k++)
                                if (used[i+k,j+hLen] || idMask[i+k,j+hLen]!=id0) { can=false; break; }
                            if (can) hLen++;
                        }
                        for (int y=0;y<hLen;y++) for (int x=0;x<wLen;x++) used[i+x,j+y]=true;

                        // Quad作成（※ローカル座標で頂点を作る）
                        float3 p = 0;
                        p[axis] = w*size;
                        p[u] = i*size;
                        p[v] = j*size;

                        float3 du = 0; du[u] = wLen*size;
                        float3 dv = 0; dv[v] = hLen*size;

                        int vbase = vertices.Count;

                        Vector3 v0 = (Vector3)p;
                        Vector3 v1 = (Vector3)(p + du);
                        Vector3 v2 = (Vector3)(p + du + dv);
                        Vector3 v3 = (Vector3)(p + dv);

                        if (dir>0)
                        {
                            vertices.AddRange(new[]{v0,v1,v2,v3});
                            triangles.AddRange(new[]{vbase, vbase+1, vbase+2, vbase, vbase+2, vbase+3});
                        }
                        else
                        {
                            vertices.AddRange(new[]{v3,v2,v1,v0});
                            triangles.AddRange(new[]{vbase, vbase+1, vbase+2, vbase, vbase+2, vbase+3});
                        }

                        // 色は素材パレットから
                        var mat = chunk.palette.Get((byte)(id0-1));
                        var col = new Color(mat.baseColor.x, mat.baseColor.y, mat.baseColor.z, 1);
                        colors.Add(col); colors.Add(col); colors.Add(col); colors.Add(col);
                    }
                }
            }
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetColors(colors);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
