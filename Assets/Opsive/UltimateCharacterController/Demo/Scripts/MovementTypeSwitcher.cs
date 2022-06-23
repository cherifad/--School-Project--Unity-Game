/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.StateSystem;

namespace Opsive.UltimateCharacterController.Demo
{
    /// <summary>
    /// Allows the character to switch between movement types.
    /// </summary>
    public class MovementTypeSwitcher : MonoBehaviour
    {
        [Tooltip("The keycode that should trigger the switch.")]
        [SerializeField] protected KeyCode m_SwitchKeycode = KeyCode.Return;
        [Tooltip("Can the top down and 2.5D movement type be switched to with a third person perspective?")]
        [SerializeField] protected bool m_IncludeTopDownPseudo3D;

        private string[] m_FirstPersonMovementStates = new string[] { "FirstPersonCombat", "FreeLook" };
        private string[] m_ThirdPersonMovementStates = new string[] { "Adventure", "ThirdPersonCombat", "RPG", "Top Down", "2.5D" };

        private GameObject m_Character;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private CharacterHealth m_CharacterHealth;

        private int m_ActiveFirstPersonIndex;
        private int m_ActiveThirdPersonIndex;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Start()
        {
            var camera = Utility.UnityEngineUtility.FindCamera(null);
            m_Character = camera.GetComponent<CameraController>().Character;
            m_CharacterLocomotion = m_Character.GetComponent<UltimateCharacterLocomotion>();
            m_CharacterHealth = m_Character.GetComponent<CharacterHealth>();

            EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);

            if (m_CharacterLocomotion.FirstPersonPerspective) {
                StateManager.SetState(m_Character, m_FirstPersonMovementStates[m_ActiveFirstPersonIndex], true);
            } else {
                StateManager.SetState(m_Character, m_ThirdPersonMovementStates[m_ActiveThirdPersonIndex], true);
            }
        }

        /// <summary>
        /// Switches the movement type when the specified keycode is pressed.
        /// </summary>
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(m_SwitchKeycode)) {
                // The character needs to be alive to switch movement types.
                if (!m_CharacterHealth.IsAlive()) {
                    return;
                }

                // The movement type cannot be switched if ride is active.
                if (m_CharacterLocomotion.IsAbilityTypeActive<Character.Abilities.Ride>()) {
                    return;
                }

                if (m_CharacterLocomotion.FirstPersonPerspective) {
                    StateManager.SetState(m_Character, m_FirstPersonMovementStates[m_ActiveFirstPersonIndex], false);
                    m_ActiveFirstPersonIndex = (m_ActiveFirstPersonIndex + 1) % m_FirstPersonMovementStates.Length;
                    StateManager.SetState(m_Character, m_FirstPersonMovementStates[m_ActiveFirstPersonIndex], true);
                } else {
#if THIRD_PERSON_CONTROLLER
                    if (!m_IncludeTopDownPseudo3D) {
                        // The state cannot be switched if the top down or 2.5D movement type is active.
                        if (m_CharacterLocomotion.ActiveMovementType is ThirdPersonController.Character.MovementTypes.TopDown ||
                            m_CharacterLocomotion.ActiveMovementType is ThirdPersonController.Character.MovementTypes.Pseudo3D) {
                            return;
                        }
                    }
#endif

                    StateManager.SetState(m_Character, m_ThirdPersonMovementStates[m_ActiveThirdPersonIndex], false);
                    m_ActiveThirdPersonIndex = (m_ActiveThirdPersonIndex + 1) % (m_ThirdPersonMovementStates.Length - (m_IncludeTopDownPseudo3D ? 0 : 2));
                    StateManager.SetState(m_Character, m_ThirdPersonMovementStates[m_ActiveThirdPersonIndex], true);
                }
            }
        }

        /// <summary>
        /// The camera perspective between first and third person has changed.
        /// </summary>
        /// <param name="inFirstPerson">Is the camera in a first person view?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            // Wait a frame before changing states to prevent the movement type from switching the same frame the movement type is currently being switched.
            Scheduler.ScheduleFixed(Time.fixedDeltaTime, UpdateStates, firstPersonPerspective);
        }

        /// <summary>
        /// Updates the states depending on the perspective that was switched.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the camera in a first person view?</param>
        private void UpdateStates(bool firstPersonPerspective)
        {
            if (firstPersonPerspective) {
                StateManager.SetState(m_Character, m_ThirdPersonMovementStates[m_ActiveThirdPersonIndex], false);
                StateManager.SetState(m_Character, m_FirstPersonMovementStates[m_ActiveFirstPersonIndex], true);
            } else {
                StateManager.SetState(m_Character, m_FirstPersonMovementStates[m_ActiveFirstPersonIndex], false);
                StateManager.SetState(m_Character, m_ThirdPersonMovementStates[m_ActiveThirdPersonIndex], true);
            }
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (m_Character != null) {
                EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
            }
        }
    }
}