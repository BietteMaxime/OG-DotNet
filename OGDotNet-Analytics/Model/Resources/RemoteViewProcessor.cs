using System.Collections.Generic;
using System.Linq;


namespace OGDotNet.Model.Resources
{
    public class RemoteViewProcessor
    {
        private readonly RestTarget _rest;
        private readonly string _activeMqSpec;

        public RemoteViewProcessor(RestTarget rest, string activeMqSpec)
        {
            _rest = rest;
            _activeMqSpec = activeMqSpec;
        }

        public IEnumerable<string> ViewNames
        {
            get
            {
                var fudgeMsg = _rest.Resolve("viewNames").GetReponse();

                return fudgeMsg.GetAllByOrdinal(1).Select(fudgeField => (string) fudgeField.Value);
            }
        }

        public RemoteView GetView(string viewName)
        {
            return new RemoteView(_rest.Resolve("views").Resolve(viewName), _activeMqSpec, viewName);
        }
    }
}