//-----------------------------------------------------------------------
// <copyright file="InterpolatedYieldCurveDefinitionMaster.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGDotNet.Mappedtypes.Financial.Analytics.IRCurve;
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
            var created = _restTarget.Resolve("definitions").Post<YieldCurveDefinitionDocument>(document);
            if (created.UniqueId == null)
            {
                throw new ArgumentException("No UID returned");
            }

            document.UniqueId = created.UniqueId;

            return document;
        }

        public YieldCurveDefinitionDocument AddOrUpdate(YieldCurveDefinitionDocument document)
        {
            return PostDefinition(document, "save");
        }

        private YieldCurveDefinitionDocument PostDefinition(YieldCurveDefinitionDocument document, string path)
        {
            var created = _restTarget.Resolve("definitions").Resolve("save").Post<YieldCurveDefinitionDocument>(document);
            if (created.UniqueId == null)
            {
                throw new ArgumentException("No UID returned");
            }

            document.UniqueId = created.UniqueId;

            return document;
        }

        public YieldCurveDefinitionDocument Get(UniqueId uniqueId)
        {
            var resp = _restTarget.Resolve("definitions", uniqueId.ToString()).Get<YieldCurveDefinitionDocument>();
            if (resp == null || resp.UniqueId == null || resp.YieldCurveDefinition == null)
            {
                throw new ArgumentException("Not found", "uniqueId");
            }
            return resp;
        }

        public void Remove(UniqueId uniqueId)
        {
            _restTarget.Resolve("definitions").Resolve(uniqueId.ToString()).Delete();
        }
        //TODO Update, correct
    }
}
