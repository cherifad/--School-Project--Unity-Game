using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Helpers;
using UnityEngine;
using UnityEditor;

namespace com.ootii.Setup
{
    public class SimpleSelectOption
    {
        public int ID;
        public string Name;

        public SimpleSelectOption(int rID, string rName)
        {
            ID = rID;
            Name = rName;
        }
    }

    public static class MotionCategoryHelper 
    {
        private static readonly Dictionary<int, SimpleSelectOption> mCategories = new Dictionary<int, SimpleSelectOption>();
        

        public static List<SimpleSelectOption> GetCategories(Func<SimpleSelectOption, object> rSortExpression)
        {
            return mCategories
                .Select(x => x.Value)
                .OrderBy(rSortExpression)
                .ToList();            
        }

        public static void AddCategory(int rID)
        {
            if (!mCategories.ContainsKey(rID))
            {
                mCategories.Add(rID, new SimpleSelectOption(rID, GetCategoryName(rID)));
            }            
        }

        public static string GetCategoryName(int rID)
        {
            if (mCategories.ContainsKey(rID))
            {
                return mCategories[rID].Name;
            }

            string lFieldName = ReflectionHelper.GetFieldName<EnumMotionCategories, int>(rID);
            return lFieldName;
        }
    }
}

