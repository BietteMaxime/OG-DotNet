using System;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Mappedtypes.Id;


namespace OGDotNet.Model.Resources
{
    public class InterpolatedYieldCurveDefinitionMaster
    {
        private readonly RestTarget _restTarget;

        public InterpolatedYieldCurveDefinitionMaster(RestTarget restTarget)
        {
            _restTarget = restTarget;
        }

          public YieldCurveDefinitionDocument Add(YieldCurveDefinitionDocument document) {
            return PostDefinition(document, "add");
          }

          public YieldCurveDefinitionDocument AddOrUpdate(YieldCurveDefinitionDocument document)
          {
              return PostDefinition(document, "addOrUpdate");
          }

          private YieldCurveDefinitionDocument PostDefinition(YieldCurveDefinitionDocument document, string path)
        {
            var respMsg = _restTarget.Resolve(path).Post <UniqueIdentifier>(document.YieldCurveDefinition, "uniqueId");
            var uid = respMsg.UniqueId;
            if (uid == null)
            {
                throw new ArgumentException("No UID returned");
            }

            document.UniqueId = uid;

            return document;
        }
    }
}
