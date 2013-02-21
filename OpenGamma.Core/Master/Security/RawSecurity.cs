// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RawSecurity.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Id;

namespace OpenGamma.Master.Security
{
    public class RawSecurity : ManageableSecurity
    {
        private readonly byte[] _rawData;

        public RawSecurity(string name, string securityType, UniqueId uniqueId, ExternalIdBundle identifiers, byte[] rawData) : base(name, securityType, uniqueId, identifiers)
        {
            _rawData = rawData;
        }

        public byte[] RawData
        {
            get { return _rawData; }
        }
    }
}
