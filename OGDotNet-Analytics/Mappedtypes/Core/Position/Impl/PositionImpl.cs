//-----------------------------------------------------------------------
// <copyright file="PositionImpl.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Position.Impl
{
    class PositionImpl : Position
    {
        public PositionImpl(UniqueIdentifier identifier, long quantity, IdentifierBundle securityKey) : base(identifier, quantity, securityKey)
        {
        }

        public static new PositionImpl FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var id = ffc.GetValue<string>("identifier");
            var secKey = deserializer.FromField<IdentifierBundle>(ffc.GetByName("securityKey"));
            var quant = ffc.GetValue<string>("quantity");

            return new PositionImpl(UniqueIdentifier.Parse(id), long.Parse(quant), secKey);
        }

        public new void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
