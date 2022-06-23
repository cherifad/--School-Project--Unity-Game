/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Character.Abilities.AI
{
    /// <summary>
    /// Base class for moving the character with a pathfinding implementation.
    /// </summary>
    public abstract class PathfindingMovement : Ability
    {
        /// <summary>
        /// Returns the desired input vector value. This will be used by the Ultimate Character Locomotion componnet.
        /// </summary>
        public abstract Vector2 InputVector { get; }
        /// <summary>
        /// Returns the desired yaw rotation value. This will be used by the Ultimate Character Locomotion component.
        /// </summary>
        public abstract float DeltaYawRotation { get; }

        /// <summary>
        /// Updates the character's input and target rotation values.
        /// </summary>
        public override void Update()
        {
            m_CharacterLocomotion.InputVector = InputVector;
            m_CharacterLocomotion.DeltaYawRotation = DeltaYawRotation;
        }
    }
}