namespace OGDotNet_Analytics.Mappedtypes.Id
{
    public class Identifier
    {
        private readonly string _scheme;
        public string Scheme
        {
            get { return _scheme; }
        }

        private readonly string _value;
        public string Value
        {
            get { return _value; }
        }

        public Identifier(string scheme, string value)
        {
            _scheme = scheme;
            _value = value;
        }

        public override string ToString()
        {
            return string.Format("{0}::{1}", Scheme, Value);
        }
    }
}