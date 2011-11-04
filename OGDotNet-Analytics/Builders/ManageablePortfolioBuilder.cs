//-----------------------------------------------------------------------
// <copyright file="ManageablePortfolioBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master.Portfolio;

namespace OGDotNet.Builders
{
    class ManageablePortfolioBuilder : BuilderBase<ManageablePortfolio>
    {
        public ManageablePortfolioBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override ManageablePortfolio DeserializeImpl(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new ManageablePortfolio(ffc.GetString("name"), UniqueId.Parse(ffc.GetString("uniqueId")));
        }
    }
}
