/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;

namespace Opsive.UltimateCharacterController.Traits
{
    /// <summary>
    /// Extends the Respawner by listening/executing character related events.
    /// </summary>
    public class CharacterRespawner : Respawner
    {
        private bool m_Active;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterActivate", OnActivate);
            m_Active = true;
        }

        /// <summary>
        /// Do the respawn by setting the position and rotation back to their starting values. Enable the GameObject and let all of the listening objects know that
        /// we have been respawned.
        /// </summary>
        public override void Respawn()
        {
            base.Respawn();

            // Execute OnCharacterImmediateTransformChange after OnRespawn to ensure all of the interested components are using the new position/rotation.
            if (m_PositioningMode != SpawnPositioningMode.None) {
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterImmediateTransformChange", true);
            }
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        protected override void OnDisable()
        {
            // If the GameObject was deactivated then the respawner shouldn't respawn.
            if (m_Active) {
                base.OnDisable();
            }
        }

        /// <summary>
        /// The character has been activated or deactivated.
        /// </summary>
        /// <param name="activate">Was the character activated?</param>
        private void OnActivate(bool activate)
        {
            m_Active = activate;
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterActivate", OnActivate);
        }
    }
}