/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Objects.CharacterAssist
{
    /// <summary>
    /// Specifies the location that the character should move to when an ability is started.
    /// </summary>
    public class AbilityStartLocation : MonoBehaviour
    {
        [Tooltip("The offset relative to the transform that the character should move towards.")]
        [SerializeField] protected Vector3 m_Offset = new Vector3(0, 0, 1);
        [Tooltip("The yaw offset relative to the transform that the character should rotate towards.")]
        [SerializeField] protected float m_YawOffset = 180;
        [Tooltip("The size of the area that the character can start the ability at. A zero value indicates that the character must land on the exact offset.")]
        [SerializeField] protected Vector3 m_Size;
        [Tooltip("The ability can start when the distance between the start location and character is less than the specified value.")]
        [Range(0.0001f, 100)] [SerializeField] protected float m_Distance = 0.01f;
        [Tooltip("The ability can start when the angle threshold between the start location and character is less than the specified value.")]
        [Range(0, 360)] [SerializeField] protected float m_Angle = 0.5f;
        [Tooltip("Is the character required to be on the ground?")]
        [SerializeField] protected bool m_RequireGrounded = true;
        [Tooltip("Should the ability wait to start until all transitions are complete?")]
        [SerializeField] private bool m_PrecisionStart = true;

        public Vector3 Offset { get { return m_Offset; } set { m_Offset = value; } }
        public float YawOffset { get { return m_YawOffset; } set { m_YawOffset = value; } }
        public Vector3 Size { get { return m_Size; } set { m_Size = value; } }
        public float Distance { get { return m_Distance; } set { m_Distance = value; } }
        public float Angle { get { return m_Angle; } set { m_Angle = value; } }
        public bool RequireGrounded { get { return m_RequireGrounded; } set { m_RequireGrounded = value; } }
        public bool PrecisionStart { get { return m_PrecisionStart; } set { m_PrecisionStart = value; } }

        private Transform m_Transform;

        public Vector3 TargetPosition { get { return m_Transform.TransformPoint(m_Offset); } }
        public Quaternion TargetRotation { get { return m_Transform.rotation * Quaternion.Euler(0, m_YawOffset, 0); } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
        }

        /// <summary>
        /// Is the character in a valid position?
        /// </summary>
        /// <param name="position">The position of the character.</param>
        /// <param name="grounded">Is the character grounded?</param>
        /// <returns>True if the position is valid.</returns>
        public bool IsPositionValid(Vector3 position, bool grounded)
        {
            var localPosition = m_Transform.InverseTransformPoint(position);
            var delta = localPosition - m_Offset + m_Size * 0.5f;
            var maxValue = Vector3.Max(Vector3.zero, delta);
            var minValue = Vector3.Min(m_Size, delta);
            if (Mathf.Abs(maxValue.x - minValue.x) <= m_Distance && 
                ((m_RequireGrounded && grounded) || (!m_RequireGrounded && (Mathf.Abs(maxValue.y - minValue.y) <= m_Distance))) &&
                Mathf.Abs(maxValue.z - minValue.z) <= m_Distance) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Is the character in a valid rotation?
        /// </summary>
        /// <param name="rotation">The rotation of the character.</param>
        /// <returns>True if the rotation is valid.</returns>
        public bool IsRotationValid(Quaternion rotation)
        {
            return Vector3.Angle(Quaternion.Euler(0, m_YawOffset, 0) * m_Transform.forward, rotation * Vector3.forward) <= m_Angle / 2 + 0.001f;
        }
    }
}