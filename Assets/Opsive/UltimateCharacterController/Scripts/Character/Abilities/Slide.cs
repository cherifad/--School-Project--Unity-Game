/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    /// The Slide ability will apply a force to the character if the character is on a steep slope.
    /// </summary>
    [DefaultStopType(AbilityStopType.Automatic)]
    public class Slide : Ability
    {
        [Tooltip("Steepness (in degrees) above which the character can slide.")]
        [SerializeField] protected float m_MinSlideLimit = 30;
        [Tooltip("Steepness (in degrees) below which the character can slide.")]
        [SerializeField] protected float m_MaxSlideLimit = 89f;
        [Tooltip("Multiplier of the ground's slide value. The slide value is determined by (1 - dynamicFriction) of the ground's physic material.")]
        [SerializeField] protected float m_Multiplier = 0.1f;

        public float MinSlideLimit { get { return m_MinSlideLimit; } set { m_MinSlideLimit = value; } }
        public float MaxSlideLimit { get { return m_MaxSlideLimit; } set { m_MaxSlideLimit = value; } }
        public float Multiplier { get { return m_Multiplier; } set { m_Multiplier = value; } }

        private float m_SlideSpeed;
        private float m_OnSteepGroundTime;

        public override bool IsConcurrent { get { return true; } }

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

            return CanSlide();
        }

        /// <summary>
        /// Returns true if the character can slide on the ground.
        /// </summary>
        /// <returns>True if the character can slide on the ground.</returns>
        private bool CanSlide()
        {
            // The character cannot slide in the air.
            if (!m_CharacterLocomotion.Grounded) {
                return false;
            }

            // The character cannot slide if the slope isn't steep enough or is too steep.
            var slope = Vector3.Angle(m_CharacterLocomotion.Up, m_CharacterLocomotion.GroundRaycastHit.normal);
            if (slope < m_MinSlideLimit + 0.3f || slope > m_MaxSlideLimit) {
                return false;
            }

            // The character can slide.
            return true;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            m_OnSteepGroundTime = 0;
        }

        /// <summary>
        /// Updates the ability. Applies a force to the controller to make the character slide
        /// </summary>
        public override void UpdatePosition()
        {
            var groundRaycastHit = m_CharacterLocomotion.GroundRaycastHit;
            // Slide at a constant speed if the slope is within the slope limit.
            var slope = Vector3.Angle(groundRaycastHit.normal, m_CharacterLocomotion.Up);
            // The slide value uses the ground's physic material to get the amount of friction of the material.
            var slide = (1 - groundRaycastHit.collider.material.dynamicFriction) * m_Multiplier;
            if (slope < m_CharacterLocomotion.SlopeLimit) {
                m_OnSteepGroundTime = 0;
                m_SlideSpeed = Mathf.Max(m_SlideSpeed, slide);
            } else { // The slope is steeper then the slope limit. Slide with an accelerating slide speed.
                if (m_OnSteepGroundTime == 0) {
                    m_OnSteepGroundTime = Time.time;
                }
                m_SlideSpeed += ((slide * ((Time.time - m_OnSteepGroundTime) * 0.125f)) * m_CharacterLocomotion.TimeScale * TimeUtility.FixedDeltaTimeScaled);
                m_SlideSpeed = Mathf.Max(slide, m_SlideSpeed);
            }

            // Add a force if the character should slide.
            if (m_SlideSpeed > 0) {
                // Only add a horizontal force to the character - the controller will take care of adding any vertical forces based on gravity.
                var direction = Vector3.ProjectOnPlane(Vector3.Cross(Vector3.Cross(groundRaycastHit.normal, -m_CharacterLocomotion.Up), groundRaycastHit.normal), m_CharacterLocomotion.Up);
                AddForce(direction.normalized * m_SlideSpeed * m_CharacterLocomotion.TimeScale * TimeUtility.FixedDeltaTimeScaled, 1, false, true);
            }
        }

        /// <summary>
        /// Stop the ability from running.
        /// </summary>
        /// <returns>True if the ability was stopped.</returns>
        public override bool CanStopAbility()
        {
            return !CanSlide();
        }
    }
}