/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Items.AnimatorAudioStates
{
    /// <summary>
    /// The RandomRecoil state will move from one state to another in a sequence order.
    /// </summary>
    public class SequenceRecoil : RecoilAnimatorAudioStateSelector
    {
        private int m_CurrentIndex = -1;

        /// <summary>
        /// Returns the current state index. -1 indicates this index is not set by the class.
        /// </summary>
        /// <returns>The current state index.</returns>
        public override int GetStateIndex()
        {
            return m_CurrentIndex;
        }

        /// <summary>
        /// Moves to the next state.
        /// </summary>
        public override void NextState()
        {
            var count = 0;
            var size = m_States.Length;
            do {
                m_CurrentIndex = (m_CurrentIndex + 1) % (size);
                count++;
            } while ((!IsStateValid(m_CurrentIndex) || !m_States[m_CurrentIndex].Enabled) && count <= size);
        }
    }
}