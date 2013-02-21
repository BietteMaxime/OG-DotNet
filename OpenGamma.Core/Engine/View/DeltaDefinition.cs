// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeltaDefinition.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenGamma.Engine.View
{
    public class DeltaDefinition
    {
        private readonly IDeltaComparer<double> _numberComparer;

        public DeltaDefinition(IDeltaComparer<double> numberComparer)
        {
            _numberComparer = numberComparer;
        }

        public IDeltaComparer<double> NumberComparer
        {
            get { return _numberComparer; }
        }
    }
}
