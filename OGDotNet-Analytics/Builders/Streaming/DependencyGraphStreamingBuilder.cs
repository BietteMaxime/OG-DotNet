//-----------------------------------------------------------------------
// <copyright file="DependencyGraphStreamingBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet.Mappedtypes.engine.depgraph;
using OGDotNet.Mappedtypes.engine.depGraph;
using OGDotNet.Model;

namespace OGDotNet.Builders.Streaming
{
    /// <summary>
    /// NOTE: IDependencyGraph has to be streamed because it gets big (LAP-43) 
    /// </summary>
    internal class DependencyGraphStreamingBuilder : StreamingFudgeBuilderBase<IDependencyGraph>
    {
        protected override IDependencyGraph Deserialize(OpenGammaFudgeContext context, IFudgeStreamReader stream, SerializationTypeMap typeMap)
        {
            string calcConfigName = null;
            var nodes = new List<DependencyNode>();

            int edgeFrom = -1;

            while (stream.HasNext)
            {
                switch (stream.MoveNext())
                {
                    case FudgeStreamElement.MessageStart:
                        break;
                    case FudgeStreamElement.MessageEnd:
                        return new DependencyGraph(calcConfigName, nodes);

                    case FudgeStreamElement.SimpleField:
                         switch (stream.FieldName)
                         {
                             case "edge":
                                 var int32 = Convert.ToInt32(stream.FieldValue);

                                 if (edgeFrom < 0)
                                 {
                                     edgeFrom = int32;
                                 }
                                 else
                                 {
                                     int to = int32;

                                     DependencyNode inputNode = nodes[edgeFrom];
                                     DependencyNode dependentNode = nodes[to];
                                     dependentNode.AddInputNode(inputNode);
                                     edgeFrom = -1;
                                 }
                                 break;
                             case "calculationConfigurationName":
                                 if (calcConfigName != null)
                                 {
                                     throw new ArgumentException();
                                 }
                                 calcConfigName = (string) stream.FieldValue;
                                 break;
                             default:
                                 if (stream.FieldOrdinal == 0 && stream.FieldType == StringFieldType.Instance)
                                 {
                                     break;
                                 }
                                 throw new ArgumentException();
                         }
                         break;
                    case FudgeStreamElement.SubmessageFieldStart:
                        if (stream.FieldName == "dependencyNode" && stream.FieldOrdinal == null)
                        {
                            var deserializeStandard = DeserializeStandard<DependencyNode>(context, stream, typeMap);
                            nodes.Add(deserializeStandard);
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