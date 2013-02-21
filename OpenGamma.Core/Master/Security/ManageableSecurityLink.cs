// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManageableSecurityLink.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Fudge.Serialization;
using OpenGamma.Core.Security;
using OpenGamma.Fudge;
using OpenGamma.Id;

namespace OpenGamma.Master.Security
{
    [FudgeSurrogate(typeof(ManageableSecurityLinkBuilder))]
    public class ManageableSecurityLink : AbstractLink<ISecurity>
    {
        public static ManageableSecurityLink Create(ExternalIdBundle securityIdBundle)
        {
            return new ManageableSecurityLink {ExternalId = securityIdBundle};
        }
    }
}