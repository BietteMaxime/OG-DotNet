//-----------------------------------------------------------------------
// <copyright file="UpdateViewDefinitionRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Engine.View;

namespace OGDotNet.Mappedtypes.financial.view
{
    public class UpdateViewDefinitionRequest
    {
        private readonly string _name;
        private readonly ViewDefinition _viewDefinition;

        public UpdateViewDefinitionRequest(string name, ViewDefinition viewDefinition)
        {
            _name = name;
            _viewDefinition = viewDefinition;
        }

        public string Name
        {
            get { return _name; }
        }

        public ViewDefinition ViewDefinition
        {
            get { return _viewDefinition; }
        }
    }
}