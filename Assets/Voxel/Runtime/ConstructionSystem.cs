using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceRequirement
{
    public ResourceType resourceType;
    public int requiredAmount;
    public int currentAmount;
    
    public bool IsFulfilled => currentAmount >= requiredAmount;
    public float Progress => (float)currentAmount / requiredAmount;
}

[System.Serializable]
public class BuildingProject
{
    [Header("基本情報")]
    public string name;
    public string description;
    public Sprite icon;
    
    [Header("必要資源")]
    public List<ResourceRequirement> requirements = new List<ResourceRequirement>();
    
    [Header("建設設定")]
    public float constructionTime = 60f; // 秒
    public Vector3Int size = Vector3Int.one; // 建物のサイズ
    public GameObject buildingPrefab; // 完成時のプレハブ
    
    [Header("報酬")]
    public int experienceReward = 100;
    public List<ResourceStack> completionRewards = new List<ResourceStack>();
    
    [Header("効果")]
    public BuildingEffect[] effects; // 建物の効果
    
    // 進行状況
    [System.NonSerialized]
    public float constructionProgress = 0f;
    [System.NonSerialized]
    public bool isStarted = false;
    [System.NonSerialized]
    public bool isCompleted = false;
    [System.NonSerialized]
    public Vector3 buildPosition;
    
    public bool CanStart()
    {
        foreach (var req in requirements)
        {
            if (!req.IsFulfilled)
                return false;
        }
        return true;
    }
    
    public float GetOverallProgress()
    {
        if (isCompleted) return 1f;
        
        float resourceProgress = 0f;
        if (requirements.Count > 0)
        {
            foreach (var req in requirements)
            {
                resourceProgress += req.Progress;
            }
            resourceProgress /= requirements.Count;
        }
        
        if (!CanStart()) return resourceProgress * 0.5f; // 資源収集段階は50%まで
        
        return 0.5f + (constructionProgress * 0.5f); // 建設段階は50%から100%
    }
}

[System.Serializable]
public enum BuildingEffectType
{
    ResourceGeneration, // 資源生成
    PopulationIncrease, // 人口増加
    HappinessBonus,    // 幸福度ボーナス
    TaxIncome,         // 税収
    ConstructionSpeedBoost // 建設速度向上
}

[System.Serializable]
public class BuildingEffect
{
    public BuildingEffectType type;
    public ResourceType affectedResource;
    public float value;
    public float interval = 60f; // 効果の発動間隔（秒）
}

public class ConstructionManager : MonoBehaviour
{
    [Header("プロジェクト")]
    public List<BuildingProject> availableProjects = new List<BuildingProject>();
    public List<BuildingProject> activeProjects = new List<BuildingProject>();
    public List<BuildingProject> completedProjects = new List<BuildingProject>();
    
    [Header("設定")]
    public int maxActiveProjects = 3;
    public float autoSaveInterval = 30f;
    
    [Header("参照")]
    public ResourceInventory resourceInventory;
    public VoxelWorld voxelWorld;
    
    [Header("UI")]
    public bool showDebugUI = true;
    Vector2 scrollPosition;
    
    public event Action<BuildingProject> OnProjectStarted;
    public event Action<BuildingProject> OnProjectCompleted;
    public event Action<BuildingProject> OnProjectCancelled;
    
    float lastAutoSaveTime;
    
    void Start()
    {
        if (!resourceInventory)
            resourceInventory = FindFirstObjectByType<ResourceInventory>();
        if (!voxelWorld)
            voxelWorld = FindFirstObjectByType<VoxelWorld>();
        
        lastAutoSaveTime = Time.time;
    }
    
    void Update()
    {
        UpdateActiveProjects();
        
        // オートセーブ
        if (Time.time - lastAutoSaveTime > autoSaveInterval)
        {
            AutoSave();
            lastAutoSaveTime = Time.time;
        }
    }
    
    void UpdateActiveProjects()
    {
        for (int i = activeProjects.Count - 1; i >= 0; i--)
        {
            var project = activeProjects[i];
            
            // 資源要件をチェック
            UpdateResourceRequirements(project);
            
            // 建設進行
            if (project.CanStart() && project.isStarted)
            {
                project.constructionProgress += Time.deltaTime / project.constructionTime;
                
                if (project.constructionProgress >= 1f)
                {
                    CompleteProject(project);
                    activeProjects.RemoveAt(i);
                }
            }
        }
    }
    
    void UpdateResourceRequirements(BuildingProject project)
    {
        foreach (var req in project.requirements)
        {
            int available = resourceInventory.GetResourceCount(req.resourceType);
            req.currentAmount = Mathf.Min(req.requiredAmount, available);
        }
    }
    
    public bool StartProject(BuildingProject projectTemplate, Vector3 position)
    {
        if (activeProjects.Count >= maxActiveProjects)
        {
            Debug.Log("同時進行できるプロジェクト数の上限に達しています");
            return false;
        }
        
        // プロジェクトのコピーを作成
        var project = JsonUtility.FromJson<BuildingProject>(JsonUtility.ToJson(projectTemplate));
        project.buildPosition = position;
        project.isStarted = false;
        
        // 建設場所をチェック
        if (!IsValidBuildLocation(position, project.size))
        {
            Debug.Log("建設場所が適切ではありません");
            return false;
        }
        
        activeProjects.Add(project);
        OnProjectStarted?.Invoke(project);
        
        Debug.Log($"プロジェクト開始: {project.name}");
        return true;
    }
    
    public bool TryStartConstruction(BuildingProject project)
    {
        if (!project.CanStart())
        {
            Debug.Log("必要な資源が不足しています");
            return false;
        }
        
        // 資源を消費
        foreach (var req in project.requirements)
        {
            resourceInventory.RemoveResource(req.resourceType, req.requiredAmount);
        }
        
        project.isStarted = true;
        project.constructionProgress = 0f;
        
        Debug.Log($"建設開始: {project.name}");
        return true;
    }
    
    void CompleteProject(BuildingProject project)
    {
        project.isCompleted = true;
        project.constructionProgress = 1f;
        
        // 建物を実際に配置
        if (project.buildingPrefab)
        {
            var building = Instantiate(project.buildingPrefab, project.buildPosition, Quaternion.identity);
            var buildingComponent = building.GetComponent<CompletedBuilding>();
            if (buildingComponent)
            {
                buildingComponent.Initialize(project);
            }
        }
        
        // 報酬を付与
        foreach (var reward in project.completionRewards)
        {
            resourceInventory.AddResource(reward.resourceType, reward.amount);
        }
        
        completedProjects.Add(project);
        OnProjectCompleted?.Invoke(project);
        
        Debug.Log($"プロジェクト完了: {project.name}");
    }
    
    public void CancelProject(BuildingProject project)
    {
        if (activeProjects.Contains(project))
        {
            // 消費済み資源の50%を返却
            if (project.isStarted)
            {
                foreach (var req in project.requirements)
                {
                    int refund = req.requiredAmount / 2;
                    resourceInventory.AddResource(req.resourceType, refund);
                }
            }
            
            activeProjects.Remove(project);
            OnProjectCancelled?.Invoke(project);
            
            Debug.Log($"プロジェクトキャンセル: {project.name}");
        }
    }
    
    bool IsValidBuildLocation(Vector3 position, Vector3Int size)
    {
        // 建設場所の妥当性をチェック
        // 地面の上に建てられるか、他の建物と重複しないかなど
        return true; // 簡略化
    }
    
    void AutoSave()
    {
        // プロジェクト情報をセーブ（実装簡略化）
        Debug.Log("プロジェクト進行状況を保存しました");
    }
    
    void OnGUI()
    {
        if (!showDebugUI) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 310, 600));
        
        GUILayout.Label("=== 建設管理 ===", GUI.skin.box);
        
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(500));
        
        // 利用可能プロジェクト
        GUILayout.Label("利用可能なプロジェクト:");
        foreach (var project in availableProjects)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(project.name);
            GUILayout.Label(project.description);
            
            foreach (var req in project.requirements)
            {
                int current = resourceInventory.GetResourceCount(req.resourceType);
                GUILayout.Label($"{req.resourceType.name}: {current}/{req.requiredAmount}");
            }
            
            if (GUILayout.Button("建設開始") && activeProjects.Count < maxActiveProjects)
            {
                StartProject(project, Vector3.zero);
            }
            GUILayout.EndVertical();
        }
        
        // アクティブプロジェクト
        if (activeProjects.Count > 0)
        {
            GUILayout.Label("進行中のプロジェクト:");
            foreach (var project in activeProjects)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(project.name);
                
                float progress = project.GetOverallProgress();
                GUILayout.Label($"進行: {progress:P1}");
                
                if (!project.isStarted && project.CanStart())
                {
                    if (GUILayout.Button("資源投入して建設開始"))
                    {
                        TryStartConstruction(project);
                    }
                }
                
                if (GUILayout.Button("キャンセル"))
                {
                    CancelProject(project);
                }
                
                GUILayout.EndVertical();
            }
        }
        
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}

// 完成した建物のコンポーネント
public class CompletedBuilding : MonoBehaviour
{
    BuildingProject projectData;
    float lastEffectTime;
    
    public void Initialize(BuildingProject project)
    {
        projectData = project;
        lastEffectTime = Time.time;
        
        // 建物の効果を開始
        InvokeRepeating(nameof(ApplyBuildingEffects), 1f, 1f);
    }
    
    void ApplyBuildingEffects()
    {
        if (projectData?.effects == null) return;
        
        var resourceInventory = FindFirstObjectByType<ResourceInventory>();
        if (!resourceInventory) return;
        
        foreach (var effect in projectData.effects)
        {
            if (Time.time - lastEffectTime >= effect.interval)
            {
                ApplyEffect(effect, resourceInventory);
            }
        }
    }
    
    void ApplyEffect(BuildingEffect effect, ResourceInventory inventory)
    {
        switch (effect.type)
        {
            case BuildingEffectType.ResourceGeneration:
                if (effect.affectedResource != null)
                {
                    inventory.AddResource(effect.affectedResource, Mathf.RoundToInt(effect.value));
                }
                break;
                
            // 他の効果タイプも実装可能
        }
    }
}

// 建設プロジェクトのプリセット
[CreateAssetMenu(menuName = "VoxelGame/BuildingProject")]
public class BuildingProjectAsset : ScriptableObject
{
    public BuildingProject projectData;
}