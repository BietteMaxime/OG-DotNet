//-----------------------------------------------------------------------
// <copyright file="TemplateTypeSelector.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OGDotNet.AnalyticsViewer.View.CellTemplates;
using OGDotNet.AnalyticsViewer.ViewModel;
using OGDotNet.Mappedtypes.Analytics.Financial.Model.Interestrate.Curve;
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Financial.Analytics;
using OGDotNet.Mappedtypes.Util.Money;
using OGDotNet.Mappedtypes.Util.Time;

namespace OGDotNet.AnalyticsViewer.View.CellTemplateSelection
{
    public class TemplateTypeSelector
    {
        private static readonly IDictionary<Type, Type> TemplateTypes = new ConcurrentDictionary<Type, Type>(new Dictionary<Type, Type>
                                                                                                                 {
                                                                                                                     {typeof(YieldCurve), typeof(YieldCurveCell)},
                                                                                                                     {typeof(VolatilitySurfaceData<Tenor, Tenor>), typeof(VolatilitySurfaceCell)},
                                                                                                                     {typeof(VolatilitySurfaceData<,>), typeof(GenericVolatilitySurfaceCell)},
                                                                                                                     {typeof(ColumnHeader), typeof(HeaderCell)},
                                                                                                                     {typeof(IEnumerable<LabelledMatrixEntry>), typeof(LabelledMatrix1DCell)},
                                                                                                                     {typeof(MultipleCurrencyAmount), typeof(MultipleCurrencyAmountCell)},
                                                                                                                     {typeof(IEnumerable<LabelledMatrixEntry2D>), typeof(LabelledMatrix2DCell)},
                                                                                                                 });

        public static bool TryGetTemplate(Type cellType, out Type templateType)
        {
            if (TemplateTypes.TryGetValue(cellType, out templateType))
            {
                return templateType != null;
            }

            if (cellType.IsGenericType)
            {
                Type genericTypeDefinition = cellType.GetGenericTypeDefinition();
                if (TemplateTypes.TryGetValue(genericTypeDefinition, out templateType))
                {
                    if (templateType != null)
                    {
                        return true;
                    }
                }
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

        public static DataTemplate BuildTemplate(object context, Type cellType)
        {
            Type templateType;
            if (TryGetTemplate(cellType, out templateType))
            {
                var comboFactory = new FrameworkElementFactory(templateType);
                comboFactory.SetValue(FrameworkElement.DataContextProperty, context);

                return new DataTemplate { VisualTree = comboFactory };
            }
            else
            {
                var comboFactory = new FrameworkElementFactory(typeof(TextBlock));
                comboFactory.SetValue(TextBlock.TextProperty, context);
                return new DataTemplate { VisualTree = comboFactory };
            }
        }

        public static DataTemplate BuildBoundTemplate(Binding binding, Type cellType)
        {
            return BuildTemplate(binding, cellType);
        }
    }
}