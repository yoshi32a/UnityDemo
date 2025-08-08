using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem; // 新Input System
#endif
#if VOXEL_USE_ZLOGGER
using ZLogger;
#endif

class VoxelBrush : MonoBehaviour
{
    public VoxelWorld world;
    public float radius = 1.2f;
    public byte paintMaterial = 1; // Rockなど

    void Update()
    {
        if (!world) return;
        var cam = Camera.main;
        if (!cam) return;

        // 1, 2, 3キーでブラシ切り替え
        if (Keyboard.current.digit1Key.wasPressedThisFrame) paintMaterial = 0;
        if (Keyboard.current.digit2Key.wasPressedThisFrame) paintMaterial = 1;
        if (Keyboard.current.digit3Key.wasPressedThisFrame) paintMaterial = 2;
        if (Keyboard.current.digit4Key.wasPressedThisFrame) paintMaterial = 3;
        if (Keyboard.current.digit5Key.wasPressedThisFrame) paintMaterial = 4;
        if (Keyboard.current.digit6Key.wasPressedThisFrame) paintMaterial = 5;

        var mouse = Mouse.current;
        if (mouse == null) return;
        var pos = mouse.position.ReadValue();

        if (mouse.leftButton.isPressed)
        {
            if (Physics.Raycast(cam.ScreenPointToRay(pos), out var hit, 1000f))
                world.ApplyBrush(hit.point, radius, -1, 255);
        }
        else if (mouse.rightButton.isPressed)
        {
            if (Physics.Raycast(cam.ScreenPointToRay(pos), out var hit, 1000f))
                world.ApplyBrush(hit.point, radius, +1, paintMaterial);
        }
    }
}
