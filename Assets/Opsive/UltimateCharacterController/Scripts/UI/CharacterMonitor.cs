/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.UI
{
    /// <summary>
    /// The CameraMonitor component allows for UI elements to mapped to a specific character (allowing for split screen and coop).
    /// </summary>
    public abstract class CharacterMonitor : StateBehavior
    {
        [Tooltip("The character that uses the UI represents. Can be null.")]
        [SerializeField] protected GameObject m_Character;

        protected GameObject m_CameraGameObject;
        protected bool m_ShowUI = true;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            var camera = UnityEngineUtility.FindCamera(m_Character);
            if (camera != null) {
                m_CameraGameObject = camera.gameObject;

                EventHandler.RegisterEvent<GameObject>(m_CameraGameObject, "OnCameraAttachCharacter", OnAttachCharacter);

                if (m_Character == null) {
                    m_Character = m_CameraGameObject.GetCachedComponent<Camera.CameraController>().Character;
                }
            }

            // Start disabled - attaching the character will enabe the component.
            enabled = false;

            if (camera == null) {
                Debug.LogError("Error: The UI element " + gameObject.name + " cannot find a character.");
            } else {
                var character = m_Character;
                m_Character = null;
                OnAttachCharacter(character);
            }
        }

        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected virtual void OnAttachCharacter(GameObject character)
        {
            if (m_Character == character) {
                return;
            }

            if (m_Character != null) {
                StateManager.LinkGameObjects(m_Character, gameObject, false);
                EventHandler.UnregisterEvent<bool>(m_Character, "OnShowUI", ShowUI);
            }

            m_Character = character;

            if (m_Character != null) {
                StateManager.LinkGameObjects(m_Character, gameObject, true);
                EventHandler.RegisterEvent<bool>(m_Character, "OnShowUI", ShowUI);
            }

            // The monitor may be in the process of being destroyed.
            enabled = m_Character != null;
        }

        /// <summary>
        /// Shows or hides the UI.
        /// </summary>
        /// <param name="show">Should the UI be shown?</param>
        private void ShowUI(bool show)
        {
            m_ShowUI = show;
            gameObject.SetActive(show && CanShowUI());
        }

        /// <summary>
        /// Can the UI be shown?
        /// </summary>
        /// <returns>True if the UI can be shown.</returns>
        protected virtual bool CanShowUI()
        {
            return true;
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            OnAttachCharacter(null);
        }
    }
}