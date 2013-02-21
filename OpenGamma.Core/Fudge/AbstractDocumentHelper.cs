// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractDocumentHelper.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Types;
using OpenGamma.Master;
using OpenGamma.Util;

namespace OpenGamma.Fudge
{
    internal static class AbstractDocumentHelper
    {
        public static void DeserializeVersionCorrection(IFudgeFieldContainer msg, out DateTimeOffset versionFromInstant, 
                                                        out DateTimeOffset versionToInstant, 
                                                        out DateTimeOffset correctionFromInstant, 
                                                        out DateTimeOffset correctionToInstant)
        {
            versionFromInstant = msg.GetValue<FudgeDateTime>("versionFromInstant").ToDateTimeOffsetWithDefault();
            correctionFromInstant = msg.GetValue<FudgeDateTime>("correctionFromInstant").ToDateTimeOffsetWithDefault();
            versionToInstant = msg.GetValue<FudgeDateTime>("versionToInstant").ToDateTimeOffsetWithDefault();
            correctionToInstant = msg.GetValue<FudgeDateTime>("correctionToInstant").ToDateTimeOffsetWithDefault();
        }

        public static void SerializeVersionCorrection(AbstractDocument doc, IAppendingFudgeFieldContainer msg)
        {
            AddDateTimeOffsetWithDefault(msg, "versionFromInstant", doc.VersionFromInstant);
            AddDateTimeOffsetWithDefault(msg, "correctionFromInstant", doc.CorrectionFromInstant);
            AddDateTimeOffsetWithDefault(msg, "versionToInstant", doc.VersionToInstant);
            AddDateTimeOffsetWithDefault(msg, "correctionToInstant", doc.CorrectionToInstant);
        }

        private static void AddDateTimeOffsetWithDefault(IAppendingFudgeFieldContainer msg, string fieldName, DateTimeOffset value)
        {
            if (value != default(DateTimeOffset))
            {
                msg.Add(fieldName, new FudgeDateTime(value));
            }
        }
    }
}