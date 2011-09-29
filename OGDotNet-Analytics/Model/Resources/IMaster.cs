//-----------------------------------------------------------------------
// <copyright file="IMaster.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master;
using OGDotNet.Mappedtypes.Util;

namespace OGDotNet.Model.Resources
{
    public interface IMaster<T> where T : AbstractDocument
    {
        RemoteChangeManger ChangeManager { get; }
        T Get(UniqueId uniqueId);
        SearchResult<T> Search(string name, PagingRequest pagingRequest);
    }
}