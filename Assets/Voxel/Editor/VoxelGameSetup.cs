using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class VoxelGameSetup : EditorWindow
{
    [MenuItem("Voxel Game/Setup Game Environment")]
    public static void ShowWindow()
    {
        GetWindow<VoxelGameSetup>("Voxel Game Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Voxel Game Environment Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create Complete Game Setup", GUILayout.Height(30)))
        {
            CreateCompleteSetup();
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("Individual Components:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("1. Create Player"))
        {
            CreatePlayer();
        }
        
        if (GUILayout.Button("2. Create Voxel World"))
        {
            CreateVoxelWorld();
        }
        
        if (GUILayout.Button("3. Create Resource Systems"))
        {
            CreateResourceSystems();
        }
        
        if (GUILayout.Button("4. Create Construction Manager"))
        {
            CreateConstructionManager();
        }
        
        if (GUILayout.Button("5. Create Time & Task Systems"))
        {
            CreateTimeAndTaskSystems();
        }
        
        if (GUILayout.Button("6. Create Game UI"))
        {
            CreateGameUI();
        }
        
        if (GUILayout.Button("7. Create Material Palette"))
        {
            CreateMaterialPalette();
        }
        
        if (GUILayout.Button("8. Create Sample Resources"))
        {
            CreateSampleResources();
        }
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Clean All Game Objects", GUILayout.Height(25)))
        {
            CleanAllGameObjects();
        }
    }
    
    void CreateCompleteSetup()
    {
        Debug.Log("=== Starting Complete Voxel Game Setup ===");
        
        CreatePlayer();
        CreateVoxelWorld();
        CreateResourceSystems();
        CreateConstructionManager();
        CreateTimeAndTaskSystems();
        CreateGameUI();
        CreateMaterialPalette();
        CreateSampleResources();
        
        // 参照を自動設定
        SetupReferences();
        
        Debug.Log("=== Complete Setup Finished! ===");
        EditorUtility.DisplayDialog("Setup Complete", "Voxel Game environment has been created successfully!\n\nPress Play to start the game.", "OK");
    }
    
    void CreatePlayer()
    {
        // 既存のPlayerを探す
        var existingPlayer = FindAnyObjectByType<PlayerController>();
        if (existingPlayer != null)
        {
            Debug.Log("Player already exists, skipping...");
            return;
        }
        
        // Player作成
        GameObject player = new GameObject("Player");
        
        // CharacterController追加
        var controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.5f;
        controller.center = new Vector3(0, 0.9f, 0);
        
        // PlayerController追加
        var playerController = player.AddComponent<PlayerController>();
        
        // Camera作成
        GameObject cameraObj = new GameObject("Player Camera");
        cameraObj.transform.SetParent(player.transform);
        cameraObj.transform.localPosition = new Vector3(0, 1.6f, 0);
        
        var camera = cameraObj.AddComponent<Camera>();
        camera.tag = "MainCamera";
        cameraObj.AddComponent<AudioListener>();
        
        playerController.playerCamera = camera;
        
        // 位置設定
        player.transform.position = new Vector3(0, 20, 0);
        
        Debug.Log("Player created successfully");
    }
    
    void CreateVoxelWorld()
    {
        // 既存のVoxelWorldを探す
        var existingWorld = FindAnyObjectByType<VoxelWorld>();
        if (existingWorld != null)
        {
            Debug.Log("VoxelWorld already exists, skipping...");
            return;
        }
        
        GameObject voxelWorld = new GameObject("VoxelWorld");
        var world = voxelWorld.AddComponent<VoxelWorld>();
        
        // 設定
        world.voxelSize = 0.5f;
        world.viewRadius = 1; // パフォーマンス向上のため縮小
        world.useProceduralTerrain = true;
        world.worldSeed = 12345;
        
        // MatVoxelマテリアルを設定
        var matVoxel = AssetDatabase.LoadAssetAtPath<Material>("Assets/Voxel/MatVoxel.mat");
        if (matVoxel != null)
        {
            world.defaultMaterial = matVoxel;
        }
        
        // VoxelBrush作成
        GameObject voxelBrush = new GameObject("VoxelBrush");
        var brush = voxelBrush.AddComponent<VoxelBrush>();
        brush.world = world;
        brush.radius = 1.2f;
        brush.paintMaterial = 1;
        brush.useFirstPerson = true;
        brush.interactionDistance = 10f;
        
        Debug.Log("VoxelWorld and VoxelBrush created successfully");
    }
    
    void CreateResourceSystems()
    {
        // ResourceInventory
        var existingInventory = FindAnyObjectByType<ResourceInventory>();
        if (existingInventory == null)
        {
            GameObject resourceInventory = new GameObject("ResourceInventory");
            var inventory = resourceInventory.AddComponent<ResourceInventory>();
            inventory.maxSlots = 30;
            inventory.showDebugUI = true;
            Debug.Log("ResourceInventory created");
        }
        
        // ResourceDropper
        var existingDropper = FindAnyObjectByType<ResourceDropper>();
        if (existingDropper == null)
        {
            GameObject resourceDropper = new GameObject("ResourceDropper");
            var dropper = resourceDropper.AddComponent<ResourceDropper>();
            Debug.Log("ResourceDropper created");
        }
    }
    
    void CreateConstructionManager()
    {
        var existingManager = FindAnyObjectByType<ConstructionManager>();
        if (existingManager != null)
        {
            Debug.Log("ConstructionManager already exists, skipping...");
            return;
        }
        
        GameObject constructionManager = new GameObject("ConstructionManager");
        var manager = constructionManager.AddComponent<ConstructionManager>();
        
        manager.maxActiveProjects = 3;
        manager.autoSaveInterval = 30f;
        manager.showDebugUI = true;
        
        Debug.Log("ConstructionManager created successfully");
    }
    
    void CreateTimeAndTaskSystems()
    {
        // TimeSystem
        var existingTime = FindAnyObjectByType<TimeSystem>();
        if (existingTime == null)
        {
            GameObject timeSystem = new GameObject("TimeSystem");
            var time = timeSystem.AddComponent<TimeSystem>();
            time.timeScale = 1f;
            time.dayDuration = 120f;
            time.currentDay = 1;
            time.currentHour = 6;
            time.daysPerSeason = 30;
            Debug.Log("TimeSystem created");
        }
        
        // DailyTaskManager
        var existingTasks = FindAnyObjectByType<DailyTaskManager>();
        if (existingTasks == null)
        {
            GameObject taskManager = new GameObject("DailyTaskManager");
            var tasks = taskManager.AddComponent<DailyTaskManager>();
            tasks.maxDailyTasks = 3;
            tasks.showTaskUI = true;
            Debug.Log("DailyTaskManager created");
        }
    }
    
    void CreateGameUI()
    {
        var existingUI = FindAnyObjectByType<VoxelGameUIToolkit>();
        if (existingUI != null)
        {
            Debug.Log("VoxelGameUIToolkit already exists, skipping...");
            return;
        }
        
        GameObject gameUI = new GameObject("VoxelGameUIToolkit");
        var ui = gameUI.AddComponent<VoxelGameUIToolkit>();
        var uiDocument = gameUI.GetComponent<UIDocument>();
        
        // PanelSettingを設定
        var panelSetting = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/Voxel/UI/VoxelPanelSetting.asset");
        if (panelSetting != null)
        {
            uiDocument.panelSettings = panelSetting;
        }
        
        Debug.Log("VoxelGameUIToolkit created successfully");
    }
    
    void CreateMaterialPalette()
    {
        string path = "Assets/Voxel/VoxelMaterialPalette.asset";
        
        // 既存チェック
        var existingPalette = AssetDatabase.LoadAssetAtPath<MaterialPalette>(path);
        if (existingPalette != null)
        {
            Debug.Log("MaterialPalette already exists, skipping...");
            return;
        }
        
        // マテリアルパレット作成
        MaterialPalette palette = ScriptableObject.CreateInstance<MaterialPalette>();
        
        // デフォルトマテリアル設定
        palette.entries = new List<MaterialPalette.Entry>
        {
            new MaterialPalette.Entry { name = "空気", color = Color.clear, hardness = 0f },
            new MaterialPalette.Entry { name = "土", color = new Color(0.5f, 0.3f, 0.1f), hardness = 1f },
            new MaterialPalette.Entry { name = "草", color = new Color(0.1f, 0.8f, 0.1f), hardness = 0.8f },
            new MaterialPalette.Entry { name = "石", color = Color.gray, hardness = 3f },
            new MaterialPalette.Entry { name = "砂", color = new Color(1f, 0.9f, 0.4f), hardness = 0.5f },
            new MaterialPalette.Entry { name = "雪", color = Color.white, hardness = 0.2f },
            new MaterialPalette.Entry { name = "木材", color = new Color(0.6f, 0.4f, 0.2f), hardness = 2f },
            new MaterialPalette.Entry { name = "葉", color = new Color(0f, 0.5f, 0f), hardness = 0.1f },
            new MaterialPalette.Entry { name = "レンガ", color = new Color(0.8f, 0.3f, 0.2f), hardness = 4f },
            new MaterialPalette.Entry { name = "ガラス", color = new Color(0.8f, 0.9f, 1f, 0.3f), hardness = 1.5f }
        };
        
        AssetDatabase.CreateAsset(palette, path);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"MaterialPalette created at {path}");
    }
    
    void CreateSampleResources()
    {
        string basePath = "Assets/Voxel/Resources/";
        
        // フォルダ作成
        if (!AssetDatabase.IsValidFolder(basePath))
        {
            AssetDatabase.CreateFolder("Assets/Voxel", "Resources");
        }
        
        // リソース定義
        var resourceData = new[]
        {
            new { name = "土", desc = "基本的な建築材料", color = new Color(0.5f, 0.3f, 0.1f) },
            new { name = "種", desc = "植物を育てるのに必要", color = new Color(0.6f, 0.4f, 0.2f) },
            new { name = "石材", desc = "堅固な建築に使用", color = Color.gray },
            new { name = "砂", desc = "ガラス作りの材料", color = new Color(1f, 0.9f, 0.4f) },
            new { name = "氷", desc = "冷却材料として使用", color = new Color(0.8f, 0.9f, 1f) },
            new { name = "木材", desc = "建築と燃料に使用", color = new Color(0.6f, 0.4f, 0.2f) },
            new { name = "鉄鉱石", desc = "金属製品の材料", color = new Color(0.4f, 0.4f, 0.4f) }
        };
        
        for (int i = 0; i < resourceData.Length; i++)
        {
            string path = $"{basePath}{resourceData[i].name}.asset";
            
            if (AssetDatabase.LoadAssetAtPath<ResourceTypeAsset>(path) == null)
            {
                ResourceTypeAsset asset = ScriptableObject.CreateInstance<ResourceTypeAsset>();
                asset.resourceData = new ResourceType
                {
                    name = resourceData[i].name,
                    description = resourceData[i].desc,
                    iconColor = resourceData[i].color,
                    maxStack = 99,
                    baseHarvestTime = 1f,
                    baseAmount = 1,
                    baseValue = i + 1,
                    isRare = i >= 5
                };
                
                AssetDatabase.CreateAsset(asset, path);
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"Sample resources created in {basePath}");
    }
    
    void SetupReferences()
    {
        Debug.Log("Setting up references between components...");
        
        // 主要コンポーネントを取得
        var player = FindAnyObjectByType<PlayerController>();
        var voxelBrush = FindAnyObjectByType<VoxelBrush>();
        var voxelWorld = FindAnyObjectByType<VoxelWorld>();
        var resourceInventory = FindAnyObjectByType<ResourceInventory>();
        var resourceDropper = FindAnyObjectByType<ResourceDropper>();
        var constructionManager = FindAnyObjectByType<ConstructionManager>();
        var taskManager = FindAnyObjectByType<DailyTaskManager>();
        var gameUI = FindAnyObjectByType<VoxelGameUIToolkit>();
        var materialPalette = AssetDatabase.LoadAssetAtPath<MaterialPalette>("Assets/Voxel/VoxelMaterialPalette.asset");
        
        // VoxelBrush参照設定
        if (voxelBrush && player && voxelWorld)
        {
            voxelBrush.player = player;
            voxelBrush.world = voxelWorld;
            voxelBrush.resourceDropper = resourceDropper;
            voxelBrush.taskManager = taskManager;
        }
        
        // VoxelWorld参照設定
        if (voxelWorld && materialPalette)
        {
            voxelWorld.palette = materialPalette;
        }
        
        // ConstructionManager参照設定
        if (constructionManager)
        {
            constructionManager.resourceInventory = resourceInventory;
            constructionManager.voxelWorld = voxelWorld;
        }
        
        // TaskManager参照設定
        if (taskManager)
        {
            taskManager.resourceInventory = resourceInventory;
        }
        
        // GameUI参照設定
        if (gameUI)
        {
            gameUI.voxelBrush = voxelBrush;
            gameUI.voxelWorld = voxelWorld;
            gameUI.resourceInventory = resourceInventory;
            gameUI.constructionManager = constructionManager;
            gameUI.timeSystem = FindAnyObjectByType<TimeSystem>();
            gameUI.taskManager = taskManager;
        }
        
        // ResourceDropperのリソース設定
        if (resourceDropper)
        {
            SetupResourceDropperData(resourceDropper);
        }
        
        // TaskManagerのサンプルタスク設定
        if (taskManager)
        {
            SetupSampleTasks(taskManager);
        }
        
        EditorUtility.SetDirty(voxelBrush);
        EditorUtility.SetDirty(voxelWorld);
        EditorUtility.SetDirty(constructionManager);
        EditorUtility.SetDirty(taskManager);
        EditorUtility.SetDirty(gameUI);
        
        Debug.Log("References setup completed!");
    }
    
    void SetupResourceDropperData(ResourceDropper dropper)
    {
        // リソースアセットを読み込み
        var resources = new ResourceType[7];
        string[] resourceNames = {"土", "種", "石材", "砂", "氷", "木材", "種"};
        
        for (int i = 0; i < resourceNames.Length; i++)
        {
            string path = $"Assets/Voxel/Resources/{resourceNames[i]}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<ResourceTypeAsset>(path);
            if (asset != null)
            {
                resources[i] = asset.resourceData;
            }
        }
        
        dropper.possibleDrops = resources;
        dropper.dropChances = new float[] { 1f, 0.3f, 1f, 1f, 1f, 1f, 0.5f };
        dropper.dropAmounts = new int[] { 1, 1, 1, 1, 1, 2, 1 };
    }
    
    void SetupSampleTasks(DailyTaskManager taskManager)
    {
        var sampleTasks = new List<DailyTask>
        {
            new DailyTask 
            { 
                name = "資源収集", 
                description = "石材を10個集めよう", 
                type = DailyTask.TaskType.CollectResource,
                targetAmount = 10,
                experienceReward = 100
            },
            new DailyTask 
            { 
                name = "建築作業", 
                description = "何かを建設しよう", 
                type = DailyTask.TaskType.BuildStructure,
                targetAmount = 1,
                experienceReward = 200
            },
            new DailyTask 
            { 
                name = "探索", 
                description = "新しい場所を探索しよう", 
                type = DailyTask.TaskType.CompleteProject,
                targetAmount = 1,
                experienceReward = 150
            }
        };
        
        taskManager.availableTasks = sampleTasks;
    }
    
    void CleanAllGameObjects()
    {
        if (EditorUtility.DisplayDialog("Clean All", "This will delete all Voxel Game objects. Are you sure?", "Yes", "Cancel"))
        {
            // 削除対象のコンポーネント型
            var typesToClean = new System.Type[]
            {
                typeof(PlayerController),
                typeof(VoxelWorld),
                typeof(VoxelBrush),
                typeof(ResourceInventory),
                typeof(ResourceDropper),
                typeof(ConstructionManager),
                typeof(TimeSystem),
                typeof(DailyTaskManager),
                typeof(VoxelGameUIToolkit)
            };
            
            foreach (var type in typesToClean)
            {
                var objects = FindObjectsByType(type, FindObjectsSortMode.None) as Component[];
                foreach (var obj in objects)
                {
                    DestroyImmediate(obj.gameObject);
                }
            }
            
            Debug.Log("All Voxel Game objects cleaned!");
        }
    }
}
