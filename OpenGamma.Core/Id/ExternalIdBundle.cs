// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExternalIdBundle.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;

using Fudge.Serialization;

using OpenGamma.Fudge;

namespace OpenGamma.Id
{
    [FudgeSurrogate(typeof(ExternalIdBundleBuilder))]
    public class ExternalIdBundle
    {
        private readonly HashSet<ExternalId> _identifiers;

        public ExternalIdBundle(params ExternalId[] identifiers) : this(new HashSet<ExternalId>(identifiers))
        {
        }
        public ExternalIdBundle(IEnumerable<ExternalId> identifiers) : this(new HashSet<ExternalId>(identifiers))
        {
        }

        private ExternalIdBundle(HashSet<ExternalId> identifiers)
        {
            _identifiers = identifiers;
        }

        public IEnumerable<ExternalId> Identifiers
        {
            get { return _identifiers; }
        }

        public bool IsEmpty()
        {
            return _identifiers.Count == 0;
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

        public static ExternalIdBundle Empty()
        {
            return new ExternalIdBundle();
        }

        public static ExternalIdBundle Create(ExternalId externalId)
        {
            return new ExternalIdBundle(externalId);
        }

        public static ExternalIdBundle Create(string scheme, string value)
        {
            return Create(ExternalId.Create(scheme, value));
        }
    }
}