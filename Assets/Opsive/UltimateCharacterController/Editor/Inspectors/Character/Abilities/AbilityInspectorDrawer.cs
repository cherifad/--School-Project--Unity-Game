/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Editor.Inspectors.Audio;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using Opsive.UltimateCharacterController.Editor.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character
{
    /// <summary>
    /// Draws a custom inspector for the base Ability type.
    /// </summary>
    [InspectorDrawer(typeof(Ability))]
    public class AbilityInspectorDrawer : InspectorDrawer
    {
        private Ability m_Ability;
        private ReorderableList m_ReorderableStartAudioClipsList;
        private ReorderableList m_ReorderableStopAudioClipsList;

        /// <summary>
        /// Called when the object should be drawn to the inspector.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        public override void OnInspectorGUI(object target, Object parent)
        {
            m_Ability = (target as Ability);

            DrawInputFieldsFields(target);

            InspectorUtility.DrawAttributeModifier(parent, (parent as Component).GetComponent<AttributeManager>(), (target as Ability).AttributeModifier);

            EditorGUILayout.BeginHorizontal();
            InspectorUtility.DrawField(target, "m_State");
            GUI.enabled = !string.IsNullOrEmpty(InspectorUtility.GetFieldValue<string>(target, "m_State"));
            // The InspectorUtility doesn't support a toggle with the text on the right.
            var field = InspectorUtility.GetField(target, "m_StateAppendItemTypeName");
            GUILayout.Space(-5);
            var value = EditorGUILayout.ToggleLeft(new GUIContent("Append Item", InspectorUtility.GetFieldTooltip(field)), (bool)field.GetValue(target), GUILayout.Width(110));
            InspectorUtility.SetFieldValue(target, "m_StateAppendItemTypeName", value);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            InspectorUtility.DrawField(target, "m_AbilityIndexParameter");

            DrawInspectorDrawerFields(target, parent);
            ObjectInspector.DrawFields(target, false);

            if (InspectorUtility.Foldout(target, "Audio")) {
                EditorGUI.indentLevel++;
                if (InspectorUtility.Foldout(target, "Start")) {
                    EditorGUI.indentLevel++;
                    AudioClipSetInspector.DrawAudioClipSet(m_Ability.StartAudioClipSet, ref m_ReorderableStartAudioClipsList, OnStartAudioClipDraw, OnStartAudioClipListAdd, OnStartAudioClipListRemove);
                    EditorGUI.indentLevel--;
                }
                DrawAudioFields();
                if (InspectorUtility.Foldout(target, "Stop")) {
                    EditorGUI.indentLevel++;
                    AudioClipSetInspector.DrawAudioClipSet(m_Ability.StopAudioClipSet, ref m_ReorderableStopAudioClipsList, OnStopAudioClipDraw, OnStopAudioClipListAdd, OnStopAudioClipListRemove);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            if (InspectorUtility.Foldout(target, "General")) {
                EditorGUI.indentLevel++;
                var itemAbilityMoveTowards = (target is MoveTowards) || target is UltimateCharacterController.Character.Abilities.Items.ItemAbility;
                if (itemAbilityMoveTowards) {
                    GUI.enabled = false;
                }
                GUI.enabled = true;
                InspectorUtility.DrawField(target, "m_AllowPositionalInput");
                InspectorUtility.DrawField(target, "m_AllowRotationalInput");
                InspectorUtility.DrawField(target, "m_UseGravity");
                InspectorUtility.DrawField(target, "m_UseRootMotionPosition");
                InspectorUtility.DrawField(target, "m_UseRootMotionRotation");
                InspectorUtility.DrawField(target, "m_DetectHorizontalCollisions");
                InspectorUtility.DrawField(target, "m_DetectVerticalCollisions");
                InspectorUtility.DrawField(target, "m_AnimatorMotion");
                if (itemAbilityMoveTowards) {
                    GUI.enabled = false;
                }
                var inventory = (parent as Component).GetComponent<UltimateCharacterController.Inventory.InventoryBase>();
                if (inventory != null && (parent as Component).GetComponent<UltimateCharacterController.Inventory.ItemSetManager>() != null) {
                    var slotCount = inventory.SlotCount;
                    if (InspectorUtility.Foldout(target, "Allow Equipped Slots")) {
                        EditorGUI.indentLevel++;
                        var mask = InspectorUtility.GetFieldValue<int>(target, "m_AllowEquippedSlotsMask");
                        var newMask = 0;
                        for (int i = 0; i < slotCount; ++i) {
                            var enabled = (mask & (1 << i)) == (1 << i);
                            if (EditorGUILayout.Toggle("Slot " + i, enabled)) {
                                newMask |= 1 << i;
                            }
                        }
                        // If all of the slots are enabled then use -1.
                        if (newMask == (1 << slotCount) - 1 || itemAbilityMoveTowards) {
                            newMask = -1;
                        }
                        if (mask != newMask) {
                            InspectorUtility.SetFieldValue(target, "m_AllowEquippedSlotsMask", newMask);
                        }
                        InspectorUtility.DrawField(target, "m_ReequipSlots");
                        if (itemAbilityMoveTowards && InspectorUtility.GetFieldValue<bool>(target, "m_ReequipSlots")) {
                            InspectorUtility.SetFieldValue(target, "m_ReequipSlots", false);
                            GUI.changed = true;
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                GUI.enabled = true;
                EditorGUI.indentLevel--;
            }

            if (InspectorUtility.Foldout(target, "UI")) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawField(target, "m_AbilityMessageText");
                InspectorUtility.DrawField(target, "m_AbilityMessageIcon");
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Draws the Ability fields related to input.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        protected void DrawInputFieldsFields(object target)
        {
            var startTypeValue = (Ability.AbilityStartType)EditorGUILayout.EnumPopup(new GUIContent("Start Type", InspectorUtility.GetFieldTooltip(target, "m_StartType")), InspectorUtility.GetFieldValue<Ability.AbilityStartType>(target, "m_StartType"));
            InspectorUtility.SetFieldValue(target, "m_StartType", startTypeValue);
            var stopTypeValue = (Ability.AbilityStopType)EditorGUILayout.EnumPopup(new GUIContent("Stop Type", InspectorUtility.GetFieldTooltip(target, "m_StopType")), InspectorUtility.GetFieldValue<Ability.AbilityStopType>(target, "m_StopType"));
            InspectorUtility.SetFieldValue(target, "m_StopType", stopTypeValue);

            // The input name field only needs to be shown if the start/stop type is set to a value which requires the button press.
            if ((startTypeValue != Ability.AbilityStartType.Automatic && startTypeValue != Ability.AbilityStartType.Manual) ||
                (stopTypeValue != Ability.AbilityStopType.Automatic && stopTypeValue != Ability.AbilityStopType.Manual)) {

                EditorGUI.BeginChangeCheck();
                // Draw a custom array inspector for the input names.
                var inputNames = InspectorUtility.GetFieldValue<string[]>(target, "m_InputNames");
                if (inputNames == null || inputNames.Length == 0) {
                    inputNames = new string[1];
                }
                EditorGUI.indentLevel++;
                for (int i = 0; i < inputNames.Length; ++i) {
                    EditorGUILayout.BeginHorizontal();
                    var fieldName = " ";
                    if (i == 0) {
                        fieldName = "Input Name";
                    }
                    inputNames[i] = EditorGUILayout.TextField(new GUIContent(fieldName, InspectorUtility.GetFieldTooltip(target, "m_InputName")), inputNames[i]);

                    if (i == inputNames.Length - 1) {
                        if (i > 0 && GUILayout.Button(InspectorStyles.RemoveIcon, InspectorStyles.NoPaddingButtonStyle, GUILayout.Width(18))) {
                            System.Array.Resize(ref inputNames, inputNames.Length - 1);
                        }
                        if (GUILayout.Button(InspectorStyles.AddIcon, InspectorStyles.NoPaddingButtonStyle, GUILayout.Width(18))) {
                            System.Array.Resize(ref inputNames, inputNames.Length + 1);
                            inputNames[inputNames.Length - 1] = inputNames[inputNames.Length - 2];
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (EditorGUI.EndChangeCheck()) {
                    InspectorUtility.SetFieldValue(target, "m_InputNames", inputNames);
                    GUI.changed = true;
                }

                // Only show the duration and wait for release options with a LongPress start/stop type.
                if (startTypeValue == Ability.AbilityStartType.LongPress || stopTypeValue == Ability.AbilityStopType.LongPress) {
                    var duration = EditorGUILayout.FloatField(new GUIContent("Long Press Duration", InspectorUtility.GetFieldTooltip(target, "m_LongPressDuration")), InspectorUtility.GetFieldValue<float>(target, "m_LongPressDuration"));
                    InspectorUtility.SetFieldValue(target, "m_LongPressDuration", duration);

                    var waitForRelease = EditorGUILayout.Toggle(new GUIContent("Wait For Long Press Release", InspectorUtility.GetFieldTooltip(target, "m_WaitForLongPressRelease")),
                                                                        InspectorUtility.GetFieldValue<bool>(target, "m_WaitForLongPressRelease"));
                    InspectorUtility.SetFieldValue(target, "m_WaitForLongPressRelease", waitForRelease);
                }
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Draws the fields related to the inspector drawer.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        protected virtual void DrawInspectorDrawerFields(object target, Object parent) { }

        /// <summary>
        /// Draws the fields related to audio.
        /// </summary>
        protected virtual void DrawAudioFields() { }

        /// <summary>
        /// The ability has been added to the Ultimate Character Locomotion. Perform any initialization.
        /// </summary>
        /// <param name="ability">The ability that has been added.</param>
        /// <param name="parent">The parent of the added ability.</param>
        public virtual void AbilityAdded(Ability ability, Object parent) { }

        /// <summary>
        /// The ability has been removed from the Ultimate Character Locomotion. Perform any destruction.
        /// </summary>
        /// <param name="ability">The ability that has been removed.</param>
        /// <param name="parent">The parent of the removed ability.</param>
        public virtual void AbilityRemoved(Ability ability, Object parent) { }

        /// <summary>
        /// Allows abilities to draw custom controls under the "Editor" foldout of the ability inspector.
        /// </summary>
        /// <param name="ability">The ability whose editor controls are being retrieved.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        /// <returns>Any custom editor controls. Can be null.</returns>
        public virtual System.Action GetEditorCallback(Ability ability, Object parent)
        {
            return null;
        }

        /// <summary>
        /// Generates the code necessary to recreate the states/transitions that are affected by the ability.
        /// </summary>
        /// <param name="ability">The ability to generate the states/transitions for.</param>
        /// <param name="animatorController">The Animator Controller to generate the states/transitions from.</param>
        /// <param name="baseDirectory">The directory that the scripts are located.</param>
        public void GenerateAnimatorCode(Ability ability, UnityEditor.Animations.AnimatorController animatorController, string baseDirectory)
        {
            AnimatorBuilder.GenerateAnimatorCode(animatorController, "AbilityIndex", ability.AbilityIndexParameter, ability, baseDirectory);
        }

        /// <summary>
        /// Returns true if the ability can build to the animator.
        /// </summary>
        public virtual bool CanBuildAnimator { get { return false; } }

        /// <summary>
        /// Adds the abilities states/transitions to the animator. 
        /// </summary>
        /// <param name="animatorController">The editor AnimatorController to add the states to.</param>
        public virtual void BuildAnimator(UnityEditor.Animations.AnimatorController animatorController) { }

        /// <summary>
        /// Draws the AudioClip element.
        /// </summary>
        private void OnStartAudioClipDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AudioClipSetInspector.OnAudioClipDraw(rect, index, m_Ability.StartAudioClipSet, null);
        }

        /// <summary>
        /// Draws the AudioClip element.
        /// </summary>
        private void OnStopAudioClipDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AudioClipSetInspector.OnAudioClipDraw(rect, index, m_Ability.StopAudioClipSet, null);
        }

        /// <summary>
        /// Adds a new AudioClip element to the AudioClipSet.
        /// </summary>
        private void OnStartAudioClipListAdd(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListAdd(list, m_Ability.StartAudioClipSet, null);
        }

        /// <summary>
        /// Adds a new AudioClip element to the AudioClipSet.
        /// </summary>
        private void OnStopAudioClipListAdd(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListAdd(list, m_Ability.StopAudioClipSet, null);
        }

        /// <summary>
        /// Remove the AudioClip element at the list index.
        /// </summary>
        private void OnStartAudioClipListRemove(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListRemove(list, m_Ability.StartAudioClipSet, null);
            m_Ability.StartAudioClipSet.AudioClips = (AudioClip[])list.list;
        }

        /// <summary>
        /// Remove the AudioClip element at the list index.
        /// </summary>
        private void OnStopAudioClipListRemove(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListRemove(list, m_Ability.StopAudioClipSet, null);
            m_Ability.StopAudioClipSet.AudioClips = (AudioClip[])list.list;
        }
    }
}