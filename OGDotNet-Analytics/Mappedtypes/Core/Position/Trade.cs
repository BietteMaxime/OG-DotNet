using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Position
{
    public abstract class Trade
    {
        //TODO: the rest of this interface
        public abstract UniqueIdentifier ParentPositionId { get; }
    }
}
