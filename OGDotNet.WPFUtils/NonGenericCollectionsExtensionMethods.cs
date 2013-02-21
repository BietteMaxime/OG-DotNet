// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NonGenericCollectionsExtensionMethods.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
