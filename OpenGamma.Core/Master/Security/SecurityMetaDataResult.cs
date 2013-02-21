// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SecurityMetaDataResult.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace OpenGamma.Master.Security
{
    public class SecurityMetaDataResult
    {
        private readonly IList<string> _securityTypes;

        public SecurityMetaDataResult(IList<string> securityTypes)
        {
            _securityTypes = securityTypes;
        }

        public IList<string> SecurityTypes
        {
            get { return _securityTypes; }
        }
    }
}