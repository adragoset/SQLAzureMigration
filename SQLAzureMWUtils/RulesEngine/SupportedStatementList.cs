using System;
using System.Text;
using System.Collections;

namespace SQLAzureMWUtils
{
    public class SupportedStatementList : CollectionBase
    {
        public SupportedStatementList()
        {
        }

        public SupportedStatementList(SupportedStatement[] arr)
        {
            AddRange(arr);
        }

        public SupportedStatement this[int index]
        {
            get { return (SupportedStatement)InnerList[index]; }
        }

        public void Add(SupportedStatement item)
        {
            InnerList.Add(item);
        }

        public void AddRange(SupportedStatement[] items)
        {
            InnerList.AddRange(items);
        }

        public void Remove(SupportedStatement item)
        {
            InnerList.Remove(item);
        }

        public int IndexOf(SupportedStatement item)
        {
            return InnerList.IndexOf(item);
        }
    }
}
