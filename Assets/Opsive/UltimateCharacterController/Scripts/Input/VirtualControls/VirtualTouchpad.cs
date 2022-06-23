/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;

namespace Opsive.UltimateCharacterController.Input.VirtualControls
{
    /// <summary>
    /// A virtual touchpad that will move the axis based on the position of the press relative to the starting press position.
    /// </summary>
    public class VirtualTouchpad : VirtualAxis, IDragHandler
    {
        private RectTransform m_RectTransform;
        private Vector2 m_LocalStartPosition;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_RectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// Callback when a pointer has pressed on the button.
        /// </summary>
        /// <param name="data">The pointer data.</param>
        public override void OnPointerDown(PointerEventData data)
        {
            base.OnPointerDown(data);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectTransform, data.pressPosition, null, out m_LocalStartPosition);
        }

        /// <summary>
        /// Callback when a pointer has dragged the button.
        /// </summary>
        /// <param name="data">The pointer data.</param>
        public void OnDrag(PointerEventData data)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(m_RectTransform, data.position, null)) {
                m_DeltaPosition += data.delta;
            }
        }

        /// <summary>
        /// Returns the value of the axis.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the axis.</returns>
        public override float GetAxis(string name)
        {
            if (!m_Pressed) {
                return 0;
            }

            if (name == m_HorizontalInputName) {
                return m_DeltaPosition.x / (m_RectTransform.sizeDelta.x - m_LocalStartPosition.x);
            }
            return m_DeltaPosition.y / (m_RectTransform.sizeDelta.y - m_LocalStartPosition.y);
        }
    }
}