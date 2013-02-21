// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BindingUtils.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Data;

namespace OGDotNet.WPFUtils
{
    public static class BindingUtils
    {
        public static Binding GetIndexerBinding(object index)
        {
            return new Binding{Mode = BindingMode.OneWay, Path = new PropertyPath(".[(0)]", index)};
        }
    }
}
