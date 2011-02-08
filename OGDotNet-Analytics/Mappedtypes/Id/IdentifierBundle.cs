using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.Id
{
    public class IdentifierBundle : IEquatable<IdentifierBundle>
    {
        private readonly HashSet<Identifier> _identifiers;

        public IdentifierBundle(HashSet<Identifier> identifiers)
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
                        var i = (Identifier) deserializer.FromField(field, typeof (Identifier));
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

        #region Equality
        public bool Equals(IdentifierBundle other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return _identifiers.SetEquals(other._identifiers);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (IdentifierBundle)) return false;
            return Equals((IdentifierBundle) obj);
        }

        public override int GetHashCode()
        {
            return _identifiers.OrderBy(i => i).Aggregate(0, (a,i)=> (a * 397) ^ i.GetHashCode() );
        }

        public static bool operator ==(IdentifierBundle left, IdentifierBundle right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(IdentifierBundle left, IdentifierBundle right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}