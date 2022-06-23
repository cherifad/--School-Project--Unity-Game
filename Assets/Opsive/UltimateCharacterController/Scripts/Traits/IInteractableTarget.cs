/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Traits
{
    /// <summary>
    /// Interface for an object that can be interacted with (such as a platform or door).
    /// </summary>
    public interface IInteractableTarget
    {
        /// <summary>
        /// Can the target be interacted with?
        /// </summary>
        /// <returns>True if the target can be interacted with.</returns>
        bool CanInteract();

        /// <summary>
        /// Interact with the target.
        /// </summary>
        void Interact();
    }
}