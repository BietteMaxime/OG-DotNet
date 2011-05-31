//-----------------------------------------------------------------------
// <copyright file="PositionBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.Core.Position.Impl;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Builders
{
    class PositionBuilder : BuilderBase<IPosition>
    {
        public PositionBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override IPosition DeserializeImpl(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var id = ffc.GetValue<string>("identifier");
            var secKey = deserializer.FromField<IdentifierBundle>(ffc.GetByName("securityKey"));
            var quant = ffc.GetValue<string>("quantity");

            return new PositionImpl(UniqueIdentifier.Parse(id), long.Parse(quant), secKey);
        }
    }
}
