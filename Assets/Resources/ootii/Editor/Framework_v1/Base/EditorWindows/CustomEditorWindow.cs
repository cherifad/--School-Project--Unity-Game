using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable MergeConditionalExpression
// ReSharper disable InlineOutVariableDeclaration

namespace com.ootii.Base.EditorWindows
{
    /// <summary>
    /// Base class for typed custom Editor windows (using defined regions)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CustomEditorWindow<T> : CustomEditorWindow where T : EditorWindow
    {
        public static T GetInstance()
        {
            string lTitle;
            bool lAutoFocus;
            EditorWindowAttribute.GetFor(typeof(T), out lTitle, out lAutoFocus);
            return GetWindow<T>(false, lTitle, lAutoFocus);
        }       
    }

    /// <summary>
    /// Base class for custom Editor windows (using defined regions)
    /// </summary>
    public abstract class CustomEditorWindow : EditorWindow
    {              
        // Window dimensions
        public virtual float MinWidth{ get { return 320f; } }
        public virtual float MinHeight{ get { return 200f; } }

        // Layout region dimensions
        public virtual float BorderThickness { get { return 0f; } }        
        public virtual float SidebarWidth { get { return 240f; } }
        public virtual float HeaderHeight { get { return 100f; } }
        public virtual float FooterHeight { get { return 60f; } }

        public virtual int HorizontalSpacing { get { return 5; } }
        public virtual int VerticalSpacing { get { return 5; } }
        

        public virtual GUIStyle BorderStyle { get { return GUI.skin.label; } }
        public virtual GUIStyle HeaderStyle { get { return GUI.skin.box; } }
        public virtual GUIStyle FooterStyle { get { return GUI.skin.box; } }
        public virtual GUIStyle SidebarStyle { get { return GUI.skin.box; } }
        public virtual GUIStyle MainStyle { get { return GUI.skin.box; } }


        // Layout areas
        protected Rect mBorderRegion;        
        protected Rect mFooterRegion;
        protected Rect mSidebarRegion;
        protected Rect mHeaderRegion;
        protected Rect mMainRegion;

        public void Awake()
        {
            this.minSize = new Vector2(MinWidth, MinHeight);
            Initialize();
        }

        public void OnGUI()
        {
            DoLayout();

            DrawContent();
        }

        protected virtual void Initialize() { }

        protected abstract void DrawContent();

        /// <summary>
        /// Draw the structure of the window
        /// </summary>
        protected virtual void DoLayout()
        {
            mBorderRegion = new Rect(
                BorderThickness, 
                BorderThickness,
                this.position.width - (BorderThickness *2), 
                this.position.height - (BorderThickness *2));

            mHeaderRegion = new Rect(
                mBorderRegion.x,
                mBorderRegion.y,
                mBorderRegion.width, 
                HeaderHeight);

            mSidebarRegion = new Rect(
                mHeaderRegion.x, 
                mHeaderRegion.yMax + VerticalSpacing,
                SidebarWidth, 
                mBorderRegion.height - (2* VerticalSpacing + mHeaderRegion.yMax + FooterHeight));
            
            mFooterRegion = new Rect(
                mHeaderRegion.x, 
                mSidebarRegion.yMax + VerticalSpacing, 
                mHeaderRegion.width,
                FooterHeight);

            mMainRegion = new Rect(
                mSidebarRegion.xMax + HorizontalSpacing, 
                mSidebarRegion.y,
                mHeaderRegion.width - (mSidebarRegion.width + HorizontalSpacing), 
                mSidebarRegion.height);
            
            GUI.Box(mBorderRegion, string.Empty, BorderStyle);
            GUI.Box(mHeaderRegion, string.Empty, HeaderStyle);            
            GUI.Box(mSidebarRegion, string.Empty, SidebarStyle);
            GUI.Box(mFooterRegion, string.Empty, FooterStyle);
            GUI.Box(mMainRegion, string.Empty, MainStyle);
        }
            
        /// <summary>
        /// Draw into a region of the custom window
        /// </summary>
        /// <param name="rRect"></param>
        /// <param name="rDrawContent"></param>
        protected void DrawRegion(Rect rRect, Action rDrawContent)
        {
            try
            {
                GUILayout.BeginArea(rRect);
                rDrawContent();
            }
            finally
            {
                GUILayout.EndArea();
            }
        }
        
        /// <summary>
        /// Draw into a scrolling-enabled region of the custom window
        /// </summary>
        /// <param name="rRect"></param>
        /// <param name="rScrollPosition"></param>
        /// <param name="rDrawContent"></param>
        /// <param name="rScrollArea"></param>
        protected void DrawRegion(Rect rRect, ref Vector2 rScrollPosition, Action rDrawContent,
            GUIStyle rScrollArea = null)
        {
            try
            {
                GUILayout.BeginArea(rRect);
                rScrollPosition = GUILayout.BeginScrollView(rScrollPosition, 
                    (rScrollArea == null ? WindowStyles.ScrollArea : rScrollArea));
                rDrawContent();
            }
            finally
            {
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
        }

        /// <summary>
        /// Parse out a string of search words
        /// </summary>
        /// <param name="rSearchText"></param>
        /// <returns></returns>
        protected static List<string> ParseSearchText(string rSearchText)
        {            
            return rSearchText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(text => text.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
