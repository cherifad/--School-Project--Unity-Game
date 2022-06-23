/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    /// An abstract class for any ability that needs another object to start (such as picking an object up, vaulting, climbing, interacting, etc).
    /// </summary>
    public abstract class DetectObjectAbilityBase : Ability
    {
        /// <summary>
        /// Specifies how to detect the object.
        /// </summary>
        public enum ObjectDetectionMode
        {
            Trigger = 1,        // Use a trigger to detect if the character is near an object.
            Charactercast = 2,  // Use the character colliders to do a cast in order to detect if the character is near an object.
            Raycast = 4,        // Use a raycast to detect if the character is near an object.
            Spherecast = 8      // Use a spherecast to detect if the character is near an object.
        }

        [Tooltip("Mask which specifies how the ability should detect other objects.")]
        [HideInInspector] [SerializeField] protected ObjectDetectionMode m_ObjectDetection = ObjectDetectionMode.Charactercast;
        [Tooltip("The LayerMask of the object or trigger that should be detected.")]
        [HideInInspector] [SerializeField] protected LayerMask m_DetectLayers = ~(1 << LayerManager.UI | 1 << LayerManager.SubCharacter | 1 << LayerManager.Overlay | 1 << LayerManager.VisualEffect);
        [Tooltip("Should the detection method use the look source direction? If false the character direction will be used.")]
        [HideInInspector] [SerializeField] protected bool m_UseLookDirection = true;
        [Tooltip("The maximum angle that the character can be relative to the forward direction of the object.")]
        [Range(0, 360)] [HideInInspector] [SerializeField] protected float m_AngleThreshold = 360;
        [Tooltip("The unique ID value of the Object Identifier component. A value of -1 indicates that this ID should not be used.")]
        [HideInInspector] [SerializeField] protected int m_ObjectID = -1;
        [Tooltip("The distance of the cast. Used if the Object Detection Mode uses anything other then a trigger detection mode.")]
        [HideInInspector] [SerializeField] protected float m_CastDistance = 1;
        [Tooltip("The number of frames that should elapse before another cast is performed. A value of 0 will allow the cast to occur every frame.")]
        [HideInInspector] [SerializeField] protected int m_CastFrameInterval = 0;
        [Tooltip("The offset to applied to the raycast or spherecast.")]
        [HideInInspector] [SerializeField] protected Vector3 m_CastOffset = new Vector3(0, 1, 0);
        [Tooltip("The radius of the spherecast.")]
        [HideInInspector] [SerializeField] protected float m_SpherecastRadius = 0.5f;

        public ObjectDetectionMode ObjectDetection { get { return m_ObjectDetection; } set { m_ObjectDetection = value; } }
        public LayerMask DetectLayers { get { return m_DetectLayers; } set { m_DetectLayers = value; } }
        public float DetectAngleThreshold { get { return m_AngleThreshold; } set { m_AngleThreshold = value; } }
        public int ObjectID { get { return m_ObjectID; } set { m_ObjectID = value; } }
        public bool CastInLookDirection { get { return m_UseLookDirection; } set { m_UseLookDirection = value; } }
        public float CastDistance { get { return m_CastDistance; } set { m_CastDistance = value; } }
        public Vector3 CastOffset { get { return m_CastOffset; } set { m_CastOffset = value; } }
        public float SpherecastRadius { get { return m_SpherecastRadius; } set { m_SpherecastRadius = value; } }

        private ILookSource m_LookSource;
        private RaycastHit m_RaycastResult;
        protected GameObject m_DetectedTriggerObject;
        protected GameObject m_DetectedObject;
        private int m_LastCastFrame;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            EventHandler.RegisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            m_LastCastFrame = -m_CastFrameInterval;
        }

        /// <summary>
        /// A new ILookSource object has been attached to the character.
        /// </summary>
        /// <param name="lookSource">The ILookSource object attached to the character.</param>
        private void OnAttachLookSource(ILookSource lookSource)
        {
            m_LookSource = lookSource;
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) {
                return false;
            }

            // The ability can start if using a trigger.
            if ((m_ObjectDetection & ObjectDetectionMode.Trigger) != 0 && m_DetectedTriggerObject != null) {
                if (ValidateObject(m_DetectedTriggerObject, false)) {
                    m_DetectedObject = m_DetectedTriggerObject;
                    return true;
                }
            }

            // No more work is necessary if no casts are necessary.
            if (m_ObjectDetection == 0 || m_ObjectDetection == ObjectDetectionMode.Trigger || (m_UseLookDirection && m_LookSource == null)) {
                return false;
            }

            // Don't perform the cast if the number of casts are being culled.
            if (m_LastCastFrame + m_CastFrameInterval > Time.frameCount) {
                return m_DetectedObject != null;
            }
            m_LastCastFrame = Time.frameCount;

            // Use the colliders on the character to detect if the character is near the object.
            var castDirection = m_UseLookDirection ? m_LookSource.LookDirection(true) : m_Transform.forward;
            if ((m_ObjectDetection & ObjectDetectionMode.Charactercast) != 0) {
                if (m_CharacterLocomotion.SingleCast(castDirection * m_CastDistance, m_DetectLayers, ref m_RaycastResult)) {
                    var hitObject = m_RaycastResult.collider.gameObject;
                    if (ValidateObject(hitObject, false)) {
                        m_DetectedObject = hitObject;
                        return true;
                    }
                }
            }

            // Use a raycast to detect if the character is near the object.
            if ((m_ObjectDetection & ObjectDetectionMode.Raycast) != 0) {
                if (Physics.Raycast(m_Transform.TransformPoint(m_CastOffset), castDirection, out m_RaycastResult, m_CastDistance, m_DetectLayers, QueryTriggerInteraction.Ignore)) {
                    var hitObject = m_RaycastResult.collider.gameObject;
                    if (ValidateObject(hitObject, false)) {
                        m_DetectedObject = hitObject;
                        return true;
                    }
                }
            }

            // Use a spherecast to detect if the character is near the object.
            if ((m_ObjectDetection & ObjectDetectionMode.Spherecast) != 0) {
                if (Physics.SphereCast(m_Transform.TransformPoint(m_CastOffset) - m_Transform.forward * m_SpherecastRadius, m_SpherecastRadius, castDirection, out m_RaycastResult,
                                        m_CastDistance, m_DetectLayers, QueryTriggerInteraction.Ignore)) {
                    var hitObject = m_RaycastResult.collider.gameObject;
                    if (ValidateObject(hitObject, false)) {
                        m_DetectedObject = hitObject;
                        return true;
                    }
                }
            }

            // The character did not detect an object.
            m_DetectedObject = null;
            return false;
        }

        /// <summary>
        /// The character has entered a trigger.
        /// </summary>
        /// <param name="other">The trigger collider that the character entered.</param>
        public override void OnTriggerEnter(Collider other)
        {
            // The object may not be detected with a trigger.
            if ((m_ObjectDetection & ObjectDetectionMode.Trigger) == 0) {
                return;
            }

            // The object has to use the correct mask.
            if (!MathUtility.InLayerMask(other.gameObject.layer, m_DetectLayers)) {
                return;
            }

            // Once the trigger object has been detected it can't be reset until the ability stops or the character leaves the trigger.
            if (m_DetectedTriggerObject != null) {
                return;
            }

            if (ValidateObject(other.gameObject, true)) {
                m_DetectedTriggerObject = other.gameObject;
            }
        }

        /// <summary>
        /// The character has exited a trigger.
        /// </summary>
        /// <param name="other">The trigger collider that the character exited.</param>
        public override void OnTriggerExit(Collider other)
        {
            // The object may not be detected with a trigger.
            if ((m_ObjectDetection & ObjectDetectionMode.Trigger) == 0) {
                return;
            }

            TriggerExit(other);
        }

        /// <summary>
        /// The character has exited a trigger.
        /// </summary>
        /// <param name="other">The trigger collider that the character exited.</param>
        /// <returns>Returns true if the entered object leaves the trigger.</returns>
        protected virtual bool TriggerExit(Collider other)
        {
            if (other.gameObject == m_DetectedTriggerObject) {
                m_DetectedTriggerObject = null;
                m_DetectedObject = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Validates the object to ensure it is valid for the current ability.
        /// </summary>
        /// <param name="obj">The object being validated.</param>
        /// <param name="fromTrigger">Is the object being validated within a trigger?</param>
        /// <returns>True if the object is valid. The object may not be valid if it doesn't have an ability-specific component attached.</returns>
        protected virtual bool ValidateObject(GameObject obj, bool withinTrigger)
        {
            // If an object id is specified then the object must have the Object Identifier component attached with the specified ID.
            if (m_ObjectID != -1) {
                var objectIdentifier = obj.GetCachedParentComponent<Objects.ObjectIdentifier>();
                if (objectIdentifier == null || objectIdentifier.ID != m_ObjectID) {
                    return false;
                }
            }

            // The object has to be within the specified angle.
            if (!withinTrigger) {
                var castDirection = m_UseLookDirection ? m_LookSource.LookDirection(true) : m_Transform.forward;
                if (Quaternion.Angle(Quaternion.LookRotation(castDirection, m_CharacterLocomotion.Up), Quaternion.LookRotation(-obj.transform.forward, m_CharacterLocomotion.Up)) <= m_AngleThreshold) {
                    return true;
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
        }
    }
}