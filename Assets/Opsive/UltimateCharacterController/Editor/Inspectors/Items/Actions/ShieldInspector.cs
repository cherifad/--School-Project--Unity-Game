/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEditor;
using UnityEngine;
using Opsive.UltimateCharacterController.Items.Actions;
using System;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions
{
    /// <summary>
    /// Shows a custom inspector for the Shield component.
    /// </summary>
    [CustomEditor(typeof(Shield))]
    public class ShieldInspector : ItemActionInspector
    {
        /// <summary>
        /// Returns the actions to draw before the State list is drawn.
        /// </summary>
        /// <returns>The actions to draw before the State list is drawn.</returns>
        protected override Action GetDrawCallback()
        {
            var baseCallback = base.GetDrawCallback();

            baseCallback += () =>
            {
                EditorGUILayout.PropertyField(PropertyFromName("m_AbsorptionFactor"));
                EditorGUILayout.PropertyField(PropertyFromName("m_AbsorbExplosions"));
                var invincibleProperty = PropertyFromName("m_Invincible");
                EditorGUILayout.PropertyField(invincibleProperty);
                GUI.enabled = !invincibleProperty.boolValue;
                EditorGUILayout.PropertyField(PropertyFromName("m_Durability"));
                GUI.enabled = true;
            };

            return baseCallback;
        }
    }
}