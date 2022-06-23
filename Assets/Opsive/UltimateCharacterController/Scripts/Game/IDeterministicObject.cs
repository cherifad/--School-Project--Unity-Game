/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Game
{
    /// <summary>
    /// Interface for any deterministic object that can be moved with no parameters.
    /// </summary>
    public interface IDeterministicObject
    {
        /// <summary>
        /// A reference to the object's transform component.
        /// </summary>
        Transform transform { get; }

        /// <summary>
        /// Moves the object.
        /// </summary>
        void Move();
    }
}