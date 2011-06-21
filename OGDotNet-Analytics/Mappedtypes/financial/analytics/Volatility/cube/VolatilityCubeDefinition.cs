//-----------------------------------------------------------------------
// <copyright file="VolatilityCubeDefinition.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.Util.Time;

namespace OGDotNet.Mappedtypes.financial.analytics.Volatility.cube
{
    public class VolatilityCubeDefinition
    {
        private readonly List<Tenor> _swapTenors;
        private readonly List<Tenor> _optionExpiries;
        private readonly List<double> _relativeStrikes;

        public VolatilityCubeDefinition(List<Tenor> swapTenors, List<Tenor> optionExpiries, List<double> relativeStrikes)
        {
            _swapTenors = swapTenors;
            _optionExpiries = optionExpiries;
            _relativeStrikes = relativeStrikes;
        }

        public List<Tenor> SwapTenors
        {
            get { return _swapTenors; }
        }

        public List<Tenor> OptionExpiries
        {
            get { return _optionExpiries; }
        }

        public List<double> RelativeStrikes
        {
            get { return _relativeStrikes; }
        }

        public IEnumerable<VolatilityPoint> AllPoints
        {
            get
            {
                foreach (var swapTenor in SwapTenors)
                {
                    foreach (var optionExpiry in OptionExpiries)
                    {
                        foreach (var relativeStrike in RelativeStrikes)
                        {
                            yield return new VolatilityPoint(swapTenor, optionExpiry, relativeStrike);
                        }
                    }
                }
            }
        }
        public static VolatilityCubeDefinition FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new VolatilityCubeDefinition(
                deserializer.FromField<List<string>>(ffc.GetByName("swapTenors")).Select(s => new Tenor(s)).ToList(),
                deserializer.FromField<List<string>>(ffc.GetByName("optionExpiries")).Select(s => new Tenor(s)).ToList(),
                ffc.GetMessage("relativeStrikes").Select(f => f.Value).Cast<double>().ToList()
                );
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
