using System.Collections.Generic;
using System.Windows.Controls;

namespace OGDotNet.WPFUtils
{
    public static class NonGenericCollectionsExtensionMethods
    {
        public static void AddRange(this GridViewColumnCollection collection, IEnumerable<GridViewColumn> columns)
        {
            foreach (var gridViewColumn in columns)
            {
                collection.Add(gridViewColumn);
            }
        }
    }
}
