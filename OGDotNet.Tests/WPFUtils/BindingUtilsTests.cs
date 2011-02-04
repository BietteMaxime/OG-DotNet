using System;
using System.Windows.Controls;
using System.Windows.Data;
using NUnit.Framework;
using OGDotNet.WPFUtils;

namespace OGDotNet.Tests.WPFUtils
{

    [TestFixture]
    public class BindingUtilsTests
    {
        [Test]
        public void BindingModeIsOneWay()
        {
            var indexerBinding = BindingUtils.GetIndexerBinding("");
            Assert.AreEqual(BindingMode.OneWay, indexerBinding.Mode);
        }


        [Test]                           
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("Abba/FestFolk")]
        [TestCase("Abba\\FestFolk")]
        [TestCase("Abba/Fest Folk")]
        [TestCase("Abba/[FestFolk]")]
        [TestCase("Abba/[FestFolk")]
        [TestCase("Abba/[FestFolk]")]
        [TestCase("Abba/Fest%Folk")]
        [TestCase("Abba/Fest^Folk")]
        public void IndexValueTests(string indexer)
        {
            string boundValue = RoundTripBinding(indexer);
            Assert.AreEqual(indexer, boundValue);
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
            public string this[String s]
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
}

