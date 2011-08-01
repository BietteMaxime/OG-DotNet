//-----------------------------------------------------------------------
// <copyright file="ParameterizedFunction.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

namespace OGDotNet.Mappedtypes.Engine.function
{
    public class ParameterizedFunction
    {
        private readonly ICompiledFunctionDefinition _function;
        private readonly IFunctionParameters _parameters;
        private readonly string _uniqueId;

        public ParameterizedFunction(ICompiledFunctionDefinition function, IFunctionParameters parameters, string uniqueId)
        {
            _function = function;
            _parameters = parameters;
            _uniqueId = uniqueId;
        }

        public ICompiledFunctionDefinition Function
        {
            get { return _function; }
        }

        public IFunctionParameters Parameters
        {
            get { return _parameters; }
        }

        public string UniqueId
        {
            get { return _uniqueId; }
        }
    }
}
