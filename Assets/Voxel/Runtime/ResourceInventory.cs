using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceType
{
    public string name;
    public string description;
    public Color iconColor = Color.white;
    public Sprite icon;
    public int maxStack = 99;
    
    [Header("取得設定")]
    public float baseHarvestTime = 1f; // 基本収集時間
    public int baseAmount = 1; // 基本取得量
    
    [Header("価値")]
    public int baseValue = 1; // 基本価値
    public bool isRare = false; // レア資源か
}

[System.Serializable]
public class ResourceStack
{
    public ResourceType resourceType;
    public int amount;
    
    public ResourceStack(ResourceType type, int amount)
    {
        this.resourceType = type;
        this.amount = amount;
    }
    
    public bool IsEmpty => amount <= 0;
    public bool IsFull => amount >= resourceType.maxStack;
    
    public int AddAmount(int addAmount)
    {
        int canAdd = Mathf.Min(addAmount, resourceType.maxStack - amount);
        amount += canAdd;
        return addAmount - canAdd; // 余り
    }
    
    public int RemoveAmount(int removeAmount)
    {
        int removed = Mathf.Min(removeAmount, amount);
        amount -= removed;
        return removed;
    }
}

public class ResourceInventory : MonoBehaviour
{
    [Header("設定")]
    public int maxSlots = 30;
    public List<ResourceStack> resources = new List<ResourceStack>();
    
    [Header("UI")]
    public bool showDebugUI = true;
    
    public event Action<ResourceType, int> OnResourceAdded;
    public event Action<ResourceType, int> OnResourceRemoved;
    public event Action OnInventoryChanged;
    
    void Start()
    {
        // インベントリを初期化
        while (resources.Count < maxSlots)
        {
            resources.Add(null);
        }
    }
    
    public int AddResource(ResourceType resourceType, int amount)
    {
        if (resourceType == null || amount <= 0) return amount;
        
        int remaining = amount;
        
        // 既存のスタックに追加を試行
        for (int i = 0; i < resources.Count; i++)
        {
            if (resources[i]?.resourceType == resourceType && !resources[i].IsFull)
            {
                remaining = resources[i].AddAmount(remaining);
                if (remaining <= 0) break;
            }
        }
        
        // 新しいスロットに追加
        while (remaining > 0)
        {
            int emptySlot = FindEmptySlot();
            if (emptySlot == -1) break; // スロットが満杯
            
            int stackAmount = Mathf.Min(remaining, resourceType.maxStack);
            resources[emptySlot] = new ResourceStack(resourceType, stackAmount);
            remaining -= stackAmount;
        }
        
        int addedAmount = amount - remaining;
        if (addedAmount > 0)
        {
            OnResourceAdded?.Invoke(resourceType, addedAmount);
            OnInventoryChanged?.Invoke();
        }
        
        return remaining; // 追加できなかった分を返す
    }
    
    public int RemoveResource(ResourceType resourceType, int amount)
    {
        if (resourceType == null || amount <= 0) return 0;
        
        int remaining = amount;
        
        for (int i = resources.Count - 1; i >= 0; i--)
        {
            if (resources[i]?.resourceType == resourceType)
            {
                int removed = resources[i].RemoveAmount(remaining);
                remaining -= removed;
                
                if (resources[i].IsEmpty)
                {
                    resources[i] = null;
                }
                
                if (remaining <= 0) break;
            }
        }
        
        int removedAmount = amount - remaining;
        if (removedAmount > 0)
        {
            OnResourceRemoved?.Invoke(resourceType, removedAmount);
            OnInventoryChanged?.Invoke();
        }
        
        return removedAmount;
    }
    
    public int GetResourceCount(ResourceType resourceType)
    {
        int count = 0;
        foreach (var stack in resources)
        {
            if (stack?.resourceType == resourceType)
            {
                count += stack.amount;
            }
        }
        return count;
    }
    
    public bool HasResource(ResourceType resourceType, int amount)
    {
        return GetResourceCount(resourceType) >= amount;
    }
    
    int FindEmptySlot()
    {
        for (int i = 0; i < resources.Count; i++)
        {
            if (resources[i] == null)
                return i;
        }
        return -1;
    }
    
    public List<ResourceType> GetUniqueResources()
    {
        var unique = new List<ResourceType>();
        foreach (var stack in resources)
        {
            if (stack != null && !unique.Contains(stack.resourceType))
            {
                unique.Add(stack.resourceType);
            }
        }
        return unique;
    }
    
    void OnGUI()
    {
        if (!showDebugUI) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.Label("=== 資源インベントリ ===", GUI.skin.box);
        
        var uniqueResources = GetUniqueResources();
        foreach (var resource in uniqueResources)
        {
            int count = GetResourceCount(resource);
            GUILayout.Label($"{resource.name}: {count}");
        }
        
        GUILayout.EndArea();
    }
}

// ブロック破壊時に資源をドロップするコンポーネント

// ScriptableObjectで資源タイプを管理
[CreateAssetMenu(menuName = "VoxelGame/ResourceType")]
public class ResourceTypeAsset : ScriptableObject
{
    public ResourceType resourceData;
}

// 資源データベース
[CreateAssetMenu(menuName = "VoxelGame/ResourceDatabase")]
public class ResourceDatabase : ScriptableObject
{
    public List<ResourceType> allResources = new List<ResourceType>();
    
    public ResourceType GetResourceByName(string name)
    {
        return allResources.Find(r => r.name == name);
    }
    
    public ResourceType GetResourceById(int id)
    {
        if (id >= 0 && id < allResources.Count)
            return allResources[id];
        return null;
    }
}
