//-----------------------------------------------------------------------
// <copyright file="ISecuritySource.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Model.Resources
{
    public interface ISecuritySource
    {
        ISecurity GetSecurity(UniqueId uid);
        ICollection<ISecurity> GetSecurities(ExternalIdBundle bundle);
        ISecurity GetSecurity(ExternalIdBundle bundle);
    }
}