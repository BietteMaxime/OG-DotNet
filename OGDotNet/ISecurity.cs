using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet
{
    public interface ISecurity : IUniqueIdentifiable
    {

        /**
         * Gets the name of the security intended for display purposes.
         * 
         * @return the name, not null
         */
        String Name { get; }


        /**
         * Gets the bundle of identifiers that define the security.
         * 
         * @return the identifiers defining the security, not null
         */
        // 
        IdentifierBundle Identifiers { get; }


        /**
         * Gets the text-based type of this security.
         * 
         * @return the text-based type of this security
         */
        String SecurityType { get; }

    }

    public class Security : ISecurity
    {
        public string Name { get; set; }
        public string SecurityType { get; set; }
        public UniqueIdentifier UniqueId { get; set; }
        public IdentifierBundle Identifiers { get; set; }

        public override string ToString()
        {
            return SecurityType + ": " + Name;
        }
    }

    public class Identifier
    {
        public string Scheme { get; set; }
        public string Value { get; set; }

        public static Identifier FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var r = new Identifier();

            foreach (var field in ffc.GetAllFields())
            {
                switch (field.Name)
                {
                    case "Scheme":
                        r.Scheme = (string) field.Value;
                        break;
                    case "Value":
                        r.Value= (string)field.Value;
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            return r;
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("Scheme", Scheme);
            a.Add("Value", Value);
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Scheme, Value);
        }
    }
}