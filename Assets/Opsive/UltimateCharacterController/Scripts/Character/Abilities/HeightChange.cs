/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    /// The HeightChange ability allows the character pose to toggle between height changes. 
    /// </summary>
    [AllowMultipleAbilityTypes]
    [DefaultInputName("Crouch")]
    [DefaultState("Crouch")]
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultStopType(AbilityStopType.ButtonToggle)]
    [DefaultAbilityIndex(3)]
    [DefaultAbilityIntData(1)]
    public class HeightChange : Ability
    {
        [Tooltip("Is the ability a concurrent ability?")]
        [SerializeField] protected bool m_ConcurrentAbility = true;
        [Tooltip("Specifies the value to set the Height Animator parameter value to.")]
        [SerializeField] protected int m_Height = 1;
        [Tooltip("The amount to adjust the height of the CapsuleCollider by when active. This is only used if the character does not have an animator.")]
        [SerializeField] protected float m_CapsuleColliderHeightAdjustment = -0.4f;

        public bool ConcurrentAbility { get { return m_ConcurrentAbility; } set { m_ConcurrentAbility = value; } }
        public float CapsuleColliderHeightAdjustment { get { return m_CapsuleColliderHeightAdjustment; } set { m_CapsuleColliderHeightAdjustment = value; } }

        public override bool IsConcurrent { get { return m_ConcurrentAbility; } }

        private Vector3[] m_StartColliderCenter;
        private float[] m_CapsuleColliderHeight;
        private Collider[] m_OverlapColliders = new Collider[1];

        /// <summary>
        /// Initialize the collider storage arrays.
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Initialize the arrays which will help determine if the ability can stop.
            m_StartColliderCenter = new Vector3[m_CharacterLocomotion.Colliders.Length];
            m_CapsuleColliderHeight = new float[m_CharacterLocomotion.Colliders.Length];
            var capsuleColliderCount = 0;
            for (int i = 0; i < m_CharacterLocomotion.Colliders.Length; ++i) {
                if (m_CharacterLocomotion.Colliders[i] is CapsuleCollider) {
                    capsuleColliderCount++;
                }
            }

            // The counts won't be equal if the character has non-CapsuleCollider colliders.
            if (capsuleColliderCount != m_CapsuleColliderHeight.Length) {
                System.Array.Resize(ref m_CapsuleColliderHeight, capsuleColliderCount);
            }
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) {
                return false;
            }

            return m_CharacterLocomotion.Grounded;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            // When the ability stops it'll check to ensure there is room for the colliders. Save the collider center/height so this check can be made.
            var capsuleColliderCount = 0;
            for (int i = 0; i < m_CharacterLocomotion.Colliders.Length; ++i) {
                if (m_CharacterLocomotion.Colliders[i] is CapsuleCollider) {
                    var capsuleCollider = (m_CharacterLocomotion.Colliders[i] as CapsuleCollider);
                    m_CapsuleColliderHeight[capsuleColliderCount] = capsuleCollider.height;
                    m_StartColliderCenter[i] = capsuleCollider.center;
                    capsuleColliderCount++;
                } else { // SphereCollider.
                    m_StartColliderCenter[i] = (m_CharacterLocomotion.Colliders[i] as SphereCollider).center;
                }
            }

            SetHeightParameter(m_Height);
            EventHandler.ExecuteEvent(m_GameObject, "OnHeightChangeAdjustHeight", m_CapsuleColliderHeightAdjustment);
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility()
        {
            m_CharacterLocomotion.EnableColliderCollisionLayer(false);
            var keepActive = false;

            // The ability can't stop if there isn't enough room for the character to occupy their original height.
            var capsuleColliderCount = 0;
            for (int i = 0; i < m_CharacterLocomotion.Colliders.Length; ++i) {
                // Determine if the collider would intersect any objects.
                if (m_CharacterLocomotion.Colliders[i] is CapsuleCollider) {
                    var capsuleCollider = m_CharacterLocomotion.Colliders[i] as CapsuleCollider;
                    Vector3 startEndCap, endEndCap;
                    MathUtility.CapsuleColliderEndCaps(m_CapsuleColliderHeight[capsuleColliderCount], capsuleCollider.radius, m_StartColliderCenter[i], MathUtility.CapsuleColliderDirection(capsuleCollider), 
                                                                capsuleCollider.transform.position, capsuleCollider.transform.rotation, out startEndCap, out endEndCap);
                    // If there is overlap then the ability can't stop.
                    if (Physics.OverlapCapsuleNonAlloc(startEndCap, endEndCap, capsuleCollider.radius, m_OverlapColliders, m_LayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore) > 0) {
                        keepActive = true;
                        break;
                    }
                    capsuleColliderCount++;
                } else { // SphereCollider.
                    var sphereCollider = m_CharacterLocomotion.Colliders[i] as SphereCollider;
                    // If there is overlap then the ability can't stop.
                    if (Physics.OverlapSphereNonAlloc(sphereCollider.transform.TransformPoint(sphereCollider.center), sphereCollider.radius,
                                                                    m_OverlapColliders, m_LayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore) > 0) {
                        keepActive = true;
                        break;
                    }
                }
            }

            m_CharacterLocomotion.EnableColliderCollisionLayer(true);
            if (keepActive) {
                return false;
            }

            return base.CanStopAbility();
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
            return ((startingAbility is SpeedChange) && startingAbility.Index > Index) || startingAbility is StoredInputAbilityBase; 
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            return ((activeAbility is SpeedChange) && activeAbility.Index > Index);
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            SetHeightParameter(0);
            EventHandler.ExecuteEvent(m_GameObject, "OnHeightChangeAdjustHeight", -m_CapsuleColliderHeightAdjustment);
        }
    }
}