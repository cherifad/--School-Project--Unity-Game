/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Audio;
using Opsive.UltimateCharacterController.Objects.CharacterAssist;

namespace Opsive.UltimateCharacterController.Traits
{
    /// <summary>
    /// Represents any object that can be interacted with by the character. Acts as a link between the character and IInteractableTarget.
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        [Tooltip("The ID of the Interactable, used by the Interact ability for filtering. A value of -1 indicates no ID.")]
        [SerializeField] protected int m_ID = -1;
        [Tooltip("The object(s) that the interaction is performend on. This component must implement the IInteractableTarget.")]
        [SerializeField] protected MonoBehaviour[] m_Targets;

        public int ID { get { return m_ID; } set { m_ID = value; } }
        public MonoBehaviour[] Targets { get { return m_Targets; } set { m_Targets = value; } }

        private IInteractableTarget[] m_InteractableTargets;
        private AbilityIKTarget[] m_IKTargets;

        public AbilityIKTarget[] IKTargets { get { return m_IKTargets; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            if (m_Targets == null || m_Targets.Length == 0) {
                Debug.LogError("Error: An IInteractableTarget must be specified in the Targets field.");
                return;
            }

            m_InteractableTargets = new IInteractableTarget[m_Targets.Length];
            for (int i = 0; i < m_Targets.Length; ++i) {
                if (m_Targets[i] == null || !(m_Targets[i] is IInteractableTarget)) {
                    Debug.LogError("Error: element " + i + " of the Targets array is null or does not subscribe to the IInteractableTarget iterface.");
                } else {
                    m_InteractableTargets[i] = m_Targets[i] as IInteractableTarget;
                }
            }

            m_IKTargets = GetComponentsInChildren<AbilityIKTarget>();
        }

        /// <summary>
        /// Determines if the character can interact with the InteractableTarget.
        /// </summary>
        /// <returns>True if the character can interact with the InteractableTarget</returns>
        public bool CanInteract()
        {
            for (int i = 0; i < m_InteractableTargets.Length; ++i) {
                if (m_InteractableTargets[i] == null || !m_InteractableTargets[i].CanInteract()) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Performs the interaction.
        /// </summary>
        public void Interact()
        {
            for (int i = 0; i < m_InteractableTargets.Length; ++i) {
                m_InteractableTargets[i].Interact();
            }
        }

        /// <summary>
        /// Returns the message that should be displayed when the object can be interacted with.
        /// </summary>
        /// <returns>The message that should be displayed when the object can be interacted with.</returns>
        public string AbilityMessage()
        {
            if (m_InteractableTargets != null) {
                for (int i = 0; i < m_InteractableTargets.Length; ++i) {
                    // Returns the message from the first IInteractableMessage object.
                    if (m_InteractableTargets[i] is IInteractableMessage) {
                        return (m_InteractableTargets[i] as IInteractableMessage).AbilityMessage();
                    }
                }
            }
            return string.Empty;
        }
    }
}