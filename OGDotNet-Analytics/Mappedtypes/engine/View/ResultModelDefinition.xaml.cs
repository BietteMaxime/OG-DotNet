//-----------------------------------------------------------------------
// <copyright file="ResultModelDefinition.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
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

        public ResultModelDefinition() : this(ResultOutputMode.TerminalOutputs)
        {
        }

        public ResultModelDefinition(ResultOutputMode defaultMode) : this(defaultMode, defaultMode, defaultMode, defaultMode, defaultMode)
        {
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
                EnumBuilder<ResultOutputMode>.Parse(ffc.GetValue<string>("aggregatePositionOutputMode")),
                EnumBuilder<ResultOutputMode>.Parse(ffc.GetValue<string>("positionOutputMode")),
                EnumBuilder<ResultOutputMode>.Parse(ffc.GetValue<string>("tradeOutputMode")),
                EnumBuilder<ResultOutputMode>.Parse(ffc.GetValue<string>("securityOutputMode")),
                EnumBuilder<ResultOutputMode>.Parse(ffc.GetValue<string>("primitiveOutputMode"))
                );
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("aggregatePositionOutputMode",EnumBuilder<ResultOutputMode>.GetJavaName(AggregatePositionOutputMode));
            a.Add("positionOutputMode", EnumBuilder<ResultOutputMode>.GetJavaName(PositionOutputMode));
            a.Add("tradeOutputMode", EnumBuilder<ResultOutputMode>.GetJavaName(TradeOutputMode));
            a.Add("securityOutputMode", EnumBuilder<ResultOutputMode>.GetJavaName(SecurityOutputMode));
            a.Add("primitiveOutputMode", EnumBuilder<ResultOutputMode>.GetJavaName(PrimitiveOutputMode));
        }
    }
}