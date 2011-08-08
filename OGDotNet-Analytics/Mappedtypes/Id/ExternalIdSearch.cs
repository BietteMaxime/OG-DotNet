//-----------------------------------------------------------------------
// <copyright file="ExternalIdSearch.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.Id
{
    public class ExternalIdSearch
    {
        private readonly IEnumerable<ExternalId> _identifiers;
        private readonly IdentifierSearchType _searchType;

        public ExternalIdSearch(IEnumerable<ExternalId> identifiers, IdentifierSearchType searchType)
        {
            _identifiers = identifiers;
            _searchType = searchType;
        }

        public IEnumerable<ExternalId> Identifiers
        {
            get { return _identifiers; }
        }

        public static ExternalIdSearch FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            throw new NotImplementedException();
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteInline(a, "identifiers", _identifiers.Select(id => id.ToString()).ToList());
            a.Add("searchType", EnumBuilder<IdentifierSearchType>.GetJavaName(_searchType));
        }
    }
}