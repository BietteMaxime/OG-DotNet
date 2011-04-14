//-----------------------------------------------------------------------
// <copyright file="Position.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Position
{
    public class Position
    {
        private readonly IdentifierBundle _securityKey;
        private readonly UniqueIdentifier _identifier;
        private readonly long _quantity;

        private Position(UniqueIdentifier identifier, long quantity, IdentifierBundle securityKey)
        {
            _securityKey = securityKey;
            _identifier = identifier;
            _quantity = quantity;
        }

        public IdentifierBundle SecurityKey
        {
            get { return _securityKey; }
        }

        public UniqueIdentifier Identifier
        {
            get { return _identifier; }
        }

        public long Quantity
        {
            get { return _quantity; }
        }

        public static Position FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var id = ffc.GetValue<string>("identifier");
            var secKey = deserializer.FromField<IdentifierBundle>(ffc.GetByName("securityKey"));
            var quant = ffc.GetValue<string>("quantity");

            return new Position(UniqueIdentifier.Parse(id), long.Parse(quant), secKey);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}