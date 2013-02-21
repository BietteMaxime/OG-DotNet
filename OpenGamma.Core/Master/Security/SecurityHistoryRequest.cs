// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SecurityHistoryRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Id;

namespace OpenGamma.Master.Security
{
    public class SecurityHistoryRequest : AbstractHistoryRequest
    {
        private readonly bool _fullDetail;

        public SecurityHistoryRequest(ObjectId objectId, bool fullDetail = true) : base(objectId)
        {
            _fullDetail = fullDetail;
        }

        public bool FullDetail
        {
            get { return _fullDetail; }
        }
    }
}
