// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SearchResultStreamingBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Id;
using OpenGamma.Master;
using OpenGamma.Model;
using OpenGamma.Util;

namespace OpenGamma.Fudge.Streaming
{
    internal class SearchResultStreamingBuilder<T> : StreamingFudgeBuilderBase<SearchResult<T>> where T : AbstractDocument
    {
        protected override SearchResult<T> Deserialize(OpenGammaFudgeContext context, IFudgeStreamReader stream, SerializationTypeMap typeMap)
        {
            bool inList = false;
            var items = new List<T>();
            Paging paging = null;
            VersionCorrection versionCorrection = null;
            while (stream.HasNext)
            {
                switch (stream.MoveNext())
                {
                    case FudgeStreamElement.MessageStart:
                        break;
                    case FudgeStreamElement.MessageEnd:
                        return new SearchResult<T>(paging, versionCorrection, items);
                    case FudgeStreamElement.SimpleField:
                        if (stream.FieldName == null && stream.FieldOrdinal == 0)
                        {
                            continue;
                        }

                        throw new ArgumentException();
                    case FudgeStreamElement.SubmessageFieldStart:
                        if (stream.FieldName == "paging" && stream.FieldOrdinal == null)
                        {
                            if (inList)
                            {
                                throw new ArgumentException();
                            }

                            paging = DeserializeStandard<Paging>(context, stream, typeMap);
                        }
                        else if (stream.FieldName == "versionCorrection" && stream.FieldOrdinal == null)
                        {
                            if (inList)
                            {
                                throw new ArgumentException();
                            }

                            versionCorrection = DeserializeStandard<VersionCorrection>(context, stream, typeMap);
                        }
                        else if (stream.FieldName == "documents" && stream.FieldOrdinal == null)
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
                        if (!inList)
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