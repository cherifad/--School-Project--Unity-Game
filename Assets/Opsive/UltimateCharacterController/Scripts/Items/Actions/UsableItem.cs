/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Items.AnimatorAudioStates;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Items.Actions
{
    /// <summary>
    /// Base class for any item that can be used.
    /// </summary>
    public abstract class UsableItem : ItemAction, IUsableItem
    {
        /// <summary>
        /// Defines the statue of the Use Ability when calling CanUseItem.
        /// </summary>
        public enum UseAbilityState
        {
            Start,  // The Use ability is starting.
            Update, // The Use ability is updating.
            None    // The ability is not used.
        }

        [Tooltip("The amount of time that must elapse before the item can be used again.")]
        [SerializeField] protected float m_UseRate = 0.1f;
        [Tooltip("Specifies if the inventory can equip an item that doesn't have any consumable items left.")]
        [SerializeField] protected bool m_CanEquipEmptyItem = true;
        [Tooltip("Should the character rotate to face the target during use?")]
        [SerializeField] protected bool m_FaceTarget = true;
        [Tooltip("The amount of extra time it takes for the ability to stop after use.")]
        [SerializeField] protected float m_StopUseAbilityDelay = 0.25f;
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemUse animation event or wait for the specified duration before being used.")]
        [SerializeField] protected AnimationEventTrigger m_UseEvent = new AnimationEventTrigger(true, 0.2f);
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemUseComplete animation event or wait for the specified duration before completing the use.")]
        [SerializeField] protected AnimationEventTrigger m_UseCompleteEvent = new AnimationEventTrigger(false, 0.05f);
        [Tooltip("Does the item require root motion position during use?")]
        [SerializeField] protected bool m_ForceRootMotionPosition;
        [Tooltip("Does the item require root motion rotation during use?")]
        [SerializeField] protected bool m_ForceRootMotionRotation;
        [Tooltip("Optionally specify an attribute that the item should affect when in use.")]
        [HideInInspector] [SerializeField] protected AttributeModifier m_AttributeModifier = new AttributeModifier();
        [Tooltip("Should the audio play when the item starts to be used? If false it will be played when the item is used.")]
        [SerializeField] protected bool m_PlayAudioOnStartUse = false;
        [Tooltip("Specifies the animator and audio state that should be triggered when the item is used.")]
        [SerializeField] protected AnimatorAudioStateSet m_UseAnimatorAudioStateSet = new AnimatorAudioStateSet(2);

        public Item Item { get { return m_Item; } }
        public float UseRate { get { return m_UseRate; } set { m_UseRate = value; } }
        public bool CanEquipEmptyItem { get { return m_CanEquipEmptyItem; } set { m_CanEquipEmptyItem = value; } }
        public bool FaceTarget { get { return m_FaceTarget; } set { m_FaceTarget = value; } }
        public float StopUseAbilityDelay { get { return m_StopUseAbilityDelay; } set { m_StopUseAbilityDelay = value; } }
        public AnimationEventTrigger UseEvent { get { return m_UseEvent; } set { m_UseEvent = value; } }
        public AnimationEventTrigger UseCompleteEvent { get { return m_UseCompleteEvent; } set { m_UseCompleteEvent = value; } }
        public bool ForceRootMotionPosition { get { return m_ForceRootMotionPosition; } set { m_ForceRootMotionPosition = value; } }
        public bool ForceRootMotionRotation { get { return m_ForceRootMotionRotation; } set { m_ForceRootMotionRotation = value; } }
        [NonSerialized] public AttributeModifier AttributeModifier { get { return m_AttributeModifier; } set { m_AttributeModifier = value; } }
        public bool PlayAudioOnStartUse { get { return m_PlayAudioOnStartUse; } set { m_PlayAudioOnStartUse = value; } }
        public AnimatorAudioStateSet UseAnimatorAudioStateSet { get { return m_UseAnimatorAudioStateSet; } set { m_UseAnimatorAudioStateSet = value; } }

        protected ILookSource m_LookSource;
        protected float m_NextAllowedUseTime;
        private bool m_InUse;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // The item may have been added at runtime in which case the look source has already been populated.
            var characterLocomotion = m_Character.GetCachedComponent<UltimateCharacterLocomotion>();
            m_LookSource = characterLocomotion.LookSource;

            m_UseAnimatorAudioStateSet.DeserializeAnimatorAudioStateSelector(m_Item, characterLocomotion);
            m_UseAnimatorAudioStateSet.Awake(m_Item.gameObject);

            m_NextAllowedUseTime = Time.time;
            if (m_AttributeModifier != null) {
                m_AttributeModifier.Initialize(m_Character);
            }
            EventHandler.RegisterEvent<ILookSource>(m_Character, "OnCharacterAttachLookSource", OnAttachLookSource);
        }

        /// <summary>
        /// A new ILookSource object has been attached to the character.
        /// </summary>
        /// <param name="lookSource">The ILookSource object attached to the character.</param>
        private void OnAttachLookSource(ILookSource lookSource)
        {
            m_LookSource = lookSource;
        }

        /// <summary>
        /// Returns the ItemType which can be used by the item.
        /// </summary>
        /// <returns>The ItemType which can be used by the item.</returns>
        public virtual ItemType GetConsumableItemType() { return null; }

        /// <summary>
        /// Returns the amount of UsableItemType which has been consumed by the UsableItem.
        /// </summary>
        /// <returns>The amount consumed of the UsableItemType.</returns>
        public virtual float GetConsumableItemTypeCount() { return 0; }

        /// <summary>
        /// Sets the UsableItemType amount on the UsableItem.
        /// </summary>
        /// <param name="amount">The amount to set the UsableItemType to.</param>
        public virtual void SetConsumableItemTypeCount(float amount) { }

        /// <summary>
        /// Removes the amount of UsableItemType which has been consumed by the UsableItem.
        /// </summary>
        public virtual void RemoveConsumableItemTypeCount() { }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="itemAbility">The itemAbility that is trying to use the item.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public virtual bool CanUseItem(ItemAbility itemAbility, UseAbilityState abilityState)
        {
            // Prevent the item from being used too soon.
            if (Time.time < m_NextAllowedUseTime) {
                return false;
            }

            // The attribute may prevent the item from being used (such as if the character doesn't have enough stamina to use the item).
            if (m_AttributeModifier != null && !m_AttributeModifier.IsValid()) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Starts the item use.
        /// </summary>
        public virtual void StartItemUse()
        {
            m_InUse = true;

            if (m_AttributeModifier != null) {
                m_AttributeModifier.EnableModifier(true);
            }

            m_UseAnimatorAudioStateSet.NextState();
            if (m_PlayAudioOnStartUse) {
                var visibleObject = m_Item.GetVisibleObject() != null ? m_Item.GetVisibleObject() : m_Character;
                m_UseAnimatorAudioStateSet.PlayAudioClip(visibleObject);
            }
        }

        /// <summary>
        /// Uses the item.
        /// </summary>
        public virtual void UseItem()
        {
            m_NextAllowedUseTime = Time.time + m_UseRate;

            // Optionally play a use sound based upon the use animation.
            if (!m_PlayAudioOnStartUse) {
                var visibleObject = m_Item.GetVisibleObject() != null ? m_Item.GetVisibleObject() : m_Character;
                m_UseAnimatorAudioStateSet.PlayAudioClip(visibleObject);
            }
        }

        /// <summary>
        /// Is the item in use?
        /// </summary>
        /// <returns>True if the item is in use.</returns>
        public virtual bool IsItemInUse()
        {
            return m_InUse;
        }

        /// <summary>
        /// Is the item waiting to be used? This will return true if the item is waiting to be charged or pulled back.
        /// </summary>
        /// <returns>Returns true if the item is waiting to be used.</returns>
        public virtual bool IsItemUsePending() { return false; }

        /// <summary>
        /// Returns the substate index that the item should be in.
        /// </summary>
        /// <returns>the substate index that the item should be in.</returns>
        public virtual int GetItemSubstateIndex()
        {
            return m_UseAnimatorAudioStateSet.GetItemSubstateIndex();
        }

        /// <summary>
        /// Allows the item to update while it is being used.
        /// </summary>
        public virtual void UseItemUpdate() { }

        /// <summary>
        /// The item has been used.
        /// </summary>
        public virtual void ItemUseComplete() { }

        /// <summary>
        /// Tries to stop the item use.
        /// </summary>
        public virtual void TryStopItemUse() { }

        /// <summary>
        /// Can the item use be stopped?
        /// </summary>
        /// <returns>True if the item use can be stopped.</returns>
        public virtual bool CanStopItemUse()
        {
            return true;
        }

        /// <summary>
        /// Stops the item use.
        /// </summary>
        public virtual void StopItemUse()
        {
            m_InUse = false;

            if (m_AttributeModifier != null) {
                m_AttributeModifier.EnableModifier(false);
            }
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<ILookSource>(m_Character, "OnCharacterAttachLookSource", OnAttachLookSource);
        }
    }
}