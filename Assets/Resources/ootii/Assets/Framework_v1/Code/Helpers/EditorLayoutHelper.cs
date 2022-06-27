#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace com.ootii.Helpers
{
    /// <summary>
    /// Helper class for performing the layout of custom editors; the intent is to eventually
    /// migrate the layout-related code from EditorHelper.
    /// </summary>
    public static class EditorLayoutHelper 
    {
        //public static void DrawRegion(Rect rRect, Action rDrawContents)
        //{
        //    try
        //    {
        //        GUILayout.BeginArea(rRect);
        //        rDrawContents();
        //    }
        //    finally
        //    {
        //        GUILayout.EndArea();
        //    }
        //}

        public static void DrawGroupBox(Action rDrawContents, GUIStyle rBoxStyle = null, bool rDisabled = false, params GUILayoutOption[] rOptions)
        {
            EditorGUI.BeginDisabledGroup(rDisabled);
            try
            {
                GUILayout.BeginVertical(rBoxStyle == null ? EditorHelper.GroupBox : rBoxStyle);
                rDrawContents();
            }
            finally
            {
                GUILayout.EndVertical();
            }

            EditorGUI.EndDisabledGroup();
        }        

        public static void DrawHorizontalGroup(Action rDrawContents, bool rDisabled = false, params GUILayoutOption[] rLayoutOptions)
        {
            EditorGUI.BeginDisabledGroup(rDisabled);
            try
            {
                GUILayout.BeginHorizontal(rLayoutOptions);

                rDrawContents();
            }
            finally
            {
                GUILayout.EndHorizontal();
            }   
            EditorGUI.EndDisabledGroup();
        }       

        public static void DrawScrollView(ref Vector2 rScrollPosition, GUIStyle rScrollArea, Action rDrawContents)
        {
            try
            {
                rScrollPosition = GUILayout.BeginScrollView(rScrollPosition, rScrollArea);

                rDrawContents();
            }
            finally
            {
                GUILayout.EndScrollView();
            }
        }


        public static void DrawMessageBox(string rMessage, MessageType rMessageType = MessageType.Info,
            GUIStyle rStyle = null)
        {
            try
            {
                EditorGUILayout.BeginVertical(rStyle == null ? EditorHelper.Box : rStyle);
                EditorHelper.DrawInspectorDescription(rMessage, rMessageType);
            }
            finally
            {
                EditorGUILayout.EndVertical();
            }
        }


        #region Rect Extensions

        /// <summary>
        /// Add padding to a Rect 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="horizontal"></param>
        /// <param name="vertical"></param>
        /// <returns></returns>
        public static Rect Pad(this Rect rect, int horizontal, int vertical)
        {
            return new Rect(
                rect.x + horizontal,
                rect.y + vertical,
                rect.width - (horizontal * 2),
                rect.height - (vertical * 2));
        }        
        
        /// <summary>
        /// A RectOffset with each dimension being the same size
        /// </summary>        
        /// <param name="size"></param>
        /// <returns></returns>
        public static RectOffset GetSquareOffset(int size)
        {
            return new RectOffset(size, size, size, size);
        }

        #endregion
    }
}
#endif
