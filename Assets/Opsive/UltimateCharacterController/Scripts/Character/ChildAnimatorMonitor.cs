/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character
{
    /// <summary>
    /// The ChildAnimatorMonitor acts as an interface for the parameters on the character's child Animator components.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class ChildAnimatorMonitor : MonoBehaviour
    {
        private static int s_HorizontalMovementHash = Animator.StringToHash("HorizontalMovement");
        private static int s_ForwardMovementHash = Animator.StringToHash("ForwardMovement");
        private static int s_PitchHash = Animator.StringToHash("Pitch");
        private static int s_YawHash = Animator.StringToHash("Yaw");
        private static int s_SpeedHash = Animator.StringToHash("Speed");
        private static int s_HeightHash = Animator.StringToHash("Height");
        private static int s_MovingHash = Animator.StringToHash("Moving");
        private static int s_AimingHash = Animator.StringToHash("Aiming");
        private static int s_MovementSetIDHash = Animator.StringToHash("MovementSetID");
        private static int s_AbilityIndexHash = Animator.StringToHash("AbilityIndex");
        private static int s_AbilityChangeHash = Animator.StringToHash("AbilityChange");
        private static int s_AbilityIntDataHash = Animator.StringToHash("AbilityIntData");
        private static int s_AbilityFloatDataHash = Animator.StringToHash("AbilityFloatData");
        private static int[] s_ItemSlotIDHash;
        private static int[] s_ItemSlotStateIndexHash;
        private static int[] s_ItemSlotStateIndexChangeHash;
        private static int[] s_ItemSlotSubstateIndexHash;

        private GameObject m_GameObject;
        private Animator m_Animator;
        private GameObject m_Character;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private AnimatorMonitor m_CharacterAnimatorMonitor;

#if FIRST_PERSON_CONTROLLER
        private bool m_FirstPersonAnimatorMonitor;
#endif
        private float m_HorizontalMovement;
        private float m_ForwardMovement;
        private float m_Pitch;
        private float m_Yaw;
        private float m_Speed;
        private int m_Height;
        private bool m_Moving;
        private bool m_Aiming;
        private int m_MovementSetID;
        private int m_AbilityIndex;
        private int m_AbilityIntData;
        private float m_AbilityFloatData;
        private int[] m_ItemSlotID;
        private int[] m_ItemSlotStateIndex;
        private int[] m_ItemSlotSubstateIndex;

        /// <summary>
        /// Cache the default values.
        /// </summary>
        private void Awake()
        {
            m_GameObject = gameObject;
            m_Animator = GetComponent<Animator>();

            m_CharacterLocomotion = gameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
#if FIRST_PERSON_CONTROLLER
            var firstPersonObjects = GetComponentInParent<FirstPersonController.Character.FirstPersonObjects>();
            m_FirstPersonAnimatorMonitor = firstPersonObjects != null;
            // If the locomotion component doesn't exist then the item is already placed under the camera.
            if (m_CharacterLocomotion == null) {
                m_CharacterLocomotion = firstPersonObjects.Character.GetCachedComponent<UltimateCharacterLocomotion>();
            }
#endif
            m_Character = m_CharacterLocomotion.gameObject;
            m_CharacterAnimatorMonitor = m_Character.GetCachedComponent<AnimatorMonitor>();

            if (m_CharacterAnimatorMonitor.HasItemParameters) {
                var slotCount = m_CharacterAnimatorMonitor.ParameterSlotCount;

                m_ItemSlotID = new int[slotCount];
                m_ItemSlotStateIndex = new int[slotCount];
                m_ItemSlotSubstateIndex = new int[slotCount];

                if (s_ItemSlotSubstateIndexHash == null) {
                    s_ItemSlotIDHash = new int[slotCount];
                    s_ItemSlotStateIndexHash = new int[slotCount];
                    s_ItemSlotStateIndexChangeHash = new int[slotCount];
                    s_ItemSlotSubstateIndexHash = new int[slotCount];
                    for (int i = 0; i < slotCount; ++i) {
                        s_ItemSlotIDHash[i] = Animator.StringToHash(string.Format("Slot{0}ItemID", i));
                        s_ItemSlotStateIndexHash[i] = Animator.StringToHash(string.Format("Slot{0}ItemStateIndex", i));
                        s_ItemSlotStateIndexChangeHash[i] = Animator.StringToHash(string.Format("Slot{0}ItemStateIndexChange", i));
                        s_ItemSlotSubstateIndexHash[i] = Animator.StringToHash(string.Format("Slot{0}ItemSubstateIndex", i));
                    }
                }
            }

            enabled = m_CharacterAnimatorMonitor != null;
            if (enabled) {
                EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
                EventHandler.RegisterEvent(m_Character, "OnCharacterSnapAnimator", SnapAnimator);
                EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
                EventHandler.RegisterEvent(m_Character, "OnRespawn", OnRespawn);
#if UNITY_EDITOR
                m_Animator.enabled = m_CharacterAnimatorMonitor.DebugAnimatorController;
#else
                m_Animator.enabled = false;
#endif
            }
        }

        /// <summary>
        /// Prepare the Animator parameters for start.
        /// </summary>
        private void Start()
        {
            SnapAnimator();
        }

        /// <summary>
        /// Synchronizes the item Animator paremeters with the character's Animator.
        /// </summary>
        public void SnapAnimator()
        {
            // The GameObject will not be enabled if the character is respawning and the weapon hasn't been equipped.
            if (m_GameObject == null || !m_GameObject.activeInHierarchy) {
                return;
            }

            m_HorizontalMovement = m_CharacterAnimatorMonitor.HorizontalMovement;
            m_ForwardMovement = m_CharacterAnimatorMonitor.ForwardMovement;
            m_Pitch = m_CharacterAnimatorMonitor.Pitch;
            m_Yaw = m_CharacterAnimatorMonitor.Yaw;
            m_Speed = m_CharacterAnimatorMonitor.Speed;
            m_Height = m_CharacterAnimatorMonitor.Height;
            m_Moving = m_CharacterAnimatorMonitor.Moving;
            m_Aiming = m_CharacterAnimatorMonitor.Aiming;
            m_MovementSetID = m_CharacterAnimatorMonitor.MovementSetID;
            m_AbilityIndex = m_CharacterAnimatorMonitor.AbilityIndex;
            m_AbilityIntData = m_CharacterAnimatorMonitor.AbilityIntData;
            m_AbilityFloatData = m_CharacterAnimatorMonitor.AbilityFloatData;
            m_Animator.SetFloat(s_HorizontalMovementHash, m_HorizontalMovement, 0, 0);
            m_Animator.SetFloat(s_ForwardMovementHash, m_ForwardMovement, 0, 0);
            m_Animator.SetFloat(s_PitchHash, m_Pitch, 0, 0);
            m_Animator.SetFloat(s_YawHash, m_Yaw, 0, 0);
            m_Animator.SetFloat(s_SpeedHash, m_Speed, 0, 0);
            m_Animator.SetFloat(s_HeightHash, m_Height, 0, 0);
            m_Animator.SetBool(s_MovingHash, m_Moving);
            m_Animator.SetBool(s_AimingHash, m_Aiming);
            m_Animator.SetInteger(s_MovementSetIDHash, m_MovementSetID);
            m_Animator.SetInteger(s_AbilityIndexHash, m_AbilityIndex);
            m_Animator.SetBool(s_AbilityChangeHash, m_CharacterAnimatorMonitor.AbilityChange);
            m_Animator.SetInteger(s_AbilityIntDataHash, m_AbilityIntData);
            m_Animator.SetFloat(s_AbilityFloatDataHash, m_AbilityFloatData, 0, 0);

            if (m_CharacterAnimatorMonitor.HasItemParameters) {
                for (int i = 0; i < m_CharacterAnimatorMonitor.ParameterSlotCount; ++i) {
                    m_ItemSlotID[i] = m_CharacterAnimatorMonitor.ItemSlotID[i];
                    m_ItemSlotStateIndex[i] = m_CharacterAnimatorMonitor.ItemSlotStateIndex[i];
                    m_ItemSlotSubstateIndex[i] = m_CharacterAnimatorMonitor.ItemSlotSubstateIndex[i];

                    m_Animator.SetInteger(s_ItemSlotIDHash[i], m_ItemSlotID[i]);
                    m_Animator.SetInteger(s_ItemSlotStateIndexHash[i], m_ItemSlotStateIndex[i]);
                    m_Animator.SetInteger(s_ItemSlotSubstateIndexHash[i], m_ItemSlotSubstateIndex[i]);
                }
            }

            // The change triggers should be enabled so the animator will snap to the idle position.
            SetAbilityChangeParameter(true);
            if (m_CharacterAnimatorMonitor.HasItemParameters) {
                for (int i = 0; i < m_CharacterAnimatorMonitor.ParameterSlotCount; ++i) {
                    SetItemStateIndexChangeParameter(i, true);
                }
            }
            // Update 0 will force the changes.
            m_Animator.Update(0);
            // Keep updating the Animator until it is no longer in a transition. This will snap the animator to the correct state immediately.
            while (IsInTrasition()) {
                m_Animator.Update(Time.fixedDeltaTime);
            }
            m_Animator.Update(0);
            m_Animator.Update(Time.fixedDeltaTime);
            // The animator should be positioned at the start of each state.
            for (int i = 0; i < m_Animator.layerCount; ++i) {
                var stateInfo = m_Animator.IsInTransition(i) ? m_Animator.GetNextAnimatorStateInfo(i) : m_Animator.GetCurrentAnimatorStateInfo(i);
                m_Animator.Play(stateInfo.fullPathHash, i, 0);
            }
            m_Animator.Update(0);
            // Prevent the change parameters from staying triggered when the animator is on the idle state.
            SetAbilityChangeParameter(false);
            if (m_CharacterAnimatorMonitor.HasItemParameters) {
                for (int i = 0; i < m_CharacterAnimatorMonitor.ParameterSlotCount; ++i) {
                    SetItemStateIndexChangeParameter(i, false);
                }
            }
        }

        /// <summary>
        /// Is the Animator Controller currently in a transition?
        /// </summary>
        /// <returns>True if any layer within the Animator Controller is within a transition.</returns>
        private bool IsInTrasition()
        {
            for (int i = 0; i < m_Animator.layerCount; ++i) {
                if (m_Animator.IsInTransition(i)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Updates the Animator at a fixed rate.
        /// </summary>
        /// <param name="deltaTime">The rate to update the animator.</param>
        public void UpdateAnimator(float deltaTime)
        {
            // The same animator may be specified across multiple active items. Only update the animator once to prevent the animation from moving too quickly.
            if (UnityEngineUtility.HasUpdatedObject(m_Animator)) {
                return;
            }
            // The animator hasn't been updated this frame. The updated object set will be cleared by the AnimatorMonitor after the frame is complete.
            UnityEngineUtility.AddUpdatedObject(m_Animator);

            m_Animator.Update(deltaTime);
        }

        /// <summary>
        /// Sets the Horizontal Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        public void SetHorizontalMovementParameter(float value, float dampingTime)
        {
            if (m_HorizontalMovement != value) {
                m_Animator.SetFloat(s_HorizontalMovementHash, value, dampingTime, TimeUtility.FixedDeltaTimeScaled / m_CharacterLocomotion.TimeScale);
                m_HorizontalMovement = m_Animator.GetFloat(s_HorizontalMovementHash);
            }
        }

        /// <summary>
        /// Sets the Forward Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        public void SetForwardMovementParameter(float value, float dampingTime)
        {
            if (m_ForwardMovement != value) {
                m_Animator.SetFloat(s_ForwardMovementHash, value, dampingTime, TimeUtility.FixedDeltaTimeScaled / m_CharacterLocomotion.TimeScale);
                m_ForwardMovement = m_Animator.GetFloat(s_ForwardMovementHash);
            }
        }

        /// <summary>
        /// Sets the Pitch parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        public void SetPitchParameter(float value, float dampingTime)
        {
            if (m_Pitch != value) {
                m_Animator.SetFloat(s_PitchHash, value, dampingTime, TimeUtility.FixedDeltaTimeScaled / m_CharacterLocomotion.TimeScale);
                m_Pitch = m_Animator.GetFloat(s_PitchHash);
            }
        }

        /// <summary>
        /// Sets the Yaw parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        public void SetYawParameter(float value, float dampingTime)
        {
            if (m_Yaw != value) {
                m_Animator.SetFloat(s_YawHash, value, dampingTime, TimeUtility.FixedDeltaTimeScaled / m_CharacterLocomotion.TimeScale);
                m_Yaw = m_Animator.GetFloat(s_YawHash);
            }
        }

        /// <summary>
        /// Sets the Speed parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        public void SetSpeedParameter(float value, float dampingTime)
        {
            if (m_Speed != value) {
                m_Animator.SetFloat(s_SpeedHash, value, dampingTime, TimeUtility.FixedDeltaTimeScaled / m_CharacterLocomotion.TimeScale);
                m_Speed = m_Animator.GetFloat(s_SpeedHash);
            }
        }

        /// <summary>
        /// Sets the Height parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetHeightParameter(int value)
        {
            if (m_Height != value) {
                m_Animator.SetFloat(s_HeightHash, value, 0, 0);
                m_Height = (int)m_Animator.GetFloat(s_HeightHash);
            }
        }

        /// <summary>
        /// Sets the Moving parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetMovingParameter(bool value)
        {
            if (m_Moving != value) {
                m_Animator.SetBool(s_MovingHash, value);
                m_Moving = value;
            }
        }

        /// <summary>
        /// Sets the Aiming parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetAimingParameter(bool value)
        {
            if (m_Aiming != value) {
                m_Animator.SetBool(s_AimingHash, value);
                m_Aiming = value;
            }
        }

        /// <summary>
        /// Sets the Movement Set ID parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetMovementSetIDParameter(int value)
        {
            if (m_MovementSetID != value) {
                m_Animator.SetInteger(s_MovementSetIDHash, value);
                m_MovementSetID = value;
            }
        }

        /// <summary>
        /// Sets the Ability Index parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetAbilityIndexParameter(int value)
        {
            if (m_AbilityIndex != value) {
                m_Animator.SetInteger(s_AbilityIndexHash, value);
                m_AbilityIndex = value;
                SetAbilityChangeParameter(true);
            }
        }

        /// <summary>
        /// Sets the Ability Index Changeparameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetAbilityChangeParameter(bool value)
        {
            if (m_Animator.GetBool(s_AbilityChangeHash) != value) {
                if (value) {
                    m_Animator.SetTrigger(s_AbilityChangeHash);
                } else {
                    m_Animator.ResetTrigger(s_AbilityChangeHash);
                }
            }
        }
        
        /// <summary>
        /// Sets the Int Data parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetAbilityIntDataParameter(int value)
        {
            if (m_AbilityIntData != value) {
                m_Animator.SetInteger(s_AbilityIntDataHash, value);
                m_AbilityIntData = value;
            }
        }

        /// <summary>
        /// Sets the Float Data parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public void SetAbilityFloatDataParameter(float value, float dampingTime)
        {
            if (m_AbilityFloatData != value) {
                m_Animator.SetFloat(s_AbilityFloatDataHash, value, dampingTime, TimeUtility.FixedDeltaTimeScaled / m_CharacterLocomotion.TimeScale);
                m_AbilityFloatData = MathUtility.Round(m_Animator.GetFloat(s_AbilityFloatDataHash), 6);
            }
        }

        /// <summary>
        /// Sets the Item ID parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot that the item occupies.</param>
        /// <param name="value">The new value.</param>
        public void SetItemIDParameter(int slotID, int value)
        {
            if (m_ItemSlotID[slotID] != value) {
                m_Animator.SetInteger(s_ItemSlotIDHash[slotID], value);
                m_ItemSlotID[slotID] = value;
                // Even though no state index was changed the trigger should be set to true so the animator can transition to the new item id.
                SetItemStateIndexChangeParameter(slotID, value != 0);
            }
        }

        /// <summary>
        /// Sets the Primary Item State Index parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot that the item occupies.</param>
        /// <param name="value">The new value.</param>
        public void SetItemStateIndexParameter(int slotID, int value)
        {
            if (m_ItemSlotStateIndex[slotID] != value) {
                m_Animator.SetInteger(s_ItemSlotStateIndexHash[slotID], value);
                m_ItemSlotStateIndex[slotID] = value;
                SetItemStateIndexChangeParameter(slotID, value != 0);
            }
        }

        /// <summary>
        /// Sets the Item State Index Change parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot of that item that should be set.</param>
        /// <param name="value">The new value.</param>
        public void SetItemStateIndexChangeParameter(int slotID, bool value)
        {
            if (m_Animator.GetBool(s_ItemSlotStateIndexChangeHash[slotID]) != value) {
                if (value) {
                    m_Animator.SetTrigger(s_ItemSlotStateIndexChangeHash[slotID]);
                } else {
                    m_Animator.ResetTrigger(s_ItemSlotStateIndexChangeHash[slotID]);
                }
            }
        }

        /// <summary>
        /// Sets the Item Substate Index parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot that the item occupies.</param>
        /// <param name="value">The new value.</param>
        public void SetItemSubstateIndexParameter(int slotID, int value)
        {
            if (m_ItemSlotSubstateIndex[slotID] != value) {
                m_Animator.SetInteger(s_ItemSlotSubstateIndexHash[slotID], value);
                m_ItemSlotSubstateIndex[slotID] = value;
            }
        }

        /// <summary>
        /// Executes an event on the EventHandler.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        public void ExecuteEvent(string eventName)
        {
#if FIRST_PERSON_CONTROLLER
            // Don't execute the event if the perspective doesn't match.
            if (m_FirstPersonAnimatorMonitor != m_CharacterLocomotion.FirstPersonPerspective) {
                return;
            }
#endif
#if UNITY_EDITOR
            if (m_CharacterAnimatorMonitor.LogEvents) {
                Debug.Log("Execute " + eventName);
            }
#endif
            EventHandler.ExecuteEvent(m_Character, eventName);
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            m_Animator.enabled = true;
            if (m_Animator.isActiveAndEnabled) {
                m_Animator.Update(0);
            }
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
#if UNITY_EDITOR
            m_Animator.enabled = m_CharacterAnimatorMonitor.DebugAnimatorController;
#else
            m_Animator.enabled = false;
#endif
            SnapAnimator();
        }

        /// <summary>
        /// The character's position or rotation has been teleported.
        /// </summary>
        /// <param name="snapAnimator">Should the animator be snapped?</param>
        private void OnImmediateTransformChange(bool snapAnimator)
        {
            if (!snapAnimator) {
                return;
            }

            SnapAnimator();
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (m_CharacterAnimatorMonitor != null) {
                EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
                EventHandler.UnregisterEvent(m_Character, "OnSnapAnimator", SnapAnimator);
                EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
                EventHandler.UnregisterEvent(m_Character, "OnRespawn", OnRespawn);
            }
        }
    }
}