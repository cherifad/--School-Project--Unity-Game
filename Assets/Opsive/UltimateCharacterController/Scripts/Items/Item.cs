/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEngine.Events;
using Opsive.UltimateCharacterController.Audio;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Items.AnimatorAudioStates;
using Opsive.UltimateCharacterController.Objects.CharacterAssist;
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Utility;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Items
{
    /// <summary>
    /// An item represents anything that can be picked up by the character. 
    /// </summary>
    public class Item : StateBehavior
    {
        [Tooltip("A reference to the object used to identify the item.")]
        [SerializeField] protected ItemType m_ItemType;
        [Tooltip("Specifies the inventory slot/spawn location of the item.")]
        [SerializeField] protected int m_SlotID;
        [Tooltip("Unique ID used for item identification within the animator.")]
        [SerializeField] protected int m_AnimatorItemID;
        [Tooltip("The movement set ID used for within the animator.")]
        [SerializeField] protected int m_AnimatorMovementSetID;
        [Tooltip("Does the item control the movement and the UI shown?")]
        [SerializeField] protected bool m_DominantItem = true;
        [Tooltip("The GameObject that is dropped when the item is removed from the character.")]
        [SerializeField] protected GameObject m_DropPrefab;
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemEquip animation event or wait for the specified duration before equipping.")]
        [SerializeField] protected AnimationEventTrigger m_EquipEvent = new AnimationEventTrigger(true, 0.3f);
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemEquipComplete animation event or wait for the specified duration before stopping the equip ability.")]
        [SerializeField] protected AnimationEventTrigger m_EquipCompleteEvent = new AnimationEventTrigger(false, 0f);
        [Tooltip("Specifies the animator and audio state from an equip.")]
        [SerializeField] protected AnimatorAudioStateSet m_EquipAnimatorAudioStateSet = new AnimatorAudioStateSet();
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemUnequip animation event or wait for the specified duration before unequipping.")]
        [SerializeField] protected AnimationEventTrigger m_UnequipEvent = new AnimationEventTrigger(true, 0.3f);
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemUnequipComplete animation event or wait for the specified duration before stopping the unequip ability.")]
        [SerializeField] protected AnimationEventTrigger m_UnequipCompleteEvent = new AnimationEventTrigger(false, 0);
        [Tooltip("Specifies the animator and audio state from an unequip.")]
        [SerializeField] protected AnimatorAudioStateSet m_UnequipAnimatorAudioStateSet = new AnimatorAudioStateSet();
        [Tooltip("The ID of the UI Monitor that the item should use.")]
        [SerializeField] protected int m_UIMonitorID;
        [Tooltip("The sprite representing the icon.")]
        [SerializeField] protected Sprite m_Icon;
        [Tooltip("Should the crosshairs be shown when the item aims?")]
        [SerializeField] protected bool m_ShowCrosshairsOnAim = true;
        [Tooltip("The sprite used for the center crosshairs image.")]
        [SerializeField] protected Sprite m_CenterCrosshairs;
        [Tooltip("The offset of the quadrant crosshairs sprites.")]
        [SerializeField] protected float m_QuadrantOffset = 5f;
        [Tooltip("The max spread of the quadrant crosshairs sprites caused by a recoil or reload.")]
        [SerializeField] protected float m_MaxQuadrantSpread = 10f;
        [Tooltip("The amount of damping to apply to the spread offset.")]
        [SerializeField] protected float m_QuadrantSpreadDamping = 0.05f;
        [Tooltip("The sprite used for the left crosshairs image.")]
        [SerializeField] protected Sprite m_LeftCrosshairs;
        [Tooltip("The sprite used for the top crosshairs image.")]
        [SerializeField] protected Sprite m_TopCrosshairs;
        [Tooltip("The sprite used for the right crosshairs image.")]
        [SerializeField] protected Sprite m_RightCrosshairs;
        [Tooltip("The sprite used for the bottom crosshairs image.")]
        [SerializeField] protected Sprite m_BottomCrosshairs;
        [Tooltip("Should the item's full screen UI be shown?")]
        [SerializeField] protected bool m_ShowFullScreenUI;
        [Tooltip("The ID of the full screen UI. This must match the ID within the FullScreenItemUIMonitor.")]
        [SerializeField] protected int m_FullScreenUIID = -1;
        [Tooltip("Unity event that is invoked when the item is picked up.")]
        [SerializeField] protected UnityEvent m_PickupItemEvent;
        [Tooltip("Unity event that is invoked when the item is equipped.")]
        [SerializeField] protected UnityEvent m_EquipItemEvent;
        [Tooltip("Unity event that is invoked when the item is unequipped.")]
        [SerializeField] protected UnityEvent m_UnequipItemEvent;
        [Tooltip("Unity event that is invoked when the item is dropped.")]
        [SerializeField] protected UnityEvent m_DropItemEvent;

        [NonSerialized] public ItemType ItemType { get { return m_ItemType; } set { m_ItemType = value; } }
        [NonSerialized] public int SlotID { get { return m_SlotID; } set { m_SlotID = value; } }
        [NonSerialized] public int AnimatorItemID { get { return m_AnimatorItemID; } set { m_AnimatorItemID = value; } }
        [NonSerialized] public int AnimatorMovementSetID { get { return m_AnimatorMovementSetID; } set { m_AnimatorMovementSetID = value; } }
        public bool DominantItem { get { return m_DominantItem; }
            set { m_DominantItem = value;
                if (Application.isPlaying) { EventHandler.ExecuteEvent(m_Character, "OnItemUpdateDominantItem", this, m_DominantItem); }
            } }
        public GameObject DropPrefab { get { return m_DropPrefab; } }
        public AnimationEventTrigger EquipEvent { get { return m_EquipEvent; } set { m_EquipEvent = value; } }
        public AnimationEventTrigger EquipCompleteEvent { get { return m_EquipCompleteEvent; } set { m_EquipCompleteEvent = value; } }
        public AnimatorAudioStateSet EquipAnimatorAudioStateSet { get { return m_EquipAnimatorAudioStateSet; } set { m_EquipAnimatorAudioStateSet = value; } }
        public AnimationEventTrigger UnequipEvent { get { return m_UnequipEvent; } set { m_UnequipEvent = value; } }
        public AnimationEventTrigger UnequipCompleteEvent { get { return m_UnequipCompleteEvent; } set { m_UnequipCompleteEvent = value; } }
        public AnimatorAudioStateSet UnequipAnimatorAudioStateSet { get { return m_UnequipAnimatorAudioStateSet; } set { m_UnequipAnimatorAudioStateSet = value; } }
        public int UIMonitorID { get { return m_UIMonitorID; } set { m_UIMonitorID = value; } }
        public Sprite Icon { get { return m_Icon; } }
        public bool ShowCrosshairsOnAim { get { return m_ShowCrosshairsOnAim; } set { m_ShowCrosshairsOnAim = value; } }
        public Sprite CenterCrosshairs { get { return m_CenterCrosshairs; } }
        public float QuadrantOffset { get { return m_QuadrantOffset; } }
        public float MaxQuadrantSpread { get { return m_MaxQuadrantSpread; } }
        public float QuadrantSpreadDamping { get { return m_QuadrantSpreadDamping; } }
        public Sprite LeftCrosshairs { get { return m_LeftCrosshairs; } }
        public Sprite TopCrosshairs { get { return m_TopCrosshairs; } }
        public Sprite RightCrosshairs { get { return m_RightCrosshairs; } }
        public Sprite BottomCrosshairs { get { return m_BottomCrosshairs; } }
        public bool ShowFullScreenUI { get { return m_ShowFullScreenUI; }
            set
            {
                m_ShowFullScreenUI = value;
                if (Application.isPlaying && DominantItem && IsActive()) {
                    EventHandler.ExecuteEvent(m_Character, "OnItemShowFullScreenUI", m_FullScreenUIID, m_ShowFullScreenUI);
                }
            }
        }
        [NonSerialized] public int FullScreenUIID { get { return m_FullScreenUIID; } set { m_FullScreenUIID = value; } }
        public UnityEvent PickupItemEvent { get { return m_PickupItemEvent; } set { m_PickupItemEvent = value; } }
        public UnityEvent EquipItemEvent { get { return m_EquipItemEvent; } set { m_EquipItemEvent = value; } }
        public UnityEvent UnequipItemEvent { get { return m_UnequipItemEvent; } set { m_UnequipItemEvent = value; } }
        public UnityEvent DropItemEvent { get { return m_DropItemEvent; } set { m_DropItemEvent = value; } }

        private GameObject m_GameObject;
        protected GameObject m_Character;
        protected UltimateCharacterLocomotion m_CharacterLocomotion;
        protected InventoryBase m_Inventory;
        protected PerspectiveItem m_ActivePerspectiveItem;
        private PerspectiveItem m_FirstPersonPerspectiveItem;
        private PerspectiveItem m_ThirdPersonPerspectiveItem;
#if FIRST_PERSON_CONTROLLER
        private GameObject[] m_FirstPersonObjects;
        private ChildAnimatorMonitor[] m_FirstPersonObjectsAnimatorMonitor;
        private ChildAnimatorMonitor m_FirstPersonPerspectiveItemAnimatorMonitor;
#endif
        private ChildAnimatorMonitor m_ThirdPersonItemAnimatorMonitor;
        private ItemAction[] m_ItemActions;
        private Dictionary<int, ItemAction> m_IDItemActionMap;

        private bool m_Started;
        private bool m_VisibleObjectActive;
        private bool m_ShouldDropAfterUnequip;
        private Vector3 m_UnequpDropPosition;
        private Quaternion m_UnequipDropRotation;

        public PerspectiveItem ActivePerspectiveItem { get { return m_ActivePerspectiveItem; } }
        public PerspectiveItem FirstPersonPerspectiveItem { get { return m_FirstPersonPerspectiveItem; } }
        public PerspectiveItem ThirdPersonPerspectiveItem { get { return m_ThirdPersonPerspectiveItem; } }
        public ItemAction[] ItemActions { get { return m_ItemActions; } }
        public bool VisibleObjectActive { get { return m_VisibleObjectActive; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            m_GameObject = gameObject;
            m_CharacterLocomotion = m_GameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
            m_Character = m_CharacterLocomotion.gameObject;
            m_Inventory = m_Character.GetCachedComponent<InventoryBase>();

            base.Awake();

            // Find the PerspectiveItems/ItemActions.
            var perspectiveItems = GetComponents<PerspectiveItem>();
            for (int i = 0; i < perspectiveItems.Length; ++i) {
                // AI agents and networked characters will not have a camera attached.
                if (perspectiveItems[i].FirstPersonItem) {
                    var camera = UnityEngineUtility.FindCamera(m_Character);
                    if (camera == null) {
                        continue;
                    }
                }

                // Initialize the perspective item manually to ensure an Object GameObject exists. This is important because the Item component will execute
                // before the FirstPersonPerspectiveItem component, but the FirstPersonPerspectiveItem component may not be completely initialized.
                // The FirstPersonPerspectiveItem component must be initialized after Item so Item.Start can be called and add the item to the inventory.
                perspectiveItems[i].Initialize(m_Character);

                if (perspectiveItems[i].FirstPersonItem) {
                    m_FirstPersonPerspectiveItem = perspectiveItems[i];
#if FIRST_PERSON_CONTROLLER
                    var firstPersonPerspectiveItem = perspectiveItems[i] as FirstPersonController.Items.FirstPersonPerspectiveItem;
                    if (firstPersonPerspectiveItem.Object != null) {
                        var baseAnimatorMonitor = firstPersonPerspectiveItem.Object.GetComponent<ChildAnimatorMonitor>();
                        if (baseAnimatorMonitor != null) {
                            m_FirstPersonObjects = new GameObject[firstPersonPerspectiveItem.AdditionalControlObjects.Length + 1];
                            m_FirstPersonObjectsAnimatorMonitor = new ChildAnimatorMonitor[firstPersonPerspectiveItem.AdditionalControlObjects.Length + 1];
                            m_FirstPersonObjects[0] = baseAnimatorMonitor.gameObject;
                            m_FirstPersonObjectsAnimatorMonitor[0] = baseAnimatorMonitor;
                            for (int j = 0; j < firstPersonPerspectiveItem.AdditionalControlObjects.Length; ++j) {
                                m_FirstPersonObjects[j + 1] = firstPersonPerspectiveItem.AdditionalControlObjects[j];
                                m_FirstPersonObjectsAnimatorMonitor[j + 1] = firstPersonPerspectiveItem.AdditionalControlObjects[j].GetComponent<ChildAnimatorMonitor>();
                            }
                        }
                    }

                    var visibleItem = firstPersonPerspectiveItem.VisibleItem;
                    if (visibleItem != null) {
                        m_FirstPersonPerspectiveItemAnimatorMonitor = visibleItem.GetComponent<ChildAnimatorMonitor>();
                    }
#endif
                } else {
                    m_ThirdPersonPerspectiveItem = perspectiveItems[i];
                    m_ThirdPersonItemAnimatorMonitor = perspectiveItems[i].Object != null ? perspectiveItems[i].Object.GetComponent<ChildAnimatorMonitor>() : null;
                }

                // The audio can be played from the actual visible object.
                if (perspectiveItems[i].GetVisibleObject() != null) {
                    AudioManager.Register(perspectiveItems[i].GetVisibleObject());
                }
            }
            // Equip/Unequip will be played from the character with a reserved index of 0.
            AudioManager.Register(m_Character);
            AudioManager.SetReserveCount(m_Character, 1);

            m_ItemActions = GetComponents<ItemAction>();
            if (m_ItemActions.Length > 1) {
                m_IDItemActionMap = new Dictionary<int, ItemAction>();
                for (int i = 0; i < m_ItemActions.Length; ++i) {
                    m_IDItemActionMap.Add(m_ItemActions[i].ID, m_ItemActions[i]);
                }
            }

            m_EquipAnimatorAudioStateSet.DeserializeAnimatorAudioStateSelector(this, m_CharacterLocomotion);
            m_UnequipAnimatorAudioStateSet.DeserializeAnimatorAudioStateSelector(this, m_CharacterLocomotion);
            m_EquipAnimatorAudioStateSet.Awake(m_GameObject);
            m_UnequipAnimatorAudioStateSet.Awake(m_GameObject);

            EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
        }

        /// <summary>
        /// Adds the item to the inventory and initializes the non-local network player.
        /// </summary>
        private void Start()
        {
            // Start may have already been called within Pickup.
            if (m_Started) {
                return;
            }
            m_Started = true;
            if (m_FirstPersonPerspectiveItem != null) {
                m_FirstPersonPerspectiveItem.ItemStarted();
            }
            if (m_ThirdPersonPerspectiveItem != null) {
                m_ThirdPersonPerspectiveItem.ItemStarted();
            }
            SetVisibleObjectActive(false);

            // Set the correct visible object for the current perspective.
            var characterLocomotion = m_Character.GetCachedComponent<UltimateCharacterLocomotion>();
            if (characterLocomotion.FirstPersonPerspective) {
                m_ActivePerspectiveItem = m_FirstPersonPerspectiveItem;
            } else {
                m_ActivePerspectiveItem = m_ThirdPersonPerspectiveItem;
            }
            m_Inventory.AddItem(this, true);
            // The character should ignore any of the item's colliders.
            var colliders = GetComponents<Collider>();
            for (int i = 0; i < colliders.Length; ++i) {
                characterLocomotion.AddSubCollider(colliders[i]);
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            // Perform any initialization for a non-local network player.
            var networkCharacter = m_Character.GetCachedComponent<Networking.Character.INetworkCharacter>();
            if (networkCharacter != null && !networkCharacter.IsLocalPlayer()) {
                // Remote players should always be in the third person view.
                OnChangePerspectives(false);
                Events.EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
            }
#endif
        }

        /// <summary>
        /// The item has been picked up by the character.
        /// </summary>
        public virtual void Pickup()
        {
            // The Active Visible Item will be null if the item is picked up at runtime.
            if (m_ActivePerspectiveItem == null) {
                Start();
            }

            if (m_FirstPersonPerspectiveItem != null) {
                m_FirstPersonPerspectiveItem.Pickup();
            }

            if (m_ThirdPersonPerspectiveItem != null) {
                m_ThirdPersonPerspectiveItem.Pickup();
            }

            if (m_ItemActions != null) {
                for (int i = 0; i < m_ItemActions.Length; ++i) {
                    m_ItemActions[i].Pickup();
                }
            }
            if (m_PickupItemEvent != null && m_PickupItemEvent.GetPersistentEventCount() > 0) {
                m_PickupItemEvent.Invoke();
            }
        }

        /// <summary>
        /// Returns the ItemAction based on the ID.
        /// </summary>
        /// <param name="id">The ID of the ItemAction to retrieve.</param>
        /// <returns>The ItemAction that corresponds to the specified ID.</returns>
        public ItemAction GetItemAction(int id)
        {
            if (m_ItemActions == null || m_ItemActions.Length == 0) {
                return null;
            }

            if (m_ItemActions.Length == 1) {
                // The ID must match.
                if (m_ItemActions[0].ID == id) {
                    return m_ItemActions[0];
                }
                return null;
            }
            
            // Multiple actions exist - look up the action based on the ID.
            ItemAction itemAction;
            if (m_IDItemActionMap.TryGetValue(id, out itemAction)) {
                return itemAction;
            }

            // The action with the specified ID wasn't found.
            return null;
        }

        /// <summary>
        /// The item will be equipped.
        /// </summary>
        public void WillEquip()
        {
            if (m_ItemActions != null) {
                for (int i = 0; i < m_ItemActions.Length; ++i) {
                    m_ItemActions[i].WillEquip();
                }
            }
        }

        /// <summary>
        /// Starts to equip the item.
        /// </summary>
        /// <param name="immediateEquip">Is the item being equipped immediately? Immediate equips will occur from the default loadout or quickly switching to the item.</param>
        public void StartEquip(bool immediateEquip)
        {
            if (immediateEquip) {
                SetVisibleObjectActive(true);
            } else {
                m_EquipAnimatorAudioStateSet.NextState();
            }

#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonPerspectiveItem != null) {
                m_FirstPersonPerspectiveItem.StartEquip(immediateEquip);
            }
#endif
            if (m_ThirdPersonPerspectiveItem != null) {
                m_ThirdPersonPerspectiveItem.StartEquip(immediateEquip);
            }
        }

        /// <summary>
        /// The item has been equipped within the inventory.
        /// </summary>
        /// <param name="immediateEquip">Is the item being equipped immediately? Immediate equips will occur from the default loadout or quickly switching to the item.</param>
        public void Equip(bool immediateEquip)
        {
            SetVisibleObjectActive(true);

            // Optionally play an equip sound based upon the equipping animation.
            if (!immediateEquip) {
                m_EquipAnimatorAudioStateSet.PlayAudioClip(m_Character, 0);
            }

            if (m_ItemActions != null) {
                for (int i = 0; i < m_ItemActions.Length; ++i) {
                    m_ItemActions[i].Equip();
                }
            }

            if (m_EquipItemEvent != null && m_EquipItemEvent.GetPersistentEventCount() > 0) {
                m_EquipItemEvent.Invoke();
            }

            // Update the full screen UI property to handle the case when the preset has already been applied.
            ShowFullScreenUI = m_ShowFullScreenUI;
        }
        /// <summary>
        /// Moves the item according to the horizontal and vertical movement, as well as the character velocity.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        public void Move(float horizontalMovement, float verticalMovement)
        {
            if (m_FirstPersonPerspectiveItem != null) {
                m_FirstPersonPerspectiveItem.Move(horizontalMovement, verticalMovement);
            }
            if (m_ThirdPersonPerspectiveItem != null) {
                m_ThirdPersonPerspectiveItem.Move(horizontalMovement, verticalMovement);
            }
        }

        /// <summary>
        /// Starts to unequip the item.
        /// </summary>
        /// <param name="immediateUnequip">Is the item being unequipped immediately? Immediate unequips will occur when the character dies.</param>
        public void StartUnequip(bool immediateUnequip)
        {
            if (!immediateUnequip) {
                m_UnequipAnimatorAudioStateSet.NextState();
                m_UnequipAnimatorAudioStateSet.PlayAudioClip(m_Character, 0);
            }

            // Notify any item actions of the unequip.
            if (m_ItemActions != null) {
                for (int i = 0; i < m_ItemActions.Length; ++i) {
                    m_ItemActions[i].StartUnequip();
                }
            }

#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonPerspectiveItem != null) {
                m_FirstPersonPerspectiveItem.StartUnequip();
            }
#endif
            if (m_ThirdPersonPerspectiveItem != null) {
                m_ThirdPersonPerspectiveItem.StartUnequip();
            }
        }

        /// <summary>
        /// The item has been unequipped within the item.
        /// </summary>
        public void Unequip()
        {
            // If the item isn't a dominant item then it doesn't move the transform or set the animator parameters.
            if (m_DominantItem) {
                SetItemIDParameter(m_SlotID, 0);
            }

            // When the item is unequipped it is no longer visible.
            ShowFullScreenUI = false;
            SetVisibleObjectActive(false);

            // Notify any item actions of the unequip.
            if (m_ItemActions != null) {
                for (int i = 0; i < m_ItemActions.Length; ++i) {
                    m_ItemActions[i].Unequip();
                }
            }

            // Notify the perspective items of the unequip.
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonPerspectiveItem != null) {
                m_FirstPersonPerspectiveItem.Unequip();
            }
#endif
            if (m_ThirdPersonPerspectiveItem != null) {
                m_ThirdPersonPerspectiveItem.Unequip();
            }

            if (m_UnequipItemEvent != null && m_UnequipItemEvent.GetPersistentEventCount() > 0) {
                m_UnequipItemEvent.Invoke();
            }

            // Drop could have been called before the item was unequipped. Now that the item is unequipped it can be dropped.
            if (m_ShouldDropAfterUnequip) {
                Drop();
            }
        }

        /// <summary>
        /// Activates or deactivates the visible objects.
        /// </summary>
        /// <param name="active">Should the visible object be activated?</param>
        public void SetVisibleObjectActive(bool active)
        {
            if (!m_Started) {
                return;
            }

            var change = m_VisibleObjectActive != active;
            m_VisibleObjectActive = active;
            // The ItemActions have a chance to disable the visible object from activating.
            if (active) {
                for (int i = 0; i < m_ItemActions.Length; ++i) {
                    if (!m_ItemActions[i].CanActivateVisibleObject()) {
                        active = false;
                        break;
                    }
                }
            }
            if (m_FirstPersonPerspectiveItem != null) {
                m_FirstPersonPerspectiveItem.SetActive(active);
            }
            if (m_ThirdPersonPerspectiveItem != null) {
                m_ThirdPersonPerspectiveItem.SetActive(active);
            }

            // The ItemActions can execute within Update so also set the enabled state based on the active state.
            for (int i = 0; i < m_ItemActions.Length; ++i) {
                m_ItemActions[i].enabled = active;
            }

            if (change && active) {
                SnapAnimator();
            }
        }

        /// <summary>
        /// Returns the current PerspectiveItem object.
        /// </summary>
        /// <returns>The current PerspectiveItem object.</returns>
        public virtual GameObject GetVisibleObject()
        {
            return m_ActivePerspectiveItem.GetVisibleObject();
        }

        /// <summary>
        /// Is the item active?
        /// </summary>
        /// <returns>True if the item is active.</param>
        public bool IsActive()
        {
            return m_VisibleObjectActive && m_ActivePerspectiveItem.IsActive();
        }

        /// <summary>
        /// Returns true if the camera can zoom.
        /// </summary>
        /// <returns>True if the camera can zoom.</returns>
        public virtual bool CanCameraZoom()
        {
            return true;
        }

        /// <summary>
        /// Synchronizes the item Animator paremeters with the character's Animator.
        /// </summary>
        public void SnapAnimator()
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SnapAnimator();
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SnapAnimator();
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && m_ThirdPersonPerspectiveItem.IsActive()) {
                m_ThirdPersonItemAnimatorMonitor.SnapAnimator();
            }
        }

        /// <summary>
        /// Updates the Animator at a fixed rate.
        /// </summary>
        /// <param name="deltaTime">The rate to update the animator.</param>
        public void UpdateAnimator(float deltaTime)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].UpdateAnimator(deltaTime);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.UpdateAnimator(deltaTime);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && m_ThirdPersonPerspectiveItem.IsActive()) {
                m_ThirdPersonItemAnimatorMonitor.UpdateAnimator(deltaTime);
            }
        }

        /// <summary>
        /// Sets the Animator's Horizontal Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        public void SetHorizontalMovementParameter(float value, float dampingTime)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetHorizontalMovementParameter(value, dampingTime);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetHorizontalMovementParameter(value, dampingTime);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && m_ThirdPersonPerspectiveItem.IsActive()) {
                m_ThirdPersonItemAnimatorMonitor.SetHorizontalMovementParameter(value, dampingTime);
            }
        }

        /// <summary>
        /// Sets the Animator's Forward Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        public void SetForwardMovementParameter(float value, float dampingTime)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetForwardMovementParameter(value, dampingTime);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetForwardMovementParameter(value, dampingTime);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && m_ThirdPersonPerspectiveItem.IsActive()) {
                m_ThirdPersonItemAnimatorMonitor.SetForwardMovementParameter(value, dampingTime);
            }
        }

        /// <summary>
        /// Sets the Animator's Pitch parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        public void SetPitchParameter(float value, float dampingTime)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetPitchParameter(value, dampingTime);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetPitchParameter(value, dampingTime);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && m_ThirdPersonPerspectiveItem.IsActive()) {
                m_ThirdPersonItemAnimatorMonitor.SetPitchParameter(value, dampingTime);
            }
        }

        /// <summary>
        /// Sets the Animator's Yaw parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        public void SetYawParameter(float value, float dampingTime)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetYawParameter(value, dampingTime);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetYawParameter(value, dampingTime);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && m_ThirdPersonPerspectiveItem.IsActive()) {
                m_ThirdPersonItemAnimatorMonitor.SetYawParameter(value, dampingTime);
            }
        }

        /// <summary>
        /// Sets the Animator's Moving parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetMovingParameter(bool value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetMovingParameter(value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetMovingParameter(value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && m_ThirdPersonPerspectiveItem.IsActive()) {
                m_ThirdPersonItemAnimatorMonitor.SetMovingParameter(value);
            }
        }

        /// <summary>
        /// Sets the Animator's Aiming parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetAimingParameter(bool value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetAimingParameter(value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetAimingParameter(value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && m_ThirdPersonPerspectiveItem.IsActive()) {
                m_ThirdPersonItemAnimatorMonitor.SetAimingParameter(value);
            }
        }

        /// <summary>
        /// Sets the Animator's Movement Set ID parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetMovementSetIDParameter(int value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetMovementSetIDParameter(value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetMovementSetIDParameter(value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && m_ThirdPersonPerspectiveItem.IsActive()) {
                m_ThirdPersonItemAnimatorMonitor.SetMovementSetIDParameter(value);
            }
        }

        /// <summary>
        /// Sets the Animator's Ability Index parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetAbilityIndexParameter(int value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetAbilityIndexParameter(value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetAbilityIndexParameter(value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && m_ThirdPersonPerspectiveItem.IsActive()) {
                m_ThirdPersonItemAnimatorMonitor.SetAbilityIndexParameter(value);
            }
        }

        /// <summary>
        /// Sets the Animator's Int Data parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetAbilityIntDataParameter(int value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetAbilityIntDataParameter(value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetAbilityIntDataParameter(value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && m_ThirdPersonPerspectiveItem.IsActive()) {
                m_ThirdPersonItemAnimatorMonitor.SetAbilityIntDataParameter(value);
            }
        }

        /// <summary>
        /// Sets the Animator's Float Data parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public void SetAbilityFloatDataParameter(float value, float dampingTime)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetAbilityFloatDataParameter(value, dampingTime);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetAbilityFloatDataParameter(value, dampingTime);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && m_ThirdPersonPerspectiveItem.IsActive()) {
                m_ThirdPersonItemAnimatorMonitor.SetAbilityFloatDataParameter(value, dampingTime);
            }
        }

        /// <summary>
        /// Sets the Animator's Speed parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        public void SetSpeedParameter(float value, float dampingTime)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetSpeedParameter(value, dampingTime);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetSpeedParameter(value, dampingTime);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && m_ThirdPersonPerspectiveItem.IsActive()) {
                m_ThirdPersonItemAnimatorMonitor.SetSpeedParameter(value, dampingTime);
            }
        }

        /// <summary>
        /// Sets the Animator's Height parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetHeightParameter(int value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetHeightParameter(value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetHeightParameter(value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && m_ThirdPersonPerspectiveItem.IsActive()) {
                m_ThirdPersonItemAnimatorMonitor.SetHeightParameter(value);
            }
        }

        /// <summary>
        /// Sets the Animator's Item ID parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot that the item occupies.</param>
        /// <param name="value">The new value.</param>
        public void SetItemIDParameter(int slotID, int value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetItemIDParameter(slotID, value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetItemIDParameter(slotID, value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && m_ThirdPersonPerspectiveItem.IsActive()) {
                m_ThirdPersonItemAnimatorMonitor.SetItemIDParameter(slotID, value);
            }
        }

        /// <summary>
        /// Sets the Animator's Item State Index parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot that the item occupies.</param>
        /// <param name="value">The new value.</param>
        public void SetItemStateIndexParameter(int slotID, int value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetItemStateIndexParameter(slotID, value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetItemStateIndexParameter(slotID, value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && m_ThirdPersonPerspectiveItem.IsActive()) {
                m_ThirdPersonItemAnimatorMonitor.SetItemStateIndexParameter(slotID, value);
            }
        }

        /// <summary>
        /// Sets the Animator's Item Substate Index parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot of that item that should be set.</param>
        /// <param name="value">The new value.</param>
        public void SetItemSubstateIndexParameter(int slotID, int value)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonObjectsAnimatorMonitor != null) {
                for (int i = 0; i < m_FirstPersonObjectsAnimatorMonitor.Length; ++i) {
                    if (!m_FirstPersonObjects[i].activeSelf) {
                        continue;
                    }
                    m_FirstPersonObjectsAnimatorMonitor[i].SetItemSubstateIndexParameter(slotID, value);
                }
            }
            if (m_FirstPersonPerspectiveItemAnimatorMonitor != null && m_FirstPersonPerspectiveItem.IsActive()) {
                m_FirstPersonPerspectiveItemAnimatorMonitor.SetItemSubstateIndexParameter(slotID, value);
            }
#endif
            if (m_ThirdPersonItemAnimatorMonitor != null && m_ThirdPersonPerspectiveItem.IsActive()) {
                m_ThirdPersonItemAnimatorMonitor.SetItemSubstateIndexParameter(slotID, value);
            }
        }

        /// <summary>
        /// The character perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        protected virtual void OnChangePerspectives(bool firstPersonPerspective)
        {
            // The object isn't active if it isn't equipped. OnChangePerspective will be sent to all items regardless of whether or not they are equipped.
            var isActive = m_ActivePerspectiveItem != null && m_ActivePerspectiveItem.IsActive();
            if (firstPersonPerspective) {
                m_ActivePerspectiveItem = m_FirstPersonPerspectiveItem;
            } else {
                m_ActivePerspectiveItem = m_ThirdPersonPerspectiveItem;
            }
            if (isActive) {
                m_ActivePerspectiveItem.SetActive(true);
            }
        }

        /// <summary>
        /// Drop the item from the character.
        /// </summary>
        /// <returns>True if the Item was dropped.</returns>
        public bool Drop()
        {
            // The item needs to first be unequipped before it can be dropped.
            if (m_VisibleObjectActive && m_CharacterLocomotion.FirstPersonPerspective) {
                m_ShouldDropAfterUnequip = true;
                var itemObject = GetVisibleObject().transform;
                m_UnequpDropPosition = itemObject.position;
                m_UnequipDropRotation = itemObject.rotation;
                return false;
            }

            ItemPickup itemPickup = null;
            // If a drop prefab exists then the character should drop a prefab of the item so it can later be picked up.
            if (m_DropPrefab != null) {
                var count = m_Inventory.GetItemTypeCount(m_ItemType);
                // The prefab can only be dropped if the inventory contains the item.
                if (count > 0) {
                    Vector3 dropPosition;
                    Quaternion dropRotation;
                    // If the item is unequipped before it is dropped then it could be holstered so the current transform should not be used.
                    if (m_ShouldDropAfterUnequip) {
                        dropPosition = m_UnequpDropPosition;
                        dropRotation = m_UnequipDropRotation;
                    } else {
                        var itemObject = GetVisibleObject().transform;
                        dropPosition = itemObject.position;
                        dropRotation = itemObject.rotation;
                    }
                    var spawnedObject = ObjectPool.Instantiate(m_DropPrefab, dropPosition, dropRotation);
                    // The ItemPickup component is responsible for allowing characters to pick up the item. Save the ItemType count
                    // to the ItemTypeAmount array so that same amount can be picked up again.
                    itemPickup = spawnedObject.GetCachedComponent<ItemPickup>();
                    if (itemPickup != null) {
                        // Return the old.
                        for (int j = 0; j < itemPickup.ItemTypeCounts.Length; ++j) {
                            ObjectPool.Return(itemPickup.ItemTypeCounts[j]);
                        }
                        var itemTypeCount = ObjectPool.Get<ItemTypeCount>();
                        itemTypeCount.Initialize(m_ItemType, 1);

                        // If the dropped Item is a usable item then the array should be larger to be able to pick up the usable ItemType.
                        var consumableItemTypes = 0;
                        UsableItem usableItem;
                        for (int i = 0; i < m_ItemActions.Length; ++i) {
                            if ((usableItem = (m_ItemActions[i] as UsableItem)) != null && usableItem.GetConsumableItemType() != null) {
                                consumableItemTypes++;
                            }
                        }

                        // Save the main ItemType.
                        var mainItemTypeCount = (m_ItemType.DroppedItemTypes != null ? m_ItemType.DroppedItemTypes.Length : 0) + 1;
                        var length = consumableItemTypes + mainItemTypeCount;
                        if (itemPickup.ItemTypeCounts.Length != length) {
                            itemPickup.ItemTypeCounts = new ItemTypeCount[length];
                        }
                        itemPickup.ItemTypeCounts[0] = itemTypeCount;
                        if (m_ItemType.DroppedItemTypes != null) {
                            for (int i = 0; i < m_ItemType.DroppedItemTypes.Length; ++i) {
                                itemTypeCount = ObjectPool.Get<ItemTypeCount>();
                                itemTypeCount.Initialize(m_ItemType.DroppedItemTypes[i], 1);
                                itemPickup.ItemTypeCounts[i + 1] = itemTypeCount;
                            }
                        }

                        // Save the usable ItemTypes if any exist.
                        ItemType consumableItemType;
                        consumableItemTypes = 0;
                        for (int i = 0; i < m_ItemActions.Length; ++i) {
                            if ((usableItem = (m_ItemActions[i] as UsableItem)) != null && (consumableItemType = usableItem.GetConsumableItemType()) != null) {
                                var itemTypeAmount = ObjectPool.Get<ItemTypeCount>();
                                var consumableDropCount = 0f;
                                // Only remove the remaining inventory if there is just one ItemType remaining. This will allow the character to keep the consumable ammo
                                // if only one item is dropped and the character has multiple of the same item.
                                if (count == 1) {
                                    consumableDropCount = m_Inventory.GetItemTypeCount(consumableItemType);
                                }
                                var remainingConsumableCount = usableItem.GetConsumableItemTypeCount(); // The count may be negative (for use by the UI).
                                itemTypeAmount.Initialize(consumableItemType, consumableDropCount + (remainingConsumableCount > 0 ? remainingConsumableCount : 0));
                                itemPickup.ItemTypeCounts[consumableItemTypes + mainItemTypeCount] = itemTypeAmount;
                            }
                        }

                        // Enable the ItemPickup.
                        itemPickup.Initialize(true);
                    }

                    // The ItemPickup may have a TrajectoryObject attached instead of a Rigidbody.
                    var trajectoryObject = spawnedObject.GetCachedComponent<Objects.TrajectoryObject>();
                    if (trajectoryObject != null) {
                        trajectoryObject.Initialize(m_CharacterLocomotion.Velocity, m_CharacterLocomotion.Torque.eulerAngles, m_Character);
                    }
                }
            }
            m_ShouldDropAfterUnequip = false;

            SetVisibleObjectActive(false);
            if (m_FirstPersonPerspectiveItem != null) {
                m_FirstPersonPerspectiveItem.Drop();
            }
            if (m_ThirdPersonPerspectiveItem != null) {
                m_ThirdPersonPerspectiveItem.Drop();
            }
            if (m_ItemActions != null) {
                for (int i = 0; i < m_ItemActions.Length; ++i) {
                    m_ItemActions[i].Drop();
                }
            }
            // The item can be removed from the inventory after it has been dropped. All corresponding DroppedItemTypes should also be dropped.
            if (m_ItemType.DroppedItemTypes != null) {
                for (int i = 0; i < m_ItemType.DroppedItemTypes.Length; ++i) {
                    var droppedItemType = m_ItemType.DroppedItemTypes[i];
                    if (m_Inventory.GetItemTypeCount(droppedItemType) > 0) {
                        for (int j = 0; j < m_Inventory.SlotCount; ++j) {
                            var item = m_Inventory.GetItem(j, droppedItemType);
                            if (item != null) {
                                m_Inventory.RemoveItem(droppedItemType, j, false);
                            }
                        }
                    }
                }
            }
            m_Inventory.RemoveItem(m_ItemType, m_SlotID, false);

            if (m_DropItemEvent != null && m_DropItemEvent.GetPersistentEventCount() > 0) {
                m_DropItemEvent.Invoke();
            }
            return true;
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            Events.EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
        }
    }
}