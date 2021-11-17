// GENERATED AUTOMATICALLY FROM 'Assets/InputSystem/MenuInputAction.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @MenuInputAction : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @MenuInputAction()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""MenuInputAction"",
    ""maps"": [
        {
            ""name"": ""Menu"",
            ""id"": ""aa1d15bb-980e-4e1f-b5bd-6fed85168c82"",
            ""actions"": [
                {
                    ""name"": ""ControlPanel"",
                    ""type"": ""Button"",
                    ""id"": ""6b3e671c-5dda-4f63-bf41-250e943ae843"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""444be5d5-566f-4109-a0a0-754489148215"",
                    ""path"": ""<Keyboard>/backspace"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Menu"",
                    ""action"": ""ControlPanel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Menu"",
            ""bindingGroup"": ""Menu"",
            ""devices"": []
        }
    ]
}");
        // Menu
        m_Menu = asset.FindActionMap("Menu", throwIfNotFound: true);
        m_Menu_ControlPanel = m_Menu.FindAction("ControlPanel", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Menu
    private readonly InputActionMap m_Menu;
    private IMenuActions m_MenuActionsCallbackInterface;
    private readonly InputAction m_Menu_ControlPanel;
    public struct MenuActions
    {
        private @MenuInputAction m_Wrapper;
        public MenuActions(@MenuInputAction wrapper) { m_Wrapper = wrapper; }
        public InputAction @ControlPanel => m_Wrapper.m_Menu_ControlPanel;
        public InputActionMap Get() { return m_Wrapper.m_Menu; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MenuActions set) { return set.Get(); }
        public void SetCallbacks(IMenuActions instance)
        {
            if (m_Wrapper.m_MenuActionsCallbackInterface != null)
            {
                @ControlPanel.started -= m_Wrapper.m_MenuActionsCallbackInterface.OnControlPanel;
                @ControlPanel.performed -= m_Wrapper.m_MenuActionsCallbackInterface.OnControlPanel;
                @ControlPanel.canceled -= m_Wrapper.m_MenuActionsCallbackInterface.OnControlPanel;
            }
            m_Wrapper.m_MenuActionsCallbackInterface = instance;
            if (instance != null)
            {
                @ControlPanel.started += instance.OnControlPanel;
                @ControlPanel.performed += instance.OnControlPanel;
                @ControlPanel.canceled += instance.OnControlPanel;
            }
        }
    }
    public MenuActions @Menu => new MenuActions(this);
    private int m_MenuSchemeIndex = -1;
    public InputControlScheme MenuScheme
    {
        get
        {
            if (m_MenuSchemeIndex == -1) m_MenuSchemeIndex = asset.FindControlSchemeIndex("Menu");
            return asset.controlSchemes[m_MenuSchemeIndex];
        }
    }
    public interface IMenuActions
    {
        void OnControlPanel(InputAction.CallbackContext context);
    }
}
