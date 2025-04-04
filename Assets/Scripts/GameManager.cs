using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{

    static InputAction MousePos;
    public static Vector2 MousePosition;

    private void Start()
    {
        MousePos = new InputAction(binding: "<Mouse>/position");
        MousePos.Enable();
    }
    private void LateUpdate()
    {
        MousePosition = MousePos.ReadValue<Vector2>();
    }

    public static Vector2 GetMousePosition()
    {
        return MousePos.ReadValue<Vector2>();
    }
}
