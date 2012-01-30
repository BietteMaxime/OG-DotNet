//-----------------------------------------------------------------------
// <copyright file="SecuritySearchResult.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Util;

namespace OGDotNet.Mappedtypes.Master.Security
{
    /// <summary>
    /// Exists for fudge to find
    /// </summary>
    internal class SecuritySearchResult : SearchResult<SecurityDocument>
    {
        public SecuritySearchResult(Paging paging, IList<SecurityDocument> documents) : base(paging, documents)
        {
        }
    }
}
