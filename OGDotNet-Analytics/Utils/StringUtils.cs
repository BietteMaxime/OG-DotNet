namespace OGDotNet_Analytics.Utils
{
    internal static class StringUtils
    {
        public static string TrimToNull(string version)
        {
            switch (version)
            {
                case null:
                case "":
                    return null;
                default:
                    return version;
            }
        }
    }
}