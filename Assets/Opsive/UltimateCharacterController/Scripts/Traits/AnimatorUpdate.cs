/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Game;

namespace Opsive.UltimateCharacterController.Traits
{
    /// <summary>
    /// Updates the Animator component at a fixed delta time.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimatorUpdate : MonoBehaviour, IDeterministicObject
    {
        private Animator m_Animator;

        private int m_DeterministicObjectIndex;

        /// <summary>
        /// Cache the componetn references.
        /// </summary>
        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_Animator.enabled = false;
        }

        /// <summary>
        /// Registers the object with the DeterministicObjectManager.
        /// </summary>
        private void OnEnable()
        {
            m_DeterministicObjectIndex = DeterministicObjectManager.RegisterObject(this);
        }

        /// <summary>
        /// Updates the Animator at a fixed delta time.
        /// </summary>
        public void Move()
        {
            m_Animator.Update(Time.fixedDeltaTime);
        }

        /// <summary>
        /// Unregisters the object with the DeterministicObjectManager.
        /// </summary>
        private void OnDisable()
        {
            DeterministicObjectManager.UnregisterDeterministicObject(m_DeterministicObjectIndex);
        }
    }
}