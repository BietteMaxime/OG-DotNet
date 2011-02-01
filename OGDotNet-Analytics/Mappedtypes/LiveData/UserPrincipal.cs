using System;

namespace OGDotNet_Analytics.Mappedtypes.LiveData
{
    [Serializable]
    public class UserPrincipal
    {
        private string userName;
        private string ipAddress;

        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        public string IpAddress
        {
            get { return ipAddress; }
            set { ipAddress = value; }
        }

        public UserPrincipal(string userName, string ipAddress)
        {
            UserName = userName;
            IpAddress = ipAddress;
        }
    }
}