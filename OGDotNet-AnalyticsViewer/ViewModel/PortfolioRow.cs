//-----------------------------------------------------------------------
// <copyright file="PortfolioRow.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.engine;

namespace OGDotNet.AnalyticsViewer.ViewModel
{
    public class PortfolioRow : DynamicRow
    {
        private readonly string _positionName;
        private readonly ComputationTargetSpecification _computationTargetSpecification;
        private readonly ISecurity _security;

        public PortfolioRow(string positionName, ISecurity security, ComputationTargetSpecification computationTargetSpecification)
        {
            _positionName = positionName;
            _computationTargetSpecification = computationTargetSpecification;
            _security = security;
        }

        public string PositionName
        {
            get
            {
                return _positionName;
            }
        }

        public ComputationTargetSpecification ComputationTargetSpecification
        {
            get { return _computationTargetSpecification; }
        }

        public ISecurity Security
        {
            get { return _security; }
        }
    }
}