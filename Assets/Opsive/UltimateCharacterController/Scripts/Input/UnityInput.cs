/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using Opsive.UltimateCharacterController.Input.VirtualControls;

namespace Opsive.UltimateCharacterController.Input
{
    /// <summary>
    /// Acts as a common base class for any type of Unity input. Works with keyboard/mouse, controller, and mobile input.
    /// </summary>
    public class UnityInput : PlayerInput
    {
        /// <summary>
        /// Specifies if any input type should be forced.
        /// </summary>
        public enum ForceInputType { None, Standalone, Virtual }

        [Tooltip("Specifies if any input type should be forced.")]
        [SerializeField] protected ForceInputType m_ForceInput;
        [Tooltip("Should the cursor be disabled?")]
        [SerializeField] protected bool m_DisableCursor = true;
        [Tooltip("Should the cursor be enabled when the escape key is pressed?")]
        [SerializeField] protected bool m_EnableCursorWithEscape = true;

        public bool DisableCursor { get { return m_DisableCursor; }
            set
            {
                if (m_Input is VirtualInput) {
                    m_DisableCursor = false;
                }
                m_DisableCursor = value;
                if (m_DisableCursor && Cursor.visible) {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                } else if (!m_DisableCursor && !Cursor.visible) {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }
        public bool EnableCursorWithEscape { get { return m_EnableCursorWithEscape; } set { m_EnableCursorWithEscape = value; } }

        private InputBase m_Input;
        private bool m_UseVirtualInput;
        private HashSet<string> m_JoystickDownSet;
        private List<string> m_ToAddJoystickDownList;
        private List<string> m_JoystickDownList;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_UseVirtualInput = m_ForceInput == ForceInputType.Virtual;
#if !UNITYEDITOR && (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP_8_1 || UNITY_BLACKBERRY)
            if (m_ForceInput != ForceInputType.Standalone) {
                var virtualControlsManager = FindObjectOfType<VirtualControlsManager>();
                m_UseVirtualInput = virtualControlsManager != null;
            }
#endif
            if (m_UseVirtualInput) {
                m_Input = new VirtualInput();
                // The cursor must be enabled for virtual controls to allow the drag events to occur.
                m_DisableCursor = false;
            } else {
                m_Input = new StandaloneInput();
            }
            m_Input.Initialize(this);
        }

        /// <summary>
        /// The component has been enabled.
        /// </summary>
        private void OnEnable()
        {
            if (!m_UseVirtualInput && m_DisableCursor) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        /// <summary>
        /// Associates the VirtualControlsManager with the VirtualInput object.
        /// </summary>
        /// <param name="virtualControlsManager">The VirtualControlsManager to associate with the VirtualInput object.</param>
        /// <returns>True if the virtual controls were registered.</returns>
        public bool RegisterVirtualControlsManager(VirtualControlsManager virtualControlsManager)
        {
            if (m_Input is VirtualInput) {
                (m_Input as VirtualInput).RegisterVirtualControlsManager(virtualControlsManager);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the VirtualControlsManager association.
        /// </summary>
        public void UnegisterVirtualControlsManager()
        {
            if (m_Input is VirtualInput) {
                (m_Input as VirtualInput).UnregisterVirtualControlsManager();
            }
        }

        /// <summary>
        /// Update the joystick and cursor state values.
        /// </summary>
        private void LateUpdate()
        {
            if (!m_UseVirtualInput) {
                // The joystick is no longer down after the axis is 0.
                if (IsControllerConnected()) {
                    if (m_JoystickDownList != null) {
                        for (int i = m_JoystickDownList.Count - 1; i > -1; --i) {
                            if (m_Input.GetAxisRaw(m_JoystickDownList[i]) <= 0.1f) {
                                m_JoystickDownSet.Remove(m_JoystickDownList[i]);
                                m_JoystickDownList.RemoveAt(i);
                            }
                        }
                    }
                    // GetButtonDown doesn't immediately add the button name to the set to prevent the GetButtonDown from returning false
                    // if it is called twice within the same frame.
                    if (m_ToAddJoystickDownList != null && m_ToAddJoystickDownList.Count > 0) {
                        if (m_JoystickDownList == null) {
                            m_JoystickDownList = new List<string>();
                        }
                        for (int i = 0; i < m_ToAddJoystickDownList.Count; ++i) {
                            m_JoystickDownSet.Add(m_ToAddJoystickDownList[i]);
                            m_JoystickDownList.Add(m_ToAddJoystickDownList[i]);
                        }
                        m_ToAddJoystickDownList.Clear();
                    }
                }

                // Enable the cursor if the escape key is pressed. Disable the cursor if it is visbile but should be disabled upon press.
                if (m_EnableCursorWithEscape && UnityEngine.Input.GetKeyDown(KeyCode.Escape)) {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                } else if (Cursor.visible && m_DisableCursor && !IsPointerOverUI() && UnityEngine.Input.GetKeyDown(KeyCode.Mouse0)) {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
#if UNITY_EDITOR
                // The cursor should be visible when the game is paused.
                if (!Cursor.visible && Time.deltaTime == 0) {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
#endif
            }
        }

        /// <summary>
        /// Internal method which returns true if the button is being pressed.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True of the button is being pressed.</returns>
        protected override bool GetButtonInternal(string name)
        {
            if (m_Input.GetButton(name, InputBase.ButtonAction.GetButton)) {
                return true;
            }
            if (IsControllerConnected() && Mathf.Abs(m_Input.GetAxisRaw(name)) == 1) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Internal method which returns true if the button was pressed this frame.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True if the button is pressed this frame.</returns>
        protected override bool GetButtonDownInternal(string name)
        {
            if (IsControllerConnected() && Mathf.Abs(m_Input.GetAxisRaw(name)) == 1) {
                if (m_JoystickDownSet == null) {
                    m_JoystickDownSet = new HashSet<string>();
                }
                // The button should only be considered down on the first frame.
                if (m_JoystickDownSet.Contains(name)) {
                    return false;
                }
                if (m_ToAddJoystickDownList == null) {
                    m_ToAddJoystickDownList = new List<string>();
                }
                m_ToAddJoystickDownList.Add(name);
                return true;
            }
            return m_Input.GetButton(name, InputBase.ButtonAction.GetButtonDown);
        }

        /// <summary>
        /// Internal method which returnstrue if the button is up.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True if the button is up.</returns>
        protected override bool GetButtonUpInternal(string name)
        {
            if (IsControllerConnected()) {
                if (m_JoystickDownSet == null) {
                    m_JoystickDownSet = new HashSet<string>();
                }
                if (m_JoystickDownSet.Contains(name) && m_Input.GetAxisRaw(name) <= 0.1f) {
                    m_JoystickDownSet.Remove(name);
                    return true;
                }
                return false;
            }
            return m_Input.GetButton(name, InputBase.ButtonAction.GetButtonUp);
        }

        /// <summary>
        /// Internal method which returns the value of the axis with the specified name.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the axis.</returns>
        protected override float GetAxisInternal(string name)
        {
            return m_Input.GetAxis(name);
        }

        /// <summary>
        /// Internal method which returns the value of the raw axis with the specified name.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the raw axis.</returns>
        protected override float GetAxisRawInternal(string name)
        {
            return m_Input.GetAxisRaw(name);
        }

        /// <summary>
        /// Returns true if the pointer is over a UI element.
        /// </summary>
        /// <returns>True if the pointer is over a UI element.</returns>
        public override bool IsPointerOverUI()
        {
            // The input will always be over a UI element with virtual inputs.
            if (m_Input is VirtualInput) {
                return false;
            }
            return base.IsPointerOverUI();
        }

        /// <summary>
        /// Enables or disables gameplay input. An example of when it will not be enabled is when there is a fullscreen UI over the main camera.
        /// </summary>
        /// <param name="enable">True if the input is enabled.</param>
        protected override void EnableGameplayInput(bool enable)
        {
            base.EnableGameplayInput(enable);

            if (enable && m_DisableCursor) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

#if !UNITY_EDITOR
        /// <summary>
        /// Does the game have focus?
        /// </summary>
        /// <param name="hasFocus">True if the game has focus.</param>
        protected override void OnApplicationFocus(bool hasFocus)
        {
            base.OnApplicationFocus(hasFocus);

            if (m_DisableCursor) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
#endif
    }
}