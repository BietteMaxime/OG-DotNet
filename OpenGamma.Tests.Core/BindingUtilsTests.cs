// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BindingUtilsTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Windows.Controls;
using System.Windows.Data;

using Xunit;
using Xunit.Extensions;

namespace OpenGamma.WPFUtils
{
    /* TODO jonathan 2012-12-21 -- this does not belong here - move to a test assembly for WPFUtils
     * 
    public class BindingUtilsTests
    {
        [Fact]
        public void BindingModeIsOneWay()
        {
            var indexerBinding = BindingUtils.GetIndexerBinding(string.Empty);
            Assert.Equal(BindingMode.OneWay, indexerBinding.Mode);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("Abba/FestFolk")]
        [InlineData("Abba\\FestFolk")]
        [InlineData("Abba/Fest Folk")]
        [InlineData("Abba/[FestFolk]")]
        [InlineData("Abba/[FestFolk")]
        [InlineData("Abba/[FestFolk]")]
        [InlineData("Abba/Fest%Folk")]
        [InlineData("Abba/Fest^Folk")]
        [InlineData("Abba/Fest[[[^]]]Folk")]
        public void IndexValueTests(string indexer)
        {
            string boundValue = RoundTripBinding(indexer);
            Assert.Equal(indexer, boundValue);
        }

        private static string RoundTripBinding(string indexer)
        {
            var indexerBinding = BindingUtils.GetIndexerBinding(indexer);

            var boundClass = new BoundClass {DataContext = new ReflectingIndexer()};
            boundClass.SetBinding(TextBlock.TextProperty, indexerBinding);

            return boundClass.BoundValue;
        }

        public class ReflectingIndexer
        {
            public string this[string s]
            {
                get { return s; }
            }
        }
        public class BoundClass : TextBlock
        {
            public string BoundValue
            {
                get
                {
                    return (string)GetValue(TextProperty);
                }
            }
        }
    }
     */
}
