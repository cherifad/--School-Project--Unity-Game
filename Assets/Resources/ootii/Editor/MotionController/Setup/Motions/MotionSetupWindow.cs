using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using com.ootii.Base.EditorWindows;
using com.ootii.Helpers;
using com.ootii.Setup.Motions;
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable ArrangeAccessorOwnerBody

namespace com.ootii.Setup
{
    /// <summary>
    /// Generic selection window for types given a specific base type.
    /// </summary>    
    [EditorWindow("Select Motions", AutoFocus = true)]
    public class MotionSetupWindow : CustomEditorWindow<MotionSetupWindow>
    {        
        /// <summary>
        /// Delegate to set the parent list
        /// </summary>
        public delegate void OnSelectedDelegate(List<Type> rTypes);                
        // Layout GUIStyle overrides        
        public override GUIStyle HeaderStyle { get { return WindowStyles.HeaderStyle; } }
        public override GUIStyle FooterStyle { get { return EditorHelper.GroupBox; } }
        public override GUIStyle MainStyle { get { return EditorHelper.Box; } }

        public override float MinWidth  { get { return 640f; } }
        public override float MinHeight  { get { return 480f; } }

        
        /// <summary>
        /// Function to call when the parent is selected
        /// </summary>
        public OnSelectedDelegate OnSelectedEvent = null;
        
        // Holds the currently active filter options        
        private readonly MotionFilterOptions mMotionFilter = new MotionFilterOptions();        

        // The master list of motions
        private readonly List<MotionSelectOption> mMotions = new List<MotionSelectOption>();

        // The filtered list of motions (what actually appears in the editor)
        private readonly List<MotionSelectOption> mFilteredMotions = new List<MotionSelectOption>();

        // The list of motion categories
        private readonly List<CategorySelectOption> mCategories = new List<CategorySelectOption>();
        private string[] mCategoryNames;
        private Dictionary<int, int> mCategoryIndices;

        private readonly List<PackSelectOption> mMotionPacks = new List<PackSelectOption>();
        private string[] mMotionPackNames;
        private Dictionary<string, int> mMotionPackIndices;
       
        // Track the selected item in the list
        private int mSelectedItemIndex = -1;        

        // The current scrollbar position
        private Vector2 mScrollPosition = Vector2.zero;                             
                
        /// <summary>
        /// Initializes the window
        /// </summary>
        protected override void Initialize()
        {
            mMotions.AddRange(
                MotionEditorCache.Instance.Motions
                    .Where(info => !string.IsNullOrEmpty(info.Name))
                    .OrderBy(info => info.Name)
                    .Select(info => new MotionSelectOption(info)));

            mCategories.AddRange(
                MotionEditorCache.Instance.Categories
                    .OrderBy(info => info.Name)
                    .Select(info => new CategorySelectOption(info)));

            // Build the list of category names (for display in the UI)
            mCategoryNames = mCategories.Select(info => info.Item.Name).ToArray();

            // Create a dictionary to map Category ID values to their index
            mCategoryIndices = mCategories
                .Select((category, index) => new {category.Item.ID, index})
                .ToDictionary(x => x.ID, x => x.index);    
            
            mMotionPacks.AddRange(
                MotionEditorCache.Instance.MotionPacks
                    .OrderBy(info => info.Name)
                    .Select(info => new PackSelectOption(info)));

            //mMotionPackNames = mMotionPacks.Select(info => info.Item.Name).ToArray();
            //mMotionPackIndices = mMotionPacks
            //    .Select((pack, index) => new {pack.Item.Name, index})
            //    .ToDictionary(x => x.Name, x => x.index);

            mMotionPackNames = MotionEditorCache.Instance.MotionPackNames.ToArray();
            mMotionPackIndices = mMotionPackNames
                .Select((packName, index) => new {name = packName, index})
                .ToDictionary(x => x.name, x => x.index);            
            
            ApplyFilter(mMotionFilter);
        }

        /// <summary>
        /// Frame update for GUI objects. Heartbeat of the window that 
        /// allows us to update the UI
        /// </summary>
        protected override void DrawContent()
        {            
            const int lHorizontalPadding = 2;
            const int lVerticalPadding = 2;
            
            // Header
            bool lFilterChanged = false;
            DrawRegion(mHeaderRegion, () => DrawFilterOptions(out lFilterChanged));            

            if (lFilterChanged)
            {
                mSelectedItemIndex = -1;
                ApplyFilter(mMotionFilter);
            }
            
            // Content
            DrawRegion(mSidebarRegion.Pad(lHorizontalPadding, lVerticalPadding), ref mScrollPosition, DrawMotionList);
            DrawRegion(mMainRegion, DrawSelectedMotion);

            // Footer
            DrawRegion(mFooterRegion, DrawFooter);            
        }
        
        /// <summary>
        /// Draw the filtering options at the top of the window
        /// </summary>
        /// <returns></returns>
        private void DrawFilterOptions(out bool rFilterChanged)
        {            
            bool lFilterChanged = false;

            GUILayout.Space(5f);            
            EditorLayoutHelper.DrawHorizontalGroup(() =>
            {
                EditorGUI.BeginChangeCheck();

                mMotionFilter.ShowObsolete = EditorGUILayout.ToggleLeft("Show Obsolete Motions", mMotionFilter.ShowObsolete);

                GUILayout.Space(25f);

                EditorGUILayout.BeginVertical();
                mMotionFilter.CategoryMask = EditorGUILayout.MaskField("Categories", mMotionFilter.CategoryMask, mCategoryNames, GUILayout.Width(400));
                mMotionFilter.PackMask = EditorGUILayout.MaskField("Motion Packs", mMotionFilter.PackMask, mMotionPackNames, GUILayout.Width(400));
                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck()) { lFilterChanged = true; }
            });
           
            EditorGUILayout.Separator();

            EditorLayoutHelper.DrawHorizontalGroup(() =>
            {
                string lOriginalSearch = mMotionFilter.SearchString;

                mMotionFilter.SearchString = GUILayout.TextField(lOriginalSearch,
                    WindowStyles.ToolbarSearchField, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(400f));

                if (mMotionFilter.SearchString != lOriginalSearch)
                {
                    lFilterChanged = true;
                }

                if (GUILayout.Button("", WindowStyles.ToolbarSearchCancelButton))
                {
                    GUI.FocusControl(null);

                    mMotionFilter.SearchString = "";
                    lFilterChanged = true;
                }
            });
            
            GUILayout.Space(5f);

            rFilterChanged = lFilterChanged;
        }
        
        /// <summary>
        /// Draw the list of motions to select from
        /// </summary>
        private void DrawMotionList()
        {
            for (int i = 0; i < mFilteredMotions.Count; i++)
            {
                try
                {
                    GUILayout.BeginHorizontal();

                    mFilteredMotions[i].Selected =
                        GUILayout.Toggle(mFilteredMotions[i].Selected, "", GUILayout.Width(16f));

                    GUIStyle lRowStyle =
                        (i == mSelectedItemIndex ? EditorHelper.SelectedLabel : EditorHelper.Label);
                    if (GUILayout.Button(mFilteredMotions[i].Item.Name, lRowStyle, GUILayout.MinWidth(100)))
                    {
                        mSelectedItemIndex = i;
                    }
                }
                finally
                {
                    GUILayout.EndHorizontal();
                }
            }
        }
     
        /// <summary>
        /// Draw the details of the currently selected motion
        /// </summary>
        private void DrawSelectedMotion()
        {
            bool lHasSelectedMotion = mSelectedItemIndex >= 0 && mSelectedItemIndex < mFilteredMotions.Count;
            if (!lHasSelectedMotion)
            {
                GUILayout.FlexibleSpace();
                return;
            }

            MotionSelectOption lMotion = mFilteredMotions[mSelectedItemIndex];

            EditorGUILayout.LabelField(
                mFilteredMotions[mSelectedItemIndex].Item.Name,
                TitleStyle,
                GUILayout.Height(20f));

            EditorLayoutHelper.DrawHorizontalGroup(() =>
            {
                if (GUILayout.Button(WindowStyles.ScriptIcon, GUI.skin.label, GUILayout.Width(20), GUILayout.Height(20)) ||
                    GUILayout.Button(lMotion.Item.FilePath, PathStyle, GUILayout.Width(300)))
                {
                    AssetHelper.OpenScriptFile(lMotion.Item.FilePath, lMotion.Item.Type);
                }

                GUILayout.FlexibleSpace();
            });            

            GUILayout.Space(3f);

            GUILayout.Label(lMotion.Item.Description, DescriptionStyle);

            GUILayout.FlexibleSpace();
            
            string lCategoryLabel = mCategories.Find(info => info.Item.ID == lMotion.Item.Category).Item.Name;
            GUILayout.Label("Category: " + lCategoryLabel, TagsStyle);
            GUILayout.Label("Motion Pack: " + lMotion.Item.PackName, TagsStyle);

            GUILayout.Space(15f);

            if (GUILayout.Button("Open Script File"))
            {
                AssetHelper.OpenScriptFile(lMotion.Item.FilePath, lMotion.Item.Type);
            }                           
        }

        /// <summary>
        /// Draw the window footer
        /// </summary>
        private void DrawFooter()
        {
            GUILayout.Space(20f);
            bool lCloseWindow = false;

            EditorLayoutHelper.DrawHorizontalGroup(() =>
            {                
                int lCount = mFilteredMotions.Count(info => info.Selected);

                if (GUILayout.Button("deselect all (" + lCount + ")", GUI.skin.label, GUILayout.Width(100f)))
                {
                    foreach (var lInfo in mFilteredMotions) { lInfo.Selected = false; }
                }

                GUILayout.FlexibleSpace();                

                if (GUILayout.Button("Select", GUILayout.Width(70)))
                {
                    if (OnSelectedEvent != null)
                    {
                        List<Type> lMotionTypes = mFilteredMotions
                            .Where(info => info.Selected)
                            .Select(info => info.Item.Type)
                            .ToList();

                        OnSelectedEvent(lMotionTypes);
                    }

                    lCloseWindow = true;
                }

                if (GUILayout.Button("Cancel", GUILayout.Width(70)))
                {
                    lCloseWindow = true;
                }

                GUILayout.Space(25f);
            });         

            GUILayout.Space(2f);

            if (lCloseWindow)
            {
                mSelectedItemIndex = -1;
                Close();
            }
        }

        /// <summary>
        /// Apply the filter to the list of motions
        /// </summary>
        /// <param name="rFilter"></param>
        /// <returns></returns>
        private int ApplyFilter(MotionFilterOptions rFilter)
        {     
            // && ((mMotionFilter.PackMask & (1 << mMotionPackIndices[info.Item.Name])) != 0)
            // Add all motions that are within the current motion pack and category masks
            var lMotions = mMotions
                .Where(info => ((mMotionFilter.CategoryMask & (1 << mCategoryIndices[info.Item.Category])) != 0)
                               && ((mMotionFilter.PackMask & (1 << FindPackIndex(info.Item.PackName))) != 0))
                .OrderBy(info => info.Item.Name).ToList();
            
            if (!rFilter.ShowObsolete)
            {
                // Remove all motions with the [ObsoleteMotion] attribute
                lMotions.RemoveAll(info => info.Item.IsObsolete);
            }
            
            var lSearchText = ParseSearchText(rFilter.SearchString);
            if (lSearchText.Count > 0)
            {
                // Remove all motions that do not match the search criteria
                lMotions.RemoveAll(info => 
                    lSearchText.Any(text => !info.Item.Name.Contains(text, StringComparison.OrdinalIgnoreCase)));
            }
                     
            mFilteredMotions.Clear();
            mFilteredMotions.AddRange(lMotions);

            return mFilteredMotions.Count;
        }

        private int FindPackIndex(string rName)
        {
            if (mMotionPackIndices.ContainsKey(rName))
            {
                return mMotionPackIndices[rName];
            }

            //Debug.Log($"Key {rName} does not exist; doing it the slower way");
            return mMotionPackNames.ToList().IndexOf(rName);
        }
      
       
        #region GUI Styles
        
        /// <summary>
        /// Style
        /// </summary>
        private static GUIStyle mTitleStyle = null;
        public static GUIStyle TitleStyle
        {
            get
            {
                if (mTitleStyle == null)
                {
                    mTitleStyle = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.UpperLeft,
                        fontSize = 14,
                        fontStyle = FontStyle.Bold,
                        wordWrap = false
                    };
                }

                return mTitleStyle;
            }
        }

        /// <summary>
        /// Style
        /// </summary>
        private static GUIStyle mTagsStyle = null;
        public static GUIStyle TagsStyle
        {
            get
            {
                if (mTagsStyle == null)
                {
                    mTagsStyle = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.UpperLeft, fontSize = 10, wordWrap = true
                    };
                }

                return mTagsStyle;
            }
        }

        /// <summary>
        /// Style
        /// </summary>
        private static GUIStyle mDescriptionStyle = null;
        public static GUIStyle DescriptionStyle
        {
            get
            {
                if (mDescriptionStyle == null)
                {
                    mDescriptionStyle = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.UpperLeft, fontSize = 12, wordWrap = true
                    };
                }

                return mDescriptionStyle;
            }
        }

        /// <summary>
        /// Style
        /// </summary>
        private static GUIStyle mPathStyle = null;
        public static GUIStyle PathStyle
        {
            get
            {
                if (mPathStyle == null)
                {
                    mPathStyle = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 10,
                        wordWrap = true,                        
                        padding = new RectOffset(0, 0, 3, 0)                        
                    };
                }

                return mPathStyle;
            }
        }

        #endregion // GUI Styles
        
        
        public class MotionFilterOptions
        {
            /// <summary>
            /// Toggle setting to display motions marked with the ObsoleteMotionAttribute
            /// </summary>
            public bool ShowObsolete = false;

            /// <summary>
            /// The search criteria for filtering the motion list
            /// </summary>
            public string SearchString = "";

            // Mask representing the currently selected categories. This is determined by the index within the list
            // so we will use a dictionary so that we can easily look up the index for a specific category ID
            public int CategoryMask = -1;

            /// <summary>
            /// Mask reprsenting the currently selected Motion Packs
            /// </summary>
            public int PackMask = -1;            
        }
    }
}

