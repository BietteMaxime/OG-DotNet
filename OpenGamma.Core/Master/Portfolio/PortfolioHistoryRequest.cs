// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PortfolioHistoryRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Id;

namespace OpenGamma.Master.Portfolio
{
    public class PortfolioHistoryRequest : AbstractHistoryRequest
    {
        private readonly int _depth;

        public PortfolioHistoryRequest(ObjectId objectId, int depth = -1) : base(objectId)
        {
            _depth = depth;
        }

        public int Depth
        {
            get { return _depth; }
        }
    }
}
