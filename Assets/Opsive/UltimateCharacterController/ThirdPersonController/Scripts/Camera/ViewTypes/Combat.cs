/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes
{
    /// <summary>
    /// The Combat View Type will inherit the functionality from the Third Person View Type while keeping the camera rotated to the same local yaw value
    /// as the character.
    /// </summary>
    [UltimateCharacterController.Camera.ViewTypes.RecommendedMovementType(typeof(Character.MovementTypes.Combat))]
    [UltimateCharacterController.Camera.ViewTypes.AddViewState("Zoom", "edafe89541fb59d4dba60703f5b1574a")]
    public class Combat : ThirdPerson
    {
        /// <summary>
        /// Rotates the camera according to the horizontal and vertical movement values.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        /// <param name="immediatePosition">Should the camera be positioned immediately?</param>
        /// <returns>The updated rotation.</returns>
        public override Quaternion Rotate(float horizontalMovement, float verticalMovement, bool immediatePosition)
        {
            m_Yaw += horizontalMovement;

            return base.Rotate(horizontalMovement, verticalMovement, immediatePosition);
        }
    }
}