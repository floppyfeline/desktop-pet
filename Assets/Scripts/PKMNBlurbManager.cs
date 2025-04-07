using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

using UnityEngine.UIElements;

public class PKMNBlurbManager : PKMNUIManager
{
    const string LABEL = "Name";
    const string MENU_TOGGLE = "NameContainer";
    const string MENU_CONTAINER = "DropUpMenu";


    private Label _label;
    private VisualElement _menuContainer;
    [SerializeField]
    private string _name => entity.Name;

    private bool _isEnabled = false;

    private float _width, _height;

    private float _timeUntilClose = -1f;

    public Action OnOpenMenu;
    [SerializeField]
    private PKMNActionsManager _actionsMenu;

    private void ToggleActionsMenu(bool state)
    {
        if (_actionsMenu != null)
        {
            _actionsMenu.SetEnabled(state);
            root.SetEnabled(state);
        }
    }

    public void SetEnabled(bool state, bool disableImmediately = false)
    {
        if (!state)
        {
            if (disableImmediately)
            {
                root.AddToClassList("Selectable");
                _label.AddToClassList("Selectable");
                root.SetEnabled(false);
                _menuContainer.SetEnabled(false);
                _isEnabled = false;
                _timeUntilClose = -1f; // No countdown needed while it's disabled
                return;
            }
            _timeUntilClose = 1f; // Start countdown
            return;
        }

        if (_actionsMenu.Opened)
        {
            SetEnabled(false, true); // Close the menu if options are open
            return;
        }

        // Enable immediately
        root.SetEnabled(true);
        _isEnabled = true;
        _timeUntilClose = -1f; // No countdown needed while it's enabled
    }

    public void EnableMenu()
    {
        root.RemoveFromClassList("Selectable");
        _label.RemoveFromClassList("Selectable");
        OnOpenMenu?.Invoke();
        _menuContainer.SetEnabled(true);
    }
    protected override void Start()
    {

        base.Start();



        _isEnabled = false;
        entity.OnHover += () => { SetEnabled(true); };
        entity.OnHoverExit += () => { SetEnabled(false); };
        entity.OnNameUpdate += UpdateText;


        // ------------------
        // Register mouse events for hover
        // ------------------
        root.RegisterCallback<MouseOverEvent>
            (evt =>
        {
            SetEnabled(true);
            IsHovering = true; // Set static variable to true when hovering
        });
        root.RegisterCallback<MouseLeaveEvent>
        (evt =>
        {
            SetEnabled(false, true);
            IsHovering = false; // Set static variable to false when not hovering
        });

        // ------------------
        // Register mouse events for actions menu
        // ------------------
        root.Q<VisualElement>("Actions").RegisterCallback<MouseDownEvent>(evt =>
        {
            ToggleActionsMenu(true);
            SetEnabled(false, true); // Close the blurb when opening the actions menu
        });

        _label = root.Q<Label>(LABEL);
        _menuContainer = root.Q<VisualElement>(MENU_CONTAINER);
        _menuContainer.SetEnabled(false); // Initially disabled
        root.Q<Button>(MENU_TOGGLE).clicked += EnableMenu;

        UpdateText(_name);

        root.Q<VisualElement>("pet").RegisterCallback<MouseDownEvent>(evt =>
        {
            entity.OnInteract.Invoke();
            entity.OnHappy?.Invoke();
            SetEnabled(false, true); // Close the menu after petting
            Debug.Log("Petting " + entity.name);
        });

        root.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            _width = evt.newRect.width;
            _height = evt.newRect.height;
        });

        _isEnabled = false;
        SetEnabled(false, true);

        OnOpenMenu += () =>
        {
            if (!_menuContainer.enabledSelf) entity.OnInteract.Invoke();
        };

        UpdateText(_name);
    }

    protected override void Update()
    {
        // Wait for the delay before disabling
        if (_timeUntilClose > 0)
        {
            _timeUntilClose -= Time.deltaTime;
            if (_timeUntilClose <= 0)
            {
                SetEnabled(false, true);
            }
        }

        if (entity == null || doc == null) return;

        base.Update();
    }
    public void UpdateText(string newText)
    {
        _label.text = newText;
    }
}
