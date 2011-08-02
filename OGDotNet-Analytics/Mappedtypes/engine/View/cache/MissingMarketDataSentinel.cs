//-----------------------------------------------------------------------
// <copyright file="MissingMarketDataSentinel.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.Engine.View.Cache
{
    public class MissingMarketDataSentinel
    {
        public static readonly MissingMarketDataSentinel Instance = new MissingMarketDataSentinel();

        private MissingMarketDataSentinel()
        {
        }

        public static MissingMarketDataSentinel FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return Instance;
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteTypeHeader(a, typeof(MissingMarketDataSentinel));
        }
    }
}
