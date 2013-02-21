// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimplePositionBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Core.Position;
using OpenGamma.Core.Position.Impl;
using OpenGamma.Id;

namespace OpenGamma.Fudge
{
    class SimplePositionBuilder : BuilderBase<IPosition>
    {
        public SimplePositionBuilder(FudgeContext context, Type type)
            : base(context, type)
        {
        }

        protected override IPosition DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var id = msg.GetValue<string>("uniqueId");
            var secKey = deserializer.FromField<ExternalIdBundle>(msg.GetByName("securityKey"));
            var quant = msg.GetValue<string>("quantity");
            var trades = deserializer.FromField<IList<ITrade>>(msg.GetByName("trades")) ?? new List<ITrade>();
            decimal quantity;
            if (!decimal.TryParse(quant, out quantity))
            {
                if (quant == "0E-8")
                {
                    quantity = 0;
                }
                else
                {
                    throw new OpenGammaException("Failed to parse quantity " + quant);
                }
            }

            return new SimplePosition(id == null ? null : UniqueId.Parse(id), quantity, secKey, trades);
        }
    }
}