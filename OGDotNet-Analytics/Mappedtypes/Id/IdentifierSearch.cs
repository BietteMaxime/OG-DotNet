//-----------------------------------------------------------------------
// <copyright file="IdentifierSearch.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.Id
{
    public class IdentifierSearch
    {
        private readonly List<Identifier> _identifiers;
        private readonly IdentifierSearchType _searchType;

        public IdentifierSearch(List<Identifier> identifiers, IdentifierSearchType searchType)
        {
            _identifiers = identifiers;
            _searchType = searchType;
        }

        public List<Identifier> Identifiers
        {
            get { return _identifiers; }
        }

        public static IdentifierSearch FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            throw new NotImplementedException();
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            foreach (var identifier in _identifiers)
            {
                a.Add("identifier", identifier);
            }
            a.Add("searchType", _searchType.ToString());
        }
    }
}