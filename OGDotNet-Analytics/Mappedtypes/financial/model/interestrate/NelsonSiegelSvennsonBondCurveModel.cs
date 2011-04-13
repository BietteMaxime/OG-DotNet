//-----------------------------------------------------------------------
// <copyright file="NelsonSiegelSvennsonBondCurveModel.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace OGDotNet.Mappedtypes.financial.model.interestrate
{
    /// <summary>
    /// TODO: this
    /// I'm not sure if it even makes sense for this logic to be here.  The web front end does the interpolation server side...
    /// </summary>
    public class NelsonSiegelSvennsonBondCurveModel
    {
        private readonly double[] _parameters;

        public NelsonSiegelSvennsonBondCurveModel(double[] parameters)
        {
            _parameters = parameters;
        }

        public double Eval(double arg)
        {
            throw new NotImplementedException();
        }
    }
}