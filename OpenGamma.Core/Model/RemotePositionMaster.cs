// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemotePositionMaster.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Financial;
using OpenGamma.Master.Position;
using OpenGamma.Model.Resources;

namespace OpenGamma.Model
{
    public class RemotePositionMaster : RemoteMaster<PositionDocument, PositionSearchRequest, PositionHistoryRequest>
    {
        public RemotePositionMaster(RestTarget restTarget)
            : base(restTarget, "positions", "positionSearches")
        {
        }
    }
}
