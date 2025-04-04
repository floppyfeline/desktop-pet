using UnityEngine;
using UnityEngine.UIElements;

public abstract class PKMNUIManager : MonoBehaviour
{
    protected UIDocument doc;
    protected VisualElement root;
    protected PKMNBrain entity;

    public static bool IsHovering = false; // Static variable to track if the mouse is hovering over the blurb

    protected virtual void Start()
    {
        doc = GetComponent<UIDocument>();
        root = doc.rootVisualElement.Q<VisualElement>("Blurb");
        entity = GetComponentInParent<PKMNBrain>();

    }
    protected virtual void Update()
    {
        // Convert world position to UI Toolkit panel space
        Vector2 uiPosition = RuntimePanelUtils.CameraTransformWorldToPanel(root.panel, entity.transform.position, Camera.main);

        // Apply position (with offset if needed)
        root.style.left = uiPosition.x; // Center horizontally
        root.style.top = uiPosition.y - 10; // Move above entity
    }


}
