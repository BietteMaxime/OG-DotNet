//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

namespace OGDotNet.Tests.ExecBuilderTestProxy
{
    /// <summary>
    /// A proxy which can be used by com.opengamma.engine.fudgemsg.BuilderTestProxyFactory.ExecBuilderTestProxy in order to test the serialization
    /// TODO DOTNET-6: make this get run as part of the build
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    TestNGRunner.CallTestNG();
                    return;
                case 1:
                    string typeHint = args[0];

                    Proxy.CycleInputToOutput(typeHint);
                    return;
            }
        }
    }
}
