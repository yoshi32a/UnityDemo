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
        return Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, maxDistance);
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