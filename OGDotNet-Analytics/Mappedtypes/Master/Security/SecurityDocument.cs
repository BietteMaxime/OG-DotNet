using System;

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