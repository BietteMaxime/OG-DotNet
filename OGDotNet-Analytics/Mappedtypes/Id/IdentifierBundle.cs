//-----------------------------------------------------------------------
// <copyright file="IdentifierBundle.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
    [FudgeSurrogate(typeof(IdentifierBundleBuilder))]
    public class IdentifierBundle
    {
        private readonly HashSet<Identifier> _identifiers;

        public IdentifierBundle(params Identifier[] identifiers) : this(new HashSet<Identifier>(identifiers)) { }
        public IdentifierBundle(IEnumerable<Identifier> identifiers) : this(new HashSet<Identifier>(identifiers)) { }

        private IdentifierBundle(HashSet<Identifier> identifiers)
        {
            _identifiers = identifiers;
        }

        public IEnumerable<Identifier> Identifiers
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