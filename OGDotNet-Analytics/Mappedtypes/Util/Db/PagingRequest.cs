namespace OGDotNet.Mappedtypes.Util.Db
{
    public class PagingRequest
    {
        private readonly int _page;
        public int Page
        {
            get { return _page; }
        }

        private readonly int _pagingSize;
        public int PagingSize
        {
            get { return _pagingSize; }
        }

        public PagingRequest(int page, int pagingSize)
        {
            _page = page;
            _pagingSize = pagingSize;
        }
    }
}