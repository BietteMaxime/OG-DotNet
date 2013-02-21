// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManageableSecurity.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Core.Security;
using OpenGamma.Id;

namespace OpenGamma.Master.Security
{
    /// <summary>
    /// TODO DOTNET-5: kill these with the .proto replacement
    /// </summary>
    public class ManageableSecurity : ISecurity
    {
        private readonly string _name;
        private readonly string _securityType;
        private readonly UniqueId _uniqueId;
        private readonly ExternalIdBundle _identifiers;

        protected ManageableSecurity(string name, string securityType, UniqueId uniqueId, ExternalIdBundle identifiers)
        {
            _name = name;
            _securityType = string.Intern(securityType); // Should be a small static set
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

        public UniqueId UniqueId
        {
            get { return _uniqueId; }
        }

        public ExternalIdBundle Identifiers
        {
            get { return _identifiers; }
        }

        public override string ToString()
        {
            return SecurityType + ": " + Name;
        }
    }
}