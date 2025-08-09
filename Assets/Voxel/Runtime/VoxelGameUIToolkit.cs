using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class VoxelGameUIToolkit : MonoBehaviour
{
    [Header("UI Documents")]
    public UIDocument uiDocument;
    
    [Header("References")]
    public VoxelBrush voxelBrush;
    public VoxelWorld voxelWorld;
    public ResourceInventory resourceInventory;
    public ConstructionManager constructionManager;
    public TimeSystem timeSystem;
    public DailyTaskManager taskManager;
    
    // UI Elements
    private VisualElement root;
    private Label timeLabel;
    private Label seasonLabel;
    private Label currentMaterialLabel;
    private VisualElement inventoryItems;
    private VisualElement tasksItems;
    private VisualElement constructionItems;
    private VisualElement materialSlots;
    private Label helpText;
    
    void Start()
    {
        // UIDocumentが無い場合は作成
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                uiDocument = gameObject.AddComponent<UIDocument>();
            }
        }
        
        // PanelSettingsを確認
        if (uiDocument.panelSettings == null)
        {
            Debug.LogWarning("VoxelGameUIToolkit: PanelSettings is null!");
            return;
        }
        
        // UXMLファイルを読み込み
        var visualTreeAsset = Resources.Load<VisualTreeAsset>("VoxelGameUI");
        
        if (visualTreeAsset != null)
        {
            uiDocument.visualTreeAsset = visualTreeAsset;
            
            // UIDocumentの設定を確認・調整
            uiDocument.sortingOrder = 100; // 他のUIより前面に表示
            
            // USSファイルを読み込み
            var styleSheet = Resources.Load<StyleSheet>("VoxelGameUI");
            if (styleSheet != null)
            {
                uiDocument.rootVisualElement.styleSheets.Add(styleSheet);
            }
            
            // ルート要素の可視性と配置を確保
            var root = uiDocument.rootVisualElement;
            root.style.width = Length.Percent(100);
            root.style.height = Length.Percent(100);
            root.style.position = Position.Absolute;
            root.style.top = 0;
            root.style.left = 0;
            
            // UI要素を取得
            InitializeUIElements();
            
            // イベントハンドラーを設定
            SetupEventHandlers();
            
            // 初期状態を設定
            UpdateUI();
            
            
            // レイアウトの強制更新
            root.MarkDirtyRepaint();
        }
        else
        {
            Debug.LogError("VoxelGameUIToolkit: Failed to load VoxelGameUI.uxml from Resources!");
        }
    }
    
    void InitializeUIElements()
    {
        root = uiDocument.rootVisualElement;
        
        if (root == null)
        {
            Debug.LogError("VoxelGameUIToolkit: Root element is null!");
            return;
        }
        
        // HUDパネルの要素
        timeLabel = root.Q<Label>("time-label");
        seasonLabel = root.Q<Label>("season-label");
        currentMaterialLabel = root.Q<Label>("current-material-label");
        
        // パネルの要素
        inventoryItems = root.Q<VisualElement>("inventory-items");
        tasksItems = root.Q<VisualElement>("tasks-items");
        constructionItems = root.Q<VisualElement>("construction-items");
        materialSlots = root.Q<VisualElement>("material-slots");
        helpText = root.Q<Label>("help-text");
        
        // マテリアルスロットを作成
        CreateMaterialSlots();
    }
    
    void CreateMaterialSlots()
    {
        if (materialSlots == null || voxelWorld?.palette?.entries == null) return;
        
        materialSlots.Clear();
        
        for (int i = 0; i < voxelWorld.palette.entries.Count && i < 9; i++)
        {
            var entry = voxelWorld.palette.entries[i];
            if (entry.name == "空気") continue; // 空気スロットはスキップ
            
            var slot = new VisualElement();
            slot.AddToClassList("material-slot");
            slot.style.backgroundColor = new StyleColor(entry.color);
            
            // 番号ラベル
            var numberLabel = new Label((i + 1).ToString());
            numberLabel.AddToClassList("material-number");
            slot.Add(numberLabel);
            
            // クリックイベント
            int materialId = i + 1;
            slot.RegisterCallback<ClickEvent>(evt => SelectMaterial(materialId));
            
            materialSlots.Add(slot);
        }
        
        UpdateSelectedMaterial();
    }
    
    void SetupEventHandlers()
    {
        // リソースインベントリの変更を監視
        if (resourceInventory != null)
        {
            resourceInventory.OnInventoryChanged += UpdateInventoryDisplay;
        }
        
        // 時間システムの変更を監視
        if (timeSystem != null)
        {
            timeSystem.OnHourChanged += (hour) => UpdateTimeDisplay();
            timeSystem.OnDayChanged += (day) => UpdateTimeDisplay();
            timeSystem.OnSeasonChanged += (season) => UpdateTimeDisplay();
        }
        
        // タスクシステムの変更を監視
        if (taskManager != null)
        {
            // タスクの変更を監視するイベントがあれば追加
        }
        
        // 建設システムの変更を監視
        if (constructionManager != null)
        {
            // 建設の変更を監視するイベントがあれば追加
        }
    }
    
    void SelectMaterial(int materialId)
    {
        if (voxelBrush != null)
        {
            voxelBrush.paintMaterial = (byte)materialId;
            UpdateSelectedMaterial();
        }
    }

    void UpdateUI()
    {
        UpdateTimeDisplay();
        UpdateCurrentMaterialDisplay();
        UpdateInventoryDisplay();
        UpdateTasksDisplay();
        UpdateConstructionDisplay();
    }
    
    void UpdateTimeDisplay()
    {
        if (timeSystem != null && timeLabel != null && seasonLabel != null)
        {
            timeLabel.text = timeSystem.GetTimeString();
            seasonLabel.text = timeSystem.GetSeasonString();
        }
    }
    
    void UpdateCurrentMaterialDisplay()
    {
        if (voxelBrush != null && currentMaterialLabel != null && voxelWorld?.palette?.entries != null)
        {
            int materialId = voxelBrush.paintMaterial;
            if (materialId >= 0 && materialId < voxelWorld.palette.entries.Count)
            {
                currentMaterialLabel.text = $"Material: {voxelWorld.palette.entries[materialId].name}";
            }
        }
    }
    
    void UpdateSelectedMaterial()
    {
        if (materialSlots == null || voxelBrush == null) return;
        
        // 全てのスロットの選択状態をリセット
        foreach (var slot in materialSlots.Children())
        {
            slot.RemoveFromClassList("selected");
        }
        
        // 現在選択されているマテリアルのスロットを選択状態にする
        int selectedIndex = voxelBrush.paintMaterial - 1; // インデックスは0ベース
        if (selectedIndex >= 0 && selectedIndex < materialSlots.childCount)
        {
            materialSlots.ElementAt(selectedIndex).AddToClassList("selected");
        }
    }
    
    void UpdateInventoryDisplay()
    {
        if (resourceInventory == null || inventoryItems == null) return;
        
        inventoryItems.Clear();
        
        var uniqueResources = resourceInventory.GetUniqueResources();
        foreach (var resource in uniqueResources)
        {
            int count = resourceInventory.GetResourceCount(resource);
            
            var resourceItem = new VisualElement();
            resourceItem.AddToClassList("resource-item");
            
            var nameLabel = new Label(resource.name);
            nameLabel.AddToClassList("resource-name");
            
            var amountLabel = new Label(count.ToString());
            amountLabel.AddToClassList("resource-amount");
            
            resourceItem.Add(nameLabel);
            resourceItem.Add(amountLabel);
            
            inventoryItems.Add(resourceItem);
        }
    }
    
    void UpdateTasksDisplay()
    {
        if (taskManager == null || tasksItems == null) return;
        
        tasksItems.Clear();
        
        foreach (var task in taskManager.currentTasks)
        {
            var taskItem = new VisualElement();
            taskItem.AddToClassList("task-item");
            
            if (task.isCompleted)
                taskItem.AddToClassList("completed");
            else if (task.currentAmount > 0)
                taskItem.AddToClassList("in-progress");
            
            var nameLabel = new Label(task.name);
            nameLabel.AddToClassList("task-name");
            
            var descLabel = new Label(task.description);
            descLabel.AddToClassList("task-description");
            
            var progressLabel = new Label($"{task.currentAmount}/{task.targetAmount}");
            progressLabel.AddToClassList("task-progress");
            
            taskItem.Add(nameLabel);
            taskItem.Add(descLabel);
            taskItem.Add(progressLabel);
            
            tasksItems.Add(taskItem);
        }
    }
    
    void UpdateConstructionDisplay()
    {
        if (constructionManager == null || constructionItems == null) return;
        
        constructionItems.Clear();
        
        foreach (var project in constructionManager.activeProjects)
        {
            var constructionItem = new VisualElement();
            constructionItem.AddToClassList("construction-item");
            
            if (project.isStarted && !project.isCompleted)
                constructionItem.AddToClassList("active");
            
            var nameLabel = new Label(project.name);
            nameLabel.AddToClassList("construction-name");
            
            var progressLabel = new Label($"{(project.GetOverallProgress() * 100):F1}%");
            progressLabel.AddToClassList("construction-progress");
            
            constructionItem.Add(nameLabel);
            constructionItem.Add(progressLabel);
            
            constructionItems.Add(constructionItem);
        }
    }
    
    void OnDestroy()
    {
        // イベントハンドラーを解除
        if (resourceInventory != null)
        {
            resourceInventory.OnInventoryChanged -= UpdateInventoryDisplay;
        }
    }
}
