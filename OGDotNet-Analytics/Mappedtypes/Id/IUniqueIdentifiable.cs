using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OGDotNet.Mappedtypes.Id
{
    interface IUniqueIdentifiable
    {
        UniqueIdentifier UniqueId { get; }
    }
}
