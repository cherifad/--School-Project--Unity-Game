using UnityEngine;
using UnityEditor;

// ReSharper disable ConvertIfStatementToNullCoalescingExpression

namespace com.ootii.Base.EditorWindows
{
    public static class WindowStyles
    {
        #region Layout Styles
        
        public static GUIStyle FooterStyle
        {
            get
            {
                if (mFooterStyle == null)
                {                    
                    Texture2D lTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "GroupBox_pro" : "GroupBoxLight");
                    mFooterStyle = new GUIStyle(GUI.skin.box)
                    {
                        normal = {background = lTexture}, padding = new RectOffset(5, 5, 10, 10)
                    };
                }

                return mFooterStyle;
            }            
        }
        private static GUIStyle mFooterStyle = null;

        public static GUIStyle HeaderStyle
        {
            get
            {
                if (mHeaderStyle == null)
                {                    
                    Texture2D lTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "GroupBox_pro" : "GroupBoxLight");
                    mHeaderStyle = new GUIStyle(GUI.skin.box)
                    {
                        normal = {background = lTexture}, padding = new RectOffset(5, 5, 10, 10)
                    };
                }

                return mHeaderStyle;
            }            
        }
        private static GUIStyle mHeaderStyle = null;

        /// <summary>
        /// A standard ScrollView in the inspector
        /// </summary>
        public static GUIStyle ScrollArea
        {
            get
            {
                if (mScrollArea == null)
                {
                    Texture2D lTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "OrangeGrayBox_pro" : "OrangeGrayBox");
                    mScrollArea = new GUIStyle(GUI.skin.box)
                    {
                        normal = {background = lTexture}, 
                        padding = new RectOffset(2, 2, 2, 2)
                    };
                }

                return mScrollArea;
            }
        }
        private static GUIStyle mScrollArea;

        


        #endregion Layout Styles


        #region Control Elements

        /// <summary>
        /// The style for a toolbar search control
        /// </summary>
        public static GUIStyle ToolbarSearchField
        {
            get
            {
                if (mToolbarSearchTextField == null)
                {
                    mToolbarSearchTextField = GUI.skin.FindStyle("ToolbarSeachTextField");
                }

                return mToolbarSearchTextField;
            }
        }
        private static GUIStyle mToolbarSearchTextField;

        /// <summary>
        /// The "cancel search" button used on the toolbar search field
        /// </summary>
        public static GUIStyle ToolbarSearchCancelButton
        {
            get
            {
                if (mToolbarSearchCancelButton == null)
                {
                    mToolbarSearchCancelButton = GUI.skin.FindStyle("ToolbarSeachCancelButton");
                }

                return mToolbarSearchCancelButton;
            }
        }
        private static GUIStyle mToolbarSearchCancelButton;

        #endregion Control Elements


        #region Images

        /// <summary>
        /// The icon to use for C# script files
        /// </summary>
        public static Texture2D ScriptIcon
        {
            get
            {
                if (mScriptIcon == null)
                {
                    mScriptIcon = EditorGUIUtility.FindTexture("cs Script Icon");
                }

                return mScriptIcon;
            }
        }
        private static Texture2D mScriptIcon;

        #endregion Images
    }
}
