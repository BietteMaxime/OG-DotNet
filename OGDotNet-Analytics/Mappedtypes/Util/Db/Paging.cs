using System;

namespace OGDotNet.Mappedtypes.Util.Db
{
    [Serializable]
    public class Paging
    {

        public int Page;
        public int PagingSize;
        public int TotalItems;

        public int CurrentPage
        {
            get{return Page;}
        }
        public int Pages
        {
            get { return ((TotalItems -1)/PagingSize) + 1; }
        }
    }
}