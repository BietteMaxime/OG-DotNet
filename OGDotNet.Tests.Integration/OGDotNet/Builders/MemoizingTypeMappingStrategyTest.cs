//-----------------------------------------------------------------------
// <copyright file="MemoizingTypeMappingStrategyTest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Reflection;
using System.Text;
using Fudge.Serialization;
using OGDotNet.Builders;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Builders
{
    public class MemoizingTypeMappingStrategyTest
    {
        [Xunit.Extensions.Fact]
        public void StrategyIsntKeptAlive()
        {
            var memoizingTypeMappingStrategy = new MemoizingTypeMappingStrategy(new NullStrategy());
            var name = new StringBuilder("SomeString").ToString();
            Assert.NotSame(name, string.Intern(name));
            var type = memoizingTypeMappingStrategy.GetType(name);
            Assert.Null(type);

            var weakName = new WeakReference(name);
            name = null;
            GC.Collect();
            Assert.True(weakName.IsAlive);

            memoizingTypeMappingStrategy = null;
            Assert.Null(memoizingTypeMappingStrategy);
            
            GC.Collect();
            Assert.False(weakName.IsAlive);
        }

        [Xunit.Extensions.Fact]
        public void AssemblyLoadClears()
        {
            var memoizingTypeMappingStrategy = new MemoizingTypeMappingStrategy(new NullStrategy());
            var name = new StringBuilder("SomeString").ToString();
            Assert.NotSame(name, string.Intern(name));
            var type = memoizingTypeMappingStrategy.GetType(name);
            Assert.Null(type);

            var weakName = new WeakReference(name);
            name = null;
            GC.Collect();
            Assert.True(weakName.IsAlive);

            //Load some assembly
            foreach (var referencedAssembly in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            {
                Assembly.Load(referencedAssembly);
            }

            GC.Collect();
            Assert.False(weakName.IsAlive);
        }

        public class NullStrategy : IFudgeTypeMappingStrategy
        {
            public string GetName(Type type)
            {
                throw new NotImplementedException();
            }

            public Type GetType(string name)
            {
                return null;
            }
        }
    }
}
