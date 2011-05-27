//-----------------------------------------------------------------------
// <copyright file="LoggingClassBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using Castle.Core.Logging;

namespace OGDotNet.Utils
{
    public class LoggingClassBase
    {
        private ILogger _globalLogger = NullLogger.Instance;
        private ILogger _logger = NullLogger.Instance;

        protected ILogger Logger { get { return _logger; } }

        public ILogger GlobalLogger
        {
            protected get
            {
                return _globalLogger;
            }
            set
            {
                _globalLogger = value;
                _logger = _globalLogger.CreateChildLogger(GetType().FullName);
            }
        }
    }
}
