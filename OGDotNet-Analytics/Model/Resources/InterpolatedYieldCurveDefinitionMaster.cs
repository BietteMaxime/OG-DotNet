using System;
using System.Net;
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
              try
              {
                  return PostDefinition(document, "add");
              }
              catch (WebException e)
              {
                  if (e.Status == WebExceptionStatus.ProtocolError) //TODO I'd like more from java land than this
                  {
                      throw new ArgumentException("Duplicate argument");
                  }
                  throw;
              }
          }

          public YieldCurveDefinitionDocument AddOrUpdate(YieldCurveDefinitionDocument document)
          {
              return PostDefinition(document, "addOrUpdate");
          }

          private YieldCurveDefinitionDocument PostDefinition(YieldCurveDefinitionDocument document, string path)
        {
            var respMsg = _restTarget.Resolve(path).Post<UniqueIdentifier>(document, "uniqueId");
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
