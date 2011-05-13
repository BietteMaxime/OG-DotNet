//-----------------------------------------------------------------------
// <copyright file="DataNotFoundException.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------


namespace OGDotNet.Mappedtypes
{
    public class DataNotFoundException : OpenGammaException
    {
        public DataNotFoundException()
        {
        }

        public DataNotFoundException(string message) : base(message)
        {
        }
    }
}