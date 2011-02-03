using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OGDotNet.WPFUtils;
using OGDotNet_Analytics.Mappedtypes.financial.analytics;
using OGDotNet_Analytics.Mappedtypes.financial.analytics.Volatility.Surface;
using OGDotNet_Analytics.Mappedtypes.financial.model.interestrate.curve;
using OGDotNet_Analytics.Utils;

namespace OGDotNet_AnalyticsViewer.View.CellTemplates
{
    /// <summary>
    /// This allows you to bind cell template according to type, late bound.
    /// Once the template has been selected this selector gets out of the way for this column, and the template doesn't change
    /// </summary>
    internal class CellTemplateSelector : DataTemplateSelector
    {
        private readonly string _column;
        private readonly GridViewColumn _gridViewColumn;

        private static readonly Dictionary<Type, Type> TemplateTypes = new Dictionary<Type, Type>
                                                                           {
                                                                               {typeof(YieldCurve), typeof(YieldCurveCell)},
                                                                               {typeof(DoubleLabelledMatrix1D), typeof(DoubleLabelledMatrixCell)},
                                                                               {typeof(VolatilitySurfaceData), typeof(VolatilitySurfaceCell)}
                                                                           };

        private static readonly Dictionary<Tuple<string, Type>, DataTemplate> TemplateCache = new Dictionary<Tuple<string, Type>, DataTemplate>();
        private static readonly Dictionary<Type, Func<object, string,object>> IndexerCache= new Dictionary<Type, Func<object, string, object>>();

        public CellTemplateSelector(string column, GridViewColumn gridViewColumn)
        {
            _column = column;
            _gridViewColumn = gridViewColumn;
        }

        private static Func<object, string,object> GetIndexer(Type t)
        {
            Func<object, string, object> ret;
            if (!IndexerCache.TryGetValue(t, out ret))
            {
                var indexerProperty = t.GetProperties().Where(p => p.GetIndexParameters().Length > 0).First();
                var getMethod = indexerProperty.GetGetMethod();
                
                ret = delegate(object item, string index)
                          {
                              return getMethod.Invoke(item, new object[] {index});
                          };
                IndexerCache.Add(t, ret);
            }
            return ret;
        }

        internal void UpdateNullTemplate(object item)
        {
            SetCellTemplate(item);
        }




        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!ReferenceEquals(this, _gridViewColumn.CellTemplateSelector))
                throw new ArgumentException("I'm about to twiddle the wrong selector");
            
            var cellValue = GetIndexer(item.GetType())(item, _column);
            if (cellValue == null)
            {
                var comboFactory = new FrameworkElementFactory(typeof(NullCell));
                comboFactory.SetValue(NullCell.CellTemplateSelectorProperty, this);
                comboFactory.SetValue(FrameworkElement.DataContextProperty, BindingUtils.GetIndexerBinding(_column));
                var itemsTemplate = new DataTemplate { VisualTree = comboFactory };
                return itemsTemplate;
            }

            SetCellTemplate(cellValue);
            return null;
        }

        private void SetCellTemplate(object cellValue)
        {
            var type = cellValue.GetType();

            var key = new Tuple<string, Type>(_column, type);

            DataTemplate ret;
            if (!TemplateCache.TryGetValue(key, out ret))
            {
                ret = BuildTemplate(_column, type);
                TemplateCache[key] = ret;
            }


            _gridViewColumn.CellTemplateSelector = null;
            _gridViewColumn.CellTemplate = ret;
        }


        private static DataTemplate BuildTemplate(string column, Type type)
        {
            Type templateType;
            if (TemplateTypes.TryGetValue(type, out templateType))
            {
                var comboFactory = new FrameworkElementFactory(templateType);
                comboFactory.SetValue(FrameworkElement.DataContextProperty, BindingUtils.GetIndexerBinding(column));  

                var itemsTemplate = new DataTemplate {VisualTree = comboFactory};

                return itemsTemplate;
            }
            else
            {
                var comboFactory = new FrameworkElementFactory(typeof(TextBlock));
                comboFactory.SetValue(TextBlock.TextProperty, BindingUtils.GetIndexerBinding(column));
                var itemsTemplate = new DataTemplate {VisualTree = comboFactory};
                return itemsTemplate;
            }
        }
    }
}