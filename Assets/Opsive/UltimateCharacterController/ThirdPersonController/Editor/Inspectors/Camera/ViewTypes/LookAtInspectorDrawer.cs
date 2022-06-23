/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes;
using Opsive.UltimateCharacterController.Editor.Inspectors;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Editor.Inspectors.Camera.ViewTypes
{
    /// <summary>
    /// Draws a custom inspector for the Look At View Type.
    /// </summary>
    [InspectorDrawer(typeof(LookAt))]
    public class LookAtInspectorDrawer : InspectorDrawer
    {
        /// <summary>
        /// Called when the object should be drawn to the inspector.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        public override void OnInspectorGUI(object target, Object parent)
        {
            InspectorUtility.DrawField(target, "m_Target");
            InspectorUtility.DrawField(target, "m_Offset");
            InspectorUtility.DrawField(target, "m_MinLookDistance");
            InspectorUtility.DrawField(target, "m_MaxLookDistance");
            InspectorUtility.DrawField(target, "m_MoveSpeed");
            InspectorUtility.DrawField(target, "m_RotationalLerpSpeed");
            InspectorUtility.DrawField(target, "m_CollisionRadius");
            InspectorUtility.DrawSpring(target, "Rotation Spring", "m_RotationSpring");
        }
    }
}