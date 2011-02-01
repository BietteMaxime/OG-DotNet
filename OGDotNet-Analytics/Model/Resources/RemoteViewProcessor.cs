using System.Collections.Generic;
using System.Linq;


namespace OGDotNet_Analytics.Model.Resources
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
                var fudgeMsg = _rest.GetSubMagic("viewNames").GetReponse();

                return fudgeMsg.GetAllByOrdinal(1).Select(fudgeField => (string) fudgeField.Value);
            }
        }

        public RemoteViewResource GetView(string viewName)
        {
            return new RemoteViewResource(_rest.GetSubMagic("views").GetSubMagic(viewName), _activeMqSpec);
        }
    }
}