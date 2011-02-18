using System;
using System.IO;
using System.Net;
using System.Reflection;
using Fudge;
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

        public YieldCurveDefinitionDocument Add(YieldCurveDefinitionDocument document)
        {
                return PostDefinition(document, "add");
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

        public YieldCurveDefinitionDocument Get(UniqueIdentifier uniqueId)
        {
            var resp = _restTarget.Resolve("curves").Resolve(uniqueId.ToString()).Get<YieldCurveDefinitionDocument>();
            if (resp == null || resp.UniqueId == null || resp.Definition == null)
            {
                throw new ArgumentException("Not found","uniqueId");
            }
            return resp;
        }

        public void Remove(UniqueIdentifier uniqueId)
        {
            _restTarget.Resolve("curves").Resolve(uniqueId.ToString()).Delete();
        }
        //TODO Update, correct
    }
}
