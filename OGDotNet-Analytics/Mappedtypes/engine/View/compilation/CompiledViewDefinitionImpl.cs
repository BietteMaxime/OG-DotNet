//-----------------------------------------------------------------------
// <copyright file="CompiledViewDefinitionImpl.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Mappedtypes.engine.view;

namespace OGDotNet.Mappedtypes.engine.View.compilation
{
    public class CompiledViewDefinitionImpl : ICompiledViewDefinition
    {
        private readonly ViewDefinition _viewDefinition;
        private readonly IPortfolio _portfolio;
        private readonly Dictionary<String, ICompiledViewCalculationConfiguration> _compiledCalculationConfigurations;
        private readonly DateTimeOffset _latestValidity;
        private readonly DateTimeOffset _earliestValidity;

        public CompiledViewDefinitionImpl(ViewDefinition viewDefinition, IPortfolio portfolio, DateTimeOffset latestValidity, DateTimeOffset earliestValidity, Dictionary<string, ICompiledViewCalculationConfiguration> compiledCalculationConfigurations)
        {
            _viewDefinition = viewDefinition;
            _compiledCalculationConfigurations = compiledCalculationConfigurations;
            _earliestValidity = earliestValidity;
            _latestValidity = latestValidity;
            _portfolio = portfolio;
        }

        public ViewDefinition ViewDefinition
        {
            get { return _viewDefinition; }
        }

        public IPortfolio Portfolio
        {
            get { return _portfolio; }
        }

        public Dictionary<ValueRequirement, ValueSpecification> LiveDataRequirements
        {
            get { return _compiledCalculationConfigurations.Values.Select(c=>c.LiveDataRequirements).SelectMany(d =>d).ToDictionary(k=>k.Key, k=>k.Value); }
        }
        
        public DateTimeOffset EarliestValidity
        {
            get { return _earliestValidity; }
        }

        public DateTimeOffset LatestValidity
        {
            get { return _latestValidity; }
        }

        public static CompiledViewDefinitionImpl FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            ViewDefinition defn = deserializer.FromField<ViewDefinition>(ffc.GetByName("viewDefintion"));
            IPortfolio portfolio = deserializer.FromField<IPortfolio>(ffc.GetByName("portfolio"));
            DateTimeOffset latestValidity = ffc.GetValue<DateTimeOffset>("latestValidity");
            DateTimeOffset earliestValidity = ffc.GetValue<DateTimeOffset>("earliestValidity");

            var configs = ffc.GetAllByName("compiledCalculationConfigurations").Select(deserializer.FromField<CompiledViewCalculationConfigurationImpl>).ToDictionary(c => c.Name, c=>(ICompiledViewCalculationConfiguration) c);
            return new CompiledViewDefinitionImpl(defn, portfolio, latestValidity, earliestValidity, configs);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
