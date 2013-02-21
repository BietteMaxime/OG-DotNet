// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractHistoryRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Id;

namespace OpenGamma.Master
{
    public abstract class AbstractHistoryRequest
    {
        private readonly ObjectId _objectId;

        protected AbstractHistoryRequest(ObjectId objectId)
        {
            _objectId = objectId;
        }

        public ObjectId ObjectId
        {
            get { return _objectId; }
        }
    }
}
