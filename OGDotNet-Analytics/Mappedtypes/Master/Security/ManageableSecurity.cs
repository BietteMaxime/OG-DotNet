//-----------------------------------------------------------------------
// <copyright file="ManageableSecurity.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Master.Security
{
    /// <summary>
    /// TODO DOTNET-5: kill these with the .proto replacement
    /// </summary>
    public class ManageableSecurity : ISecurity
    {
        private readonly string _name;
        private readonly string _securityType;
        private readonly UniqueIdentifier _uniqueId;
        private readonly IdentifierBundle _identifiers;

        protected ManageableSecurity(string name, string securityType, UniqueIdentifier uniqueId, IdentifierBundle identifiers)
        {
            _name = name;
            _securityType = securityType;
            _uniqueId = uniqueId;
            _identifiers = identifiers;
        }

        public string Name
        {
            get { return _name; }
        }

        public string SecurityType
        {
            get { return _securityType; }
        }

        public UniqueIdentifier UniqueId
        {
            get { return _uniqueId; }
        }

        public IdentifierBundle Identifiers
        {
            get { return _identifiers; }
        }

        public override string ToString()
        {
            return SecurityType + ": " + Name;
        }
    }
}