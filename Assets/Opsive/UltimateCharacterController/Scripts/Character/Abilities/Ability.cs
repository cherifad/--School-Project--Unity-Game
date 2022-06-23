/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using System;
using Opsive.UltimateCharacterController.Audio;
using Opsive.UltimateCharacterController.Character.Effects;
using Opsive.UltimateCharacterController.Input;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Motion;
using Opsive.UltimateCharacterController.Objects.CharacterAssist;
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    /// Abilities extend the character controller to allow new functionality without having to directly change the core character controller code. Abilities can change 
    /// any aspect of the character controller, from disabling gravity to setting the Animator Controller parameters. Abilities are synchronized over the network.
    /// </summary>
    [Serializable]
    public abstract class Ability : StateObject
    {
        /// <summary>
        /// Specifies how the ability can be started. 
        /// </summary>
        public enum AbilityStartType
        {
            Automatic,      // The ability will try to be started every update.
            Manual,         // The ability must be started with TryStartAbility.
            ButtonDown,     // The ability will start when the specified button is down.
            DoublePress,    // The ability will start when the specified button is pressed twice.
            LongPress,      // The ability will start when the specified button has been pressed for more than the specified duration.
            Tap,            // The ability will start when the specified button is quickly tapped.
            Axis            // The ability will start when the specified axis is a non-zero value.
        }
        /// <summary>
        /// Specifies how the ability can be stopped. Button toggle means that the same button has to be pressed again after the ability has started to stop the ability.
        /// </summary>
        public enum AbilityStopType
        {
            Automatic,      // The ability will try to be stopped every update.
            Manual,         // The ability must be stopped with TryStopAbility.
            ButtonUp,       // The ability will stop when the specified button is up.
            ButtonDown,     // The ability will stop when the specified button is down.
            ButtonToggle,   // The ability will stop when the same button as been pressed again after the ability has started.
            LongPress,      // The ability will stop when the specified button has been pressed for more than the specified duration.
            Axis            // The ability will stop when the specified axis is a zero value.
        }
        /// <summary>
        /// Specifies if the ability should override the bool value of the controller.
        /// </summary>
        public enum AbilityBoolOverride
        {
            NoOverride,     // The ability should not override the value.
            True,           // The ability should set the value to true.
            False           // The ability should set the value to false.
        }

        [Tooltip("Can the ability be activated?")]
        [HideInInspector] [SerializeField] protected bool m_Enabled = true;
        [Tooltip("Specifies how the ability can be started.")]
        [HideInInspector] [SerializeField] protected AbilityStartType m_StartType = AbilityStartType.Automatic;
        [Tooltip("Specifies how the ability can be stopped.")]
        [HideInInspector] [SerializeField] protected AbilityStopType m_StopType = AbilityStopType.Manual;
        [Tooltip("The button name(s) that can start or stop the ability.")]
        [HideInInspector] [SerializeField] protected string[] m_InputNames;
        [Tooltip("Specifies how long the button should be pressed down until the ability starts/stops. Only used when the ability has a start/stop type of LongPress.")]
        [HideInInspector] [SerializeField] protected float m_LongPressDuration = 0.5f;
        [Tooltip("Should the long press wait to be activated until the button has been released?")]
        [HideInInspector] [SerializeField] protected bool m_WaitForLongPressRelease;
        [Tooltip("Optionally specify an attribute that the ability should affect when active.")]
        [HideInInspector] [SerializeField] protected Traits.AttributeModifier m_AttributeModifier = new Traits.AttributeModifier();
        [Tooltip("Specifies the name of the state that the ability should use when active.")]
        [HideInInspector] [SerializeField] protected string m_State;
        [Tooltip("Should the ItemType name be appened to the name of the state name?")]
        [HideInInspector] [SerializeField] protected bool m_StateAppendItemTypeName;
        [Tooltip("Specifies the value to set the Ability Index parameter to. -1 will not set the parameter.")]
        [HideInInspector] [SerializeField] protected int m_AbilityIndexParameter = -1;
        [Tooltip("A set of AudioClips that can be played when the ability is started.")]
        [HideInInspector] [SerializeField] protected AudioClipSet m_StartAudioClipSet = new AudioClipSet();
        [Tooltip("A set of AudioClips that can be played when the ability is started.")]
        [HideInInspector] [SerializeField] protected AudioClipSet m_StopAudioClipSet = new AudioClipSet();
        [Tooltip("Does the ability allow positional input?")]
        [HideInInspector] [SerializeField] protected bool m_AllowPositionalInput = true;
        [Tooltip("Does the ability allow rotational input?")]
        [HideInInspector] [SerializeField] protected bool m_AllowRotationalInput = true;
        [Tooltip("Should the character use gravity while the ability is active?")]
        [HideInInspector] [SerializeField] protected AbilityBoolOverride m_UseGravity = AbilityBoolOverride.NoOverride;
        [Tooltip("Can the character use root motion for positioning?")]
        [HideInInspector] [SerializeField] protected AbilityBoolOverride m_UseRootMotionPosition = AbilityBoolOverride.NoOverride;
        [Tooltip("Can the character use root motion for rotation?")]
        [HideInInspector] [SerializeField] protected AbilityBoolOverride m_UseRootMotionRotation = AbilityBoolOverride.NoOverride;
        [Tooltip("Should the character detect horizontal collisions while the ability is active?")]
        [HideInInspector] [SerializeField] protected AbilityBoolOverride m_DetectHorizontalCollisions = AbilityBoolOverride.NoOverride;
        [Tooltip("Should the character detect vertical collisions while the ability is active?")]
        [HideInInspector] [SerializeField] protected AbilityBoolOverride m_DetectVerticalCollisions = AbilityBoolOverride.NoOverride;
        [Tooltip("A reference to the AnimatorMotion that the ability uses.")]
        [HideInInspector] [SerializeField] protected AnimatorMotion m_AnimatorMotion;
        [Tooltip("A mask specifying which slots can have the item equipped when the ability is active.")]
        [HideInInspector] [SerializeField] protected int m_AllowEquippedSlotsMask = -1;
        [Tooltip("Should the ability equip the slots that were unequipped when the ability started?")]
        [HideInInspector] [SerializeField] protected bool m_ReequipSlots = true;
        [Tooltip("The text that should be shown by the message monitor when the ability can start.")]
        [HideInInspector] [SerializeField] protected string m_AbilityMessageText;
        [Tooltip("The sprite that should be drawn by the message monitor when the ability can start.")]
        [HideInInspector] [SerializeField] protected Sprite m_AbilityMessageIcon;

        public bool Enabled { get { return m_Enabled; } set { m_Enabled = value; if (!m_Enabled && IsActive) { StopAbility(true, false); } } }
        public AbilityStartType StartType
        {
            get { return m_StartType; }
            set
            {
                var autoStartType = m_StartType == AbilityStartType.Automatic;
                m_StartType = value;
                if (autoStartType && IsActive && m_StartType != AbilityStartType.Automatic) {
                    StopAbility();
                }
            }
        }
        public AbilityStopType StopType { get { return m_StopType; } set { m_StopType = value; } }
        public string[] InputNames { get { return m_InputNames; } set { m_InputNames = value; } }
        public float LongPressDuration { get { return m_LongPressDuration; } set { m_LongPressDuration = value; } }
        public bool WaitForLongPressRelease { get { return m_WaitForLongPressRelease; } set { m_WaitForLongPressRelease = value; } }
        [Utility.NonSerialized] public Traits.AttributeModifier AttributeModifier { get { return m_AttributeModifier; } set { m_AttributeModifier = value; } }
        public string State { get { return m_State; } set { m_State = value; } }
        public virtual int AbilityIndexParameter { get { return m_AbilityIndexParameter; } set { m_AbilityIndexParameter = value; } }
        public AudioClipSet StartAudioClipSet { get { return m_StartAudioClipSet; } set { m_StartAudioClipSet = value; } }
        public AudioClipSet StopAudioClipSet { get { return m_StopAudioClipSet; } set { m_StopAudioClipSet = value; } }
        public bool AllowPositionalInput { get { return m_AllowPositionalInput; } set { m_AllowPositionalInput = value; } }
        public bool AllowRotationalInput { get { return m_AllowRotationalInput; } set { m_AllowRotationalInput = value; } }
        public AbilityBoolOverride UseGravity { get { return m_UseGravity; } set { m_UseGravity = value; } }
        public AbilityBoolOverride UseRootMotionPosition { get { return m_UseRootMotionPosition; } set { m_UseRootMotionPosition = value; } }
        public AbilityBoolOverride UseRootMotionRotation { get { return m_UseRootMotionRotation; } set { m_UseRootMotionRotation = value; } }
        public AbilityBoolOverride DetectHorizontalCollisions { get { return m_DetectHorizontalCollisions; } set { m_DetectHorizontalCollisions = value; } }
        public AbilityBoolOverride DetectVerticalCollisions { get { return m_DetectVerticalCollisions; } set { m_DetectVerticalCollisions = value; } }
        public AnimatorMotion AnimatorMotion { get { return m_AnimatorMotion; } set { m_AnimatorMotion = value; } }
        public int AllowEquippedSlotsMask { get { return m_AllowEquippedSlotsMask; } set { m_AllowEquippedSlotsMask = value; } }
        public bool ReequipSlots { get { return m_ReequipSlots; } set { m_ReequipSlots = value; } }
        public virtual string AbilityMessageText { get { return m_AbilityMessageText; } set { m_AbilityMessageText = value; } }
        public Sprite AbilityMessageIcon { get { return m_AbilityMessageIcon; } set { m_AbilityMessageIcon = value; } }

        protected UltimateCharacterLocomotion m_CharacterLocomotion;
        protected GameObject m_GameObject;
        protected Transform m_Transform;
        protected AnimatorMonitor m_AnimatorMonitor;
        protected LayerManager m_LayerManager;
        protected Inventory.InventoryBase m_Inventory;

        private bool m_AbilityMessageCanStart;
        private bool[] m_ButtonUp;
        private int m_Index = -1;
        private int m_ActiveIndex = -1;
        private int m_InputIndex = -1;
        private float m_InputAxisValue;
        private bool[] m_ActiveInput;
        private float m_StartTime = -1;
        private int m_ActiveCount;

        public bool IsActive { get { return m_ActiveIndex != -1; } }
        [Utility.NonSerialized] public int Index { get { return m_Index; } set { m_Index = value; } }
        [Utility.NonSerialized] public int ActiveIndex { get { return m_ActiveIndex; } set { m_ActiveIndex = value; } }
        [Utility.NonSerialized, Snapshot] public int InputIndex { get { return m_InputIndex; } set { m_InputIndex = value; } }
        [Utility.NonSerialized, Snapshot] public float InputAxisValue { get { return m_InputAxisValue; } set { m_InputAxisValue = value; } }
        public float StartTime { get { return m_StartTime; } }
        public bool AbilityMessageCanStart
        {
            set {
                if (m_AbilityMessageCanStart != value) {
                    m_AbilityMessageCanStart = value;
                    Events.EventHandler.ExecuteEvent(m_GameObject, "OnAbilityMessageCanStart", this, value);
                }
            }
        }

        public virtual bool IsConcurrent { get { return false; } }
        public virtual bool IgnorePriority { get { return false; } }
        public virtual bool CanReceiveMultipleStarts { get { return false; } }
        public virtual bool CanStayActivatedOnDeath { get { return false; } }
        [Utility.NonSerialized] public virtual int AbilityIntData { get { return -1; } set { /* Intentionally left blank. */ } }
        public virtual float AbilityFloatData { get { return -1; } }
#if UNITY_EDITOR
        // Allows the ability to show a brief description within the inspector's ReorderableList.
        public virtual string InspectorListDescription { get { return string.Empty; } }
#endif
        protected bool AllowRootMotionPosition { set { m_CharacterLocomotion.AllowRootMotionPosition = value; } }
        protected bool AllowRootMotionRotation { set { m_CharacterLocomotion.AllowRootMotionRotation = value; } }

        /// <summary>
        /// Initializes the ability to the specified character controller.
        /// </summary>
        /// <param name="characterLocomotion">The character locomotion component to initialize the ability to.</param>
        /// <param name="index">The prioirty index of the ability within the controller.</param>
        public void Initialize(UltimateCharacterLocomotion characterLocomotion, int index)
        {
            m_CharacterLocomotion = characterLocomotion;
            m_GameObject = characterLocomotion.gameObject;
            m_Transform = characterLocomotion.transform;
            m_AnimatorMonitor = m_GameObject.GetCachedComponent<AnimatorMonitor>();
            m_LayerManager = m_GameObject.GetCachedComponent<LayerManager>();
            m_Inventory = m_GameObject.GetCachedComponent<Inventory.InventoryBase>();
            m_Index = index;

            // Initialze the ButtonUp for as many InputNames there are.
            if (m_InputNames != null && m_InputNames.Length > 0) {
                m_ButtonUp = new bool[m_InputNames.Length];
                for (int i = 0; i < m_InputNames.Length; ++i) {
                    m_ButtonUp[i] = true;
                }
                m_ActiveInput = new bool[m_InputNames.Length];
            }

            if (m_AttributeModifier != null) {
                m_AttributeModifier.Initialize(m_GameObject);
            }

            // The StateObject class needs to initialize itself.
            Initialize(m_GameObject);
        }

        /// <summary>
        /// Can the input start the ability?
        /// </summary>
        /// <param name="playerInput">A reference to the input component.</param>
        /// <returns>True if the input can start the ability.</returns>
        public virtual bool CanInputStartAbility(PlayerInput playerInput)
        {
            if (m_InputNames != null && m_InputNames.Length > 0) {
                for (int i = 0; i < m_InputNames.Length; ++i) {
                    if (m_ActiveInput[i] && !CanReceiveMultipleStarts) {
                        continue;
                    }

                    // For any start types that require the button to be down make sure it has first returned to the up state before checking if it is down again.
                    if (!m_ButtonUp[i] && (m_StartType == AbilityStartType.ButtonDown || m_StartType == AbilityStartType.DoublePress || 
                                            m_StartType == AbilityStartType.LongPress || m_StartType == AbilityStartType.Tap)) {
                        m_ButtonUp[i] = !playerInput.GetButton(m_InputNames[i]);
                    }

                    if (m_ButtonUp[i] &&
                        (m_StartType == AbilityStartType.ButtonDown && playerInput.GetButton(m_InputNames[i])) ||
                        (m_StartType == AbilityStartType.DoublePress && playerInput.GetDoublePress(m_InputNames[i])) ||
                        (m_StartType == AbilityStartType.LongPress && playerInput.GetLongPress(m_InputNames[i], m_LongPressDuration, m_WaitForLongPressRelease)) ||
                        (m_StartType == AbilityStartType.Tap && playerInput.GetTap(m_InputNames[i]))) {
                        m_InputIndex = i;
                        return true;
                    }

                    float axisValue;
                    if (m_StartType == AbilityStartType.Axis && Mathf.Abs((axisValue = playerInput.GetAxisRaw(m_InputNames[i]))) > 0.00001f) {
                        m_InputIndex = i;
                        m_InputAxisValue = axisValue;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Can the input stop the ability?
        /// </summary>
        /// <param name="playerInput">A reference to the input component.</param>
        /// <returns>True if the input can stop the ability.</returns>
        public virtual bool CanInputStopAbility(PlayerInput playerInput)
        {
            if (m_InputNames != null && m_InputNames.Length > 0) {
                for (int i = 0; i < m_InputNames.Length; ++i) {
                    if (!m_ActiveInput[i]) {
                        continue;
                    }
                    if (m_StopType == AbilityStopType.ButtonToggle) {
                        var inputButton = playerInput.GetButton(m_InputNames[i]);
                        // A toggled button means the button has to be pressed and released before the Ability can be stopped.
                        if (m_ButtonUp[i] && inputButton) {
                            m_ButtonUp[i] = false;
                            m_InputIndex = i;
                            return true;
                        } else if (!m_ButtonUp[i] && !inputButton) {
                            // Now that the button is up the Ability can be stopped when the button is down again.
                            m_ButtonUp[i] = true;
                        }
                    } else if ((m_StopType == AbilityStopType.ButtonUp && !playerInput.GetButton(m_InputNames[i])) ||
                               (m_StopType == AbilityStopType.ButtonDown && playerInput.GetButton(m_InputNames[i])) ||
                               (m_StopType == AbilityStopType.LongPress && playerInput.GetLongPress(m_InputNames[i], m_LongPressDuration, m_WaitForLongPressRelease))) {
                        m_InputIndex = i;
                        return true;
                    } else if (m_StopType == AbilityStopType.Axis && Mathf.Abs(playerInput.GetAxisRaw(m_InputNames[i])) <= 0.00001f) {
                        m_InputIndex = i;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Method called by MonoBehaviour.Awake. Can be used for initialization.
        /// </summary>
        public virtual void Awake() { }

        /// <summary>
        /// Method called by MonoBehaviour.Start. This method is called on all abilities when the MonoBehaviour.Start method is called.
        /// </summary>
        public virtual void Start() { }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public virtual bool CanStartAbility() { return (m_AttributeModifier == null || m_AttributeModifier.IsValid()); }

        /// <summary>
        /// Returns the possible AbilityStartLocations that the character can move towards.
        /// </summary>
        /// <returns>The possible AbilityStartLocations that the character can move towards.</returns>
        public virtual AbilityStartLocation[] GetStartLocations() { return null; }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public virtual bool ShouldBlockAbilityStart(Ability startingAbility) { return false; }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public virtual bool ShouldStopActiveAbility(Ability activeAbility) { return false; }

        /// <summary>
        /// The ability will start - perform any initialization before starting.
        /// </summary>
        /// <returns>True if the ability should start.</returns>
        public virtual bool AbilityWillStart() { return true; }

        /// <summary>
        /// Tries to start the ability.
        /// </summary>
        /// <returns>True if the ability was successfully started.</param>
        public bool StartAbility()
        {
            return m_CharacterLocomotion.TryStartAbility(this);
        }

        /// <summary>
        /// Starts executing the ability.
        /// </summary>
        /// <param name="index">The index of the started ability.</param>
        public void StartAbility(int index)
        {
            m_ActiveIndex = index;

            AbilityStarted();
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected virtual void AbilityStarted()
        {
            AbilityStarted(true);
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        /// <param name="enableAttributeModifier">Should the attribute modifier be enabled?</param>
        protected void AbilityStarted(bool enableAttributeModifier)
        {
            SetState(true);

            AbilityMessageCanStart = false;
            if (m_InputIndex != -1) {
                m_ActiveInput[m_InputIndex] = true;
                // The button is no longer up if the start type requires the button to be done.
                if (m_StartType == AbilityStartType.ButtonDown || m_StartType == AbilityStartType.DoublePress ||
                    m_StartType == AbilityStartType.LongPress || m_StartType == AbilityStartType.Tap) {
                    m_ButtonUp[m_InputIndex] = false;
                }
            }
            m_InputIndex = -1;

            m_StartAudioClipSet.PlayAudioClip(m_GameObject);

            if (m_UseGravity == AbilityBoolOverride.True) {
                m_CharacterLocomotion.ForceUseGravity = true;
            } else if (m_UseGravity == AbilityBoolOverride.False) {
                m_CharacterLocomotion.AllowUseGravity = false;
            }
            if (m_UseRootMotionPosition == AbilityBoolOverride.True) {
                m_CharacterLocomotion.ForceRootMotionPosition = true;
            } else if (m_UseRootMotionPosition == AbilityBoolOverride.False) {
                m_CharacterLocomotion.AllowRootMotionPosition = false;
            }
            if (m_UseRootMotionRotation == AbilityBoolOverride.True) {
                m_CharacterLocomotion.ForceRootMotionRotation = true;
            } else if (m_UseRootMotionPosition == AbilityBoolOverride.False) {
                m_CharacterLocomotion.AllowRootMotionRotation = false;
            }
            if (m_DetectHorizontalCollisions == AbilityBoolOverride.True) {
                m_CharacterLocomotion.ForceHorizontalCollisionDetection = true;
            } else if (m_DetectHorizontalCollisions == AbilityBoolOverride.False) {
                m_CharacterLocomotion.AllowHorizontalCollisionDetection = false;
            }
            if (m_DetectVerticalCollisions == AbilityBoolOverride.True) {
                m_CharacterLocomotion.ForceVerticalCollisionDetection = true;
            } else if (m_DetectVerticalCollisions == AbilityBoolOverride.False) {
                m_CharacterLocomotion.AllowVerticalCollisionDetection = false;
            }
            m_StartTime = Time.time;
            m_ActiveCount++;

            if (enableAttributeModifier && m_AttributeModifier != null) {
                m_AttributeModifier.EnableModifier(true);
            }

            if (m_Inventory != null && m_ActiveCount == 1) {
                Events.EventHandler.RegisterEvent<Item, int>(m_GameObject, "OnInventoryEquipItem", OnEquipItem);
                Events.EventHandler.RegisterEvent<Item, int>(m_GameObject, "OnInventoryUnequipItem", OnUnequipItem);
            }
        }

        /// <summary>
        /// Activates or detactives the abilities state.
        /// </summary>
        /// <param name="active">Is the ability active?</param>
        protected void SetState(bool active)
        {
            if (!string.IsNullOrEmpty(m_State)) {
                StateManager.SetState(m_GameObject, m_State, active);
                if (m_Inventory != null && m_StateAppendItemTypeName) {
                    for (int i = 0; i < m_Inventory.SlotCount; ++i) {
                        var item = m_Inventory.GetItem(i);
                        if (item != null && item.IsActive()) {
                            var itemStateName = m_State + item.ItemType.name;
                            StateManager.SetState(m_GameObject, itemStateName, active);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// An item has been equipped.
        /// </summary>
        /// <param name="itemType">The equipped item.</param>
        /// <param name="slotID">The slot that the item now occupies.</param>
        private void OnEquipItem(Item item, int slotID)
        {
            if (IsActive && !string.IsNullOrEmpty(m_State)) {
                var itemStateName = m_State + item.ItemType.name;
                StateManager.SetState(m_GameObject, itemStateName, true);
            }
        }

        /// <summary>
        /// An item has been unequipped.
        /// </summary>
        /// <param name="item">The item that was unequipped.</param>
        /// <param name="slotID">The slot that the item was unequipped from.</param>
        private void OnUnequipItem(Item item, int slotID)
        {
            if (IsActive && !string.IsNullOrEmpty(m_State)) {
                var itemStateName = m_State + item.ItemType.name;
                StateManager.SetState(m_GameObject, itemStateName, false);
            }
        }

        /// <summary>
        /// A general purpose update for inactive abilities.
        /// </summary>
        public virtual void InactiveUpdate() { }
        
        /// <summary>
        /// Updates the ability. Called before the character movements are applied.
        /// </summary>
        public virtual void Update()
        {
            // The attribute has a chance to stop the ability if it reached the min value. An example use case for this is if the stamina ran out and the character
            // can no longer run.
            if (m_AttributeModifier != null && !m_AttributeModifier.IsValid()) {
                StopAbility();
            }
        }

        /// <summary>
        /// Update the controller's rotation values.
        /// </summary>
        public virtual void UpdateRotation() { }

        /// <summary>
        /// Verify the rotation values. Called immediately before the rotation is applied.
        /// </summary>
        public virtual void ApplyRotation() { }

        /// <summary>
        /// Update the controller's position values.
        /// </summary>
        public virtual void UpdatePosition() { }

        /// <summary>
        /// Verify the position values. Called immediately before the position is applied.
        /// </summary>
        public virtual void ApplyPosition() { }

        /// <summary>
        /// Update the ability's Animator parameters. Called after the position has been applied.
        /// </summary>
        public virtual void UpdateAnimator() { }

        /// <summary>
        /// Updates the ability after the character movements have been applied.
        /// </summary>
        public virtual void LateUpdate() { }

        /// <summary>
        /// Callback when the ability tries to be stopped. This method allows for any actions required before the ability actually stops.
        /// </summary>
        public virtual void WillTryStopAbility() { }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public virtual bool CanStopAbility() { return true; }

        /// <summary>
        /// Can the ability be force stopped?
        /// </summary>
        /// <returns>True if the ability can be force stopped.</returns>
        public virtual bool CanForceStopAbility() { return true; }

        /// <summary>
        /// Stop the ability from running.
        /// </summary>
        /// <returns>Was the ability stopped?</returns>
        public bool StopAbility()
        {
            return StopAbility(false, false);
        }

        /// <summary>
        /// Stop the ability from running.
        /// </summary>
        /// <param name="force">Should the ability be force stopped?</param>
        /// <returns>Was the ability stopped?</returns>
        public bool StopAbility(bool force)
        {
            return StopAbility(force, false);
        }

        /// <summary>
        /// Stop the ability from running.
        /// </summary>
        /// <param name="force">Should the ability be force stopped?</param>
        /// <param name="fromController">Is the ability being stopped from the UltimateCharacterController?</param>
        /// <returns>Was the ability stopped?</returns>
        public bool StopAbility(bool force, bool fromController)
        {
            // If the ability wasn't stopped from the character controller then call the controller's stop ability method. The controller must be aware of the stopping.
            if (!fromController) {
                return m_CharacterLocomotion.TryStopAbility(this, force);
            }

            m_ActiveIndex = -1;

            AbilityStopped(force);

            return true;
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected virtual void AbilityStopped(bool force)
        {
            m_InputIndex = -1;
            m_StopAudioClipSet.PlayAudioClip(m_GameObject);

            SetState(false);
            ResetInput(force);

            if (m_UseGravity == AbilityBoolOverride.True) {
                m_CharacterLocomotion.ForceUseGravity = false;
            } else if (m_UseGravity == AbilityBoolOverride.False) {
                m_CharacterLocomotion.AllowUseGravity = true;
            }
            if (m_UseRootMotionPosition == AbilityBoolOverride.True) {
                m_CharacterLocomotion.ForceRootMotionPosition = false;
            } else if (m_UseRootMotionPosition == AbilityBoolOverride.False) {
                m_CharacterLocomotion.AllowRootMotionPosition = true;
            }
            if (m_UseRootMotionRotation == AbilityBoolOverride.True) {
                m_CharacterLocomotion.ForceRootMotionRotation = false;
            } else if (m_UseRootMotionPosition == AbilityBoolOverride.False) {
                m_CharacterLocomotion.AllowRootMotionRotation = true;
            }
            if (m_DetectHorizontalCollisions == AbilityBoolOverride.True) {
                m_CharacterLocomotion.ForceHorizontalCollisionDetection = false;
            } else if (m_DetectHorizontalCollisions == AbilityBoolOverride.False) {
                m_CharacterLocomotion.AllowHorizontalCollisionDetection = true;
            }
            if (m_DetectVerticalCollisions == AbilityBoolOverride.True) {
                m_CharacterLocomotion.ForceVerticalCollisionDetection = false;
            } else if (m_DetectVerticalCollisions == AbilityBoolOverride.False) {
                m_CharacterLocomotion.AllowVerticalCollisionDetection = true;
            }
            m_StartTime = -1;
            m_ActiveCount--;

            if (m_AttributeModifier != null) {
                m_AttributeModifier.EnableModifier(false);
            }
            if (m_ActiveCount == 0 && m_Inventory != null) {
                Events.EventHandler.UnregisterEvent<Item, int>(m_GameObject, "OnInventoryEquipItem", OnEquipItem);
                Events.EventHandler.UnregisterEvent<Item, int>(m_GameObject, "OnInventoryUnequipItem", OnUnequipItem);
            }
        }

        /// <summary>
        /// Resets the input variables back to their starting value.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected void ResetInput(bool force)
        {
            if (m_ActiveInput != null) {
                for (int i = 0; i < m_ActiveInput.Length; ++i) {
                    m_ActiveInput[i] = false;
                    // If the ability is force stopped then the button up state should be reset so it can be started again when the button is down.
                    if (force && (m_StartType == AbilityStartType.ButtonDown || m_StartType == AbilityStartType.DoublePress ||
                                    m_StartType == AbilityStartType.LongPress || m_StartType == AbilityStartType.Tap)) {
                        m_ButtonUp[i] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the ButtonUp variable to the correct value.
        /// </summary>
        /// <param name="playerInput">A reference to the input component.</param>
        public void CheckInput(PlayerInput playerInput)
        {
            if (m_ButtonUp == null) {
                return;
            }

            // Reset the button input if the input shouldn't be checked.
            var forceUp = m_StartType == AbilityStartType.Automatic || m_StartType == AbilityStartType.Axis || m_StartType == AbilityStartType.Manual || !ShouldCheckInput();
            for (int i = 0; i < m_ButtonUp.Length; ++i) {
                m_ButtonUp[i] = forceUp || !playerInput.GetButton(m_InputNames[i]);
            }
        }

        /// <summary>
        /// Should the input be checked to ensure button up is using the correct value?
        /// </summary>
        protected virtual bool ShouldCheckInput() { return true; }

        /// <summary>
        /// The Animator is requesting the animation's delta position and rotation. The ability can use an AnimationMotion ScriptableObject to generate move data without using
        /// root motion.
        /// </summary>
        /// <param name="updatePosition">Is the position being updated? If false then the rotation is used.</param>
        public void OnAnimatorMove(bool updatePosition)
        {
            if (m_AnimatorMotion == null) {
                return;
            }

            // Evaluate the curve at the current time.
            if (updatePosition) {
                var deltaPosition = m_CharacterLocomotion.AnimatorDeltaPosition;
                m_AnimatorMotion.EvaluatePosition(Time.time - m_StartTime, ref deltaPosition);
                m_CharacterLocomotion.AnimatorDeltaPosition += m_Transform.TransformDirection(deltaPosition);
            } else { // Update rotation.
                var deltaRotation = m_CharacterLocomotion.AnimatorDeltaRotation;
                m_AnimatorMotion.EvaluateRotation(Time.time - m_StartTime, ref deltaRotation);
                m_CharacterLocomotion.AnimatorDeltaRotation *= deltaRotation;
            }
        }

        /// <summary>
        /// The character has entered a trigger.
        /// </summary>
        /// <param name="other">The trigger collider that the character entered.</param>
        public virtual void OnTriggerEnter(Collider other) { }

        /// <summary>
        /// The character has exited a trigger.
        /// </summary>
        /// <param name="other">The trigger collider that the character exited.</param>
        public virtual void OnTriggerExit(Collider other) { }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public virtual void OnDestroy() { }

        /// <summary>
        /// Helper method which returns the component on the GameObject of type T.
        /// </summary>
        /// <typeparam name="T">The type of component to return.</typeparam>
        /// <returns>The component of type T. Can be null.</returns>
        protected T GetComponent<T>()
        {
            return m_GameObject.GetComponent<T>();
        }

        /// <summary>
        /// Helper method which returns the ability of type T.
        /// </summary>
        /// <typeparam name="T">The type of ability to return.</typeparam>
        /// <returns>The ability of type T. Can be null.</returns>
        protected T GetAbility<T>() where T : Ability
        {
            return m_CharacterLocomotion.GetAbility<T>();
        }

        /// <summary>
        /// Helper method which returns the ability of type T with the specified index.
        /// </summary>
        /// <typeparam name="T">The type of ability to return.</typeparam>
        /// <param name="index">The index of the ability. -1 will ignore the index.</param>
        /// <returns>The ability of type T with the specified index. Can be null.</returns>
        protected T GetAbility<T>(int index) where T : Ability
        {
            return m_CharacterLocomotion.GetAbility<T>(index);
        }

        /// <summary>
        /// Helper method which returns the abilities of type T.
        /// </summary>
        /// <typeparam name="T">The type of ability to return.</typeparam>
        /// <returns>The abilities of type T. Can be null.</returns>
        protected T[] GetAbilities<T>() where T : Ability
        {
            return m_CharacterLocomotion.GetAbilities<T>();
        }

        /// <summary>
        /// Helper method which returns the abilities of type T with the specified index.
        /// </summary>
        /// <typeparam name="T">The type of ability to return.</typeparam>
        /// <param name="index">The index of the ability. -1 will ignore the index.</param>
        /// <returns>The abilities of type T with the specified index. Can be null.</returns>
        protected T[] GetAbilities<T>(int index) where T : Ability
        {
            return m_CharacterLocomotion.GetAbilities<T>(index);
        }

        /// <summary>
        /// Helper method which returns the effect of type T.
        /// </summary>
        /// <typeparam name="T">The type of effect to return.</typeparam>
        /// <returns>The effect of type T. Can be null.</returns>
        protected T GetEffect<T>() where T : Effect
        {
            return m_CharacterLocomotion.GetEffect<T>();
        }

        /// <summary>
        /// Helper method which adds a force to the character. The force will either be an external or soft force.
        /// </summary>
        /// <param name="force">The force to add.</param>
        protected void AddForce(Vector3 force)
        {
            m_CharacterLocomotion.AddForce(force);
        }

        /// <summary>
        /// Helper method which adds a force to the character in the specified number of frames.  The force will either be an external or soft force.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        protected void AddForce(Vector3 force, int frames)
        {
            m_CharacterLocomotion.AddForce(force, frames);
        }

        /// <summary>
        /// Helper method which adds a force to the character in the specified number of frames.  The force will either be an external or soft force.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        /// <param name="scaleByMass">Should the force be scaled by the character's mass?</param>
        /// <param name="scaleByTime">Should the force be scaled by the timescale?</param>
        protected void AddForce(Vector3 force, int frames, bool scaleByMass, bool scaleByTime)
        {
            m_CharacterLocomotion.AddForce(force, frames, scaleByMass, scaleByTime);
        }

        /// <summary>
        /// Helper method which adds a force relative to the character. The force will either be an external or soft force.
        /// </summary>
        /// <param name="force">The force to add.</param>
        protected void AddRelativeForce(Vector3 force)
        {
            m_CharacterLocomotion.AddRelativeForce(force);
        }

        /// <summary>
        /// Helper method which adds a force relative to the character in the specified number of frames. The force will either be an external or soft force.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        protected void AddRelativeForce(Vector3 force, int frames)
        {
            m_CharacterLocomotion.AddRelativeForce(force, frames);
        }

        /// <summary>
        /// Helper method which adds a force relative to the character in the specified number of frames. The force will either be an external or soft force.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        /// <param name="scaleByMass">Should the force be scaled by the character's mass?</param>
        /// <param name="scaleByTime">Should the force be scaled by the timescale?</param>
        protected void AddRelativeForce(Vector3 force, int frames, bool scaleByMass, bool scaleByTime)
        {
            m_CharacterLocomotion.AddRelativeForce(force, frames, scaleByMass, scaleByTime);
        }

        /// <summary>
        /// Helper method which sets the Ability Index parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        protected void SetAbilityIndexParameter(int value)
        {
            // An AnimatorMonitor is not required.
            if (m_AnimatorMonitor == null) {
                return;
            }

            m_AnimatorMonitor.SetAbilityIndexParameter(value);
        }

        /// <summary>
        /// Helper method which sets the Int Dat parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        protected void SetAbilityIntDataParameter(int value)
        {
            // An AnimatorMonitor is not required.
            if (m_AnimatorMonitor == null) {
                return;
            }

            m_AnimatorMonitor.SetAbilityIntDataParameter(value);
        }

        /// <summary>
        /// Helper method which sets the Float Data parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        protected void SetAbilityFloatDataParameter(float value)
        {
            // An AnimatorMonitor is not required.
            if (m_AnimatorMonitor == null) {
                return;
            }

            m_AnimatorMonitor.SetAbilityFloatDataParameter(value);
        }

        /// <summary>
        /// Helper method which sets the Float Data parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        protected void SetAbilityFloatDataParameter(float value, float dampingTime)
        {
            // An AnimatorMonitor is not required.
            if (m_AnimatorMonitor == null) {
                return;
            }

            m_AnimatorMonitor.SetAbilityFloatDataParameter(value, dampingTime);
        }

        /// <summary>
        /// Helper method which sets the Speed parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        protected void SetSpeedParameter(float value)
        {
            // An AnimatorMonitor is not required.
            if (m_AnimatorMonitor == null) {
                return;
            }

            m_AnimatorMonitor.SetSpeedParameter(value);
        }

        /// <summary>
        /// Helper method which sets the Height parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        protected void SetHeightParameter(int value)
        {
            // An AnimatorMonitor is not required.
            if (m_AnimatorMonitor == null) {
                return;
            }

            m_AnimatorMonitor.SetHeightParameter(value);
        }

        /// <summary>
        /// Returns true if the camera can zoom.
        /// </summary>
        /// <returns>True if the camera can zoom.</returns>
        public virtual bool CanCameraZoom() { return true; }
    }
}