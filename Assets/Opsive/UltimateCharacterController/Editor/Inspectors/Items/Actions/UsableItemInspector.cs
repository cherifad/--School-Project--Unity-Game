/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Editor.Inspectors.Items.AnimatorAudioState;
using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using System;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions
{
    /// <summary>
    /// Shows a custom inspector for the UsableItem component.
    /// </summary>
    [CustomEditor(typeof(UsableItem))]
    public abstract class UsableItemInspector : ItemActionInspector
    {
        private const string c_EditorPrefsSelectedUseAnimatorAudioStateSetIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedUseAnimatorAudioStateSetIndex";
        private const string c_EditorPrefsSelectedUseAnimatorAudioStateSetStateIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedUseAnimatorAudioStateSetStateIndex";
        private string SelectedUseAnimatorAudioStateSetIndexKey { get { return c_EditorPrefsSelectedUseAnimatorAudioStateSetIndexKey + "." + target.GetType() + "." + target.name + m_UsableItem.ID; } }

        protected Item m_Item;
        private UsableItem m_UsableItem;
        private ReorderableList m_ReorderableUseAnimatorAudioStateSetList;
        private ReorderableList m_ReorderableUseAnimatorAudioStateSetAudioList;
        private ReorderableList m_ReorderableUseAnimatorAudioStateSetStateList;

        /// <summary>
        /// Initialize any starting values.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            m_UsableItem = target as UsableItem;
            m_Item = m_UsableItem.GetComponent<Item>();
            m_UsableItem.UseAnimatorAudioStateSet.DeserializeAnimatorAudioStateSelector(m_Item, m_Item.GetComponentInParent<UltimateCharacterController.Character.UltimateCharacterLocomotion>());
            
            // After an undo or redo has been performed the animator audio state states need to be deserialized.
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        /// <summary>
        /// Perform any cleanup when the inspector has been disabled.
        /// </summary>
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        /// <summary>
        /// Returns the actions to draw before the State list is drawn.
        /// </summary>
        /// <returns>The actions to draw before the State list is drawn.</returns>
        protected override Action GetDrawCallback()
        {
            var baseCallback = base.GetDrawCallback();

            baseCallback += () =>
            {
                if (Foldout("Use")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_UseRate"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_CanEquipEmptyItem"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_FaceTarget"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_StopUseAbilityDelay"));
                    InspectorUtility.DrawAnimationEventTrigger(target, "Use Event", PropertyFromName("m_UseEvent"));
                    InspectorUtility.DrawAnimationEventTrigger(target, "Use Complete Event", PropertyFromName("m_UseCompleteEvent"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ForceRootMotionPosition"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ForceRootMotionRotation"));
                    InspectorUtility.DrawAttributeModifier(m_Item, m_Item.GetComponentInParent<AttributeManager>(), m_UsableItem.AttributeModifier);
                    if (Foldout("Animator Audio")) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(PropertyFromName("m_PlayAudioOnStartUse"));
                        AnimatorAudioStateSetInspector.DrawAnimatorAudioStateSet(m_UsableItem, m_UsableItem.UseAnimatorAudioStateSet, true, 
                                    ref m_ReorderableUseAnimatorAudioStateSetAudioList, OnAnimatorAudioStateListDraw, OnAnimatorAudioStateListSelect,
                                    OnAnimatorAudioStateListAdd, OnAnimatorAudioStateListRemove, SelectedUseAnimatorAudioStateSetIndexKey,
                                    ref m_ReorderableUseAnimatorAudioStateSetList, OnUseAudioListElementDraw, OnUseAudioListAdd, OnUseAudioListRemove,
                                    ref m_ReorderableUseAnimatorAudioStateSetStateList, OnAnimatorAudioStateSetStateListDraw, OnAnimatorAudioStateSetStateListAdd, 
                                    OnAnimatorAudioStateSetStateListReorder, OnAnimatorAudioStateSetStateListRemove, 
                                    GetSelectedAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedUseAnimatorAudioStateSetIndexKey)));
                        EditorGUI.indentLevel--;
                    }
                }
            };

            return baseCallback;
        }

        /// <summary>
        /// Draws the AudioStateSet element.
        /// </summary>
        private void OnAnimatorAudioStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateElementDraw(m_UsableItem.UseAnimatorAudioStateSet, rect, index, isActive, isFocused, target);
        }

        /// <summary>
        /// A new element has been selected.
        /// </summary>
        private void OnAnimatorAudioStateListSelect(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateSelect(list, SelectedUseAnimatorAudioStateSetIndexKey);
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnAnimatorAudioStateListAdd(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateListAdd(list, m_UsableItem.UseAnimatorAudioStateSet, SelectedUseAnimatorAudioStateSetIndexKey);
        }

        /// <summary>
        /// Removes the state at the list index.
        /// </summary>
        private void OnAnimatorAudioStateListRemove(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateListRemove(list, m_UsableItem.UseAnimatorAudioStateSet, SelectedUseAnimatorAudioStateSetIndexKey);
        }

        /// <summary>
        /// Draws all of the added audio clip elements.
        /// </summary>
        private void OnUseAudioListElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AnimatorAudioStateSetInspector.OnAudioClipDraw(rect, index, m_UsableItem.UseAnimatorAudioStateSet.States, SelectedUseAnimatorAudioStateSetIndexKey, target);
        }

        /// <summary>
        /// Adds a new audio clip element to the list.
        /// </summary>
        private void OnUseAudioListAdd(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAudioClipListAdd(list, m_UsableItem.UseAnimatorAudioStateSet.States, SelectedUseAnimatorAudioStateSetIndexKey, target);
        }

        /// <summary>
        /// Removes the audio clip at the list index.
        /// </summary>
        private void OnUseAudioListRemove(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAudioClipListRemove(list, m_UsableItem.UseAnimatorAudioStateSet.States, SelectedUseAnimatorAudioStateSetIndexKey, target);
        }

        /// <summary>
        /// Returns the state index key for the specified AnimatorAudioStateSet type.
        /// </summary>
        private string GetSelectedAnimatorAudioStateSetStateIndexKey(int index)
        {
            return c_EditorPrefsSelectedUseAnimatorAudioStateSetStateIndexKey + "." + target.GetType() + "." + target.name + "." + index;
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnAnimatorAudioStateSetStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            var animatorAudioState = m_UsableItem.UseAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedUseAnimatorAudioStateSetIndexKey)];

            // The index may be out of range if the component was copied.
            if (index >= animatorAudioState.States.Length) {
                m_ReorderableUseAnimatorAudioStateSetStateList.index = -1;
                EditorPrefs.SetInt(GetSelectedAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedUseAnimatorAudioStateSetIndexKey)), m_ReorderableUseAnimatorAudioStateSetStateList.index);
                return;
            }

            StateInspector.OnStateListDraw(animatorAudioState, animatorAudioState.States, rect, index);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(target, "Change Value");
                StateInspector.UpdateDefaultStateValues(animatorAudioState.States);
            }
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnAnimatorAudioStateSetStateListAdd(ReorderableList list)
        {
            StateInspector.OnStateListAdd(AddExistingAnimatorAudioStateSetStatePreset, CreateAnimatorAudioStateSetStatePreset);
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingAnimatorAudioStateSetStatePreset()
        {
            var animatorAudioState = m_UsableItem.UseAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedUseAnimatorAudioStateSetIndexKey)];
            var states = StateInspector.AddExistingPreset(animatorAudioState.GetType(), animatorAudioState.States, m_ReorderableUseAnimatorAudioStateSetStateList, GetSelectedAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedUseAnimatorAudioStateSetIndexKey)));
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableUseAnimatorAudioStateSetStateList.serializedProperty);
                Undo.RecordObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreateAnimatorAudioStateSetStatePreset()
        {
            var animatorAudioState = m_UsableItem.UseAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedUseAnimatorAudioStateSetIndexKey)];
            var states = StateInspector.CreatePreset(animatorAudioState, animatorAudioState.States, m_ReorderableUseAnimatorAudioStateSetStateList, GetSelectedAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedUseAnimatorAudioStateSetIndexKey)));
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableUseAnimatorAudioStateSetStateList.serializedProperty);
                Undo.RecordObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnAnimatorAudioStateSetStateListReorder(ReorderableList list)
        {
            var animatorAudioState = m_UsableItem.UseAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedUseAnimatorAudioStateSetIndexKey)];

            // Use the dummy array in order to determine what element the selected index was swapped with.
            var copiedStates = new UltimateCharacterController.StateSystem.State[animatorAudioState.States.Length];
            Array.Copy(animatorAudioState.States, copiedStates, animatorAudioState.States.Length);
            for (int i = 0; i < animatorAudioState.States.Length; ++i) {
                var element = list.serializedProperty.GetArrayElementAtIndex(i);
                if (element.intValue != i) {
                    animatorAudioState.States[i] = copiedStates[element.intValue];
                    element.intValue = i;
                }
            }

            var states = StateInspector.OnStateListReorder(animatorAudioState.States);
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableUseAnimatorAudioStateSetStateList.serializedProperty);
                Undo.RecordObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnAnimatorAudioStateSetStateListRemove(ReorderableList list)
        {
            var animatorAudioState = m_UsableItem.UseAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedUseAnimatorAudioStateSetIndexKey)];
            var states = StateInspector.OnStateListRemove(animatorAudioState.States, GetSelectedAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedUseAnimatorAudioStateSetIndexKey)), list);
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableUseAnimatorAudioStateSetStateList.serializedProperty);
                Undo.RecordObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// Deserialize the animator audio state set after an undo/redo.
        /// </summary>
        protected virtual void OnUndoRedo()
        {
            m_UsableItem.UseAnimatorAudioStateSet.DeserializeAnimatorAudioStateSelector(m_Item, m_Item.GetComponentInParent<UltimateCharacterController.Character.UltimateCharacterLocomotion>());
            Repaint();
        }
    }
}