// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteAvailableOutputs.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Engine.View.Helper;
using OpenGamma.Id;
using OpenGamma.Util;

namespace OpenGamma.Model.Resources
{
    public class RemoteAvailableOutputs
    {
        private readonly RestTarget _restTarget;

        private int _maxNodes = -1;
        private int _maxPositions = -1;

        public RemoteAvailableOutputs(RestTarget restTarget)
        {
            _restTarget = restTarget;
        }

        public int MaxNodes
        {
            get { return _maxNodes; }
            set { _maxNodes = value; }
        }

        public int MaxPositions
        {
            get { return _maxPositions; }
            set { _maxPositions = value; }
        }

        public IAvailableOutputs GetPortfolioOutputs(UniqueId portfolioId, string timeStamp = "now")
        {
            ArgumentChecker.NotNull(portfolioId, "portfolioId");
            ArgumentChecker.NotNull(timeStamp, "timeStamp");

            RestTarget target = _restTarget.Resolve("portfolio").Resolve(timeStamp);
            if (_maxNodes > 0)
                target = target.Resolve("nodes", _maxNodes.ToString());
            if (_maxPositions > 0)
                target = target.Resolve("positions", _maxPositions.ToString());
            target = target.Resolve(portfolioId.ToString());
            return target.Get<IAvailableOutputs>();
        }
    }
}
