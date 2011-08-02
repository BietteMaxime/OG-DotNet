//-----------------------------------------------------------------------
// <copyright file="ViewDefinitionCompiledCall.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Engine.View.Compilation;

namespace OGDotNet.Mappedtypes.Engine.View.Listener
{
    public class ViewDefinitionCompiledCall
    {
        private readonly CompiledViewDefinitionImpl _compiledViewDefinition;

        public ViewDefinitionCompiledCall(CompiledViewDefinitionImpl compiledViewDefinition)
        {
            _compiledViewDefinition = compiledViewDefinition;
        }

        public CompiledViewDefinitionImpl CompiledViewDefinition
        {
            get { return _compiledViewDefinition; }
        }
    }
}
