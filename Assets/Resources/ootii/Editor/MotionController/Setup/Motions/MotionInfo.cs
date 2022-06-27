using System;
using System.Reflection;
using com.ootii.Base;

// ReSharper disable UseNegatedPatternMatching

namespace com.ootii.Setup.Motions
{   
    /// <summary>
    /// Class containing the relevant info used for setting up Motions via script. The fields are populated
    /// via reflection.
    /// </summary>        
    public class MotionInfo : BaseInfo
    {        
        public Type Type;

        /// <summary>
        /// Physical path to the script file (for opening in the IDE)
        /// </summary>
        public string FilePath { get { return Type.Name + ".cs"; } }

        /// <summary>
        /// ID from EnumMotionCategories representing the category of the motion
        /// </summary>
        public int Category;

        /// <summary>
        /// The motion pack to which the motion belongs (if any)
        /// </summary>
        public string PackName;

        public bool IsObsolete;

        //public bool Selected;

        //public MotionInfo Clone()
        //{
        //    return (MotionInfo) this.MemberwiseClone();
        //}
    }

    public class MotionSelectOption : SelectOption<MotionInfo> 
    {
        public MotionSelectOption(MotionInfo rItem) : base(rItem) { }
    }

    [Serializable]
    public class MotionPackSetupInfo : BaseInfo
    {        
        /// <summary>
        /// Type of the Motion Pack Definition files (used to get methods and properties via reflection)
        /// </summary>
        public Type Type;

        public PropertyInfo[] Properties;

        //public bool Selected;

        //public MotionPackSetupInfo Clone()
        //{
        //    return (MotionPackSetupInfo) this.MemberwiseClone();
        //}
    }

    public class PackSelectOption : SelectOption<MotionPackSetupInfo>
    {
        public PackSelectOption(MotionPackSetupInfo rItem) : base(rItem) { }
    }

    public class MotionCategoryInfo : BaseInfo
    {
        public int ID;                
    }

    public class CategorySelectOption: SelectOption<MotionCategoryInfo>
    {
        public CategorySelectOption(MotionCategoryInfo rItem) : base(rItem) { }
    }
}