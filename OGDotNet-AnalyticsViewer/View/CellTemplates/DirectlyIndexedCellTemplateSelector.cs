//-----------------------------------------------------------------------
// <copyright file="DirectlyIndexedCellTemplateSelector.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using OGDotNet.Utils;
using OGDotNet.WPFUtils;

namespace OGDotNet.AnalyticsViewer.View.CellTemplates
{
    public class DirectlyIndexedCellTemplateSelector<T> : CellTemplateSelector
    {
        private readonly T _index;

        private static readonly Memoizer<Type, Func<object, T, object>> IndexerMemoizer = new Memoizer<Type, Func<object, T, object>>(BuildIndexer);

        public DirectlyIndexedCellTemplateSelector(T index, GridViewColumn gridViewColumn) : base(gridViewColumn)
        {
            _index = index;
        }

        protected override Binding Binding
        {
            get { return BindingUtils.GetIndexerBinding(_index); }
        }

        protected override object GetCellValue(object item)
        {
            return IndexerMemoizer.Get(item.GetType())(item, _index);
        }

        private static Func<object, T, object> BuildIndexer(Type t)
        {
            var indexerProperty = t.GetProperties().Where(p => p.GetIndexParameters().Length > 0).First();
            var getMethod = indexerProperty.GetGetMethod();

            return (item, index) => getMethod.Invoke(item, new object[] { index });
        }
    }
}