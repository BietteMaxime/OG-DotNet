//-----------------------------------------------------------------------
// <copyright file="TradeImpl.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Position.Impl
{
    [FudgeSurrogate(typeof(TradeBuilder))]
    class TradeImpl : ITrade
    {
        private readonly UniqueIdentifier _uniqueId;
        private readonly UniqueIdentifier _parentPositionId;

        public TradeImpl(UniqueIdentifier uniqueId, UniqueIdentifier parentPositionId)
        {
            _uniqueId = uniqueId;
            _parentPositionId = parentPositionId;
        }

        public UniqueIdentifier ParentPositionId
        {
            get { return _parentPositionId; }
        }

        public UniqueIdentifier UniqueId
        {
            get { return _uniqueId; }
        }
    }
}
