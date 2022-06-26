using System;
using com.ootii.Helpers;
using UnityEditor;

namespace com.ootii.Base.EditorWindows
{
    /// <summary>
    /// Define properties for a custom Editor Window that may need to be accessed
    /// by static methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EditorWindowAttribute : Attribute
    {
        public string Title { get; private set; }
        public bool AutoFocus { get; set; }

        public EditorWindowAttribute(string rTitle)
        {
            Title = rTitle;
        }

        /// <summary>
        /// Get the attribute for the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rTitle"></param>
        /// <param name="rAutoFocus"></param>
        public static void GetFor<T>(out string rTitle, out bool rAutoFocus) where T : EditorWindow
        {
            GetFor(typeof(T), out rTitle, out rAutoFocus);
        }

        /// <summary>
        /// Get the attribute for the specified type
        /// </summary>
        /// <param name="rType"></param>
        /// <param name="rTitle"></param>
        /// <param name="rAutoFocus"></param>
        public static void GetFor(Type rType, out string rTitle, out bool rAutoFocus)
        {
            var lAttribute = ReflectionHelper.GetAttribute<EditorWindowAttribute>(rType);
            rTitle = lAttribute != null ? lAttribute.Title : string.Empty;
            rAutoFocus = lAttribute != null && lAttribute.AutoFocus;
        }
    }
}