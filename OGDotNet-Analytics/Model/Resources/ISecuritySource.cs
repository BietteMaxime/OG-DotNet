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
        ISecurity GetSecurity(UniqueIdentifier uid);
        ICollection<ISecurity> GetSecurities(IdentifierBundle bundle);
        ISecurity GetSecurity(IdentifierBundle bundle);
    }
}