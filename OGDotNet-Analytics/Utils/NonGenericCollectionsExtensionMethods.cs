using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace OGDotNet_Analytics.Utils
{
    static class NonGenericCollectionsExtensionMethods
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
