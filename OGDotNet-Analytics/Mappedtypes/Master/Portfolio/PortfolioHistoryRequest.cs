//-----------------------------------------------------------------------
// <copyright file="PortfolioHistoryRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Master.Portfolio
{
    public class PortfolioHistoryRequest
    {
        //TODO this, and base class
        private readonly ObjectId _objectId;

        public PortfolioHistoryRequest(ObjectId objectId)
        {
            _objectId = objectId;
        }

        public ObjectId ObjectId
        {
            get { return _objectId; }
        }
    }
}
