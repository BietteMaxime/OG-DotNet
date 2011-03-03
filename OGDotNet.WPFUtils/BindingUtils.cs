using System.Windows;
using System.Windows.Data;

namespace OGDotNet.WPFUtils
{
    public static class BindingUtils
    {
        public static Binding GetIndexerBinding(string index)
        {
            return new Binding{Mode = BindingMode.OneWay, Path =  new PropertyPath(".[(0)]", index)};
        }
    }
}
