using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OGDotNet_Analytics.Mappedtypes.financial.model.interestrate.curve;

namespace OGDotNet_Analytics
{
    public class PrimitiveCellTemplateSelector : DataTemplateSelector
    {
        private readonly string _column;

        private static readonly Dictionary<Tuple<string, Type>, DataTemplate> TemplateCache = new Dictionary<Tuple<string, Type>, DataTemplate>();

        public PrimitiveCellTemplateSelector(string column)
        {
            _column = column;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            var column = _column;
            var cellValue = ((MainWindow.PrimitiveRow)item)[column];
            var type = cellValue == null ? typeof(Object) : cellValue.GetType();

            var key = new Tuple<string, Type>(column, type);

            DataTemplate ret;
            if (! TemplateCache.TryGetValue(key, out ret))
            {
                ret = BuildTemplate(column, type);
                TemplateCache[key] = ret;
            }
            return ret;
        }

        private static DataTemplate BuildTemplate(string column, Type type)
        {
            var binding = new Binding(String.Format(".[{0}]", column));//TODO bugs galore
                

                

            if (typeof(YieldCurve).IsAssignableFrom(type) )
            {
                var comboFactory = new FrameworkElementFactory(typeof(YieldCurveCell));
                comboFactory.SetValue(FrameworkElement.DataContextProperty, binding);  

                var itemsTemplate = new DataTemplate {VisualTree = comboFactory};

                return itemsTemplate;
            }
            else
            {
                var comboFactory = new FrameworkElementFactory(typeof(TextBlock));
                comboFactory.SetValue(TextBlock.TextProperty, binding);
                var itemsTemplate = new DataTemplate {VisualTree = comboFactory};
                return itemsTemplate;
            }
        }
    }
}