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
    
    [Header("資源システム")]
    public ResourceDropper resourceDropper;
    public DailyTaskManager taskManager;
    
    [Header("デバッグ設定")]
    public bool debugMode = true; // デバッグ情報を表示
    public bool showRaycast = true; // レイキャスト情報を表示
    public bool showCrosshair = true; // 画面中央にクロスヘアを表示
    public bool showInteractionArea = false; // インタラクション可能エリアを表示

    void Start()
    {
        // プレイヤーが設定されていない場合は探す
        if (!player && useFirstPerson)
        {
            player = FindFirstObjectByType<PlayerController>();
        }
        
        // 資源システムの参照を取得
        if (!resourceDropper)
            resourceDropper = FindFirstObjectByType<ResourceDropper>();
        if (!taskManager)
            taskManager = FindFirstObjectByType<DailyTaskManager>();
    }

    void Update()
    {
        if (!world) return;
        
        // 数字キーでマテリアル切り替え
        HandleMaterialSelection();
        
        // F1キーでデバッグ情報をクリップボードにコピー
        if (debugMode && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            CopyDebugInfoToClipboard();
        }
        
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
                    if (debugMode)
                    {
                        // コピー用まとめログ
                        string debugInfo = $"=== DEBUG INFO ===\n" +
                            $"Player Y: {player.playerCamera.transform.position.y:F2}\n" +
                            $"Ground Y: {world.terrainSettings.baseHeight}\n" +
                            $"Hit Point: {hit.point}\n" +
                            $"Distance: {hit.distance:F2}\n" +
                            $"Object: {hit.collider.name}\n";
                        
                        if (world.TryGetVoxelAt(hit.point, out Voxel voxel))
                        {
                            debugInfo += $"Voxel: Density={voxel.density}, Material={voxel.material}\n";
                        }
                        else
                        {
                            debugInfo += $"Voxel: Not Found\n";
                        }
                        debugInfo += $"===================";
                        
                        Debug.Log(debugInfo);
                    }
                    DestroyBlock(hit.point, hit.normal);
                }
                else if (debugMode)
                {
                    string debugInfo = $"=== DEBUG INFO ===\n" +
                        $"Player Y: {player.playerCamera.transform.position.y:F2}\n" +
                        $"Ground Y: {world.terrainSettings.baseHeight}\n" +
                        $"No Hit - Max distance: {interactionDistance}\n" +
                        $"Camera Forward: {player.playerCamera.transform.forward}\n" +
                        $"===================";
                    Debug.Log(debugInfo);
                }
            }
            else
            {
                // 従来のカメラベース操作
                var cam = Camera.main;
                if (cam && Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out var hit, 1000f))
                {
                    DestroyBlock(hit.point, hit.normal);
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
    
    void DestroyBlock(Vector3 position, Vector3 normal = default)
    {
        if (debugMode)
        {
            Debug.Log($"[VoxelBrush] Attempting to destroy block at position: {position}, normal: {normal}");
        }
        
        // 法線方向を考慮してボクセル内部の位置を取得
        Vector3 adjustedPosition;
        
        if (normal != Vector3.zero)
        {
            // 法線方向の逆向き（内側）に少しオフセット
            adjustedPosition = position - normal * (world.voxelSize * 0.1f);
        }
        else
        {
            // 法線情報がない場合は下向きにオフセット（従来の方法）
            adjustedPosition = position + Vector3.down * (world.voxelSize * 0.1f);
        }
        
        if (debugMode)
        {
            Debug.Log($"[VoxelBrush] Adjusted position: {position} -> {adjustedPosition}");
        }
        
        // 破壊前のブロック情報を取得
        if (world.TryGetVoxelAt(adjustedPosition, out Voxel voxel) && voxel.density > 0)
        {
            if (debugMode)
            {
                Debug.Log($"[VoxelBrush] Block found - Density: {voxel.density}, Material: {voxel.material}");
            }
            
            // エフェクトを生成（元の位置で）
            if (blockBreakEffect)
            {
                var effect = Instantiate(blockBreakEffect, position, Quaternion.identity);
                Destroy(effect, effectDuration);
                
                if (debugMode)
                {
                    Debug.Log($"[VoxelBrush] Break effect spawned at: {position}");
                }
            }
            
            // 資源ドロップ処理
            if (resourceDropper)
            {
                resourceDropper.OnBlockDestroyed(position, voxel.material);
            }
            
            // タスク進捗更新（資源収集タスク）
            if (taskManager)
            {
                // マテリアルに応じて適切なResourceTypeを取得する必要がある
                // ここは簡略化のため省略
            }
            
            // ブロックを削除（調整後の位置で）
            world.ApplyBrush(adjustedPosition, radius, -1, 255);
            
            if (debugMode)
            {
                Debug.Log($"[VoxelBrush] Block destruction completed successfully");
            }
        }
        else
        {
            if (debugMode)
            {
                if (world.TryGetVoxelAt(adjustedPosition, out Voxel emptyVoxel))
                {
                    Debug.Log($"[VoxelBrush] Block at adjusted position {adjustedPosition} is empty - Density: {emptyVoxel.density}, Material: {emptyVoxel.material}");
                    
                    // 元の位置も確認
                    if (world.TryGetVoxelAt(position, out Voxel originalVoxel))
                    {
                        Debug.Log($"[VoxelBrush] Original position {position} - Density: {originalVoxel.density}, Material: {originalVoxel.material}");
                    }
                }
                else
                {
                    Debug.Log($"[VoxelBrush] No voxel found at adjusted position: {adjustedPosition}");
                }
            }
        }
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
    
    // デバッグ用GUI表示
    void OnGUI()
    {
        if (!debugMode) return;
        
        // クロスヘア表示
        if (showCrosshair)
        {
            float centerX = Screen.width / 2f;
            float centerY = Screen.height / 2f;
            float size = 10f;
            
            // 十字線を描画
            GUI.color = Color.red;
            GUI.DrawTexture(new Rect(centerX - size/2, centerY - 1, size, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(centerX - 1, centerY - size/2, 2, size), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
        
        // デバッグ情報表示
        GUI.color = Color.yellow;
        GUIStyle style = new GUIStyle();
        style.fontSize = 12;
        style.normal.textColor = Color.yellow;
        
        string debugText = $"Debug Mode: ON\n";
        debugText += $"Interaction Distance: {interactionDistance:F1}m\n";
        debugText += $"Current Material: {GetCurrentMaterialName()}\n";
        debugText += $"Cursor Locked: {Cursor.lockState == CursorLockMode.Locked}\n";
        
        if (player && player.playerCamera)
        {
            var camPos = player.playerCamera.transform.position;
            debugText += $"Camera Position: {camPos}\n";
            debugText += $"Camera Forward: {player.playerCamera.transform.forward}\n";
            
            // レイキャストテスト
            if (player.GetLookingAt(interactionDistance, out RaycastHit hit))
            {
                debugText += $"Looking At: {hit.collider.name} ({hit.distance:F2}m)\n";
                debugText += $"Hit Point: {hit.point}\n";
                
                // 最新のボクセル情報を取得
                if (world.TryGetVoxelAt(hit.point, out Voxel voxel))
                {
                    debugText += $"Voxel: Density={voxel.density}, Material={voxel.material}\n";
                }
                else
                {
                    debugText += $"Voxel: Not Found\n";
                }
            }
            else
            {
                debugText += $"Looking At: Nothing\n";
            }
            
            // 地形の高さ情報を表示
            float groundHeightUnity = world.terrainSettings.baseHeight * world.voxelSize;
            debugText += $"Ground Height (Unity): {groundHeightUnity:F2}\n";
            debugText += $"Ground Height (Voxels): {world.terrainSettings.baseHeight}\n";
            debugText += $"Player Y: {camPos.y:F2}, Expected Y: {groundHeightUnity + 3:F2}\n";
        }
        
        debugText += $"\n[F1] Copy Debug Info to Clipboard";
        debugText += $"\n[T] Teleport to Ground Surface";
        
        GUI.Label(new Rect(10, 10, 450, 220), debugText, style);
        GUI.color = Color.white;
    }
    
    void CopyDebugInfoToClipboard()
    {
        if (!player || !player.playerCamera) return;
        
        var camPos = player.playerCamera.transform.position;
        string clipboardText = $"=== VOXEL GAME DEBUG INFO ===\n" +
            $"Player Position: {camPos}\n" +
            $"Player Y: {camPos.y:F2}\n" +
            $"Ground Height (Unity): {world.terrainSettings.baseHeight * world.voxelSize:F2}\n" +
            $"Ground Height (Voxels): {world.terrainSettings.baseHeight}\n" +
            $"VoxelSize: {world.voxelSize}\n" +
            $"Interaction Distance: {interactionDistance}\n" +
            $"Current Material: {GetCurrentMaterialName()}\n" +
            $"Cursor Locked: {Cursor.lockState == CursorLockMode.Locked}\n";
        
        if (player.GetLookingAt(interactionDistance, out RaycastHit hit))
        {
            clipboardText += $"Looking At: {hit.collider.name}\n" +
                $"Hit Point: {hit.point}\n" +
                $"Distance: {hit.distance:F2}m\n";
            
            if (world.TryGetVoxelAt(hit.point, out Voxel voxel))
            {
                clipboardText += $"Voxel: Density={voxel.density}, Material={voxel.material}\n";
            }
            else
            {
                clipboardText += $"Voxel: Not Found\n";
            }
        }
        else
        {
            clipboardText += $"Looking At: Nothing\n";
        }
        
        clipboardText += $"=============================";
        
        GUIUtility.systemCopyBuffer = clipboardText;
        Debug.Log("Debug info copied to clipboard!");
        Debug.Log(clipboardText);
    }
}
