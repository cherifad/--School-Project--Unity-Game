/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Input;
using Opsive.UltimateCharacterController.SurfaceSystem;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    /// The Jump ability allows the character to jump into the air. Jump is only active when the character has a positive y velocity.
    /// </summary>
    [DefaultInputName("Jump")]
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultStopType(AbilityStopType.Automatic)]
    [DefaultAbilityIndex(1)]
    [DefaultUseRootMotionPosition(AbilityBoolOverride.False)]
    [DefaultUseRootMotionRotation(AbilityBoolOverride.False)]
    public class Jump : Ability
    {
        [Tooltip("Prevents the jump ability from starting if there is an object above the character within the specified distance. Set to -1 to disable.")]
        [SerializeField] protected float m_MinCeilingJumpHeight = 0.05f;
        [Tooltip("The amount of force that should be applied when the character jumps.")]
        [SerializeField] protected float m_Force = 0.22f;
        [Tooltip("A multiplier applied to the force while moving sideways.")]
        [SerializeField] protected float m_SidewaysForceMultiplier = 0.8f;
        [Tooltip("A multiplier applied to the force while moving backwards.")]
        [SerializeField] protected float m_BackwardsForceMultiplier = 0.7f;
        [Tooltip("The number of frames that the force is applied in.")]
        [SerializeField] protected int m_Frames = 1;
        [Tooltip("Determines how quickly the jump force wears off.")]
        [SerializeField] protected float m_ForceDamping = 0.08f;
        [Tooltip("Specifies if the ability should wait for the OnAnimatorJump animation event or wait for the specified duration before applying the jump force.")]
        [SerializeField] protected AnimationEventTrigger m_JumpEvent = new AnimationEventTrigger(true, 0f);
        [Tooltip("The Surface Impact triggered when the character jumps.")]
        [SerializeField] protected SurfaceImpact m_JumpSurfaceImpact;
        [Tooltip("The amount of force to add per frame if the jump button is being held down continuously. This is a common feature for providing increased jump control in platform games.")]
        [SerializeField] protected float m_ForceHold = 0.003f;
        [Tooltip("Determines how quickly the jump hold force wears off.")]
        [SerializeField] protected float m_ForceDampingHold = 0.5f;
        [Tooltip("Specifies the number of times the character can perform a repeated jump (double jump, triple jump, etc). Set to -1 to allow an infinite number of repeated jumps.")]
        [SerializeField] protected int m_MaxRepeatedJumpCount;
        [Tooltip("The amount of force that applied when the character performs a repeated jump.")]
        [SerializeField] protected float m_RepeatedJumpForce = 0.11f;
        [Tooltip("A vertical velocity value below the specified amount will stop the ability.")]
        [SerializeField] protected float m_VerticalVelocityStopThreshold = -0.001f;
        [Tooltip("The number of seconds that the jump ability has to wait after landing before it can start again.")]
        [SerializeField] protected float m_RecurrenceDelay = 0.2f;

        public float MinCeilingJumpHeight { get { return m_MinCeilingJumpHeight; } set { m_MinCeilingJumpHeight = value; } }
        public float Force { get { return m_Force; } set { m_Force = value; } }
        public float SidewaysForceMultiplier { get { return m_SidewaysForceMultiplier; } set { m_SidewaysForceMultiplier = value; } }
        public float BackwardsForceMultiplier { get { return m_BackwardsForceMultiplier; } set { m_BackwardsForceMultiplier = value; } }
        public int Frames { get { return m_Frames; } set { m_Frames = value; } }
        public float ForceDamping { get { return m_ForceDamping; } set { m_ForceDamping = value; } }
        public AnimationEventTrigger JumpEvent { get { return m_JumpEvent; } set { m_JumpEvent = value; } }
        public SurfaceImpact JumpSurfaceImpact { get { return m_JumpSurfaceImpact; } set { m_JumpSurfaceImpact = value; } }
        public float ForceHold { get { return m_ForceHold; } set { m_ForceHold = value; } }
        public float ForceDampingHold { get { return m_ForceDampingHold; } set { m_ForceDampingHold = value; } }
        public int MaxRepeatedJumpCount { get { return m_MaxRepeatedJumpCount; } set { m_MaxRepeatedJumpCount = value; } }
        public float RepeatedJumpForce { get { return m_RepeatedJumpForce; } set { m_RepeatedJumpForce = value; } }
        public float VerticalVelocityStopThreshold { get { return m_VerticalVelocityStopThreshold; } set { m_VerticalVelocityStopThreshold = value; } }
        public float RecurrenceDelay { get { return m_RecurrenceDelay; } set { m_RecurrenceDelay = value; } }

        private UltimateCharacterLocomotionHandler m_Handler;
        private ActiveInputEvent m_HoldInput;
        private ActiveInputEvent m_RepeatedJumpInput;

        private RaycastHit m_RaycastResult;
        private bool m_JumpApplied;
        private bool m_Jumping;
        private bool m_ApplyHoldForce;
        private float m_HoldForce;
        private int m_RepeatedJumpCount;
        private float m_LandTime = -1;

        [Snapshot] protected float HoldForce { get { return m_HoldForce; } set { m_HoldForce = value; } }
        [Snapshot] protected bool JumpApplied { get { return m_JumpApplied; } set { m_JumpApplied = value; } }
        [Snapshot] protected bool Jumping { get { return m_Jumping; } set { m_Jumping = value; } }

        public override float AbilityFloatData { get { if (m_Jumping) { return m_CharacterLocomotion.LocalLocomotionVelocity.y; } return -1; } }
        public override int AbilityIntData { get { return (m_JumpApplied ? 0 : 1); } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_Handler = m_GameObject.GetCachedComponent<UltimateCharacterLocomotionHandler>();

            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorJump", ApplyJumpForce);
        }

        /// <summary>
        /// Can the ability be started?
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) {
                return false;
            }

            // The character can't jump if they aren't on the ground nor if they recently landed.
            if (!m_CharacterLocomotion.Grounded || m_LandTime + m_RecurrenceDelay > Time.realtimeSinceStartup) {
                return false;
            }

            // The character can't jump if the slope is too steep.
            var slope = Vector3.Angle(m_CharacterLocomotion.Up, m_CharacterLocomotion.GroundRaycastHit.normal);
            if (slope > m_CharacterLocomotion.SlopeLimit) {
                return false;
            }

            if (m_MinCeilingJumpHeight != -1) {
                // Ensure the space above is clear to get off of the ground.
                if (m_CharacterLocomotion.SingleCast(m_CharacterLocomotion.Up * (m_MinCeilingJumpHeight + m_CharacterLocomotion.SkinWidth + m_CharacterLocomotion.ColliderSpacing),
                                    m_LayerManager.SolidObjectLayers, ref m_RaycastResult)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            m_ApplyHoldForce = true;
            m_HoldForce = 0;
            m_RepeatedJumpCount = 0;

            if (!m_JumpEvent.WaitForAnimationEvent) {
                Scheduler.ScheduleFixed(m_JumpEvent.Duration, ApplyJumpForce);
            }

            if (m_ForceHold > 0) {
                if (m_Handler != null && InputIndex != -1) {
                    m_HoldInput = ObjectPool.Get<ActiveInputEvent>();
                    m_HoldInput.Initialize(ActiveInputEvent.Type.ButtonUp, InputNames[InputIndex], "OnJumpAbilityReleaseHold");
                    m_Handler.RegisterInputEvent(m_HoldInput);
                }
                EventHandler.RegisterEvent(m_GameObject, "OnJumpAbilityReleaseHold", OnReleaseHold);
            }

            if (m_MaxRepeatedJumpCount > 0 || m_MaxRepeatedJumpCount == -1) {
                if (m_Handler != null && InputIndex != -1) {
                    m_RepeatedJumpInput = ObjectPool.Get<ActiveInputEvent>();
                    m_RepeatedJumpInput.Initialize(ActiveInputEvent.Type.ButtonDown, InputNames[InputIndex], "OnJumpAbilityRepeatedJump");
                    m_Handler.RegisterInputEvent(m_RepeatedJumpInput);
                }
                EventHandler.RegisterEvent(m_GameObject, "OnJumpAbilityRepeatedJump", OnRepeatedJump);
            }

            base.AbilityStarted();
        }

        /// <summary>
        /// The character has either landed or just left the ground.
        /// </summary>
        /// <param name="grounded">Is the character on the ground?</param>
        private void OnGrounded(bool grounded)
        {
            if (grounded) {
                if (IsActive) {
                    StopAbility(true);
                }
                // Remember the land time to prevent jumping more than the JumpReoccuranceDelay.
                m_LandTime = Time.unscaledTime;
            }
        }

        /// <summary>
        /// The character should start the jump.
        /// </summary>
        private void ApplyJumpForce()
        {
            if (IsActive && !m_JumpApplied) {
                // A surface effect can optionally play when the character leaves the ground.
                if (m_JumpSurfaceImpact != null) {
                    SurfaceManager.SpawnEffect(m_CharacterLocomotion.GroundRaycastHit, m_JumpSurfaceImpact, m_CharacterLocomotion.GravityDirection, m_CharacterLocomotion.TimeScale, m_GameObject);
                }
                // Do not set the Jumping variable because the ability should be active for at least one frame. If Jumping was set there is a chance
                // the ability could stop right away if the character jumps while moving down a slope.
                m_JumpApplied = true;
                var force = m_Force;
                // Prevent the character from jumping as high when moving backwards or sideways.
                if (m_CharacterLocomotion.InputVector.y < 0) {
                    force *= Mathf.Lerp(1, m_BackwardsForceMultiplier, Mathf.Abs(m_CharacterLocomotion.InputVector.y));
                } else {
                    // The character's forward movement will contribute to a full jump force.
                    force *= Mathf.Lerp(1, m_SidewaysForceMultiplier, Mathf.Abs(m_CharacterLocomotion.InputVector.x) - Mathf.Abs(m_CharacterLocomotion.InputVector.y));
                }
                AddForce(m_CharacterLocomotion.Up * force, m_Frames, false, true);

                // Ensure the character is in the air after jumping.
                Scheduler.ScheduleFixed(Time.fixedDeltaTime + .001f, EnsureAirborne);
            }
        }

        /// <summary>
        /// After jumping the character should be in the air. If the character is not in the air then another object prevented the character from jumping and the
        /// jump ability should be stopped.
        /// </summary>
        private void EnsureAirborne()
        {
            if (!m_CharacterLocomotion.Grounded) {
                return;
            }

            StopAbility(true);
        }

        /// <summary>
        /// The user is no longer holding the jump button down.
        /// </summary>
        private void OnReleaseHold()
        {
            m_ApplyHoldForce = false;
        }

        /// <summary>
        /// The ability should perform a repeated jump.
        /// </summary>
        private void OnRepeatedJump()
        {
            // Perform the repeated jump if the number of jumps is less than the max count or the character can do an infinite number of repeated jumps.
            if (m_RepeatedJumpCount < m_MaxRepeatedJumpCount || m_MaxRepeatedJumpCount == -1) {
                AddForce(m_CharacterLocomotion.Up * m_RepeatedJumpForce, 1, false, true);
                m_RepeatedJumpCount++;
            }
        }

        /// <summary>
        /// Allows for the Jump ability to add 
        /// </summary>
        public override void UpdatePosition()
        {
            if (!m_Jumping) {
                if (m_JumpApplied) {
                    m_Jumping = true;
                }
                return;
            }

            var force = 0f;
            var deltaTime = m_CharacterLocomotion.TimeScaleSquared * Utility.TimeUtility.FramerateFixedDeltaTimeScaled;
            // Continuously apply a damping force while in the air.
            if (m_ForceDamping > 0) {
                var localExternalForce = m_CharacterLocomotion.LocalExternalForce;
                var targetForce = localExternalForce.y / (1 + m_ForceDamping * deltaTime);
                force = (targetForce - localExternalForce.y);
            }

            // Allow a force and damping to be applied when the input button is held down.
            if (m_ForceHold > 0 && m_ApplyHoldForce) {
                m_HoldForce += m_ForceHold;
                m_HoldForce /= (1 + m_ForceDampingHold * deltaTime);
                force += m_HoldForce;
            }

            // When the jump force is added it is added to the character's external force. Dampen this external force.
            if (force != 0) {
                AddForce(m_CharacterLocomotion.Up * force, 1, false, false);
            }
        }

        /// <summary>
        /// Update the Animator for the ability. Called after the movement has been applied.
        /// </summary>
        public override void UpdateAnimator()
        {
            // Set the Float Data parameter for the blend tree.
            if (m_Jumping) {
                SetAbilityFloatDataParameter(m_CharacterLocomotion.LocalLocomotionVelocity.y);
            }
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility()
        {
            // The Jump ability is done if the velocity is less than a the specified value.
            if (m_Jumping && m_CharacterLocomotion.LocalLocomotionVelocity.y <= m_VerticalVelocityStopThreshold) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            m_Jumping = false;
            m_JumpApplied = false;

            // Unregister for the ability input events.
            if (m_ForceHold > 0) {
                if (m_Handler != null) {
                    m_Handler.UnregisterInputEvent(m_HoldInput);
                    ObjectPool.Return(m_HoldInput);
                }
                EventHandler.UnregisterEvent(m_GameObject, "OnJumpAbilityReleaseHold", OnReleaseHold);
            }

            if (m_MaxRepeatedJumpCount > 0 || m_MaxRepeatedJumpCount == -1) {
                if (m_Handler != null) {
                    m_Handler.UnregisterInputEvent(m_RepeatedJumpInput);
                    ObjectPool.Return(m_RepeatedJumpInput);
                }
                EventHandler.UnregisterEvent(m_GameObject, "OnJumpAbilityRepeatedJump", OnRepeatedJump);
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
            return startingAbility is HeightChange;
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorJump", ApplyJumpForce);
            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
        }
    }
}