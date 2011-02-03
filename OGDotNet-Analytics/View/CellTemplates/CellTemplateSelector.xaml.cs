using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OGDotNet_Analytics.Mappedtypes.financial.analytics;
using OGDotNet_Analytics.Mappedtypes.financial.analytics.Volatility.Surface;
using OGDotNet_Analytics.Mappedtypes.financial.model.interestrate.curve;
using OGDotNet_Analytics.Utils;

namespace OGDotNet_Analytics.View.CellTemplates
{
    internal class CellTemplateSelector : DataTemplateSelector
    {
        private readonly string _column;

        private static readonly Dictionary<Type, Type> TemplateTypes = new Dictionary<Type, Type>
                                                                           {
                                                                               {typeof(YieldCurve), typeof(YieldCurveCell)},
                                                                               {typeof(DoubleLabelledMatrix1D), typeof(DoubleLabelledMatrixCell)},
                                                                               {typeof(VolatilitySurfaceData), typeof(VolatilitySurfaceCell)}
                                                                           };

        private static readonly Dictionary<Tuple<string, Type>, DataTemplate> TemplateCache = new Dictionary<Tuple<string, Type>, DataTemplate>();
        private static readonly Dictionary<Type, Func<object, string,object>> IndexerCache= new Dictionary<Type, Func<object, string, object>>();

        public CellTemplateSelector(string column)
        {
            _column = column;
        }

        public static Func<object, string,object> GetIndexer(Type t)
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
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var cellValue = GetIndexer(item.GetType())(item, _column);
            var type = cellValue == null ? typeof(Object) : cellValue.GetType();

            var key = new Tuple<string, Type>(_column, type);

            DataTemplate ret;
            if (! TemplateCache.TryGetValue(key, out ret))
            {
                ret = BuildTemplate(_column, type);
                TemplateCache[key] = ret;
            }
            return ret;
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