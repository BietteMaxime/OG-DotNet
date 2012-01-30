//-----------------------------------------------------------------------
// <copyright file="FudgeListWrapperStreamingBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Util.FudgeMsg;
using OGDotNet.Model;

namespace OGDotNet.Builders.Streaming
{
    internal class FudgeListWrapperStreamingBuilder<T> : StreamingFudgeBuilderBase<FudgeListWrapper<T>>
    {
        protected override FudgeListWrapper<T> Deserialize(OpenGammaFudgeContext context, IFudgeStreamReader stream, SerializationTypeMap typeMap)
        {
            bool inList = false;
            var items = new List<T>();
            while (stream.HasNext)
            {
                switch (stream.MoveNext())
                {
                    case FudgeStreamElement.MessageStart:
                        break;
                    case FudgeStreamElement.MessageEnd:
                        return new FudgeListWrapper<T>(items);
                    case FudgeStreamElement.SimpleField:
                        if (stream.FieldName == null && stream.FieldOrdinal == 0)
                        {
                            continue;
                        }
                        throw new ArgumentException();
                    case FudgeStreamElement.SubmessageFieldStart:
                        if (stream.FieldName == "list" && stream.FieldOrdinal == null)
                        {
                            if (inList)
                            {
                                throw new ArgumentException();
                            }
                            inList = true;
                        }
                        else if (inList)
                        {
                            var deserializeStandard = DeserializeStandard<T>(context, stream, typeMap);
                            items.Add(deserializeStandard);
                        }
                        else
                        {
                            throw new ArgumentException();
                        }
                        break;
                    case FudgeStreamElement.SubmessageFieldEnd:
                        if (! inList)
                        {
                            throw new ArgumentException();
                        }
                        inList = false;
                        break;
                    default:
                        break; // Unknown
                }
            }
            throw new ArgumentException();
        }
    }
}