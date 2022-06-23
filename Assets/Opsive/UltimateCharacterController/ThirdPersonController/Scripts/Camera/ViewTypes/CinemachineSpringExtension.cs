/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

#if ULTIMATE_CHARACTER_CONTROLLER_CINEMACHINE
using UnityEngine;
using Cinemachine;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes
{
    /// <summary>
    /// Cinemachine extension allowing the state to adjust to the Cinemachine ViewType spring values.
    /// </summary>
    public class CinemachineSpringExtension : CinemachineExtension
    {
        private Vector3 m_PositionCorrection;
        private Quaternion m_OrientationCorrection = Quaternion.identity;

        public Vector3 PositionCorrection { set { m_PositionCorrection = value; } }
        public Quaternion OrientationCorrection { set { m_OrientationCorrection = value; } }

        /// <summary>
        /// Called after the virtual camera has implemented each stage in the pipeline. 
        /// </summary>
        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            state.PositionCorrection = m_PositionCorrection;
            state.OrientationCorrection = m_OrientationCorrection;
        }
    }
}
#endif