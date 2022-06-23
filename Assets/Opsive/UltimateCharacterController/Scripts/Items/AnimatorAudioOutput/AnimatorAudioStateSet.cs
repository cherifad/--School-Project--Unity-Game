/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Audio;
using Opsive.UltimateCharacterController.Utility;
using Opsive.UltimateCharacterController.StateSystem;

namespace Opsive.UltimateCharacterController.Items.AnimatorAudioStates
{
    /// <summary>
    /// The AnimatorAudioStateSet represets a set of animation parameters and audio clips that should be played together.
    /// </summary>
    [System.Serializable]
    public class AnimatorAudioStateSet
    {
        /// <summary>
        /// Contains a single animator and audio clip state.
        /// </summary>
        [System.Serializable]
        public class AnimatorAudioState : StateObject
        {
            [Tooltip("Is the AnimatorAudioState enabled?")]
            [SerializeField] protected bool m_Enabled = true;
            [Tooltip("Can the state be selected when the character is moving?")]
            [SerializeField] protected bool m_AllowDuringMovement = true;
            [Tooltip("Does the state require the character to be grounded?")]
            [SerializeField] protected bool m_RequireGrounded;
            [Tooltip("The value of the animator's Item Substate Index parameter.")]
            [SerializeField] protected int m_ItemSubstateIndex;
            [Tooltip("Contains an array of AudioClips.")]
            [SerializeField] protected AudioClipSet m_AudioClipSet = new AudioClipSet();

            public bool Enabled { get { return m_Enabled; } set { m_Enabled = value; } }
            public bool AllowDuringMovement { get { return m_AllowDuringMovement; } set { m_AllowDuringMovement = value; } }
            public bool RequireGrounded { get { return m_RequireGrounded; } set { m_RequireGrounded = value; } }
            public int ItemSubstateIndex { get { return m_ItemSubstateIndex; } set { m_ItemSubstateIndex = value; } }
            public AudioClipSet AudioClipSet { get { return m_AudioClipSet; } set { m_AudioClipSet = value; } }

            /// <summary>
            /// Default constructor.
            /// </summary>
            public AnimatorAudioState() { }

            /// <summary>
            /// Constructor with one parameter.
            /// </summary>
            /// <param name="itemSubstateIndex">The value of the animator's Item Substate Index parameter.</param>
            public AnimatorAudioState(int itemSubstateIndex)
            {
                m_ItemSubstateIndex = itemSubstateIndex;
            }

            /// <summary>
            /// Plays the audio clip with a random set index.
            /// </summary>
            /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
            /// <param name="reservedIndex">The index of the component that should be played. -1 indicates any component.</param>
            public void PlayAudioClip(GameObject gameObject, int reservedIndex)
            {
                m_AudioClipSet.PlayAudioClip(gameObject, reservedIndex);
            }
        }

        [Tooltip("The serialization data for the AnimatorAudioStateState.")]
        [SerializeField] protected Serialization m_AnimatorAudioStateSelectorData;
        [Tooltip("An array of possible states for the animator parameter and audio clip.")]
        [SerializeField] protected AnimatorAudioState[] m_States;

        public Serialization AnimatorAudioStateSelectorData { get { return m_AnimatorAudioStateSelectorData; }
            set
            {
                m_AnimatorAudioStateSelectorData = value;
                if (!Application.isPlaying) {
                    DeserializeAnimatorAudioStateSelector(null, null);
                }
            }
        }
        public AnimatorAudioState[] States { get { return m_States; } set { m_States = value; } }

        private AnimatorAudioStateSelector m_AnimatorAudioStateSelector;

        public AnimatorAudioStateSelector AnimatorAudioStateSelector
        {
            get
            {
                if (!Application.isPlaying && m_AnimatorAudioStateSelector == null) { DeserializeAnimatorAudioStateSelector(null, null); }
                return m_AnimatorAudioStateSelector;
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AnimatorAudioStateSet() { }

        /// <summary>
        /// Constructor with one parameter.
        /// </summary>
        /// <param name="itemSubstateIndex">The value of the animator's Item Substate Index parameter.</param>
        public AnimatorAudioStateSet(int itemSubstateParameter)
        {
            m_States = new AnimatorAudioState[] { new AnimatorAudioState(itemSubstateParameter) };
        }

        /// <summary>
        /// Deserializes the AnimatorAudioStateSelector data.
        /// </summary>
        /// <param name="item">A reference to the item that the state belongs to.</param>
        /// <param name="characterLocomotion">A reference to the character that the state belongs to.</param>
        public void DeserializeAnimatorAudioStateSelector(Item item, Character.UltimateCharacterLocomotion characterLocomotion)
        {
            if (m_AnimatorAudioStateSelectorData != null) {
                m_AnimatorAudioStateSelector = m_AnimatorAudioStateSelectorData.DeserializeFields(MemberVisibility.Public) as AnimatorAudioStateSelector;
                if (m_AnimatorAudioStateSelector != null && Application.isPlaying) {
                    m_AnimatorAudioStateSelector.Initialize(item.gameObject, characterLocomotion, item, m_States);
                }
            }
        }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        /// <param name="gameObject">The GameObject that the AnimatorAudioStateSet is attached to.</param>
        public void Awake(GameObject gameObject)
        {
            for (int i = 0; i < m_States.Length; ++i) {
                m_States[i].Initialize(gameObject);
            }
        }

        /// <summary>
        /// Returns the current state index of the selector. -1 indicates this index is not set by the class.
        /// </summary>
        /// <returns>The current state index.</returns>
        public int GetStateIndex()
        {
            if (m_AnimatorAudioStateSelector == null || m_States.Length == 0) {
                return -1;
            }
            return m_AnimatorAudioStateSelector.GetStateIndex();
        }

        /// <summary>
        /// Returns the Item Substate Index parameter value. -1 indicates this value is not set by the class.
        /// </summary>
        /// <returns>The current Item Substate Index parameter value.</returns>
        public int GetItemSubstateIndex()
        {
            if (m_AnimatorAudioStateSelector == null || m_States.Length == 0) {
                return -1;
            }
            var stateIndex = m_AnimatorAudioStateSelector.GetStateIndex();
            if (stateIndex == -1 || stateIndex >= m_States.Length) {
                return -1;
            }
            return m_States[stateIndex].ItemSubstateIndex + m_AnimatorAudioStateSelector.GetAdditionalItemSubstateIndex();
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        public void PlayAudioClip(GameObject gameObject)
        {
            PlayAudioClip(gameObject, -1);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="reservedIndex">The index of the component that should be played. -1 indicates any component.</param>
        public void PlayAudioClip(GameObject gameObject, int reservedIndex)
        {
            if (m_AnimatorAudioStateSelector == null || m_States.Length == 0) {
                return;
            }
            var stateIndex = m_AnimatorAudioStateSelector.GetStateIndex();
            if (stateIndex == -1 || stateIndex >= m_States.Length) {
                return;
            }
            m_States[stateIndex].PlayAudioClip(gameObject, reservedIndex);
        }

        /// <summary>
        /// Moves to the next state of the selector.
        /// </summary>
        public void NextState()
        {
            if (m_AnimatorAudioStateSelector == null) {
                return;
            }

            m_AnimatorAudioStateSelector.NextState();
        }
    }
}