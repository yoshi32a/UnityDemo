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
        Debug.Log("VoxelGameUIToolkit: Start called");
        
        // UIDocumentが無い場合は作成
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                uiDocument = gameObject.AddComponent<UIDocument>();
                Debug.Log("VoxelGameUIToolkit: Created new UIDocument");
            }
        }
        
        // PanelSettingsを確認
        if (uiDocument.panelSettings == null)
        {
            Debug.LogWarning("VoxelGameUIToolkit: PanelSettings is null!");
        }
        else
        {
            Debug.Log("VoxelGameUIToolkit: PanelSettings found");
        }
        
        // UXMLファイルを読み込み
        var visualTreeAsset = Resources.Load<VisualTreeAsset>("VoxelGameUI");
        
        if (visualTreeAsset != null)
        {
            Debug.Log("VoxelGameUIToolkit: VisualTreeAsset loaded successfully");
            uiDocument.visualTreeAsset = visualTreeAsset;
            
            // UIDocumentの設定を確認・調整
            uiDocument.sortingOrder = 100; // 他のUIより前面に表示
            
            // USSファイルを読み込み
            var styleSheet = Resources.Load<StyleSheet>("VoxelGameUI");
            if (styleSheet != null)
            {
                uiDocument.rootVisualElement.styleSheets.Add(styleSheet);
                Debug.Log("VoxelGameUIToolkit: StyleSheet loaded successfully");
            }
            else
            {
                Debug.LogWarning("VoxelGameUIToolkit: StyleSheet not found, applying inline styles");
            }
            
            // ルート要素の可視性と配置を確保
            var root = uiDocument.rootVisualElement;
            root.style.width = Length.Percent(100);
            root.style.height = Length.Percent(100);
            root.style.position = Position.Absolute;
            root.style.top = 0;
            root.style.left = 0;
            root.style.backgroundColor = new Color(0, 0, 0, 0.1f); // デバッグ用の薄い背景
            
            Debug.Log($"VoxelGameUIToolkit: Root style applied - width: {root.style.width}, height: {root.style.height}");
            
            // UI要素を取得
            InitializeUIElements();
            
            // イベントハンドラーを設定
            SetupEventHandlers();
            
            // 初期状態を設定
            UpdateUI();
            
            Debug.Log("VoxelGameUIToolkit: UI initialized successfully");
            
            // レイアウトの強制更新
            root.MarkDirtyRepaint();
            
            // 次フレームでデバッグ情報を確認
            StartCoroutine(DelayedDebug());
        }
        else
        {
            Debug.LogError("VoxelGameUIToolkit: Failed to load VoxelGameUI.uxml from Resources!");
        }
    }
    
    void InitializeUIElements()
    {
        root = uiDocument.rootVisualElement;
        Debug.Log($"VoxelGameUIToolkit: Root element found: {root != null}");
        
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
        
        // 要素が見つかったかログ出力
        Debug.Log($"VoxelGameUIToolkit: timeLabel found: {timeLabel != null}");
        Debug.Log($"VoxelGameUIToolkit: inventoryItems found: {inventoryItems != null}");
        Debug.Log($"VoxelGameUIToolkit: materialSlots found: {materialSlots != null}");
        
        // マテリアルスロットを作成
        CreateMaterialSlots();
        
        // テスト用に簡単なテキストとスタイルを設定
        if (timeLabel != null)
        {
            timeLabel.text = "TEST: UI Working!";
            timeLabel.style.color = Color.red;
            timeLabel.style.fontSize = 24;
            timeLabel.style.position = Position.Absolute;
            timeLabel.style.top = 50;
            timeLabel.style.left = 50;
            timeLabel.style.backgroundColor = Color.black;
            timeLabel.style.paddingTop = 10;
            timeLabel.style.paddingBottom = 10;
            timeLabel.style.paddingLeft = 20;
            timeLabel.style.paddingRight = 20;
            timeLabel.style.width = 300;
            timeLabel.style.height = 50;
            timeLabel.style.display = DisplayStyle.Flex;
            timeLabel.style.visibility = Visibility.Visible;
            timeLabel.style.opacity = 1;
            Debug.Log($"VoxelGameUIToolkit: Test label styled - display: {timeLabel.style.display}, visibility: {timeLabel.style.visibility}");
        }
        else
        {
            Debug.LogError("VoxelGameUIToolkit: timeLabel is null!");
            
            // 代替として新しいラベルを直接作成
            var testLabel = new Label("DIRECT TEST LABEL");
            testLabel.style.position = Position.Absolute;
            testLabel.style.top = 100;
            testLabel.style.left = 100;
            testLabel.style.color = Color.yellow;
            testLabel.style.fontSize = 30;
            testLabel.style.backgroundColor = Color.blue;
            testLabel.style.paddingTop = 20;
            testLabel.style.paddingBottom = 20;
            testLabel.style.paddingLeft = 20;
            testLabel.style.paddingRight = 20;
            root.Add(testLabel);
            Debug.Log("VoxelGameUIToolkit: Direct test label added");
        }
        
        // メインコンテナにも背景を設定
        var mainContainer = root.Q<VisualElement>("main-container");
        if (mainContainer != null)
        {
            mainContainer.style.width = Length.Percent(100);
            mainContainer.style.height = Length.Percent(100);
            mainContainer.style.position = Position.Absolute;
            Debug.Log("VoxelGameUIToolkit: Main container styled");
        }
        
        // 絶対確実に表示されるテスト要素を追加
        var absoluteTestElement = new VisualElement();
        absoluteTestElement.style.position = Position.Absolute;
        absoluteTestElement.style.top = 0;
        absoluteTestElement.style.left = 0;
        absoluteTestElement.style.width = 200;
        absoluteTestElement.style.height = 100;
        absoluteTestElement.style.backgroundColor = Color.magenta;
        absoluteTestElement.style.borderBottomColor = Color.white;
        absoluteTestElement.style.borderBottomWidth = 3;
        absoluteTestElement.style.borderTopColor = Color.white;
        absoluteTestElement.style.borderTopWidth = 3;
        absoluteTestElement.style.borderLeftColor = Color.white;
        absoluteTestElement.style.borderLeftWidth = 3;
        absoluteTestElement.style.borderRightColor = Color.white;
        absoluteTestElement.style.borderRightWidth = 3;
        
        var absoluteTestLabel = new Label("UI TOOLKIT ACTIVE");
        absoluteTestLabel.style.color = Color.white;
        absoluteTestLabel.style.fontSize = 16;
        absoluteTestLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        absoluteTestElement.Add(absoluteTestLabel);
        
        root.Add(absoluteTestElement);
        Debug.Log("VoxelGameUIToolkit: Absolute test element added directly to root");
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
    
    void Update()
    {
        UpdateUI();
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
    
    System.Collections.IEnumerator DelayedDebug()
    {
        yield return null; // 1フレーム待つ
        Debug.Log("=== DELAYED DEBUG INFO ===");
        DebugUIState();
    }
    
    void DebugUIState()
    {
        Debug.Log($"UIDocument enabled: {uiDocument.enabled}");
        Debug.Log($"UIDocument gameObject active: {uiDocument.gameObject.activeInHierarchy}");
        Debug.Log($"Root element child count: {root.childCount}");
        Debug.Log($"Panel settings: {uiDocument.panelSettings?.name}");
        Debug.Log($"Visual tree asset: {uiDocument.visualTreeAsset?.name}");
        Debug.Log($"Root element style display: {root.style.display}");
        Debug.Log($"Root element style visibility: {root.style.visibility}");
        Debug.Log($"Root element resolved style width: {root.resolvedStyle.width}");
        Debug.Log($"Root element resolved style height: {root.resolvedStyle.height}");
        Debug.Log($"Root element layout width: {root.layout.width}");
        Debug.Log($"Root element layout height: {root.layout.height}");
        
        // Camera情報も確認
        var camera = Camera.main;
        if (camera != null)
        {
            Debug.Log($"Main camera: {camera.name}, enabled: {camera.enabled}");
        }
        
        // 各UI要素の状態も確認
        if (timeLabel != null)
        {
            Debug.Log($"Time label text: '{timeLabel.text}', layout: {timeLabel.layout}");
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
