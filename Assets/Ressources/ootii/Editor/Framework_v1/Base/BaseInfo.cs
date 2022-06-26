using System;

namespace com.ootii.Base
{
    [Serializable]
    public abstract class BaseInfo
    {
        public string Name;        
        public string Description;        
    }

    [Serializable]
    public class SelectOption<T> where T : BaseInfo
    {
        public bool Selected;        
        public T Item;

        public SelectOption(T rItem)
        {
            Item = rItem;
        }
    }    
}
