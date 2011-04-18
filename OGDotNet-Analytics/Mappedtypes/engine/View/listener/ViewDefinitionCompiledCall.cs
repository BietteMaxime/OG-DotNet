using OGDotNet.Mappedtypes.engine.View.compilation;

namespace OGDotNet.Mappedtypes.engine.View.listener
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
