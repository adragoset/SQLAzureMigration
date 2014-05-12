using System;
using System.Collections;

namespace SQLAzureMWUtils
{
    public class NotSupportedList : CollectionBase
    {
        public NotSupportedList()
        {
        }

        public NotSupportedList(NotSupported[] arr)
        {
            AddRange(arr);
        }

        public NotSupported this[int index]
        {
            get { return (NotSupported) InnerList[index]; }
        }

        public void Add(NotSupported item)
        {
            InnerList.Add(item);
        }

        public void AddRange(NotSupported[] items)
        {
            InnerList.AddRange(items);
        }

        public void Remove(NotSupported item)
        {
            InnerList.Remove(item);
        }

        public int IndexOf(NotSupported item)
        {
            return InnerList.IndexOf(item);
        }
    }
}
