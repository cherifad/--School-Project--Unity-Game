/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character.Abilities
{
    /// <summary>
    /// Draws a custom inspector for the DetectObjectAbilityBase ability.
    /// </summary>
    [InspectorDrawer(typeof(DetectObjectAbilityBase))]
    public class DetectObjectAbilityBaseInspectorDrawer : AbilityInspectorDrawer
    {
        /// <summary>
        /// Draws the fields related to the inspector drawer.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        protected override void DrawInspectorDrawerFields(object target, Object parent)
        {
            base.DrawInspectorDrawerFields(target, parent);

            // Draw ObjectDetectionMode manually so it'll use the MaskField.
            var objectDetection = (int)InspectorUtility.GetFieldValue<DetectObjectAbilityBase.ObjectDetectionMode>(target, "m_ObjectDetection");
            var objectDetectionString = System.Enum.GetNames(typeof(DetectObjectAbilityBase.ObjectDetectionMode));
            var value = EditorGUILayout.MaskField(new GUIContent("Object Detection", InspectorUtility.GetFieldTooltip(target, "m_ObjectDetection")), objectDetection, objectDetectionString);
            if (value != objectDetection) {
                InspectorUtility.SetFieldValue(target, "m_ObjectDetection", value);
            }
            EditorGUI.indentLevel++;
            InspectorUtility.DrawField(target, "m_DetectLayers");
            InspectorUtility.DrawField(target, "m_UseLookDirection");
            InspectorUtility.DrawField(target, "m_AngleThreshold");
            InspectorUtility.DrawField(target, "m_ObjectID");

            // No other fields need to be drawn if the ability doesn't use a cast to detect objects.
            var objectDetectionEnumValue = (DetectObjectAbilityBase.ObjectDetectionMode)value;
            if (value == 0 || objectDetectionEnumValue == DetectObjectAbilityBase.ObjectDetectionMode.Trigger) {
                EditorGUI.indentLevel--;
                return;
            }

            if ((objectDetectionEnumValue & DetectObjectAbilityBase.ObjectDetectionMode.Charactercast) != 0 ||
                (objectDetectionEnumValue & DetectObjectAbilityBase.ObjectDetectionMode.Raycast) != 0 ||
                (objectDetectionEnumValue & DetectObjectAbilityBase.ObjectDetectionMode.Spherecast) != 0) {
                InspectorUtility.DrawField(target, "m_CastDistance");
                InspectorUtility.DrawField(target, "m_CastFrameInterval");
                if ((objectDetectionEnumValue & DetectObjectAbilityBase.ObjectDetectionMode.Raycast) != 0 ||
                    (objectDetectionEnumValue & DetectObjectAbilityBase.ObjectDetectionMode.Spherecast) != 0) {
                    InspectorUtility.DrawField(target, "m_CastOffset");
                    if ((objectDetectionEnumValue & DetectObjectAbilityBase.ObjectDetectionMode.Spherecast) != 0) {
                        InspectorUtility.DrawField(target, "m_SpherecastRadius");
                    }
                }
            }
            EditorGUI.indentLevel--;
        }
    }
}