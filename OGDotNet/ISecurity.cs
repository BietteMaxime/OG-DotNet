using System;
using Fudge;

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

    class Security : ISecurity
    {
        private readonly UniqueIdentifier _uniqueId;
        private readonly string _name;
        private readonly string _securityType;

        public Security(UniqueIdentifier uniqueId, string name, string securityType)
        {
            _uniqueId = uniqueId;
            _name = name;
            _securityType = securityType;
        }

        public UniqueIdentifier UniqueId
        {
            get { return _uniqueId; }
        }

        public string Name
        {
            get { return _name; }
        }

        public IdentifierBundle Identifiers
        {
            get { throw new NotImplementedException(); }//TODO
        }

        public string SecurityType
        {
            get { throw new NotImplementedException(); }//TODO
        }

        public const String NAME_KEY = "name";
        public const String SECURITY_TYPE_KEY = "securityType";
        public const String UNIQUE_ID_KEY = "uniqueId";
        public const String IDENTIFIERS_KEY = "identifiers";

        public static ISecurity FromFudgeMsg(FudgeMsg fudgeMsg)
        {
            string name = (string) fudgeMsg.GetByName(NAME_KEY).Value;
            string securityType = (string)fudgeMsg.GetByName(SECURITY_TYPE_KEY).Value;
            UniqueIdentifier uniqueId = UniqueIdentifier.FromFudgeMsg((FudgeMsg) fudgeMsg.GetByName(UNIQUE_ID_KEY).Value);
            return new Security(uniqueId, name, securityType);
        }
    }
}