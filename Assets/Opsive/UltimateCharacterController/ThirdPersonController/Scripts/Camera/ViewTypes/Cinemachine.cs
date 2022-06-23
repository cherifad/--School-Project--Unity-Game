/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

#if ULTIMATE_CHARACTER_CONTROLLER_CINEMACHINE
using UnityEngine;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Camera.ViewTypes;
using Opsive.UltimateCharacterController.Motion;
using Opsive.UltimateCharacterController.Utility;
using Cinemachine;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes
{
    /// <summary>
    /// Allows Cinemachine to be used by the CameraController. If a FreeLook virtual camera is used the CinemachineSpringExtension component should be added so it will respond to spring events.
    /// Version 2.1 or later of Cinemachine is required.
    /// </summary>
    public class Cinemachine : ViewType
    {
        [Tooltip("The distance that the character should look ahead.")]
        [SerializeField] protected float m_LookDirectionDistance = 100;
        [Tooltip("The field of view of the main camera.")]
        [SerializeField] protected float m_FieldOfView = 70f;
        [Tooltip("The damping time of the field of view angle when changed.")]
        [SerializeField] protected float m_FieldOfViewDamping = 0.25f;
        [Tooltip("The positional spring used for regular movement.")]
        [SerializeField] protected Spring m_PositionSpring;
        [Tooltip("The rotational spring used for regular movement.")]
        [SerializeField] protected Spring m_RotationSpring;
        [Tooltip("The positional spring which returns to equilibrium after a small amount of time (for recoil).")]
        [SerializeField] protected Spring m_SecondaryPositionSpring;
        [Tooltip("The rotational spring which returns to equilibrium after a small amount of time (for recoil).")]
        [SerializeField] protected Spring m_SecondaryRotationSpring;

        public override float LookDirectionDistance { get { return m_LookDirectionDistance; } }
        public float FieldOfView { get { return m_FieldOfView; } set { m_FieldOfView = value; } }
        public float FieldOfViewDamping { get { return m_FieldOfViewDamping; } set { m_FieldOfViewDamping = value; } }
        public Spring PositionSpring
        {
            get { return m_PositionSpring; }
            set
            {
                m_PositionSpring = value;
                if (m_PositionSpring != null) { m_PositionSpring.Initialize(false, true); }
            }
        }
        public Spring RotationSpring
        {
            get { return m_RotationSpring; }
            set
            {
                m_RotationSpring = value;
                if (m_RotationSpring != null) { m_RotationSpring.Initialize(true, true); }
            }
        }
        public Spring SecondaryPositionSpring
        {
            get { return m_SecondaryPositionSpring; }
            set
            {
                m_SecondaryPositionSpring = value;
                if (m_SecondaryPositionSpring != null) { m_SecondaryPositionSpring.Initialize(false, true); }
            }
        }
        public Spring SecondaryRotationSpring
        {
            get { return m_SecondaryRotationSpring; }
            set
            {
                m_SecondaryRotationSpring = value;
                if (m_SecondaryRotationSpring != null) { m_SecondaryRotationSpring.Initialize(true, true); }
            }
        }

        private UnityEngine.Camera m_Camera;
        private CinemachineBrain m_Brain;
        private Transform m_CrosshairsTransform;
        private AimAssist m_AimAssist;
        private Transform m_WorldUpOverride;
        private CinemachineFreeLook m_FreeLook;
        private CinemachineSpringExtension m_SpringExtension;

        private Vector3 m_CrosshairsLocalPosition;
        private Quaternion m_CrosshairsDeltaRotation;
        private float m_Pitch;
        private float m_FieldOfViewChangeTime;

        private Vector3 m_PrevPositionSpringValue;
        private Vector3 m_PrevPositionSpringVelocity;
        private Vector3 m_PrevRotationSpringValue;
        private Vector3 m_PrevRotationSpringVelocity;
        private float m_PrevFieldOfViewDamping;
        private int m_StateChangeFrame = -1;

        public override float Pitch
        {
            get
            {
                var localRotation = MathUtility.InverseTransformQuaternion(m_CharacterTransform.rotation, m_Brain.CurrentCameraState.FinalOrientation).eulerAngles;
                return localRotation.x;
            }
        }
        public override float Yaw
        {
            get
            {
                var localRotation = MathUtility.InverseTransformQuaternion(m_CharacterTransform.rotation, m_Brain.CurrentCameraState.FinalOrientation).eulerAngles;
                return localRotation.y;
            }
        }
        public override Quaternion CharacterRotation { get { return m_CharacterTransform.rotation; } }
        public override bool FirstPersonPerspective { get { return false; } }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_Brain = m_CameraController.GetComponent<CinemachineBrain>();
            if (m_Brain == null) {
                Debug.LogError("Error: A Cinemachine camera must first be setup in order to be used by the Cinemachine ViewType.");
                return;
            }
            m_Camera = m_CameraController.gameObject.GetCachedComponent<UnityEngine.Camera>();
            m_AimAssist = m_GameObject.GetCachedComponent<AimAssist>();
            m_WorldUpOverride = new GameObject("CM WorldUp").transform;
            m_Brain.m_WorldUpOverride = m_WorldUpOverride;

            // Initialize the springs.
            m_PositionSpring.Initialize(false, false);
            m_RotationSpring.Initialize(true, true);
            m_SecondaryPositionSpring.Initialize(false, false);
            m_SecondaryRotationSpring.Initialize(true, true);
        }

        /// <summary>
        /// The view type has changed.
        /// </summary>
        /// <param name="activate">Should the current view type be activated?</param>
        /// <param name="pitch">The pitch of the camera (in degrees).</param>
        /// <param name="yaw">The yaw of the camera (in degrees).</param>
        /// <param name="characterRotation">The rotation of the character.</param>
        public override void ChangeViewType(bool activate, float pitch, float yaw, Quaternion characterRotation)
        {
            m_Brain.enabled = activate;
            if (activate) {
                m_Pitch = pitch;
                if (m_Camera.fieldOfView != m_FieldOfView) {
                    m_FieldOfViewChangeTime = Time.time + m_FieldOfViewDamping / m_CharacterLocomotion.TimeScale;
                }

                // A virtual camera will not exist when Cinemachine first starts.
                if (m_Brain.ActiveVirtualCamera == null) {
                    m_Brain.m_CameraActivatedEvent.AddListener(ActivatedVirtualCamera);
                } else {
                    UpdateFreeLookCameraValues();
                }
            } else {
                m_FreeLook = null;
            }
        }

        /// <summary>
        /// Cinemachine has activated the virtual camera.
        /// </summary>
        private void ActivatedVirtualCamera(ICinemachineCamera firstCamera, ICinemachineCamera secondCamera)
        {
            m_Brain.m_CameraActivatedEvent.RemoveListener(ActivatedVirtualCamera);

            UpdateFreeLookCameraValues();
        }

        /// <summary>
        /// Updates the Cinemachine FreeLook properties to reset to the character's location.
        /// </summary>
        private void UpdateFreeLookCameraValues()
        {
            m_FreeLook = m_Brain.ActiveVirtualCamera as CinemachineFreeLook;
            if (m_FreeLook == null) {
                return;
            }

            m_SpringExtension = m_FreeLook.GetComponent<CinemachineSpringExtension>();

            if (m_FreeLook.m_BindingMode == CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp) {
                Debug.LogWarning("Warning: The FreeLook Virtual Camera is set to a Simple Follow With World Up binding mode. This will prevent the view type from correctly setting the starting axis values.");
            }
            
            // The rotation should be adjusted so the camera starts behind the player.
            var localRotation = MathUtility.InverseTransformQuaternion(m_CharacterTransform.rotation, m_Transform.rotation).eulerAngles;
            var axis = m_FreeLook.m_XAxis;
            axis.Value -= localRotation.y;
            m_FreeLook.m_XAxis = axis;

            axis = m_FreeLook.m_YAxis;
            axis.Value += (m_Pitch - localRotation.x);
            m_FreeLook.m_YAxis = axis;

            UpdateFieldOfView(true);
        }

        /// <summary>
        /// Reset the ViewType's variables.
        /// </summary>
        /// <param name="characterRotation">The rotation of the character.</param>
        public override void Reset(Quaternion characterRotation)
        {
            UpdateFreeLookCameraValues();
            if (m_SpringExtension != null) {
                m_SpringExtension.PositionCorrection = Vector3.zero;
                m_SpringExtension.OrientationCorrection = Quaternion.identity;
            }

            m_PositionSpring.Reset();
            m_RotationSpring.Reset();
            m_SecondaryPositionSpring.Reset();
            m_SecondaryRotationSpring.Reset();
        }

        /// <summary>
        /// Sets the crosshairs to the specified transform.
        /// </summary>
        /// <param name="crosshairs">The transform of the crosshairs.</param>
        public override void SetCrosshairs(Transform crosshairs)
        {
            m_CrosshairsTransform = crosshairs;

            if (m_CrosshairsTransform != null) {
                var screenPoint = RectTransformUtility.WorldToScreenPoint(null, m_CrosshairsTransform.position);
                m_CrosshairsDeltaRotation = Quaternion.LookRotation(m_Camera.ScreenPointToRay(screenPoint).direction, m_Transform.up) * Quaternion.Inverse(m_Transform.rotation);
                m_CrosshairsLocalPosition = m_CrosshairsTransform.localPosition;
            }
        }

        /// <summary>
        /// Returns the delta rotation caused by the crosshairs.
        /// </summary>
        /// <returns>The delta rotation caused by the crosshairs.</returns>
        public override Quaternion GetCrosshairsDeltaRotation()
        {
            if (m_CrosshairsTransform == null) {
                return Quaternion.identity;
            }

            // The crosshairs direction should only be updated when it changes.
            if (m_CrosshairsLocalPosition != m_CrosshairsTransform.localPosition) {
                var screenPoint = RectTransformUtility.WorldToScreenPoint(null, m_CrosshairsTransform.position);
                m_CrosshairsDeltaRotation = Quaternion.LookRotation(m_Camera.ScreenPointToRay(screenPoint).direction, m_Transform.up) * Quaternion.Inverse(m_Transform.rotation);
                m_CrosshairsLocalPosition = m_CrosshairsTransform.localPosition;
            }

            return m_CrosshairsDeltaRotation;
        }

        /// <summary>
        /// Updates the camera field of view.
        /// </summary>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        public override void UpdateFieldOfView(bool immediateUpdate)
        {
            if (m_FreeLook == null) {
                return;
            }

            var lens = m_FreeLook.m_Lens;
            if (lens.FieldOfView != m_FieldOfView) {
                var zoom = (immediateUpdate || m_FieldOfViewDamping == 0) ? 1 : ((Time.time - m_FieldOfViewChangeTime) / (m_FieldOfViewDamping / m_CharacterLocomotion.TimeScale));
                lens.FieldOfView = Mathf.SmoothStep(m_Camera.fieldOfView, m_FieldOfView, zoom);
                m_FreeLook.m_Lens = lens;
            }
        }

        /// <summary>
        /// Rotates the camera according to the horizontal and vertical movement values.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated rotation.</returns>
        public override Quaternion Rotate(float horizontalMovement, float verticalMovement, bool immediateUpdate)
        {
            // The up direction should always match.
            m_WorldUpOverride.up = m_CharacterLocomotion.Up;

            if (m_SpringExtension != null) {
                var orientationCorrection = Quaternion.identity;

                // If aim assist has a target then the camera should look in the specified direction.
                if (m_AimAssist != null) {
                    m_AimAssist.UpdateBreakForce(Mathf.Abs(horizontalMovement) + Mathf.Abs(verticalMovement));
                    if (m_AimAssist.HasTarget()) {
                        var rotation = m_Brain.CurrentCameraState.FinalOrientation;
                        var assistRotation = rotation * MathUtility.InverseTransformQuaternion(rotation, m_AimAssist.TargetRotation(rotation));
                        orientationCorrection *= assistRotation;
                    }
                }

                orientationCorrection *= Quaternion.Euler(m_RotationSpring.Value) * Quaternion.Euler(m_SecondaryRotationSpring.Value);
                m_SpringExtension.OrientationCorrection = orientationCorrection;
            }

            // Cinemachine manages the camera's orientation - don't change it.
            return m_Brain.CurrentCameraState.FinalOrientation;
        }

        /// <summary>
        /// Moves the camera according to the current pitch and yaw values.
        /// </summary>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated position.</returns>
        public override Vector3 Move(bool immediateUpdate)
        {
            if (m_SpringExtension != null) {
                m_SpringExtension.PositionCorrection = m_PositionSpring.Value + m_SecondaryPositionSpring.Value;
            }

            return m_Brain.CurrentCameraState.FinalPosition;
        }

        /// <summary>
        /// Returns the direction that the character is looking.
        /// </summary>
        /// <param name="lookPosition">The position that the character is looking from.</param>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <param name="layerMask">The LayerMask value of the objects that the look direction can hit.</param>
        /// <param name="useRecoil">Should recoil be included in the look direction?</param>
        /// <returns>The direction that the character is looking.</returns>
        public override Vector3 LookDirection(Vector3 lookPosition, bool characterLookDirection, int layerMask, bool useRecoil)
        {
            var crosshairsDeltaRotation = characterLookDirection ? Quaternion.identity : GetCrosshairsDeltaRotation();
            return (m_Brain.CurrentCameraState.FinalOrientation * crosshairsDeltaRotation) * Vector3.forward;
        }

        /// <summary>
        /// Adds a positional force to the ViewType.
        /// </summary>
        /// <param name="force">The force to add.</param>
        public override void AddPositionalForce(Vector3 force)
        {
            Debug.Log(force);
            m_PositionSpring.AddForce(force);
        }

        /// <summary>
        /// Adds a secondary force to the ViewType.
        /// </summary>
        /// <param name="force">The force to add.</param>
        public override void AddRotationalForce(Vector3 force)
        {
            m_RotationSpring.AddForce(force);
        }

        /// <summary>
        /// Adds a secondary positional force to the ViewType.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="restAccumulation">The percent of the force to accumulate to the rest value.</param>
        public override void AddSecondaryPositionalForce(Vector3 force, float restAccumulation)
        {
            if (restAccumulation > 0 && (m_AimAssist == null || !m_AimAssist.HasTarget())) {
                m_SecondaryPositionSpring.RestValue += force * restAccumulation;
            }
            m_SecondaryPositionSpring.AddForce(force);
        }

        /// <summary>
        /// Adds a delayed rotational force to the ViewType.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="restAccumulation">The percent of the force to accumulate to the rest value.</param>
        public override void AddSecondaryRotationalForce(Vector3 force, float restAccumulation)
        {
            if (restAccumulation > 0 && (m_AimAssist == null || !m_AimAssist.HasTarget())) {
                var springRest = m_SecondaryRotationSpring.RestValue;
                springRest.z += force.z * restAccumulation;
                m_SecondaryRotationSpring.RestValue = springRest;
            }
            m_SecondaryRotationSpring.AddForce(force);
        }

        /// <summary>
        /// Callback when the StateManager will change the active state on the current object.
        /// </summary>
        public override void StateWillChange()
        {
            // Remember the interal spring values so they can be restored if a new spring is applied during the state change.
            m_PrevPositionSpringValue = m_PositionSpring.Value;
            m_PrevPositionSpringVelocity = m_PositionSpring.Velocity;
            m_PrevRotationSpringValue = m_RotationSpring.Value;
            m_PrevRotationSpringVelocity = m_RotationSpring.Velocity;
            // Multiple state changes can occur within the same frame. Only remember the first damping value.
            if (m_StateChangeFrame != Time.frameCount) {
                m_PrevFieldOfViewDamping = m_FieldOfViewDamping;
            }
        }

        /// <summary>
        /// Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        public override void StateChange()
        {
            if (m_Camera.fieldOfView != m_FieldOfView) {
                m_FieldOfViewChangeTime = Time.time;
                if (m_CameraController.ActiveViewType == this) {
                    // The field of view and location should get a head start if the damping was previously 0. This will allow the field of view and location
                    // to move back to the original value when the state is no longer active.
                    if (m_PrevFieldOfViewDamping == 0) {
                        var lens = m_FreeLook.m_Lens;
                        lens.FieldOfView = (m_Camera.fieldOfView + m_FieldOfView) * 0.5f;
                        m_FreeLook.m_Lens = lens;
                    }
                }
            }

            m_PositionSpring.Value = m_PrevPositionSpringValue;
            m_PositionSpring.Velocity = m_PrevPositionSpringVelocity;
            m_RotationSpring.Value = m_PrevRotationSpringValue;
            m_RotationSpring.Velocity = m_PrevRotationSpringVelocity;
        }
    }
}
#endif