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