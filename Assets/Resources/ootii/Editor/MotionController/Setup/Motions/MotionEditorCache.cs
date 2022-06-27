using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Helpers;

// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable ConvertClosureToMethodGroup
// ReSharper disable UsePatternMatching

namespace com.ootii.Setup.Motions
{
    public class MotionEditorCache
    {   
        /// <summary>
        /// The master cache of Motion types
        /// </summary>
        public List<MotionInfo> Motions { get { return mMotions; } }        
        private readonly List<MotionInfo> mMotions;

        /// <summary>
        /// The master cache of Motion categories (ID + Name pairs)
        /// </summary>
        public List<MotionCategoryInfo> Categories { get { return mCategories; } }
        private readonly List<MotionCategoryInfo> mCategories;

        /// <summary>
        /// The master cahce of Motion Pack types
        /// </summary>
        public List<MotionPackSetupInfo> MotionPacks { get { return mMotionPacks; } }
        private readonly List<MotionPackSetupInfo> mMotionPacks;

        public List<string> MotionPackNames { get { return mMotionPackNames; } }
        private readonly List<string> mMotionPackNames;
        
        private MotionEditorCache()
        {
            // Scan for types in all assemblies that inherit from MotionControllerMotion            
            mMotions = AssemblyHelper.FoundTypes
                .Where(type => !type.IsAbstract && typeof(MotionControllerMotion).IsAssignableFrom(type))
                .Select(type => CreateMotionInfo(type))
                .ToList();

            // Scan for types in all assemblies that inherit from MotionPackDefinition            
            mMotionPacks = AssemblyHelper.FoundTypes
                .Where(type => !type.IsAbstract && typeof(MotionPackDefinition).IsAssignableFrom(type))
                .Select(type => CreateMotionPackInfo(type))                
                .ToList();

            mMotionPackNames = mMotions                
                .Select(info => info.PackName)
                .Distinct()
                .OrderBy(name => name)
                .ToList();


            //Debug.Log($"Cached {mMotions.Count} motion types");

            // Get all distinct category IDs from the master cache of motions and build 
            // the ID + Name pairs for them
            mCategories = mMotions
                .Select(info => info.Category)
                .Distinct()
                .Select(id => new MotionCategoryInfo {ID = id, Name = TryGetCategoryName(id)})                
                .ToList();

            //Debug.Log($"Cached { mCategories } motion categories");            
        }

        /// <summary>
        /// Get a sorted list of motions
        /// </summary>
        /// <param name="rSortExpression"></param>
        /// <returns></returns>
        public List<MotionInfo> GetMotions(Func<MotionInfo, object> rSortExpression)
        {
            return mMotions.OrderBy(rSortExpression).ToList();//.Select(info => info.Clone())
        }

        public List<MotionPackSetupInfo> GetMotionPacks(Func<MotionPackSetupInfo, object> rSortExpression)
        {
            return mMotionPacks.OrderBy(rSortExpression).ToList();
        }

        /// <summary>
        /// Get a sorted list of categories
        /// </summary>
        /// <param name="rSortExpression"></param>
        /// <returns></returns>
        public List<MotionCategoryInfo> GetCategories(Func<MotionCategoryInfo, object> rSortExpression)
        {
            return mCategories.OrderBy(rSortExpression).ToList();
        }

        
        /// <summary>
        /// Attempt to create the category's display name from the name of the const field with
        /// a value equal to the specified ID
        /// </summary>
        /// <param name="rCategoryID"></param>
        /// <returns></returns>
        private static string TryGetCategoryName(int rCategoryID)
        {
            try
            {
                string fieldName = ReflectionHelper.GetFieldName<EnumMotionCategories, int>(rCategoryID);
                return StringHelper.ToTitleCase(fieldName.Replace("_", " "));                
            }
            catch (Exception)
            {
                return "Undefined";
            }
        }

        /// <summary>
        /// Create the MotionInfo for the provided Type
        /// </summary>
        /// <param name="rType"></param>
        /// <returns></returns>
        private static MotionInfo CreateMotionInfo(Type rType)
        {
            var lMotionInfo = new MotionInfo
            {
                Name = MotionNameAttribute.GetName(rType),
                Description = MotionDescriptionAttribute.GetDescription(rType),
                IsObsolete = ObsoleteMotionAttribute.Get(rType),
                Type = rType                
            };            

            var lMotion = Activator.CreateInstance(rType) as MotionControllerMotion;
            if (lMotion != null)
            {
                lMotionInfo.Category = lMotion.Category;
                lMotionInfo.PackName = lMotion.Pack;
            }

            return lMotionInfo;
        }

        private static MotionPackSetupInfo CreateMotionPackInfo(Type rType)
        {
            PropertyInfo[] lStaticMethods = rType.GetProperties(BindingFlags.Static | BindingFlags.Public);
            
            var lMotionPackInfo = new MotionPackSetupInfo { Type = rType, Properties = lStaticMethods };
            foreach (var lPropertyInfo in lStaticMethods)
            {
                if (lPropertyInfo.Name == "PackName")
                {
                    lMotionPackInfo.Name = lPropertyInfo.GetValue(null, null) as string;
                }
            }            

            return lMotionPackInfo;
        }

        #region Instance Management

        private static readonly object mLock = new object();
        private static volatile MotionEditorCache mInstance;

        static MotionEditorCache() { }

        public static MotionEditorCache Instance
        {
            get
            {
                if (mInstance == null)
                {
                    lock (mLock)
                    {
                        if (mInstance == null) mInstance = new MotionEditorCache();
                    }
                }

                return mInstance;
            }
        }

        #endregion Instance Management
    }
}