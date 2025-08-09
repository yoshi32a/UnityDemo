using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;
    
    [Header("視点設定")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;
    
    [Header("カメラ")]
    public Camera playerCamera;
    public float cameraHeight = 1.6f;
    
    CharacterController controller;
    Vector3 velocity;
    float xRotation = 0f;
    
    Vector2 moveInput;
    Vector2 lookInput;
    bool isJumping;
    bool isSprinting;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // カメラ設定 - MainCameraを取得してPlayerCameraとして使用
        if (!playerCamera)
        {
            playerCamera = Camera.main;
            if (!playerCamera)
            {
                Debug.LogError("MainCameraが見つかりません！シーンにMainCameraを配置してください。");
                return;
            }
        }
        
        // カメラをプレイヤーに追従させる
        playerCamera.transform.SetParent(transform);
        playerCamera.transform.localPosition = new Vector3(0, cameraHeight, 0);
        playerCamera.transform.localRotation = Quaternion.identity;
        
        // プレイヤー位置を地形の高さに調整
        var voxelWorld = FindFirstObjectByType<VoxelWorld>();
        if (voxelWorld != null)
        {
            // 正しい地表の高さを計算（baseHeightはボクセル単位なので、Unity座標に変換）
            float groundHeightInUnityUnits = voxelWorld.terrainSettings.baseHeight * voxelWorld.voxelSize;
            // プレイヤーを地表の3m上に配置
            Vector3 newPos = new Vector3(0f, groundHeightInUnityUnits + 3f, 0f); // 原点に配置
            
            // CharacterControllerを一時的に無効化して確実に移動
            controller.enabled = false;
            transform.position = newPos;
            controller.enabled = true;
            
            // 重力速度をリセット
            velocity = Vector3.zero;
            
            Debug.Log($"Player spawn setup - BaseHeight (voxels): {voxelWorld.terrainSettings.baseHeight}, VoxelSize: {voxelWorld.voxelSize}");
            Debug.Log($"Ground height (Unity units): {groundHeightInUnityUnits}, Player spawn position: {newPos}");
        }
        
        Debug.Log($"Player position: {transform.position}, Camera position: {playerCamera.transform.position}");
        Debug.Log($"Camera settings - nearClip: {playerCamera.nearClipPlane}, farClip: {playerCamera.farClipPlane}, cullingMask: {playerCamera.cullingMask}");
        Debug.Log($"Camera clearFlags: {playerCamera.clearFlags}, backgroundColor: {playerCamera.backgroundColor}");
        
        // PlayerCameraがUIを上書きしないよう設定
        var urpCameraData = playerCamera.GetComponent<UniversalAdditionalCameraData>();
        if (urpCameraData != null)
        {
            // UIレンダリングを無効化
            urpCameraData.renderPostProcessing = false;
            urpCameraData.renderShadows = true;
        }
        
        // レイヤーマスクでUIレイヤー（5番）を除外
        playerCamera.cullingMask = ~(1 << 5); // UIレイヤーを描画しない
        playerCamera.depth = 0;
        
        // テスト用：背景色を設定
        playerCamera.clearFlags = CameraClearFlags.SolidColor;
        playerCamera.backgroundColor = Color.blue;
        
        // マウスカーソルをロック
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        HandleMovement();
        HandleLook();
        
        // ESCでカーソル表示切り替え
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
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
        
        // Tキーでプレイヤーを地表に移動
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            var voxelWorld = FindFirstObjectByType<VoxelWorld>();
            if (voxelWorld != null)
            {
                float groundHeightInUnityUnits = voxelWorld.terrainSettings.baseHeight * voxelWorld.voxelSize;
                Vector3 newPos = new Vector3(transform.position.x, groundHeightInUnityUnits + 3f, transform.position.z);
                
                // 重力速度をリセット
                velocity = Vector3.zero;
                
                // CharacterControllerで移動
                controller.enabled = false;
                transform.position = newPos;
                controller.enabled = true;
                
                Debug.Log($"Manual teleport - Ground (Unity units): {groundHeightInUnityUnits}, New position: {newPos}");
                Debug.Log($"BaseHeight (voxels): {voxelWorld.terrainSettings.baseHeight}, VoxelSize: {voxelWorld.voxelSize}");
            }
        }
    }
    
    void HandleMovement()
    {
        // 入力取得
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            moveInput = Vector2.zero;
            
            if (keyboard.wKey.isPressed) moveInput.y += 1;
            if (keyboard.sKey.isPressed) moveInput.y -= 1;
            if (keyboard.aKey.isPressed) moveInput.x -= 1;
            if (keyboard.dKey.isPressed) moveInput.x += 1;
            
            moveInput = moveInput.normalized;
            
            isJumping = keyboard.spaceKey.wasPressedThisFrame;
            isSprinting = keyboard.leftShiftKey.isPressed;
        }
        
        // 移動方向計算
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        
        // 速度決定
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        controller.Move(move * currentSpeed * Time.deltaTime);
        
        // ジャンプ
        if (isJumping && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        // 重力
        velocity.y += gravity * Time.deltaTime;
        
        // 地面についている場合、下向き速度をリセット
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        controller.Move(velocity * Time.deltaTime);
    }
    
    void HandleLook()
    {
        var mouse = Mouse.current;
        if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
        {
            lookInput = mouse.delta.ReadValue() * mouseSensitivity * 0.1f;
            
            // 水平回転（Y軸）
            transform.Rotate(Vector3.up * lookInput.x);
            
            // 垂直回転（X軸）
            xRotation -= lookInput.y;
            xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }
    
    // 現在見ている位置を取得
    public bool GetLookingAt(float maxDistance, out RaycastHit hit)
    {
        var rayOrigin = playerCamera.transform.position;
        var rayDirection = playerCamera.transform.forward;
        
        // 複数のレイキャストを試行（中央、少しずらした位置）
        bool hitResult = Physics.Raycast(rayOrigin, rayDirection, out hit, maxDistance);
        
        // デバッグ情報（VoxelBrushからのデバッグモードを参照するため）
        var voxelBrush = FindFirstObjectByType<VoxelBrush>();
        if (voxelBrush != null && voxelBrush.debugMode)
        {
            if (voxelBrush.showRaycast)
            {
                Debug.DrawRay(rayOrigin, rayDirection * maxDistance, hitResult ? Color.red : Color.white, 0.1f);
                
                // 追加のレイキャストでより詳細な情報を取得
                var allHits = Physics.RaycastAll(rayOrigin, rayDirection, maxDistance);
                Debug.Log($"[PlayerController] Total hits detected: {allHits.Length}");
                
                for (int i = 0; i < allHits.Length; i++)
                {
                    var h = allHits[i];
                    Debug.Log($"[PlayerController] Hit {i}: {h.collider.name} at {h.point} (distance: {h.distance:F2})");
                }
            }
            
            if (hitResult)
            {
                Debug.Log($"[PlayerController] Primary Raycast HIT - Object: {hit.collider.name}, Point: {hit.point}, Distance: {hit.distance:F2}, Normal: {hit.normal}");
                Debug.Log($"[PlayerController] Collider Bounds: {hit.collider.bounds.min} to {hit.collider.bounds.max}");
            }
            else
            {
                Debug.Log($"[PlayerController] Raycast MISS - Origin: {rayOrigin}, Direction: {rayDirection}, MaxDistance: {maxDistance}");
                Debug.Log($"[PlayerController] Camera euler: {playerCamera.transform.eulerAngles}");
            }
        }
        
        return hitResult;
    }
    
    // ブロックを配置する位置を取得
    public bool GetBlockPlacePosition(float maxDistance, out Vector3 position)
    {
        position = Vector3.zero;
        
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, maxDistance))
        {
            // ヒット面の法線方向に少しオフセット
            position = hit.point + hit.normal * 0.5f;
            return true;
        }
        
        return false;
    }
}