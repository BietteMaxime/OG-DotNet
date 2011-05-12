//-----------------------------------------------------------------------
// <copyright file="TradeImpl.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
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
        private readonly UniqueIdentifier _uniqueId;
        private readonly UniqueIdentifier _parentPositionId;

        public TradeImpl(UniqueIdentifier uniqueId, UniqueIdentifier parentPositionId)
        {
            _uniqueId = uniqueId;
            _parentPositionId = parentPositionId;
        }

        public override UniqueIdentifier ParentPositionId
        {
            get { return _parentPositionId; }
        }

        public override UniqueIdentifier UniqueId
        {
            get { return _uniqueId; }
        }

        public static TradeImpl FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new TradeImpl(UniqueIdentifier.Parse(ffc.GetString("uniqueId")), UniqueIdentifier.Parse(ffc.GetString("parentPositionId")));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
