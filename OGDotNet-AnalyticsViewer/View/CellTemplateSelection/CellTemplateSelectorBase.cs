//-----------------------------------------------------------------------
// <copyright file="CellTemplateSelectorBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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

namespace OGDotNet.AnalyticsViewer.View.CellTemplateSelection
{
    /// <summary>
    /// This allows you to bind cell template according to type of bound values, late bound.
    /// Once the template has been selected this selector gets out of the way for this column, and the template doesn't change
    /// </summary>
    public abstract class CellTemplateSelectorBase : DataTemplateSelector
    {
        internal void UpdateNullTemplate(object item)
        {
            SetCellTemplate(item);
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!ReferenceEquals(this, GetCurrentCellTemplateSelector()))
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

        protected abstract DataTemplateSelector GetCurrentCellTemplateSelector();
        protected abstract void SetCurrentCellTemplateSelector(DataTemplateSelector selector);
        protected abstract void SetCurrentCellTemplate(DataTemplate template);
        protected abstract Binding Binding { get; }

        /// <summary>
        /// NOTE: should be equivalent to evaluating the Binding on item, but may be faster
        /// </summary>
        protected abstract object GetCellValue(object item);

        private void SetCellTemplate(object cellValue)
        {
            var type = cellValue.GetType();

            var template = BuildIndexedTemplate(type);

            SetCurrentCellTemplateSelector(null);
            SetCurrentCellTemplate(template);
        }

        private static readonly Memoizer<Binding, Type, DataTemplate> DefaultIndexedTemplateMemoizer = new Memoizer<Binding, Type, DataTemplate>(BuildIndexedTemplateImpl);
        
        /// <summary>
        /// Virtual to allow templates to be embedded etc.
        /// NOTE: this needs to be fast
        /// </summary>
        protected virtual DataTemplate BuildIndexedTemplate(Type cellType)
        {
            return DefaultIndexedTemplateMemoizer.Get(Binding, cellType);
        }

        private static DataTemplate BuildIndexedTemplateImpl(Binding binding, Type cellType)
        {
            return TemplateTypeSelector.BuildBoundTemplate(binding, cellType);
        }
    }
}