/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Utility
{
    /// <summary>
    /// Determines if an editor is visible within the inspector window.
    /// </summary>
    public static class InspectorVisibility
    {
        private const string c_WindowTypeTitle = "InspectorWindow";
        private const string c_TrackerString = "m_Tracker";
        private const string c_ActiveEditorsString = "activeEditors";
        private const string c_ScrollPositionString = "m_ScrollPosition";
        private const BindingFlags c_BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static EditorWindow[] s_UnityEditorWindows;
        private static Dictionary<UnityEditor.Editor, EditorWindow> s_EditorEditorWindow = new Dictionary<UnityEditor.Editor, EditorWindow>();
        private static Dictionary<EditorWindow, Func<Vector2>> s_EditorWindowScrollPositionFunction = new Dictionary<EditorWindow, Func<Vector2>>();

        /// <summary>
        /// Is the editor visible within the inspector?
        /// </summary>
        /// <param name="editor">The editor that may not be visible.</param>
        /// <returns>True if the editor is visible within the inspector.</returns>
        public static bool IsVisible(UnityEditor.Editor editor)
        {
            if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout) {
                return true;
            }

            // Cache the EditorWindow that the editor object is being drawn within.
            EditorWindow editorWindow = null;
            if (!s_EditorEditorWindow.TryGetValue(editor, out editorWindow)) {
                if (s_UnityEditorWindows == null) {
                    s_UnityEditorWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
                }

                for (int i = 0; i < s_UnityEditorWindows.Length; ++i) {
                    var windowType = s_UnityEditorWindows[i].GetType();
                    if (windowType.Name == c_WindowTypeTitle) {
                        // Perform lots of error checking so no errors will be thrown if the names change in future versions of Unity.
                        var trackerField = windowType.GetField(c_TrackerString, c_BindingFlags);
                        if (trackerField == null) {
                            continue;
                        }
                        // The tracker contains all of the editor inspectors.
                        var tracker = trackerField.GetValue(s_UnityEditorWindows[i]);
                        if (tracker == null) {
                            continue;
                        }
                        var activeEditorProperty = tracker.GetType().GetProperty(c_ActiveEditorsString, c_BindingFlags);
                        if (activeEditorProperty == null) {
                            continue;
                        }
                        var activeEditors = activeEditorProperty.GetGetMethod(true).Invoke(tracker, null) as UnityEditor.Editor[];
                        if (activeEditors == null) {
                            continue;
                        }
                        // The active editors property contains a list of all of the editors within the current window. Search through the editors to determine if the
                        // current editor is being drawn here.
                        for (int j = 0; j < activeEditors.Length; ++j) {
                            if (activeEditors[j] == editor) {
                                editorWindow = s_UnityEditorWindows[i];
                                break;
                            }
                        }
                        if (editorWindow != null) {
                            break;
                        }
                    }
                }
                s_EditorEditorWindow.Add(editor, editorWindow);
            }

            // If the editor window is null then something changed within Unity. Return true as a failsafe so all of the inspectors will still be drawn.
            if (editorWindow == null) {
                return true;
            }

            Func<Vector2> scrollPositionFunction;
            if (!s_EditorWindowScrollPositionFunction.TryGetValue(editorWindow, out scrollPositionFunction)) {
                // Create the delegate because it's faster to access then reflected fields.
                var fieldExpression = Expression.Field(Expression.Constant(editorWindow), c_ScrollPositionString);
                if (fieldExpression != null) {
                    scrollPositionFunction = Expression.Lambda<Func<Vector2>>(fieldExpression).Compile();
                }
                s_EditorWindowScrollPositionFunction.Add(editorWindow, scrollPositionFunction);
            }

            // If the function is null then something changed within Unity. Return true as a failsafe so all of the inspectors will still be drawn.
            if (scrollPositionFunction == null) {
                return true;
            }
            
            // The scroll position exists. Determine if the last rect is within view.
            var lastRect = GUILayoutUtility.GetLastRect();
            if (Screen.height + scrollPositionFunction().y < lastRect.y) {
                return false;
            }
            return true;
        }
    }
}