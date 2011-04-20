//-----------------------------------------------------------------------
// <copyright file="JavaException.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

namespace OGDotNet.Mappedtypes.engine.View.listener
{
    public class JavaException
    {
        private readonly string _type;
        private readonly string _message;

        public JavaException(string type, string message)
        {
            _type = type;
            _message = message;
        }

        public string Type
        {
            get { return _type; }
        }

        public string Message
        {
            get { return _message; }
        }

        public override string ToString()
        {
            return string.Format("[{0}: {1}]", Type, Message);
        }
    }
}