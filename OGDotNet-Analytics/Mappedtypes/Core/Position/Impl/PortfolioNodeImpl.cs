//-----------------------------------------------------------------------
// <copyright file="PortfolioNodeImpl.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Position.Impl
{
    internal class PortfolioNodeImpl : PortfolioNode
    {
        public PortfolioNodeImpl(UniqueIdentifier identifier, string name, IList<PortfolioNode> subNodes, IList<IPosition> positions) : base(identifier, name, subNodes, positions)
        {
        }

        public static new PortfolioNodeImpl FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new PortfolioNodeImpl(
                UniqueIdentifier.Parse(ffc.GetString("identifier")), ffc.GetString("name"), 
                deserializer.FromField<IList<PortfolioNode>>(ffc.GetByName("subNodes")) ?? new List<PortfolioNode>(),
                deserializer.FromField<IList<IPosition>>(ffc.GetByName("positions")) ?? new List<IPosition>());
        }

        public new void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
