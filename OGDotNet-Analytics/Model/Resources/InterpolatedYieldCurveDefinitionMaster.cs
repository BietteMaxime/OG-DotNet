using System;
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

          public YieldCurveDefinitionDocument Add(YieldCurveDefinitionDocument document) {
            return PostDefinition(document, "add");
          }

          public YieldCurveDefinitionDocument AddOrUpdate(YieldCurveDefinitionDocument document)
          {
              return PostDefinition(document, "addOrUpdate");
          }

          private YieldCurveDefinitionDocument PostDefinition(YieldCurveDefinitionDocument document, string path)
        {
            var fudgeSerializer = FudgeConfig.GetFudgeSerializer();
            var msg = fudgeSerializer.SerializeToMsg(document.YieldCurveDefinition);
            FudgeMsg reqMsg = new FudgeMsg(new Field("definition", msg));
            var respMsg = _restTarget.Resolve(path).Post(FudgeContext, reqMsg);

            UniqueIdentifier uid = fudgeSerializer.Deserialize<UniqueIdentifier>((FudgeMsg)respMsg.GetMessage("uniqueId"));
            if (uid == null)
            {
                throw new ArgumentException("No UID returned");
            }

            document.UniqueId = uid;

            return document;
        }

        private static FudgeContext FudgeContext
        {
            get
            {
                return FudgeConfig.GetFudgeContext();
            }
        }
    }
}
