using System;
using System.Windows.Data;

namespace OGDotNet.WPFUtils
{
    public static class BindingUtils
    {
        public static Binding GetIndexerBinding(string index)
        {
            //need to ^ escape some things http://msdn.microsoft.com/en-us/library/ms752300.aspx

            var safeIndex = index.Replace("^","^^").Replace("[", "^[").Replace("]","^]");
            return new Binding(String.Format(".[{0}]", safeIndex));
        }
    }
}
