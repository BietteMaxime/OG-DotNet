//-----------------------------------------------------------------------
// <copyright file="OpenGammaException.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace OGDotNet.Mappedtypes
{
    public class OpenGammaException : Exception
    {
        public OpenGammaException()
        {
        }

        public OpenGammaException(string message) : base(message)
        {
        }

        public OpenGammaException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
