using UnityEngine;

public class ResourceDropper : MonoBehaviour
{
    public ResourceType[] possibleDrops;
    public float[] dropChances; // 0-1の確率
    public int[] dropAmounts;   // ドロップ量
    
    ResourceInventory playerInventory;
    
    void Start()
    {
        playerInventory = FindFirstObjectByType<ResourceInventory>();
    }
    
    public void OnBlockDestroyed(Vector3 position, byte materialId)
    {
        if (possibleDrops == null || possibleDrops.Length == 0) return;
        
        // マテリアルIDに応じた資源をドロップ
        int dropIndex = GetDropIndexForMaterial(materialId);
        if (dropIndex >= 0 && dropIndex < possibleDrops.Length)
        {
            if (UnityEngine.Random.value <= dropChances[dropIndex])
            {
                var resourceType = possibleDrops[dropIndex];
                int amount = dropAmounts[dropIndex];
                
                if (playerInventory)
                {
                    playerInventory.AddResource(resourceType, amount);
                    ShowPickupEffect(position, resourceType, amount);
                }
            }
        }
    }
    
    int GetDropIndexForMaterial(byte materialId)
    {
        // マテリアルIDに応じてドロップする資源を決定
        return materialId switch
        {
            1 => 0, // 土 -> 土資源
            2 => 1, // 草 -> 種
            3 => 2, // 石 -> 石材
            4 => 3, // 砂 -> 砂
            5 => 4, // 雪 -> 氷
            6 => 5, // 木材 -> 木材
            7 => 6, // 葉 -> 種
            _ => -1
        };
    }
    
    void ShowPickupEffect(Vector3 position, ResourceType resourceType, int amount)
    {
        // パーティクルや浮上テキストを表示
        Debug.Log($"取得: {resourceType.name} x{amount}");
        
        // TODO: パーティクルエフェクトやUI表示を追加
    }
}