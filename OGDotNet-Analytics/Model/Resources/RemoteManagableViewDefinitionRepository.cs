//-----------------------------------------------------------------------
// <copyright file="RemoteManagableViewDefinitionRepository.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.financial.view;

namespace OGDotNet.Model.Resources
{
    public class RemoteManagableViewDefinitionRepository
    {
        private readonly RestTarget _rest;

        public RemoteManagableViewDefinitionRepository(RestTarget rest)
        {
            _rest = rest;
        }

        public void AddViewDefinition(AddViewDefinitionRequest request)
        {
            _rest.Resolve("viewDefinitions").Post(request);
        }

        public void UpdateViewDefinition(UpdateViewDefinitionRequest request)
        {
            _rest.Resolve("viewDefinitions").Resolve(request.Name).Put(request);
        }

        public void RemoveViewDefinition(string name)
        {
            _rest.Resolve("viewDefinitions").Resolve(name).Delete();
        }
    }
}