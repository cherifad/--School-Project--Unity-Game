/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using System;
using System.Collections.Generic;
using Attribute = Opsive.UltimateCharacterController.Traits.Attribute;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Traits
{
    /// <summary>
    /// Shows a custom inspector for the AttributeManager.
    /// </summary>
    [CustomEditor(typeof(AttributeManager))]
    public class AttributeManagerInspector : InspectorBase
    {
        private const string c_EditorPrefsSelectedAttributeIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Traits.SelectedAttributeTypeIndex";
        private const string c_EditorPrefsSelectedAttributeStateIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Traits.SelectedAttributeStateIndex";
        private string SelectedAttributeIndexKey { get { return c_EditorPrefsSelectedAttributeIndexKey + "." + target.GetType() + "." + target.name; } }

        private AttributeManager m_AttributeManager;
        private ReorderableList m_ReorderableAttributeList;
        private ReorderableList m_ReorderableAttributeStateList;

        /// <summary>
        /// Initialize the AttributeManager.
        /// </summary>
        private void OnEnable()
        {
            m_AttributeManager = target as AttributeManager;
        }

        /// <summary>
        /// Draws the attributes list.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            ReorderableListSerializationHelper.DrawReorderableList(ref m_ReorderableAttributeList, this, m_AttributeManager.Attributes, "m_Attributes",
                                                            OnAttributeListDrawHeader, OnAttributeListDraw, null, OnAttributeListAdd, OnAttributeListRemove, OnAttributeListSelect,
                                                            DrawSelectedAttribute, SelectedAttributeIndexKey, false, false);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetDirty(target);
            }
        }

        /// <summary>
        /// Draws the header for the attribute list.
        /// </summary>
        private void OnAttributeListDrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Attribute");
        }

        /// <summary>
        /// Draws all of the added attributes.
        /// </summary>
        private void OnAttributeListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            // The index may be out of range if the component was copied.
            if (index >= m_AttributeManager.Attributes.Length) {
                m_ReorderableAttributeList.index = -1;
                EditorPrefs.SetInt(SelectedAttributeIndexKey, m_ReorderableAttributeList.index);
                return;
            }

            var label = m_AttributeManager.Attributes[index].Name + " (Value: " + m_AttributeManager.Attributes[index].Value + ")";
            EditorGUI.LabelField(rect, label); 
        }

        /// <summary>
        /// Adds a new attribute to the list.
        /// </summary>
        private void OnAttributeListAdd(ReorderableList list)
        {
            var attributes = m_AttributeManager.Attributes;
            if (attributes == null) {
                attributes = new Attribute[1];
            } else {
                Array.Resize(ref attributes, attributes.Length + 1);
            }

            var attribute = Activator.CreateInstance(typeof(Attribute)) as Attribute;
            var name = "New Attribute";
            // Use the name of the last attribute element.
            if (m_ReorderableAttributeList.index > -1 && m_ReorderableAttributeList.index < attributes.Length - 1) {
                name = attributes[m_ReorderableAttributeList.index].Name;
            }
            // The name must be unique.
            if (!IsUniqueName(attributes, name)) {
                var postfixIndex = 1;
                while (!IsUniqueName(attributes, name + " " + postfixIndex)) {
                    postfixIndex++;
                }
                name += " " + postfixIndex;
            }
            attribute.Name = name;
            attributes[attributes.Length - 1] = attribute;
            m_AttributeManager.Attributes = attributes;

            // Select the newly added attribute.
            m_ReorderableAttributeList.index = attributes.Length - 1;
            EditorPrefs.SetInt(SelectedAttributeIndexKey, m_ReorderableAttributeList.index);
            InspectorUtility.SetDirty(m_AttributeManager);
        }

        /// <summary>
        /// Is the state name unique compared to the other attributes?
        /// </summary>
        private static bool IsUniqueName(Attribute[] attributes, string name)
        {
            // A blank string is not unique.
            if (string.IsNullOrEmpty(name)) {
                return false;
            }

            // A name is not unique if it is equal to any other attribute name.
            for (int i = 0; i < attributes.Length; ++i) {
                if (attributes[i] != null && attributes[i].Name == name) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Remove the attribute at the list index.
        /// </summary>
        private void OnAttributeListRemove(ReorderableList list)
        {
            var attributes = new List<Attribute>(m_AttributeManager.Attributes);

            // Remove the element.
            Undo.RecordObject(target, "Change Value");
            attributes.RemoveAt(list.index);
            m_AttributeManager.Attributes = attributes.ToArray();


            // Update the index to point to no longer point to the now deleted view type.
            list.index = list.index - 1;
            if (list.index == -1 && attributes.Count > 0) {
                list.index = 0;
            }
            EditorPrefs.SetInt(SelectedAttributeIndexKey, list.index);
            InspectorUtility.SetDirty(m_AttributeManager);
        }

        /// <summary>
        /// A new element has been selected within the list.
        /// </summary>
        private void OnAttributeListSelect(ReorderableList list)
        {
            EditorPrefs.SetInt(SelectedAttributeIndexKey, list.index);
            // The view type's state list should start out fresh so a reference doesn't have to be cached for each view type.
            m_ReorderableAttributeStateList = null;
        }

        /// <summary>
        /// Draws the specified attribute.
        /// </summary>
        private void DrawSelectedAttribute(object attribute)
        {
            EditorGUI.BeginChangeCheck();

            // The name must be unique.
            var name = InspectorUtility.GetFieldValue<string>(attribute, "m_Name");
            var desiredName = EditorGUILayout.TextField(new GUIContent("Name", "The name of the attribute."), name);
            if (name != desiredName && IsUniqueName(m_AttributeManager.Attributes, desiredName)) {
                InspectorUtility.SetFieldValue(attribute, "m_Name", desiredName);
            }
            var minValue = InspectorUtility.GetFieldValue<float>(attribute, "m_MinValue");
            var maxValue = InspectorUtility.GetFieldValue<float>(attribute, "m_MaxValue");
            minValue = EditorGUILayout.DelayedFloatField(new GUIContent("Min Value", "The minimum attribute value."), minValue);
            if (minValue > maxValue) {
                maxValue = minValue;
            }
            maxValue = EditorGUILayout.DelayedFloatField(new GUIContent("Max Value", "The maximum attribute value."), maxValue);
            if (maxValue < minValue) {
                minValue = maxValue;
            }
            InspectorUtility.SetFieldValue(attribute, "m_MinValue", minValue);
            InspectorUtility.SetFieldValue(attribute, "m_MaxValue", maxValue);

            InspectorUtility.DrawField(attribute, "m_Value");
            var value = InspectorUtility.GetFieldValue<float>(attribute, "m_Value");
            if (maxValue < value) {
                InspectorUtility.SetFieldValue(attribute, "m_Value", maxValue);
                value = maxValue;
            } else if (minValue > value) {
                InspectorUtility.SetFieldValue(attribute, "m_Value", minValue);
                value = minValue;
            }
            if (value > maxValue) {
                InspectorUtility.SetFieldValue(attribute, "m_MaxValue", value);
            } else if (value < minValue) {
                InspectorUtility.SetFieldValue(attribute, "m_MinValue", value);
            }
            InspectorUtility.DrawField(attribute, "m_AutoUpdateValueType");
            var autoUpdateValueType = InspectorUtility.GetFieldValue<Attribute.AutoUpdateValue>(attribute, "m_AutoUpdateValueType");
            if (autoUpdateValueType != Attribute.AutoUpdateValue.None) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawField(attribute, "m_AutoUpdateStartDelay");
                InspectorUtility.DrawField(attribute, "m_AutoUpdateInterval");
                InspectorUtility.DrawField(attribute, "m_AutoUpdateAmount");
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(target, "Change Value");
                InspectorUtility.SetDirty(m_AttributeManager);
            }

            if (InspectorUtility.Foldout(attribute, new GUIContent("States"), false)) {
                // The Attribute class derives from system.object at the base level and reorderable lists can only operate on Unity objects. To get around this restriction
                // create a dummy array within a Unity object that corresponds to the number of elements within the view type's state list. When the reorderable list is drawn
                // the view type object will be used so it's like the dummy object never existed.
                var selectedViewType = attribute as Attribute;
                var gameObject = new GameObject();
                var stateIndexHelper = gameObject.AddComponent<StateInspectorHelper>();
                stateIndexHelper.StateIndexData = new int[selectedViewType.States.Length];
                for (int i = 0; i < stateIndexHelper.StateIndexData.Length; ++i) {
                    stateIndexHelper.StateIndexData[i] = i;
                }
                var stateIndexSerializedObject = new SerializedObject(stateIndexHelper);
                m_ReorderableAttributeStateList = StateInspector.DrawStates(m_ReorderableAttributeStateList, serializedObject, stateIndexSerializedObject.FindProperty("m_StateIndexData"),
                                                            GetSelectedAttributeStateIndexKey(selectedViewType), OnAttributeStateListDraw, OnAttributeStateListAdd, OnAttributeStateListReorder,
                                                            OnAttributeStateListRemove);
                DestroyImmediate(gameObject);
            }
        }

        /// <summary>
        /// Returns the state index key for the specified attribute.
        /// </summary>
        private string GetSelectedAttributeStateIndexKey(Attribute attribute)
        {
            return c_EditorPrefsSelectedAttributeStateIndexKey + "." + target.GetType() + "." + target.name + "." + attribute.Name;
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnAttributeStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            var attribute = m_AttributeManager.Attributes[EditorPrefs.GetInt(SelectedAttributeIndexKey)];

            // The index may be out of range if the component was copied.
            if (index >= m_AttributeManager.Attributes[EditorPrefs.GetInt(SelectedAttributeIndexKey)].States.Length) {
                m_ReorderableAttributeStateList.index = -1;
                EditorPrefs.SetInt(GetSelectedAttributeStateIndexKey(attribute), m_ReorderableAttributeStateList.index);
                return;
            }

            StateInspector.OnStateListDraw(attribute, attribute.States, rect, index);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(target, "Change Value");
                InspectorUtility.SetDirty(m_AttributeManager);

                StateInspector.UpdateDefaultStateValues(attribute.States);
            }
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnAttributeStateListAdd(ReorderableList list)
        {
            StateInspector.OnStateListAdd(AddExistingViewTypePreset, CreateViewTypePreset);
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingViewTypePreset()
        {
            var attribute = m_AttributeManager.Attributes[EditorPrefs.GetInt(SelectedAttributeIndexKey)];
            var states = StateInspector.AddExistingPreset(attribute.GetType(), attribute.States, m_ReorderableAttributeStateList, GetSelectedAttributeStateIndexKey(attribute));
            if (attribute.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableAttributeStateList.serializedProperty);
                Undo.RecordObject(target, "Change Value");
                attribute.States = states;
                InspectorUtility.SetDirty(m_AttributeManager);
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreateViewTypePreset()
        {
            var attribute = m_AttributeManager.Attributes[EditorPrefs.GetInt(SelectedAttributeIndexKey)];
            var states = StateInspector.CreatePreset(attribute, attribute.States, m_ReorderableAttributeStateList, GetSelectedAttributeStateIndexKey(attribute));
            if (attribute.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableAttributeStateList.serializedProperty);
                Undo.RecordObject(target, "Change Value");
                attribute.States = states;
                InspectorUtility.SetDirty(m_AttributeManager);
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnAttributeStateListReorder(ReorderableList list)
        {
            var attribute = m_AttributeManager.Attributes[EditorPrefs.GetInt(SelectedAttributeIndexKey)];

            // Use the dummy array in order to determine what element the selected index was swapped with.
            var copiedStates = new UltimateCharacterController.StateSystem.State[attribute.States.Length];
            Array.Copy(attribute.States, copiedStates, attribute.States.Length);
            for (int i = 0; i < attribute.States.Length; ++i) {
                var element = list.serializedProperty.GetArrayElementAtIndex(i);
                if (element.intValue != i) {
                    attribute.States[i] = copiedStates[element.intValue];
                    element.intValue = i;
                }
            }

            var states = StateInspector.OnStateListReorder(attribute.States);
            if (attribute.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableAttributeStateList.serializedProperty);
                Undo.RecordObject(target, "Change Value");
                attribute.States = states;
                InspectorUtility.SetDirty(m_AttributeManager);
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnAttributeStateListRemove(ReorderableList list)
        {
            var attribute = m_AttributeManager.Attributes[EditorPrefs.GetInt(SelectedAttributeIndexKey)];
            var states = StateInspector.OnStateListRemove(attribute.States, GetSelectedAttributeStateIndexKey(attribute), list);
            if (attribute.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableAttributeStateList.serializedProperty);
                Undo.RecordObject(target, "Change Value");
                attribute.States = states;
                InspectorUtility.SetDirty(m_AttributeManager);
            }
        }
    }
}