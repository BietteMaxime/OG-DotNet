using System;
using System.Text;
using System.Windows.Data;

namespace OGDotNet.WPFUtils
{
    public static class BindingUtils
    {
        public static Binding GetIndexerBinding(string index)
        {
            //need to ^ escape some things http://msdn.microsoft.com/en-us/library/ms752300.aspx
            //I could attempt to guess, but escaping everything is way easier
            
            var stringBuilder = new StringBuilder(new string('^', index.Length*2));
            
            for (int i = 0; i < index.Length; i += 1)
            {
                stringBuilder[i*2 +1 ] = index[i];
            }
            return new Binding(String.Format(".[{0}]", stringBuilder)){Mode = BindingMode.OneWay};
        }
    }
}
