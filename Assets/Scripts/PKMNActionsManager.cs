using System;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PKMNActionsManager : PKMNUIManager
{
    public Action OnClose;
    public bool Opened = false;

    // mode toggle label
    private Label _modeLabel;

    public void SetEnabled(bool state)
    {
        root.SetEnabled(state);
        Opened = state;
    }
    protected override void Start()
    {
        base.Start();
        SetEnabled(false);
        root.Q<VisualElement>("set-mode-btn").RegisterCallback<MouseDownEvent>(e =>
        {
            ChangeModes();
        });
        _modeLabel = root.Q<Label>("mode-label");

        root.RegisterCallback<MouseOverEvent>(e =>
        {
            IsHovering = true;
        });
        root.RegisterCallback<MouseLeaveEvent>(e =>
        {
            IsHovering = false;
        });

        root.Q<VisualElement>("close").RegisterCallback<MouseDownEvent>(e =>
        {
            SetEnabled(false);
            entity.OnInteract?.Invoke();
        });
    }
    protected override void Update()
    {
        base.Update();
    }
    private void ChangeModes()
    {
        if (entity.Mode == PKMNMode.Roaming)
        {
            entity.Mode = PKMNMode.Stationary;
            _modeLabel.text = "Mode: Stay Put";
        }
        else
        {
            entity.Mode = PKMNMode.Roaming;
            _modeLabel.text = "Mode: Roaming";
        }
    }
}
