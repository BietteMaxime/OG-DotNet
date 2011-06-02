//-----------------------------------------------------------------------
// <copyright file="MissingLiveDataSentinel.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.engine.View.cache
{
    public class MissingLiveDataSentinel
    {
        public static readonly MissingLiveDataSentinel Instance = new MissingLiveDataSentinel();

        private MissingLiveDataSentinel()
        {
        }

        public static MissingLiveDataSentinel FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return Instance;
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteTypeHeader(a, typeof(MissingLiveDataSentinel));
        }
    }
}
