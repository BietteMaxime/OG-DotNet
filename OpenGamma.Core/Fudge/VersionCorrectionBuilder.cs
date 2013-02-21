// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VersionCorrectionBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;
using Fudge.Types;

using OpenGamma.Id;
using OpenGamma.Util;

namespace OpenGamma.Fudge
{
    class VersionCorrectionBuilder : BuilderBase<VersionCorrection>
    {
        public VersionCorrectionBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override VersionCorrection DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var versionOf = ((FudgeDateTime)msg.GetValue("versionAsOf")).ToDateTimeOffsetWithDefault();
            var correctedTo = ((FudgeDateTime)msg.GetValue("correctedTo")).ToDateTimeOffsetWithDefault();
            return new VersionCorrection(versionOf, correctedTo);
        }

        protected override void SerializeImpl(VersionCorrection obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            var fudgeVersion = obj.VersionAsOf.ToFudgeDateTimeOffsetWithDefault();
            if (fudgeVersion != null)
                msg.Add("versionAsOf", fudgeVersion);
            var fudgeCorrection = obj.CorrectedTo.ToFudgeDateTimeOffsetWithDefault();
            if (fudgeCorrection != null)
            {
                msg.Add("correctedTo", fudgeCorrection);
            }
        }
    }
}
