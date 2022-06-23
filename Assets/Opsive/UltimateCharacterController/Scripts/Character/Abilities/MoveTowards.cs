/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Objects.CharacterAssist;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    /// Moves the character to the specified start location. This ability will be called manually by the controller and should not be started by the user.
    /// </summary>
    [DefaultStartType(AbilityStartType.Manual)]
    [DefaultAllowPositionalInput(false)]
    [DefaultAllowRotationalInput(false)]
    public class MoveTowards : Ability
    {
        [Tooltip("The multiplier to apply to the input vecotr. Allows the character to move towards the destination faster.")]
        [SerializeField] private float m_InputMultiplier = 1;

        public override bool IsConcurrent { get { return true; } }

        private AbilityStartLocation m_StartLocation;
        private Ability m_OnArriveAbility;

        private SpeedChange[] m_SpeedChangeAbilities;
        private float m_MovementMultiplier;
        private Vector3 m_TargetDirection;
        private bool m_Arrived;
        private bool m_PrecisionStartWait;

        public Ability OnArriveAbility { get { return m_OnArriveAbility; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            m_SpeedChangeAbilities = m_CharacterLocomotion.GetAbilities<SpeedChange>();
        }

        /// <summary>
        /// Starts moving to the specified start location.
        /// </summary>
        /// <param name="startLocations">The locations the character can move towards. If multiple locations are possible then the closest valid location will be used.</param>
        /// <param name="onArriveAbility">The ability that should be started as soon as the character arrives at the location.</param>
        /// <returns>True if the MoveTowards ability is started.</returns>
        public bool StartMoving(AbilityStartLocation[] startLocations, Ability onArriveAbility)
        {
            // MoveTowards doesn't need to start if there is no start location.
            if (startLocations == null || startLocations.Length == 0) {
                return false;
            }

            // The arrive ability must exist and be unique. If the ability is already set then StartMoving may have been triggered because the arrive ability
            // should start.
            if (onArriveAbility == null || onArriveAbility == m_OnArriveAbility) {
                return false;
            }

            // No reason to start if the character is already in a valid start location.
            for (int i = 0; i < startLocations.Length; ++i) {
                if (startLocations[i].IsPositionValid(m_Transform.position, m_CharacterLocomotion.Grounded) && startLocations[i].IsRotationValid(m_Transform.rotation)) {
                    return false;
                }
            }

            // The character needs to move - start the ability.
            m_StartLocation = GetClosestStartLocation(startLocations);
            m_OnArriveAbility = onArriveAbility;

            // The movement speed will depend on the current speed the character is moving.
            m_MovementMultiplier = 1;
            if (m_SpeedChangeAbilities != null) {
                for (int i = 0; i < m_SpeedChangeAbilities.Length; ++i) {
                    if (m_SpeedChangeAbilities[i].IsActive) {
                        m_MovementMultiplier = m_SpeedChangeAbilities[i].SpeedChangeMultiplier;
                        break;
                    }
                }
            }

            if (m_OnArriveAbility.Index < Index) {
                Debug.LogWarning("Warning: " + m_OnArriveAbility.GetType().Name + " has a higher priority then the MoveTowards ability. This will cause unintended behavior.");
            }

            StartAbility();

            return true;
        }

        /// <summary>
        /// Returns the closest start location out of the possible AbilityStartLocations.
        /// </summary>
        /// <param name="startLocations">The locations the character can move towards.</param>
        /// <returns>The best location out of the possible AbilityStartLocations.</returns>
        private AbilityStartLocation GetClosestStartLocation(AbilityStartLocation[] startLocations)
        {
            // If only one location is available then it is the closest.
            if (startLocations.Length == 1) {
                return startLocations[0];
            }

            // Multiple locations are available. Choose the closest location.
            AbilityStartLocation startLocation = null;
            var closestDistance = float.MaxValue;
            float distance;
            for (int i = 0; i < startLocations.Length; ++i) {
                if ((distance = (m_Transform.position - startLocations[i].TargetPosition).sqrMagnitude) < closestDistance) {
                    closestDistance = distance;
                    startLocation = startLocations[i];
                }
            }

            return startLocation;
        }

        /// <summary>
        /// The ability will start - perform any initialization before starting.
        /// </summary>
        /// <returns>True if the ability should start.</returns>
        public override bool AbilityWillStart()
        {
            m_AllowEquippedSlotsMask = m_OnArriveAbility.AllowEquippedSlotsMask;
            return true;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            m_Arrived = false;

            // Force independent look so the ability will have complete control over the rotation.
            for (int i = 0; i < m_CharacterLocomotion.MovementTypes.Length; ++i) {
                m_CharacterLocomotion.MovementTypes[i].ForceIndependentLook = true;
            }
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
            return (startingAbility is Items.ItemAbility) || startingAbility.Index > Index || startingAbility is StoredInputAbilityBase;
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            return activeAbility is Items.ItemAbility || activeAbility is StoredInputAbilityBase;
        }

        /// <summary>
        /// Updates the ability.
        /// </summary>
        public override void Update()
        {
            base.Update();

            // The input and target rotation values should move towards the target.
            var arrived = m_StartLocation.IsRotationValid(m_Transform.rotation);
            var rotation = GetTargetRotation() * Quaternion.Inverse(m_Transform.rotation);
            m_CharacterLocomotion.DeltaYawRotation = Utility.MathUtility.ClampInnerAngle(rotation.eulerAngles.y);

            m_TargetDirection = m_Transform.InverseTransformDirection(m_StartLocation.TargetPosition - m_Transform.position);
            if (!m_StartLocation.IsPositionValid(m_Transform.position, m_CharacterLocomotion.Grounded)) {
                m_CharacterLocomotion.InputVector = GetInputVector(m_TargetDirection);
                arrived = false;
            }

            // The character should completely stop moving when they have arrived.
            if (arrived && !m_Arrived) {
                m_CharacterLocomotion.ResetRotationPosition();
                m_Arrived = true;
                // Return early if the start location requires a precision start. This will allow the animator to start transitioning the next frame.
                if (m_StartLocation.PrecisionStart) {
                    m_PrecisionStartWait = true;
                    return;
                }
            }

            // Keep the MoveTowards ability active until the character has arrived at the destination and the ItemEquipVerifier ability isn't active.
            // This will prevent the character from sliding when ItemEquipVerifier is active and MoveTowards is not active.
            if (arrived && (m_CharacterLocomotion.ItemEquipVerifierAbility == null || !m_CharacterLocomotion.ItemEquipVerifierAbility.IsActive)) {
                if (!m_StartLocation.PrecisionStart || !m_PrecisionStartWait) {
                    // Stop the ability before starting the OnArrive ability so MoveTowards doesn't prevent the ability from starting.
                    StopAbility();
                    m_CharacterLocomotion.TryStartAbility(m_OnArriveAbility, true, true);
                    m_OnArriveAbility = null;
                }

                // After the character is no longer in transition the arrive ability can start. This will ensure the character always starts in the correct location.
                // For some abilities it doesn't matter if the character is in a precise position and in that case the precision start field can be disabled.
                if (m_StartLocation.PrecisionStart && !m_AnimatorMonitor.IsInTransition(0)) {
                    m_PrecisionStartWait = false;
                }
            }
        }

        /// <summary>
        /// Returns the rotation that the character should rotate towards.
        /// </summary>
        /// <returns>The rotation that the character should rotate towards.</returns>
        protected virtual Quaternion GetTargetRotation()
        {
            return Quaternion.LookRotation(m_StartLocation.TargetRotation * Vector3.forward, m_CharacterLocomotion.Up);
        }

        /// <summary>
        /// Returns the input vector that the character should move with.
        /// </summary>
        /// <param name="direction">The direction that the character should move towards.</param>
        /// <returns>The input vector that the character should move with.</returns>
        protected virtual Vector2 GetInputVector(Vector3 direction)
        {
            var inputVector = Vector2.zero;
            inputVector.x = direction.x;
            inputVector.y = direction.z;
            return inputVector.normalized * m_InputMultiplier * m_MovementMultiplier;
        }

        /// <summary>
        /// Ensure the move direction is valid.
        /// </summary>
        public override void ApplyPosition()
        {
            // Prevent the character from jittering back and forth to land precisely on the target.
            var moveDirection = m_Transform.InverseTransformDirection(m_CharacterLocomotion.MoveDirection);
            if (Mathf.Abs(moveDirection.x) > Mathf.Abs(m_TargetDirection.x)) {
                moveDirection.x = m_TargetDirection.x;
            }
            if (Mathf.Abs(moveDirection.z) > Mathf.Abs(m_TargetDirection.z)) {
                moveDirection.z = m_TargetDirection.z;
            }
            m_CharacterLocomotion.MoveDirection = m_Transform.TransformDirection(moveDirection);
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            // Reset the force independet look parameter set within StartAbility.
            for (int i = 0; i < m_CharacterLocomotion.MovementTypes.Length; ++i) {
                m_CharacterLocomotion.MovementTypes[i].ForceIndependentLook = false;
            }
        }
    }
}