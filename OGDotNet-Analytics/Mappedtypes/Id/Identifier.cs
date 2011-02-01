using System;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet_Analytics.Mappedtypes.Id
{
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
            return string.Format("{0}::{1}", Scheme, Value);
        }
    }
}