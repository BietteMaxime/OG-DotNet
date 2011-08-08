//-----------------------------------------------------------------------
// <copyright file="ExternalIdBundle.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.Id
{
    [FudgeSurrogate(typeof(ExternalIdBundleBuilder))]
    public class ExternalIdBundle
    {
        private readonly HashSet<ExternalId> _identifiers;

        public ExternalIdBundle(params ExternalId[] identifiers) : this(new HashSet<ExternalId>(identifiers)) { }
        public ExternalIdBundle(IEnumerable<ExternalId> identifiers) : this(new HashSet<ExternalId>(identifiers)) { }

        private ExternalIdBundle(HashSet<ExternalId> identifiers)
        {
            _identifiers = identifiers;
        }

        public IEnumerable<ExternalId> Identifiers
        {
            get { return _identifiers; }
        }

        public override string ToString()
        {
            return new StringBuilder()
              .Append("Bundle")
              .Append("[")
              .Append(string.Join(", ", _identifiers))
              .Append("]")
              .ToString();
        }
    }
}