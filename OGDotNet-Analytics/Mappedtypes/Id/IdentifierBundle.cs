//-----------------------------------------------------------------------
// <copyright file="IdentifierBundle.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.Id
{
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

        public static IdentifierBundle FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var identifiers = new HashSet<Identifier>();

            foreach (var field in ffc.GetAllFields())
            {
                switch (field.Name)
                {
                    case "ID":
                        var i = (Identifier)deserializer.FromField(field, typeof(Identifier));
                        identifiers.Add(i);
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            return new IdentifierBundle(identifiers);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            foreach (var identifier in _identifiers)
            {
                a.Add("ID", identifier);
            }
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