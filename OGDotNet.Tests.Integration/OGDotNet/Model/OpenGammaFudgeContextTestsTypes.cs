// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenGammaFudgeContextTestsTypes.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace OGDotNet.Mappedtypes
{
    public static class OpenGammaFudgeContextTestsTypes
    {
        [Serializable]
        public class A
        {
            public B B;
        }
        [Serializable]
        public class B
        {
            public C C;
        }
        [Serializable]
        public class C
        {
            public A A;
        }
    }
}