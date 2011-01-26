using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet
{
    /**
* Provides uniform access to objects that can supply a unique identifier.
* <p>
* This interface makes no guarantees about the thread-safety of implementations.
* However, wherever possible calls to this method should be thread-safe.
*/
    public class IdentifierBundle
    {
        private readonly HashSet<Identifier> _identifiers;

        private IdentifierBundle(HashSet<Identifier> identifiers)
        {
            _identifiers = identifiers;
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
    }
}