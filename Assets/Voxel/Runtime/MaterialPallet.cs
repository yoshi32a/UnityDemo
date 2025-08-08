using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "Voxel/MaterialPalette")]
class MaterialPalette : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string name;
        public Color color = Color.gray;
        [Range(0.1f, 10f)] public float hardness = 1f;
    }

    public List<Entry> entries = new();

    public VoxelMaterial Get(byte id)
    {
        if (id >= entries.Count) return new VoxelMaterial{ baseColor=new float4(1,0,1,1), hardness=1};
        var e = entries[id];
        return new VoxelMaterial{ baseColor = new float4(e.color.r,e.color.g,e.color.b,1), hardness = e.hardness};
    }
}
