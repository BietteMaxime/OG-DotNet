//-----------------------------------------------------------------------
// <copyright file="CellTemplateSelector.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OGDotNet.AnalyticsViewer.ViewModel;
using OGDotNet.Mappedtypes.financial.analytics;
using OGDotNet.Mappedtypes.financial.analytics.Volatility.Surface;
using OGDotNet.Mappedtypes.financial.model.interestrate.curve;
using OGDotNet.Mappedtypes.Util.Time;
using OGDotNet.Utils;
using OGDotNet.WPFUtils;

namespace OGDotNet.AnalyticsViewer.View.CellTemplates
{
    /// <summary>
    /// This allows you to bind cell template according to type, late bound.
    /// Once the template has been selected this selector gets out of the way for this column, and the template doesn't change
    /// </summary>
    internal class CellTemplateSelector : DataTemplateSelector
    {
        private readonly ColumnHeader _column;
        private readonly GridViewColumn _gridViewColumn;

        private static readonly IDictionary<Type, Type> TemplateTypes = new ConcurrentDictionary<Type, Type>(new Dictionary<Type, Type>
                                                                           {
                                                                               {typeof(YieldCurve), typeof(YieldCurveCell)},
                                                                               {typeof(VolatilitySurfaceData<Tenor, Tenor>), typeof(VolatilitySurfaceCell)},
                                                                               {typeof(ColumnHeader), typeof(HeaderCell)},
                                                                               {typeof(IEnumerable<LabelledMatrixEntry>), typeof(LabelledMatrix1DCell)},
                                                                           });

        private static readonly Memoizer<ColumnHeader, Type, DataTemplate> TemplateMemoizer = new Memoizer<ColumnHeader, Type, DataTemplate>(BuildIndexedTemplate);

        private static readonly Memoizer<Type, Func<object, ColumnHeader, object>> IndexerMemoizer = new Memoizer<Type, Func<object, ColumnHeader, object>>(BuildIndexer);

        public CellTemplateSelector(ColumnHeader column, GridViewColumn gridViewColumn)
        {
            _column = column;
            _gridViewColumn = gridViewColumn;
        }

        private static Func<object, ColumnHeader, object> BuildIndexer(Type t)
        {
            var indexerProperty = t.GetProperties().Where(p => p.GetIndexParameters().Length > 0).First();
            var getMethod = indexerProperty.GetGetMethod();

            return (item, index) => getMethod.Invoke(item, new object[] { index });
        }

        internal void UpdateNullTemplate(object item)
        {
            SetCellTemplate(item);
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!ReferenceEquals(this, _gridViewColumn.CellTemplateSelector))
                throw new ArgumentException("I'm about to twiddle the wrong selector");

            object cellValue = GetCellValue(item);
            if (cellValue == null)
            {//Let's late bind when we get a value
                var comboFactory = new FrameworkElementFactory(typeof(NullCell));
                comboFactory.SetValue(NullCell.CellTemplateSelectorProperty, this);
                comboFactory.SetValue(FrameworkElement.DataContextProperty, BindingUtils.GetIndexerBinding(_column));
                var itemsTemplate = new DataTemplate { VisualTree = comboFactory };
                return itemsTemplate;
            }
            else
            {
                SetCellTemplate(cellValue);
                return null;
            }
        }

        private object GetCellValue(object item)
        {
            return IndexerMemoizer.Get(item.GetType())(item, _column);
        }

        private void SetCellTemplate(object cellValue)
        {
            var type = cellValue.GetType();

            var template = TemplateMemoizer.Get(_column, type);

            _gridViewColumn.CellTemplateSelector = null;
            _gridViewColumn.CellTemplate = template;
        }

        private static DataTemplate BuildIndexedTemplate(object indexer, Type cellType)
        {
            return BuildTemplate(BindingUtils.GetIndexerBinding(indexer), cellType);
        }
        public static DataTemplate BuildTemplate(object context, Type cellType)
        {
            Type templateType;
            if (TryGetTemplate(cellType, out templateType))
            {
                var comboFactory = new FrameworkElementFactory(templateType);
                comboFactory.SetValue(FrameworkElement.DataContextProperty, context);

                var itemsTemplate = new DataTemplate { VisualTree = comboFactory };

                return itemsTemplate;
            }
            else
            {
                var comboFactory = new FrameworkElementFactory(typeof(TextBlock));
                comboFactory.SetValue(TextBlock.TextProperty, context);
                var itemsTemplate = new DataTemplate { VisualTree = comboFactory };
                return itemsTemplate;
            }
        }

        private static bool TryGetTemplate(Type cellType, out Type templateType)
        {
            if (TemplateTypes.TryGetValue(cellType, out templateType))
            {
                return templateType != null;
            }
            foreach (var type in TemplateTypes)
            {
                if (type.Key.IsAssignableFrom(cellType))
                {
                    TemplateTypes[cellType] = type.Value;
                    templateType = type.Value;
                    return true;
                }
            }
            TemplateTypes[cellType] = null;
            return false;
        }
    }
}