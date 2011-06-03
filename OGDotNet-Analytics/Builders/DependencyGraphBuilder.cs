//-----------------------------------------------------------------------
// <copyright file="DependencyGraphBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.engine.depgraph;
using OGDotNet.Mappedtypes.engine.depGraph;

namespace OGDotNet.Builders
{
    class DependencyGraphBuilder : BuilderBase<IDependencyGraph>
    {
        public DependencyGraphBuilder(FudgeContext context, Type type)
            : base(context, type)
        {
        }

        public override IDependencyGraph DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            string calcConfigName = msg.GetString("calculationConfigurationName");
            var nodes = msg.GetAllByName("dependencyNode").Select(deserializer.FromField<DependencyNode>).ToList();

            List<IFudgeField> edgeConnections = msg.GetAllByName("edge").ToList();

            for (int i = 0; i < edgeConnections.Count; i += 2)
            {
                var from = Convert.ToInt32(edgeConnections[i].Value);
                var to = Convert.ToInt32(edgeConnections[i + 1].Value);
                DependencyNode inputNode = nodes[from];
                DependencyNode dependentNode = nodes[to];
                dependentNode.AddInputNode(inputNode);
            }

            return new DependencyGraph(calcConfigName, nodes);
        }
    }
}
