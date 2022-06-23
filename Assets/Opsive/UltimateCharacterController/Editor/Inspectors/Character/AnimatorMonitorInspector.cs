/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEditor;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors
{
    /// <summary>
    /// Shows a custom inspector for the AnimatorMonitor component.
    /// </summary>
    [CustomEditor(typeof(AnimatorMonitor))]
    public class AnimatorMonitorInspector : InspectorBase
    {
        /// <summary>
        /// Draws the inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            if (Foldout("Time")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName("m_HorizontalMovementDampingTime"));
                EditorGUILayout.PropertyField(PropertyFromName("m_ForwardMovementDampingTime"));
                EditorGUILayout.PropertyField(PropertyFromName("m_PitchDampingTime"));
                EditorGUILayout.PropertyField(PropertyFromName("m_YawDampingTime"));
                EditorGUI.indentLevel--;
            }
            if (Foldout("Debug")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName("m_DebugAnimatorController"));
                if (PropertyFromName("m_DebugAnimatorController").boolValue) {
                    EditorGUILayout.HelpBox("Debug Animator Controller is enabled. Animations will not be precisely aligned with the character and there will be jitter. " +
                                            "This field is useful for watching the Animator Controller update in the editor.", MessageType.Warning);
                }
                EditorGUILayout.PropertyField(PropertyFromName("m_LogAbilityParameterChanges"));
                EditorGUILayout.PropertyField(PropertyFromName("m_LogItemParameterChanges"));
                EditorGUILayout.PropertyField(PropertyFromName("m_LogEvents"));
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(target, "Value Change");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetDirty(target);
            }
        }
    }
}