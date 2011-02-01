using System.Collections.Generic;

using OGDotNet_Analytics.Mappedtypes.Util.Db;

namespace OGDotNet_Analytics.Mappedtypes.Master
{
    public class AbstractSearchResult<TDocument> //where TDocument extends Document
    {
        public Paging Paging { get; set; }
        public IList<TDocument> Documents { get; set; }
    }
}