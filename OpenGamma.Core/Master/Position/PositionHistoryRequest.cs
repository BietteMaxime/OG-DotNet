// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PositionHistoryRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Id;

namespace OpenGamma.Master.Position
{
    public class PositionHistoryRequest : AbstractHistoryRequest
    {
        public PositionHistoryRequest(ObjectId objectId)
            : base(objectId)
        {
        }
    }
}
