namespace OGDotNet_Analytics.Mappedtypes.LiveData
{
    public class UserPrincipal
    {
        private readonly string _userName;
        private readonly string _ipAddress;

        public string UserName
        {
            get { return _userName; }
        }

        public string IpAddress
        {
            get { return _ipAddress; }
        }

        public UserPrincipal(string userName, string ipAddress)
        {
            _userName = userName;
            _ipAddress = ipAddress;
        }
    }
}