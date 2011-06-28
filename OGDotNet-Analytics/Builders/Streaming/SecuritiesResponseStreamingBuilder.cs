//-----------------------------------------------------------------------
// <copyright file="SecuritiesResponseStreamingBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Model;
using OGDotNet.Model.Resources;

namespace OGDotNet.Builders.Streaming
{
    internal class SecuritiesResponseStreamingBuilder : StreamingFudgeBuilderBase<SecuritiesResponse>
    {
        protected override SecuritiesResponse Deserialize(OpenGammaFudgeContext context, IFudgeStreamReader stream, SerializationTypeMap typeMap)
        {
            var securities = new List<ISecurity>();
            while (stream.HasNext)
            {
                switch (stream.MoveNext())
                {
                    case FudgeStreamElement.MessageStart:
                        break;
                    case FudgeStreamElement.MessageEnd:
                        return new SecuritiesResponse(securities);
                    case FudgeStreamElement.SimpleField:
                        throw new ArgumentException();
                    case FudgeStreamElement.SubmessageFieldStart:
                        if (stream.FieldName == "security" && stream.FieldOrdinal == null)
                        {
                            var deserializeStandard = DeserializeStandard<ISecurity>(context, stream, typeMap);
                            securities.Add(deserializeStandard);
                        }
                        else
                        {
                            throw new ArgumentException();
                        }
                        break;
                    case FudgeStreamElement.SubmessageFieldEnd:
                        throw new ArgumentException();
                    default:
                        break; // Unknown
                }
            }
            throw new ArgumentException();
        }
    }
}