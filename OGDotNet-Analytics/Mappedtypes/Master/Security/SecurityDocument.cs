//-----------------------------------------------------------------------
// <copyright file="SecurityDocument.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

namespace OGDotNet.Mappedtypes.Master.Security
{
    public class SecurityDocument
    {
        private readonly string _uniqueId;
        private readonly ManageableSecurity _security;
        public string UniqueId { get { return _uniqueId; } }
        public ManageableSecurity Security{ get { return _security; }}//TODO type this with proto replacement

        public SecurityDocument(string uniqueId, ManageableSecurity security)
        {
            _uniqueId = uniqueId;
            _security = security;
        }

        public override string ToString()
        {
            return Security.ToString();
        }
    }
}