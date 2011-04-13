//-----------------------------------------------------------------------
// <copyright file="AddViewDefinitionRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.engine.view;

namespace OGDotNet.Mappedtypes.financial.view
{
    public class AddViewDefinitionRequest
    {
        private readonly ViewDefinition _viewDefinition;

        public AddViewDefinitionRequest(ViewDefinition viewDefinition)
        {
            _viewDefinition = viewDefinition;
        }

        public ViewDefinition ViewDefinition
        {
            get { return _viewDefinition; }
        }
    }
}