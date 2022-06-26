using System;
using com.ootii.Helpers;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Contains information about the motion pack that will be used
    /// when setting up the motions
    /// </summary>
    public abstract class MotionPackDefinition
    {        
        /// <summary>
        /// Defines the friendly name of the motion pack
        /// </summary>
        public static string PackName
        {
            get { return ""; }
        }

#if UNITY_EDITOR                
        /// <summary>
        /// Flag to determine if we include the motion pack in editor lists. This allows us to hide any motion pack 
        /// definitions that are for internal use.
        /// </summary>
        public static bool HidePack
        {
            get { return false; }
        }

        /// <summary>
        /// Draws the inspector for the pack
        /// </summary>
        /// <returns>Determines if the editor has been modified</returns>
        public static bool OnPackInspector(MotionController rMotionController)
        {
            return false;
        }

        /// <summary>
        /// Get the Animator Controller attached to the character
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <returns></returns>
        public static AnimatorController GetAnimatorController(MotionController rMotionController)
        {
            Animator lAnimator = rMotionController.Animator;
            if (lAnimator == null)
            {
                lAnimator = rMotionController.gameObject.GetComponent<Animator>();
                if (lAnimator == null) { return null; }
            }

            return lAnimator.runtimeAnimatorController as AnimatorController;
        }        
  
        /// <summary>
        /// Shows the "completed" dialog when a motion pack has been set up
        /// </summary>
        /// <param name="rPackName"></param>
        protected static void ShowCompletedDialog(string rPackName)
        {
            EditorUtility.DisplayDialog("Motion Pack: " + rPackName, "Motion pack imported. ", "ok");
        }

        /// <summary>
        /// Displays the standard help text and the pack title
        /// </summary>
        /// <param name="rTitle">Name of motion pack</param>
        /// <param name="rUserGuideLink">Optional link to User's Guide</param>
        protected static void DrawStandardHeader(string rTitle, string rUserGuideLink = "")
        {
            EditorGUILayout.BeginVertical();
            EditorHelper.DrawSmallTitle(rTitle);
            if (!string.IsNullOrEmpty(rUserGuideLink))
            {
                EditorHelper.DrawLink("See the latest User's Guide", rUserGuideLink);
            }
            else
            {
                EditorGUILayout.LabelField("See included documentation:", EditorHelper.SmallBoldLabel);
            }
            EditorGUILayout.LabelField("1. Download and import animations.", EditorHelper.SmallLabel);
            EditorGUILayout.LabelField("2. Unzip and replace animation meta files.", EditorHelper.SmallLabel);
            EditorGUILayout.LabelField("3. Select options and create motions.", EditorHelper.SmallLabel);

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// New version to draw the pack header and verify that its animations are present.
        /// </summary>
        /// <param name="rTitle"></param>
        /// <param name="rUserGuideLink"></param>
        /// <param name="rAnimationPath"></param>
        /// <returns></returns>
        protected static bool DrawPackHeader(string rTitle, string rUserGuideLink = "", string rAnimationPath = "")
        {
            bool lEnableSetup = true;
            EditorGUILayout.BeginVertical();
            EditorHelper.DrawSmallTitle(rTitle);
            if (!string.IsNullOrEmpty(rUserGuideLink))
            {
                EditorHelper.DrawLink("See the latest User's Guide", rUserGuideLink);
            }
            else
            {
                EditorGUILayout.LabelField("See included documentation:", EditorHelper.SmallBoldLabel);
            }
            EditorGUILayout.LabelField("1. Download and import animations.", EditorHelper.SmallLabel);
            if (!string.IsNullOrEmpty(rAnimationPath))
            {
                lEnableSetup = VerifyAnimationFile(rAnimationPath);
            }
            EditorGUILayout.LabelField("2. Unzip and replace animation meta files.", EditorHelper.SmallLabel);
            EditorGUILayout.LabelField("3. Select options and create motions.", EditorHelper.SmallLabel);

            EditorGUILayout.EndVertical();

            // If a centralized animation path has been set, then we'll still display the status message, but we won't 
            // disable the pack setup button.
            if (EditorPrefs.GetString("ootii_altanimationpath", string.Empty).Length > 0)
            {
                lEnableSetup = true;
            }

            return lEnableSetup;
        }

        /// <summary>
        /// Verifies that the specified animation file exists and displays a status message informing the user.
        /// </summary>
        /// <param name="rPath"></param>
        /// <returns></returns>
        public static bool VerifyAnimationFile(string rPath)
        {
            bool lFileExists = AssetHelper.VerifyFilePath(rPath);

            EditorGUI.indentLevel++;
            if (lFileExists)
            {
                EditorGUILayout.HelpBox("Animation path verified.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("The animations could not be located. Please refer to the User's Guide.", MessageType.Warning);
            }

            EditorGUI.indentLevel--;

            return lFileExists;
        }

        /// <summary>
        /// Displays the controls for selecting the path to the folder containing the animation files: 
        /// a textbox and a button which opens a folder select panel.
        /// 
        /// If an EditorPrefs key is passed in, it also stores the selected path in EditorPrefs 
        /// </summary>
        /// <param name="rAnimationPath"></param>
        /// <param name="rPrefsKey"></param>
        /// <param name="rChanged"></param>
        /// <param name="rTitle"></param>
        /// <returns></returns>
        public static string DrawAnimationPathSelect(string rAnimationPath, string rPrefsKey, out bool rChanged, string rTitle = "Animation Path")
        {
            bool lPathChanged = false;
            if (string.IsNullOrEmpty(rAnimationPath) || !AssetDatabase.IsValidFolder(rAnimationPath))
            {
                EditorHelper.DrawInspectorDescription(
                    "Invalid or no animation path specified. The animator controller cannot be set up correctly without " +
                    "the path to the folder containing the .fbx or .anim files.", MessageType.Warning);
            }

            string lAnimationPath = EditorHelper.FolderSelect(
                    new GUIContent(rTitle, "Path to the folder containing the animation files."),
                    rAnimationPath,
                    out lPathChanged);

            if (lPathChanged)
            {
                rAnimationPath = lAnimationPath;
                if (!string.IsNullOrEmpty(rPrefsKey))
                {
                    EditorPrefs.SetString(rPrefsKey, rAnimationPath);
                }
            }

            rChanged = lPathChanged;
            return rAnimationPath;
        }

        protected static void DrawPackSection(string rTitle, Action rDrawContentsAction, bool rDisabled = false)
        {
            EditorGUI.BeginDisabledGroup(rDisabled);
            try
            {
                GUILayout.BeginVertical(EditorHelper.Box);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(4f);
                EditorGUILayout.LabelField(rTitle, EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                rDrawContentsAction();
            }
            finally
            {
                GUILayout.EndVertical();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Separator();
        }
#endif
    }
}
