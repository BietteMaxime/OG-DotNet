//-----------------------------------------------------------------------
// <copyright file="CellTemplateSelector.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OGDotNet.Utils;

namespace OGDotNet.AnalyticsViewer.View.CellTemplates
{
    /// <summary>
    /// This allows you to bind cell template according to type of bound values, late bound.
    /// Once the template has been selected this selector gets out of the way for this column, and the template doesn't change
    /// </summary>
    public abstract class CellTemplateSelector : DataTemplateSelector
    {
        private readonly GridViewColumn _gridViewColumn;

        private static readonly Memoizer<Binding, Type, DataTemplate> TemplateMemoizer = new Memoizer<Binding, Type, DataTemplate>(BuildIndexedTemplate);

        protected CellTemplateSelector(GridViewColumn gridViewColumn)
        {
            _gridViewColumn = gridViewColumn;
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
                comboFactory.SetValue(FrameworkElement.DataContextProperty, Binding);
                var itemsTemplate = new DataTemplate { VisualTree = comboFactory };
                return itemsTemplate;
            }
            else
            {
                SetCellTemplate(cellValue);
                return null;
            }
        }

        protected abstract Binding Binding { get; }

        /// <summary>
        /// NOTE: should be equivalent to evaluating the Binding on item, but may be faster
        /// </summary>
        protected abstract object GetCellValue(object item);

        private void SetCellTemplate(object cellValue)
        {
            var type = cellValue.GetType();

            var template = TemplateMemoizer.Get(Binding, type);

            _gridViewColumn.CellTemplateSelector = null;
            _gridViewColumn.CellTemplate = template;
        }

        private static DataTemplate BuildIndexedTemplate(Binding binding, Type cellType)
        {
            return TemplateTypeSelector.BuildBoundTemplate(binding, cellType);
        }
    }
}