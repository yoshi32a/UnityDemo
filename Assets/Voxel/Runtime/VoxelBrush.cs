using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem; // 新Input System
#endif
#if VOXEL_USE_ZLOGGER
using ZLogger;
#endif

public class VoxelBrush : MonoBehaviour
{
    public VoxelWorld world;
    public float radius = 1.2f;
    public byte paintMaterial = 1; // デフォルトマテリアル
    public float interactionDistance = 10f;
    
    [Header("プレイヤー設定")]
    public PlayerController player;
    public bool useFirstPerson = true; // 一人称視点を使用
    
    [Header("ブロック破壊エフェクト")]
    public GameObject blockBreakEffect; // パーティクルプレハブ
    public float effectDuration = 1f;

    void Start()
    {
        // プレイヤーが設定されていない場合は探す
        if (!player && useFirstPerson)
        {
            player = FindObjectOfType<PlayerController>();
        }
    }

    void Update()
    {
        if (!world) return;
        
        // 数字キーでマテリアル切り替え
        HandleMaterialSelection();
        
        // マウス入力処理
        HandleMouseInput();
    }
    
    void HandleMaterialSelection()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) paintMaterial = 1; // 土
        if (Keyboard.current.digit2Key.wasPressedThisFrame) paintMaterial = 2; // 草
        if (Keyboard.current.digit3Key.wasPressedThisFrame) paintMaterial = 3; // 石
        if (Keyboard.current.digit4Key.wasPressedThisFrame) paintMaterial = 4; // 砂
        if (Keyboard.current.digit5Key.wasPressedThisFrame) paintMaterial = 5; // 雪
        if (Keyboard.current.digit6Key.wasPressedThisFrame) paintMaterial = 6; // 木材
        if (Keyboard.current.digit7Key.wasPressedThisFrame) paintMaterial = 7; // 葉
        if (Keyboard.current.digit8Key.wasPressedThisFrame) paintMaterial = 8; // レンガ
        if (Keyboard.current.digit9Key.wasPressedThisFrame) paintMaterial = 9; // ガラス
    }
    
    void HandleMouseInput()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;
        
        // カーソルがロックされていない場合は操作しない
        if (useFirstPerson && Cursor.lockState != CursorLockMode.Locked)
            return;
        
        if (mouse.leftButton.isPressed)
        {
            // ブロック破壊
            if (useFirstPerson && player)
            {
                if (player.GetLookingAt(interactionDistance, out RaycastHit hit))
                {
                    DestroyBlock(hit.point);
                }
            }
            else
            {
                // 従来のカメラベース操作
                var cam = Camera.main;
                if (cam && Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out var hit, 1000f))
                {
                    DestroyBlock(hit.point);
                }
            }
        }
        else if (mouse.rightButton.isPressed)
        {
            // ブロック配置
            if (useFirstPerson && player)
            {
                if (player.GetBlockPlacePosition(interactionDistance, out Vector3 position))
                {
                    PlaceBlock(position);
                }
            }
            else
            {
                // 従来のカメラベース操作
                var cam = Camera.main;
                if (cam && Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out var hit, 1000f))
                {
                    PlaceBlock(hit.point);
                }
            }
        }
    }
    
    void DestroyBlock(Vector3 position)
    {
        // エフェクトを生成
        if (blockBreakEffect)
        {
            var effect = Instantiate(blockBreakEffect, position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }
        
        // ブロックを削除
        world.ApplyBrush(position, radius, -1, 255);
    }
    
    void PlaceBlock(Vector3 position)
    {
        world.ApplyBrush(position, radius, +1, paintMaterial);
    }
    
    // UI表示用：現在選択中のマテリアル名を取得
    public string GetCurrentMaterialName()
    {
        if (world && world.palette && paintMaterial < world.palette.entries.Count)
        {
            return world.palette.entries[paintMaterial].name;
        }
        return $"Material {paintMaterial}";
    }
}
