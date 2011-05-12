using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Position.Impl
{
    class TradeImpl : Trade
    {
        private readonly UniqueIdentifier _parentPositionId;

        public TradeImpl(UniqueIdentifier parentPositionId)
        {
            _parentPositionId = parentPositionId;
        }

        public override UniqueIdentifier ParentPositionId
        {
            get { return _parentPositionId; }
        }

        public static TradeImpl FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new TradeImpl(UniqueIdentifier.Parse(ffc.GetString("parentPositionId")));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
