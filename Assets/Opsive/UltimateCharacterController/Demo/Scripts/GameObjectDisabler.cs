/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Demo
{
    /// <summary>
    /// Disables the specified GameObject when the character enters the trigger.
    /// </summary>
    public class GameObjectDisabler : MonoBehaviour
    {
        [Tooltip("The GameObject that should be disabled.")]
        [SerializeField] protected GameObject m_DisableGameObject;
        [Tooltip("The LayerMask of the character.")]
        [SerializeField] protected LayerMask m_LayerMask = 1 << LayerManager.Character;

        /// <summary>
        /// An object has entered the trigger.
        /// </summary>
        /// <param name="other">The object that entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character)) {
                return;
            }

            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            m_DisableGameObject.SetActive(false);
        }

        /// <summary>
        /// An object has left the trigger.
        /// </summary>
        /// <param name="other">The object that exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character)) {
                return;
            }

            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            m_DisableGameObject.SetActive(true);
        }
    }
}