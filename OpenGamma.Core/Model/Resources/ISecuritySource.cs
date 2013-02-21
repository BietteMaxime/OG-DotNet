// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISecuritySource.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using OpenGamma.Core.Security;
using OpenGamma.Id;

namespace OpenGamma.Model.Resources
{
    public interface ISecuritySource
    {
        ISecurity GetSecurity(UniqueId uid);
        ICollection<ISecurity> GetSecurities(ExternalIdBundle bundle);
        ISecurity GetSecurity(ExternalIdBundle bundle);
    }
}