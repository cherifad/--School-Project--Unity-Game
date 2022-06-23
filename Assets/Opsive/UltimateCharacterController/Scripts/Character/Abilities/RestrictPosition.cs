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
    /// The RestrictPosition ability restricts the character to the specified position.
    /// </summary>
    [DefaultStartType(AbilityStartType.Automatic)]
    [DefaultStopType(AbilityStopType.Manual)]
    public class RestrictPosition : Ability
    {
        /// <summary>
        /// Specifies how to restrict the character's position.
        /// </summary>
        public enum RestrictionType
        {
            RestrictX,  // Restricts the local X position.
            RestrictZ,  // Restricts the local Z position.
            RestrictXZ  // Restricts the local X and Z position.
        }

        [Tooltip("Specifies how to restrict the character's position.")]
        [HideInInspector] [SerializeField] protected RestrictionType m_Restriction = RestrictionType.RestrictXZ;
        [Tooltip("If restricting the X axis, specifies the minimum local X position the character can move to.")]
        [HideInInspector] [SerializeField] protected float m_MinXPosition;
        [Tooltip("If restricting the X axis, specifies the maximum local X position the character can move to.")]
        [HideInInspector] [SerializeField] protected float m_MaxXPosition;
        [Tooltip("If restricting the Z axis, specifies the minimum local Z position the character can move to.")]
        [HideInInspector] [SerializeField] protected float m_MinZPosition;
        [Tooltip("If restricting the Z axis, specifies the maximum local Z position the character can move to.")]
        [HideInInspector] [SerializeField] protected float m_MaxZPosition;
        [Tooltip("Should the movement animations stop playing when the character has reached the restricted position?")]
        [HideInInspector] [SerializeField] protected bool m_StopAnimation = true;
        [Tooltip("A small buffer which specifies how soon before the character is restricted the animations should stop playing.")]
        [HideInInspector] [SerializeField] protected float m_StopAnimationBuffer = 1;

        public override bool IsConcurrent { get { return true; } }

        /// <summary>
        /// Restrict the move direction if the character would be outside the valid position.
        /// </summary>
        public override void ApplyPosition()
        {
            var targetPosition = m_Transform.position + m_CharacterLocomotion.MoveDirection;
            var lookRotation = Quaternion.LookRotation(Vector3.forward, m_CharacterLocomotion.Up);
            var localTargetPosition = MathUtility.InverseTransformPoint(Vector3.zero, lookRotation, targetPosition);

            // Restrict the x axis if the constraint is set to anything but RestrictZ.
            if (m_Restriction != RestrictionType.RestrictZ) {
                if (localTargetPosition.x < m_MinXPosition) {
                    localTargetPosition.x = m_MinXPosition;
                } else if (localTargetPosition.x > m_MaxXPosition) {
                    localTargetPosition.x = m_MaxXPosition;
                }
            }

            // Restrict the z axis if the constraint is set to anything but RestrictX.
            if (m_Restriction != RestrictionType.RestrictX) {
                if (localTargetPosition.z < m_MinZPosition) {
                    localTargetPosition.z = m_MinZPosition;
                } else if (localTargetPosition.z > m_MaxZPosition) {
                    localTargetPosition.z = m_MaxZPosition;
                }
            }
            targetPosition = MathUtility.TransformPoint(Vector3.zero, lookRotation, localTargetPosition);
            m_CharacterLocomotion.MoveDirection = targetPosition - m_Transform.position;

            // When the character moves into a restricted position the movement animation can stop playing to prevent the character
            // from looking like they are walking into an invisible wall.
            if (m_StopAnimation) {
                // Convert the 2D vector into a 3D vector so the local comparisons are valid.
                var inputVector = (Vector3)m_CharacterLocomotion.InputVector;
                inputVector.z = inputVector.y;
                inputVector.y = 0;
                var localInputVector = m_Transform.TransformDirection(inputVector);

                // Restrict the x axis if the constraint is set to anything but RestrictZ.
                if (m_Restriction != RestrictionType.RestrictZ) {
                    if ((localInputVector.x < 0 && localTargetPosition.x < m_MinXPosition + m_StopAnimationBuffer) ||
                        (localInputVector.x > 0 && localTargetPosition.x > m_MaxXPosition - m_StopAnimationBuffer)) {
                        localInputVector.x = 0;
                    }
                }
                // Restrict the z axis if the constraint is set to anything but RestrictX.
                if (m_Restriction != RestrictionType.RestrictX) {
                    if ((localInputVector.z < 0 && localTargetPosition.z < m_MinZPosition + m_StopAnimationBuffer) ||
                        (localInputVector.z > 0 && localTargetPosition.z > m_MaxZPosition - m_StopAnimationBuffer)) {
                        localInputVector.z = 0;
                    }
                }

                // Convert the 3D vector back to 2D.
                inputVector = m_Transform.InverseTransformDirection(localInputVector);
                inputVector.y = inputVector.z;
                inputVector.z = 0;
                m_CharacterLocomotion.InputVector = inputVector;
            }
        }
    }
}