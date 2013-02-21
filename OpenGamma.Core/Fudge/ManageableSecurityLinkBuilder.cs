// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManageableSecurityLinkBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Fudge;
using OpenGamma.Core.Security;
using OpenGamma.Master;
using OpenGamma.Master.Security;

namespace OpenGamma.Fudge
{
    class ManageableSecurityLinkBuilder : AbstractLinkBuilder<ISecurity>
    {
        public ManageableSecurityLinkBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override AbstractLink<ISecurity> CreateLink()
        {
            return new ManageableSecurityLink();
        }
    }
}