/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character
{
    /// <summary>
    /// Rotates and sets the CapsuleCollider height so it always matches the same relative location/size of the character.
    /// </summary>
    public class CapsuleColliderPositioner : StateBehavior
    {
        [Tooltip("Should the positioner rotate the collider to match the targets?")]
        [SerializeField] protected bool m_RotateCollider;
        [Tooltip("A reference to the target that is near the first end cap.")]
        [SerializeField] protected Transform m_FirstEndCapTarget;
        [Tooltip("A reference to the target that is near the second end cap.")]
        [SerializeField] protected Transform m_SecondEndCapTarget;
        [Tooltip("The padding on top of the second end cap target.")]
        [SerializeField] protected float m_SecondEndCapPadding;

        public bool RotateCollider { get { return m_RotateCollider; } set { m_RotateCollider = value; } }
        [NonSerialized] public Transform FirstEndCapTarget { get { return m_FirstEndCapTarget; } set { m_FirstEndCapTarget = value; } }
        [NonSerialized] public Transform SecondEndCapTarget { get { return m_SecondEndCapTarget; } set { m_SecondEndCapTarget = value; Initialize(); } }
        [NonSerialized] public float SecondEndCapPadding { get { return m_SecondEndCapPadding; } set { m_SecondEndCapPadding = value; } }

        private Transform m_Transform;
        private CapsuleCollider m_CapsuleCollider;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private GameObject m_CharacterGameObject;
        private Transform m_CharacterTransform;
        private LayerManager m_LayerManager;

        private Collider[] m_OverlapColliders;
        private Vector3 m_FirstEndCapOffset;
        private Vector3 m_SecondEndCapOffset;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_Transform = transform;
            m_CapsuleCollider = GetComponent<CapsuleCollider>();
            m_CharacterLocomotion = gameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
            m_CharacterGameObject = m_CharacterLocomotion.gameObject;
            m_CharacterTransform = m_CharacterLocomotion.transform;
            m_LayerManager = gameObject.GetCachedParentComponent<LayerManager>();
            StateManager.LinkGameObjects(m_CharacterTransform.gameObject, gameObject, true);

            m_OverlapColliders = new Collider[1];

            EventHandler.RegisterEvent(m_CharacterGameObject, "OnAnimatorSnapped", AnimatorSnapped);
            EventHandler.RegisterEvent<bool>(m_CharacterGameObject, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_CharacterGameObject, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_CharacterGameObject, "OnRespawn", OnRespawn);

            Initialize();

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            // The positioner cannot be used with server autoritative implementations.
            var networkCharacter = m_CharacterGameObject.GetCachedComponent<Networking.Character.INetworkCharacter>();
            if (networkCharacter != null && networkCharacter.IsServerAuthoritative()) {
                enabled = false;
                Debug.LogWarning("Warning: The CapsuleColliderPositioner has been disabled. Unity bug 985643 needs to be fixed for it to work over a server authoritative network.");
            }
#endif
        }

        /// <summary>
        /// The animator has snapped into position. Reinitialized.
        /// </summary>
        private void AnimatorSnapped()
        {
            // The positioner only needs to be initialized once.
            EventHandler.UnregisterEvent(m_CharacterGameObject, "OnAnimatorSnapped", AnimatorSnapped);

            Initialize();
        }

        /// <summary>
        /// Sets the initial end cap offsets.
        /// </summary>
        private void Initialize()
        {
            // If the first and second end cap targets are null then the component is likely added at runtime.
            if (!Application.isPlaying || (m_FirstEndCapTarget == null && m_SecondEndCapTarget == null)) {
                return;
            }

            // If the CapsuleCollider doesn't have a second end cap then the character doesn't have an animator. The end cap should be
            // a child GameObject so abilities (such as HeightChange) can adjust the collider height.
            if (m_SecondEndCapTarget == null) {
                m_SecondEndCapTarget = new GameObject("EndCap").transform;
                m_SecondEndCapTarget.SetParentOrigin(m_Transform);

                var localPosition = m_SecondEndCapTarget.localPosition;
                localPosition.y = m_CapsuleCollider.height;
                m_SecondEndCapTarget.localPosition = localPosition;

                EventHandler.RegisterEvent<float>(m_CharacterGameObject, "OnHeightChangeAdjustHeight", AdjustCapsuleColliderHeight);
            }

            Vector3 firstEndCap, secondEndCap;
            MathUtility.CapsuleColliderEndCaps(m_CapsuleCollider, transform.position, transform.rotation, out firstEndCap, out secondEndCap);

            if ((m_FirstEndCapTarget.position - firstEndCap).sqrMagnitude > (m_SecondEndCapTarget.position - firstEndCap).sqrMagnitude) {
                // The second target may be closer to the first end cap than the first target is. Switch the targets.
                var target = m_FirstEndCapTarget;
                m_FirstEndCapTarget = m_SecondEndCapTarget;
                m_SecondEndCapTarget = target;
            }
            m_FirstEndCapOffset = m_Transform.InverseTransformDirection(m_FirstEndCapTarget.position - firstEndCap);
            m_SecondEndCapOffset = m_Transform.InverseTransformDirection(m_SecondEndCapTarget.position - secondEndCap) - m_CharacterTransform.up * m_SecondEndCapPadding;
        }

        /// <summary>
        /// Perform the rotation and height changes.
        /// </summary>
        private void FixedUpdate()
        {
            UpdateRotationHeight();
        }

        /// <summary>
        /// Does the actual rotation and height changes. This method is separate from FixedUpdate so the rotation/height can be determined 
        /// during server reconciliation on the network.
        /// </summary>
        public void UpdateRotationHeight()
        {
            m_CharacterLocomotion.EnableColliderCollisionLayer(false);

            // Update the rotation of the CapsuleCollider so it is rotated in the same direction as the end cap targets.
            Vector3 firstEndCap, secondEndCap;
            Vector3 direction, localDirection, offset;
            if (m_RotateCollider) {
                direction = m_SecondEndCapTarget.position - m_FirstEndCapTarget.position;
                localDirection = MathUtility.InverseTransformDirection(direction, m_CharacterTransform.rotation);
                var targetRotation = m_Transform.localRotation * Quaternion.FromToRotation(m_Transform.localRotation * Vector3.up, localDirection.normalized);
                targetRotation = MathUtility.TransformQuaternion(m_CharacterTransform.rotation, targetRotation);

                // Ensure the new rotation doesn't overlap with any other objects. The collider spacing is added to prevent the collider from overlapping 
                // another collider. If the colliders overlap then the locomotion component won't be able to detect any collisions.
                MathUtility.CapsuleColliderEndCaps(m_CapsuleCollider, m_Transform.position, targetRotation, out firstEndCap, out secondEndCap);
                offset = m_CharacterLocomotion.Up * m_CharacterLocomotion.ColliderSpacing;
                if (Physics.OverlapCapsuleNonAlloc(firstEndCap + offset, secondEndCap + offset, m_CapsuleCollider.radius + m_CharacterLocomotion.ColliderSpacing, m_OverlapColliders, m_LayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore) > 0) {
                    // At least one non-character collider overlaps the collider with the target rotation. Don't update the rotation.
                    // The end caps need to be updated with the original rotation.
                    MathUtility.CapsuleColliderEndCaps(m_CapsuleCollider, m_Transform.position, m_Transform.rotation, out firstEndCap, out secondEndCap);
                } else {
                    // No colliders overlap the target rotation - update the Transform rotation.
                    m_Transform.rotation = targetRotation;
                }
            } else {
                MathUtility.CapsuleColliderEndCaps(m_CapsuleCollider, m_Transform.position, m_Transform.rotation, out firstEndCap, out secondEndCap);
            }

            // After the CapsuleCollider has rotated determine the new height of the CapsuleCollider. This can be done by determining the current
            // end cap locations and then getting the offset from the start end cap offsets.
            var firstEndCapOffset = m_Transform.InverseTransformDirection(m_FirstEndCapTarget.position - firstEndCap);
            var secondEndCapOffset = m_Transform.InverseTransformDirection(m_SecondEndCapTarget.position - secondEndCap);
            offset = m_SecondEndCapOffset - m_FirstEndCapOffset;
            localDirection = ((secondEndCapOffset - firstEndCapOffset) - offset);

            // Determine if the new height would cause any collisions. If it does not then apply the height changes. A negative height change will never cause any
            // collisions so the OverlapCapsule does not need to be checked.
            if (localDirection.y < 0 || !m_CharacterLocomotion.UsingVerticalCollisionDetection ||
                        Physics.OverlapCapsuleNonAlloc(firstEndCap + m_CharacterLocomotion.Up * localDirection.y, secondEndCap + m_CharacterLocomotion.Up * localDirection.y,
                                    m_CapsuleCollider.radius + m_CharacterLocomotion.ColliderSpacing, m_OverlapColliders, m_LayerManager.SolidObjectLayers,
                                    QueryTriggerInteraction.Ignore) == 0) {
                // Adjust the CapsuleCollider height and center to account for the new offset.
                m_CapsuleCollider.height += localDirection.y;
                var center = m_CapsuleCollider.center;
                center.y += localDirection.y / 2;
                m_CapsuleCollider.center = center;
            }
            m_CharacterLocomotion.EnableColliderCollisionLayer(true);
        }

        /// <summary>
        /// Adjusts the collider height by the given amount.
        /// </summary>
        /// <param name="amount">The amount to adjust the collider height by.</param>
        private void AdjustCapsuleColliderHeight(float amount)
        {
            var localPosition = m_SecondEndCapTarget.localPosition;
            localPosition.y = m_CapsuleCollider.height + amount;
            m_SecondEndCapTarget.localPosition = localPosition;
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            enabled = false;
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
            enabled = true;
        }

        /// <summary>
        /// The character's position or rotation has been teleported.
        /// </summary>
        /// <param name="snapAnimator">Should the animator be snapped?</param>
        private void OnImmediateTransformChange(bool snapAnimator)
        {
            UpdateRotationHeight();
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent(m_CharacterGameObject, "OnAnimatorSnapped", Initialize);
            EventHandler.UnregisterEvent<float>(m_CharacterGameObject, "OnHeightChangeAdjustHeight", AdjustCapsuleColliderHeight);
            EventHandler.UnregisterEvent<bool>(m_CharacterGameObject, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_CharacterGameObject, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_CharacterGameObject, "OnRespawn", OnRespawn);
        }
    }
}