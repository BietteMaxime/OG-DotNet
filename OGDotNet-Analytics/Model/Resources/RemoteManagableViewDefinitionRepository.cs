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
    }
}