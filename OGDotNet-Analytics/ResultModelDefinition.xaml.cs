using System;
using System.Reflection.Emit;
using Fudge;
using Fudge.Serialization;
using OGDotNet_Analytics.MappedTypes.engine.depgraph.DependencyGraph;

namespace OGDotNet_Analytics
{
    public class ResultModelDefinition
    {
        public string AggregatePositionOutputMode { get; set; }
        public string PositionOutputMode { get; set; }
        public string TradeOutputMode { get; set; }
        public string SecurityOutputMode { get; set; }
        public string PrimitiveOutputMode { get; set; }
        //TODO shouldOutputResult
    }

    namespace MappedTypes.engine.depgraph.DependencyGraph
    {


        public enum ResultOutputMode
        {
            NONE, TERMINAL_OUTPUTS, ALL

            //TODO shouldOutputResult
        }
    }
}