// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewPermissionException.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace OpenGamma.Engine.View.Permission
{
    public class ViewPermissionException : Exception
    {
        public ViewPermissionException(string message) : base(message)
        {
        }
    }
}
