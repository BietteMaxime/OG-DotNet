//-----------------------------------------------------------------------
// <copyright file="GenericUtilsTest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Utils;
using Xunit;

namespace OGDotNet.Tests.OGDotNet.Utils
{
    public class GenericUtilsTest
    {
        public class GenericOneArgType<TA>
        {
            private readonly TA _a;

            public GenericOneArgType(TA a)
            {
                _a = a;
            }

            public TA A
            {
                get { return _a; }
            }
        }

        public class GenericTwoArgType<TA, TB>
        {
            private readonly GenericOneArgType<TA> _a;
            private readonly GenericOneArgType<TB> _b;

            public GenericTwoArgType(TA a, TB b)
                : this(new GenericOneArgType<TA>(a), new GenericOneArgType<TB>(b))
            {
            }

            private GenericTwoArgType(GenericOneArgType<TA> a, GenericOneArgType<TB> b)
            {
                _a = a;
                _b = b;
            }

            public GenericOneArgType<TA> A
            {
                get { return _a; }
            }

            public GenericOneArgType<TB> B
            {
                get { return _b; }
            }
        }

        [Fact(Timeout = 10000)]
        public void TestCache()
        {
            for (int i = 0; i < 1000000; i++)
            {
                CallOneArg();
                CallTwoArg();
            }
        }

        [Fact]
        public void CallOneArg()
        {
            var x = new GenericOneArgType<string>("X");
            var obj = GenericUtils.Call(typeof(GenericUtilsTest), "GenericMethodOneArg", typeof(GenericOneArgType<>), x);
            Assert.IsType(typeof(GenericOneArgType<string>), obj);
            var y = (GenericOneArgType<string>)obj;
            Assert.Equal(x.A, y.A);
        }

        public static GenericTwoArgType<TA, TB> GenericMethodTwoArg<TA, TB>(GenericTwoArgType<TA, TB> a)
        {
            return a;
        }

        [Fact]
        public void CallTwoArg()
        {
            var x = new GenericTwoArgType<string, int>("X", 1);
            var obj = GenericUtils.Call(typeof(GenericUtilsTest), "GenericMethodTwoArg", typeof(GenericOneArgType<>), x);
            Assert.IsType(typeof(GenericTwoArgType<string, int>), obj);
            var y = (GenericTwoArgType<string, int>)obj;
            Assert.Equal(x.A, y.A);
            Assert.Equal(x.B, y.B);
        }

        public static GenericOneArgType<T> GenericMethodOneArg<T>(GenericOneArgType<T> a)
        {
            return a;
        }
    }
}
