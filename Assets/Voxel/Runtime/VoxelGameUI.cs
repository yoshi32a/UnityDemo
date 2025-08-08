using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class VoxelGameUI : MonoBehaviour
{
    [Header("UI要素")]
    public GameObject hudPanel;
    public Text materialText;
    public Text helpText;
    public GameObject inventoryPanel;
    public GameObject materialSlotPrefab;
    public Transform materialSlotsContainer;
    
    [Header("参照")]
    public VoxelBrush voxelBrush;
    public VoxelWorld voxelWorld;
    
    bool showHelp = true;
    bool showInventory = false;
    
    List<GameObject> materialSlots = new List<GameObject>();
    
    void Start()
    {
        // HUDパネルを作成（Canvas必須）
        if (!hudPanel)
        {
            CreateDefaultUI();
        }
        
        // インベントリUIを初期化
        InitializeInventory();
        
        // 初期状態
        if (inventoryPanel) inventoryPanel.SetActive(false);
    }
    
    void Update()
    {
        // ヘルプ表示切り替え
        if (UnityEngine.InputSystem.Keyboard.current.f1Key.wasPressedThisFrame)
        {
            showHelp = !showHelp;
            if (helpText) helpText.gameObject.SetActive(showHelp);
        }
        
        // インベントリ表示切り替え
        if (UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame)
        {
            showInventory = !showInventory;
            if (inventoryPanel) inventoryPanel.SetActive(showInventory);
            
            // インベントリ表示時はカーソルを表示
            if (showInventory)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        // UI更新
        UpdateUI();
    }
    
    void UpdateUI()
    {
        // 現在のマテリアル表示
        if (materialText && voxelBrush)
        {
            string materialName = voxelBrush.GetCurrentMaterialName();
            materialText.text = $"マテリアル: {materialName}";
        }
        
        // ヘルプテキスト
        if (helpText && showHelp)
        {
            helpText.text = GetHelpText();
        }
        
        // インベントリのハイライト更新
        UpdateInventoryHighlight();
    }
    
    void InitializeInventory()
    {
        if (!materialSlotsContainer || !voxelWorld || !voxelWorld.palette) return;
        
        // 既存のスロットをクリア
        foreach (var slot in materialSlots)
        {
            if (slot) Destroy(slot);
        }
        materialSlots.Clear();
        
        // マテリアルパレットからスロットを生成
        for (int i = 0; i < voxelWorld.palette.entries.Count && i < 10; i++)
        {
            var entry = voxelWorld.palette.entries[i];
            GameObject slot = CreateMaterialSlot(i, entry);
            materialSlots.Add(slot);
        }
    }
    
    GameObject CreateMaterialSlot(int index, MaterialPalette.Entry entry)
    {
        GameObject slot;
        
        if (materialSlotPrefab)
        {
            slot = Instantiate(materialSlotPrefab, materialSlotsContainer);
        }
        else
        {
            // デフォルトスロットを作成
            slot = new GameObject($"MaterialSlot_{index}");
            slot.transform.SetParent(materialSlotsContainer);
            
            var image = slot.AddComponent<Image>();
            image.color = entry.color;
            
            var rectTransform = slot.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(50, 50);
        }
        
        // クリックイベントを追加
        var button = slot.GetComponent<Button>();
        if (!button) button = slot.AddComponent<Button>();
        
        int materialId = index;
        button.onClick.AddListener(() => SelectMaterial(materialId));
        
        // テキストラベル
        var textObj = new GameObject("Label");
        textObj.transform.SetParent(slot.transform);
        var text = textObj.AddComponent<Text>();
        text.text = $"{index + 1}\n{entry.name}";
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 12;
        text.color = Color.white;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return slot;
    }
    
    void SelectMaterial(int materialId)
    {
        if (voxelBrush)
        {
            voxelBrush.paintMaterial = (byte)(materialId + 1);
        }
    }
    
    void UpdateInventoryHighlight()
    {
        if (!voxelBrush) return;
        
        for (int i = 0; i < materialSlots.Count; i++)
        {
            var slot = materialSlots[i];
            if (!slot) continue;
            
            var image = slot.GetComponent<Image>();
            if (image)
            {
                // 選択中のマテリアルをハイライト
                if (i == voxelBrush.paintMaterial - 1)
                {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
                    slot.transform.localScale = Vector3.one * 1.1f;
                }
                else
                {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, 0.7f);
                    slot.transform.localScale = Vector3.one;
                }
            }
        }
    }
    
    string GetHelpText()
    {
        return @"【操作方法】
WASD: 移動
マウス: 視点操作
Space: ジャンプ
Shift: ダッシュ
左クリック: ブロック破壊
右クリック: ブロック配置
1-9: マテリアル選択
Tab: インベントリ
ESC: カーソル表示
F1: ヘルプ表示切替";
    }
    
    void CreateDefaultUI()
    {
        // Canvas作成
        GameObject canvasObj = new GameObject("GameUI");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // HUDパネル
        hudPanel = new GameObject("HUDPanel");
        hudPanel.transform.SetParent(canvasObj.transform);
        var hudRect = hudPanel.AddComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0, 1);
        hudRect.anchorMax = new Vector2(0, 1);
        hudRect.pivot = new Vector2(0, 1);
        hudRect.anchoredPosition = new Vector2(10, -10);
        hudRect.sizeDelta = new Vector2(300, 100);
        
        // マテリアル表示テキスト
        GameObject matTextObj = new GameObject("MaterialText");
        matTextObj.transform.SetParent(hudPanel.transform);
        materialText = matTextObj.AddComponent<Text>();
        materialText.text = "マテリアル: なし";
        materialText.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        materialText.fontSize = 16;
        materialText.color = Color.white;
        
        var matTextRect = matTextObj.GetComponent<RectTransform>();
        matTextRect.anchorMin = Vector2.zero;
        matTextRect.anchorMax = new Vector2(1, 0.5f);
        matTextRect.offsetMin = Vector2.zero;
        matTextRect.offsetMax = Vector2.zero;
        
        // ヘルプテキスト
        GameObject helpTextObj = new GameObject("HelpText");
        helpTextObj.transform.SetParent(canvasObj.transform);
        helpText = helpTextObj.AddComponent<Text>();
        helpText.text = GetHelpText();
        helpText.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        helpText.fontSize = 14;
        helpText.color = new Color(1, 1, 1, 0.8f);
        helpText.alignment = TextAnchor.UpperRight;
        
        var helpTextRect = helpTextObj.GetComponent<RectTransform>();
        helpTextRect.anchorMin = new Vector2(1, 1);
        helpTextRect.anchorMax = new Vector2(1, 1);
        helpTextRect.pivot = new Vector2(1, 1);
        helpTextRect.anchoredPosition = new Vector2(-10, -10);
        helpTextRect.sizeDelta = new Vector2(250, 300);
        
        // インベントリパネル
        inventoryPanel = new GameObject("InventoryPanel");
        inventoryPanel.transform.SetParent(canvasObj.transform);
        var invImage = inventoryPanel.AddComponent<Image>();
        invImage.color = new Color(0, 0, 0, 0.8f);
        
        var invRect = inventoryPanel.GetComponent<RectTransform>();
        invRect.anchorMin = new Vector2(0.5f, 0.5f);
        invRect.anchorMax = new Vector2(0.5f, 0.5f);
        invRect.pivot = new Vector2(0.5f, 0.5f);
        invRect.anchoredPosition = Vector2.zero;
        invRect.sizeDelta = new Vector2(600, 400);
        
        // マテリアルスロットコンテナ
        GameObject slotsContainer = new GameObject("MaterialSlots");
        slotsContainer.transform.SetParent(inventoryPanel.transform);
        materialSlotsContainer = slotsContainer.transform;
        
        var gridLayout = slotsContainer.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(60, 60);
        gridLayout.spacing = new Vector2(10, 10);
        gridLayout.padding = new RectOffset(20, 20, 20, 20);
        
        var slotsRect = slotsContainer.GetComponent<RectTransform>();
        slotsRect.anchorMin = Vector2.zero;
        slotsRect.anchorMax = Vector2.one;
        slotsRect.offsetMin = Vector2.zero;
        slotsRect.offsetMax = Vector2.zero;
    }
}