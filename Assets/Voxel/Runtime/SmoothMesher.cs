using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// バナンザ風の滑らかな地形メッシュを生成
/// Marching Cubesアルゴリズムを使用してボクセルデータから滑らかなメッシュを作成
/// </summary>
public static class SmoothMesher
{
    // Marching Cubesの頂点補間
    static Vector3 VertexInterp(float isolevel, Vector3 p1, Vector3 p2, float v1, float v2)
    {
        if (math.abs(isolevel - v1) < 0.00001f) return p1;
        if (math.abs(isolevel - v2) < 0.00001f) return p2;
        if (math.abs(v1 - v2) < 0.00001f) return p1;
        
        float mu = (isolevel - v1) / (v2 - v1);
        return Vector3.Lerp(p1, p2, mu);
    }
    
    // 密度値から滑らかな値を計算（周囲のボクセルを考慮）
    static float GetSmoothDensity(VoxelChunk chunk, int x, int y, int z)
    {
        var n = VoxelConst.ChunkSize;
        
        if (x < 0 || x >= n || y < 0 || y >= n || z < 0 || z >= n)
            return 0;
            
        var voxel = chunk.Get(x, y, z);
        return voxel.density > 0 ? 1.0f : 0.0f; // 簡単化：0 or 1
    }
    
    public static void BuildSmoothMesh(VoxelChunk chunk, ref Mesh mesh)
    {
        // 一旦GreedyMesherにフォールバック（隙間問題を解決するため）
        GreedyMesher.BuildMesh(chunk, ref mesh);
        return;
        
        /* 隙間修正後に再実装予定 - 現在コメントアウト中
        var n = VoxelConst.ChunkSize;
        var size = chunk.voxelSize;
        
        var vertices = new List<Vector3>(8192);
        var normals = new List<Vector3>(8192);
        var colors = new List<Color>(8192);
        var triangles = new List<int>(16384);
        
        // 簡易版Marching Cubes実装
        float isolevel = 0.5f; // 表面のしきい値（0〜255の範囲で128→0.5に変更）
        
        // まず隙間を解決するため step=1 に戻す
        int step = 1;
        for (int x = 0; x < n - step; x += step)
        for (int y = 0; y < n - step; y += step)
        for (int z = 0; z < n - step; z += step)
        {
            // 8頂点の密度値を取得
            float[] densities = new float[8];
            Vector3[] corners = new Vector3[8];
            byte materialId = 0;
            
            for (int i = 0; i < 8; i++)
            {
                int dx = (i & 1) * step;
                int dy = ((i & 2) >> 1) * step;
                int dz = ((i & 4) >> 2) * step;
                
                corners[i] = new Vector3((x + dx) * size, (y + dy) * size, (z + dz) * size);
                densities[i] = GetSmoothDensity(chunk, x + dx, y + dy, z + dz);
                
                if (densities[i] > 0)
                {
                    var voxel = chunk.Get(x + dx, y + dy, z + dz);
                    if (voxel.material > 0) materialId = voxel.material;
                }
            }
            
            // キューブ内に表面がない場合はスキップ
            bool hasInside = false;
            bool hasOutside = false;
            for (int i = 0; i < 8; i++)
            {
                if (densities[i] > isolevel) hasInside = true;
                if (densities[i] <= isolevel) hasOutside = true;
            }
            
            if (!hasInside || !hasOutside) continue;
            
            // 簡易的なメッシュ生成（6面の中心点を結ぶ）
            if (materialId > 0 && materialId < chunk.palette.entries.Count)
            {
                var color = chunk.palette.entries[materialId].color;
                
                // 立方体の中心点
                Vector3 center = new Vector3((x + 0.5f) * size, (y + 0.5f) * size, (z + 0.5f) * size);
                
                // 6面それぞれについて三角形を生成（隣接面が空の場合のみ）
                for (int face = 0; face < 6; face++)
                {
                    // 隣接するボクセルをチェック
                    int nx = x, ny = y, nz = z;
                    switch (face)
                    {
                        case 0: nz += 1; break; // 前面
                        case 1: nz -= 1; break; // 背面  
                        case 2: nx += 1; break; // 右面
                        case 3: nx -= 1; break; // 左面
                        case 4: ny += 1; break; // 上面
                        case 5: ny -= 1; break; // 下面
                    }
                    
                    // 隣接が空気でない場合はスキップ
                    if (nx >= 0 && nx < n && ny >= 0 && ny < n && nz >= 0 && nz < n)
                    {
                        var neighborVoxel = chunk.Get(nx, ny, nz);
                        if (neighborVoxel.density > 0) continue; // 隣接が実体なら面は不要
                    }
                    
                    Vector3 normal = Vector3.zero;
                    Vector3[] faceVerts = new Vector3[4];
                    
                    switch (face)
                    {
                        case 0: // 前面 (Z+)
                            normal = Vector3.forward;
                            faceVerts[0] = corners[4];
                            faceVerts[1] = corners[5];
                            faceVerts[2] = corners[7];
                            faceVerts[3] = corners[6];
                            break;
                        case 1: // 背面 (Z-)
                            normal = Vector3.back;
                            faceVerts[0] = corners[0];
                            faceVerts[1] = corners[2];
                            faceVerts[2] = corners[3];
                            faceVerts[3] = corners[1];
                            break;
                        case 2: // 右面 (X+)
                            normal = Vector3.right;
                            faceVerts[0] = corners[1];
                            faceVerts[1] = corners[3];
                            faceVerts[2] = corners[7];
                            faceVerts[3] = corners[5];
                            break;
                        case 3: // 左面 (X-)
                            normal = Vector3.left;
                            faceVerts[0] = corners[0];
                            faceVerts[1] = corners[4];
                            faceVerts[2] = corners[6];
                            faceVerts[3] = corners[2];
                            break;
                        case 4: // 上面 (Y+)
                            normal = Vector3.up;
                            faceVerts[0] = corners[2];
                            faceVerts[1] = corners[6];
                            faceVerts[2] = corners[7];
                            faceVerts[3] = corners[3];
                            break;
                        case 5: // 下面 (Y-)
                            normal = Vector3.down;
                            faceVerts[0] = corners[0];
                            faceVerts[1] = corners[1];
                            faceVerts[2] = corners[5];
                            faceVerts[3] = corners[4];
                            break;
                    }
                    
                    // 面の密度をチェック
                    float faceDensity = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        int idx = 0;
                        for (int j = 0; j < 8; j++)
                        {
                            if (Vector3.Distance(corners[j], faceVerts[i]) < 0.001f)
                            {
                                faceDensity += densities[j];
                                break;
                            }
                        }
                    }
                    faceDensity /= 4;
                    
                    // 表面に近い面だけ描画
                    if (math.abs(faceDensity - isolevel) < 100)
                    {
                        // 面の頂点を少しノイズで歪ませる
                        for (int i = 0; i < 4; i++)
                        {
                            float noise = Mathf.PerlinNoise(faceVerts[i].x * 0.5f, faceVerts[i].z * 0.5f) * 0.1f;
                            faceVerts[i] += normal * noise * size;
                        }
                        
                        // 頂点を追加
                        int baseIdx = vertices.Count;
                        vertices.AddRange(faceVerts);
                        
                        // 法線を追加（スムージング用に後で計算）
                        for (int i = 0; i < 4; i++)
                        {
                            normals.Add(normal);
                            colors.Add(color);
                        }
                        
                        // 三角形インデックス
                        triangles.Add(baseIdx);
                        triangles.Add(baseIdx + 1);
                        triangles.Add(baseIdx + 2);
                        
                        triangles.Add(baseIdx);
                        triangles.Add(baseIdx + 2);
                        triangles.Add(baseIdx + 3);
                    }
                }
            }
        }
        
        // スムーズな法線を計算
        if (vertices.Count > 0)
        {
            var smoothNormals = CalculateSmoothNormals(vertices, triangles);
            normals = smoothNormals;
        }
        
        // Debug.Log($"SmoothMesher: Generated {vertices.Count} vertices, {triangles.Count/3} triangles");
        
        // メッシュ更新
        mesh.Clear();
        if (vertices.Count > 0)
        {
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            // Debug.Log($"SmoothMesher: Mesh updated successfully");
        }
        else
        {
            Debug.LogWarning("SmoothMesher: No vertices generated - falling back to GreedyMesher");
            GreedyMesher.BuildMesh(chunk, ref mesh);
        }
        */
    }
    
    // スムーズな法線を計算
    static List<Vector3> CalculateSmoothNormals(List<Vector3> vertices, List<int> triangles)
    {
        var normals = new List<Vector3>(vertices.Count);
        for (int i = 0; i < vertices.Count; i++)
        {
            normals.Add(Vector3.zero);
        }
        
        // 各三角形の法線を計算して頂点に加算
        for (int i = 0; i < triangles.Count; i += 3)
        {
            int i0 = triangles[i];
            int i1 = triangles[i + 1];
            int i2 = triangles[i + 2];
            
            Vector3 v0 = vertices[i0];
            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            
            Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
            
            normals[i0] = normals[i0] + normal;
            normals[i1] = normals[i1] + normal;
            normals[i2] = normals[i2] + normal;
        }
        
        // 正規化
        for (int i = 0; i < normals.Count; i++)
        {
            normals[i] = normals[i].normalized;
        }
        
        return normals;
    }
}