using System;

namespace OGDotNet.Mappedtypes.engine.depGraph.DependencyGraph
{
    public enum ResultOutputMode
    {
        //TODO shouldOutputResult
        None,
        TerminalOutputs,
        All
    }

    public static class ResultOutputModeMethods
    {

        internal static ResultOutputMode Parse(string getValue)
        {
            ResultOutputMode ret;
            if (! Enum.TryParse(getValue.Replace("_",""), true, out ret))
            {
                throw new ArgumentException();
            }
            return ret;
        }
    }
}
