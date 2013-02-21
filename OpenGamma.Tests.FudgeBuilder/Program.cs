// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenGamma
{
    /// <summary>
    /// A proxy which can be used by the BuilderTestProxyFactory.ExecBuilderTestProxy in the OG-Util Java project to
    /// test the serialization
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
