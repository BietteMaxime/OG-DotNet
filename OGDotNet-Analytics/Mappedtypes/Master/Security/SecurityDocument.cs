using System;

namespace OGDotNet.Mappedtypes.Master.Security
{
    [Serializable]
    public class SecurityDocument
    {
        public string UniqueId;
        public ManageableSecurity Security;

        public override string ToString()
        {
            return Security.ToString();
        }
    }
}