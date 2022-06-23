/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Character.Identifiers
{
    /// <summary>
    /// Identifying component which specifies the object should be hidden while in first person view.
    /// </summary>
    public class ThirdPersonObject : MonoBehaviour
    {
        [Tooltip("Should the object be visible when the character dies? This value will only be checked if the PerspectiveMonitor.ObjectDeathVisiblity is set to ThirdPersonObjectDetermined.")]
        [SerializeField] protected bool m_FirstPersonVisibleOnDeath = false;

        public bool FirstPersonVisibleOnDeath { get { return m_FirstPersonVisibleOnDeath; } }
    }
}