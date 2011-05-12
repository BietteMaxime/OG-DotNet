//-----------------------------------------------------------------------
// <copyright file="PortfolioNode.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Position.Impl;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Position
{
    public class PortfolioNode : IUniqueIdentifiable
    {
        private readonly UniqueIdentifier _identifier;
        private readonly string _name;
        private readonly IList<PortfolioNode> _subNodes;
        private readonly IList<Position> _positions;

        public PortfolioNode(UniqueIdentifier identifier, string name, IList<PortfolioNode> subNodes, IList<Position> positions)
        {
            _identifier = identifier;
            _name = name;
            _subNodes = subNodes;
            _positions = positions;
        }

        public string Name
        {
            get { return _name; }
        }

        public IList<PortfolioNode> SubNodes
        {
            get { return _subNodes; }
        }

        public IList<Position> Positions
        {
            get { return _positions; }
        }

        public UniqueIdentifier UniqueId
        {
            get { return _identifier; }
        }

        public static PortfolioNode FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return PortfolioNodeImpl.FromFudgeMsg(ffc, deserializer);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}