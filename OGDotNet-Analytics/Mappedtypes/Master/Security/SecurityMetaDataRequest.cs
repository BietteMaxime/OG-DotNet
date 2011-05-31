//-----------------------------------------------------------------------
// <copyright file="SecurityMetaDataRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
namespace OGDotNet.Mappedtypes.Master.Security
{
    public class SecurityMetaDataRequest
    {
        private readonly bool _securityTypes;

        public SecurityMetaDataRequest() : this(true)
        {
        }

        public SecurityMetaDataRequest(bool securityTypes)
        {
            _securityTypes = securityTypes;
        }

        public bool SecurityTypes
        {
            get { return _securityTypes; }
        }
    }
}