using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.engine.depGraph.DependencyGraph;

namespace OGDotNet.Mappedtypes.engine.View
{
    public class ResultModelDefinition
    {
        private readonly ResultOutputMode _aggregatePositionOutputMode;
        private readonly ResultOutputMode _positionOutputMode;
        private readonly ResultOutputMode _tradeOutputMode;
        private readonly ResultOutputMode _securityOutputMode;
        private readonly ResultOutputMode _primitiveOutputMode;

        public ResultModelDefinition(ResultOutputMode aggregatePositionOutputMode, ResultOutputMode positionOutputMode, ResultOutputMode tradeOutputMode,  ResultOutputMode securityOutputMode, ResultOutputMode primitiveOutputMode)
        {
            _aggregatePositionOutputMode = aggregatePositionOutputMode;
            _positionOutputMode = positionOutputMode;
            _tradeOutputMode = tradeOutputMode;
            _securityOutputMode = securityOutputMode;
            _primitiveOutputMode = primitiveOutputMode;
        }

        public ResultOutputMode AggregatePositionOutputMode
        {
            get { return _aggregatePositionOutputMode; }
        }

        public ResultOutputMode PositionOutputMode
        {
            get { return _positionOutputMode; }
        }

        public ResultOutputMode TradeOutputMode
        {
            get { return _tradeOutputMode; }
        }

        public ResultOutputMode SecurityOutputMode
        {
            get { return _securityOutputMode; }
        }

        public ResultOutputMode PrimitiveOutputMode
        {
            get { return _primitiveOutputMode; }
        }

        public static ResultModelDefinition FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new ResultModelDefinition(
                ResultOutputModeMethods.Parse(ffc.GetValue<string>("aggregatePositionOutputMode")),
                ResultOutputModeMethods.Parse(ffc.GetValue<string>("positionOutputMode")),
                ResultOutputModeMethods.Parse(ffc.GetValue<string>("tradeOutputMode")),
                ResultOutputModeMethods.Parse(ffc.GetValue<string>("securityOutputMode")),
                ResultOutputModeMethods.Parse(ffc.GetValue<string>("primitiveOutputMode"))
                );
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}