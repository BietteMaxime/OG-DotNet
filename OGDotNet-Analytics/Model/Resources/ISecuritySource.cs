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
        Security GetSecurity(UniqueIdentifier uid);
        ICollection<Security> GetSecurities(IdentifierBundle bundle);
        Security GetSecurity(IdentifierBundle bundle);
    }
}