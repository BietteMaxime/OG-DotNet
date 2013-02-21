// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewDefinition.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Engine.Value;
using OpenGamma.Id;
using OpenGamma.LiveData;
using OpenGamma.Util.Money;

namespace OpenGamma.Engine.View
{
    public class ViewDefinition
    {
        private readonly UniqueId _portfolioId;
        private readonly UserPrincipal _marketDataUser;

        private readonly ResultModelDefinition _resultModelDefinition;

        private readonly Currency _defaultCurrency;

        private readonly Dictionary<string, ViewCalculationConfiguration> _calculationConfigurationsByName;

        private UniqueId _uniqueId;

        public ViewDefinition(string name, ResultModelDefinition resultModelDefinition = null, UniqueId portfolioId = null, UserPrincipal user = null, Currency defaultCurrency = null, TimeSpan? minDeltaCalcPeriod = null, TimeSpan? maxDeltaCalcPeriod = null, TimeSpan? minFullCalcPeriod = null, TimeSpan? maxFullCalcPeriod = null, Dictionary<string, ViewCalculationConfiguration> calculationConfigurationsByName = null, UniqueId uniqueID = null)
        {
            Name = name;
            _uniqueId = uniqueID;
            _portfolioId = portfolioId;
            _marketDataUser = user ?? UserPrincipal.DefaultUser;
            _resultModelDefinition = resultModelDefinition ?? new ResultModelDefinition();
            _defaultCurrency = defaultCurrency;
            MinDeltaCalcPeriod = minDeltaCalcPeriod;
            MaxDeltaCalcPeriod = maxDeltaCalcPeriod;
            MinFullCalcPeriod = minFullCalcPeriod;
            MaxFullCalcPeriod = maxFullCalcPeriod;
            _calculationConfigurationsByName = calculationConfigurationsByName ?? new Dictionary<string, ViewCalculationConfiguration>();
        }

        public ViewDefinition(string name, UniqueId portfolioId, UserPrincipal marketDataUser)
        {
            Name = name;
            _portfolioId = portfolioId;
            _marketDataUser = marketDataUser ?? UserPrincipal.DefaultUser;
            _resultModelDefinition = new ResultModelDefinition();
            _calculationConfigurationsByName = new Dictionary<string, ViewCalculationConfiguration>();
        }

        public ViewDefinition(string name, UserPrincipal marketDataUser)
        {
            Name = name;
            _marketDataUser = marketDataUser ?? UserPrincipal.DefaultUser;
        }

        public string Name { get; set; }

        public UniqueId PortfolioId
        {
            get { return _portfolioId; }
        }

        public UserPrincipal MarketDataUser
        {
            get { return _marketDataUser; }
        }

        public ResultModelDefinition ResultModelDefinition
        {
            get { return _resultModelDefinition; }
        }

        public Currency DefaultCurrency
        {
            get { return _defaultCurrency; }
        }

        public TimeSpan? MinDeltaCalcPeriod { get; set; }

        public TimeSpan? MaxDeltaCalcPeriod { get; set; }

        public TimeSpan? MinFullCalcPeriod { get; set; }

        public TimeSpan? MaxFullCalcPeriod { get; set; }

        public Dictionary<string, ViewCalculationConfiguration> CalculationConfigurationsByName
        {
            get { return _calculationConfigurationsByName; }
        }

        public UniqueId UniqueId
        {
            get { return _uniqueId; }
            set { _uniqueId = value; }
        }

        public override string ToString()
        {
            return string.Format("[ViewDefinition {0}]", Name);
        }

        public void AddCalculationConfiguration(ViewCalculationConfiguration calcConfig)
        {
            _calculationConfigurationsByName.Add(calcConfig.Name, calcConfig);
        }

        public static ViewDefinition FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var name = ffc.GetValue<string>("name");
            string uniqueIdStr = ffc.GetString("uniqueId");
            UniqueId uniqueId = (uniqueIdStr != null)
                               ? UniqueId.Parse(uniqueIdStr)
                               : deserializer.FromField<UniqueId>(ffc.GetByName("uniqueId"));

            var resultModelDefinition = deserializer.FromField<ResultModelDefinition>(ffc.GetByName("resultModelDefinition"));

            UniqueId portfolioIdentifier = ValueRequirement.GetUniqueIdentifier(ffc, deserializer, "identifier");
            var user = deserializer.FromField<UserPrincipal>(ffc.GetByName("user"));

            var currency = ffc.GetValue<Currency>("currency");

            var minDeltaCalcPeriod = ReadNullableTimeSpanField(ffc, "minDeltaCalcPeriod");
            var maxDeltaCalcPeriod = ReadNullableTimeSpanField(ffc, "maxDeltaCalcPeriod");

            var minFullCalcPeriod = ReadNullableTimeSpanField(ffc, "fullDeltaCalcPeriod");
            var maxFullCalcPeriod = ReadNullableTimeSpanField(ffc, "maxFullCalcPeriod");

            var calculationConfigurationsByName = ffc.GetAllByName("calculationConfiguration")
                .Select(deserializer.FromField<ViewCalculationConfiguration>)
                .ToDictionary(vcc => vcc.Name);

            return new ViewDefinition(name, resultModelDefinition, portfolioIdentifier, user, currency, minDeltaCalcPeriod, maxDeltaCalcPeriod, minFullCalcPeriod, maxFullCalcPeriod, calculationConfigurationsByName, uniqueId);
        }

        private static TimeSpan? ReadNullableTimeSpanField(IFudgeFieldContainer ffc, string fieldName)
        {
            var deltaCalcMillis = ffc.GetLong(fieldName);
            return deltaCalcMillis.HasValue ? (TimeSpan?) TimeSpan.FromMilliseconds(deltaCalcMillis.Value) : null;
        }

        private static void WriteNullableTimeSpanField(IAppendingFudgeFieldContainer message, string name, TimeSpan? value)
        {
            if (value.HasValue)
            {
                message.Add(name, (long) value.Value.TotalMilliseconds);
            }
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer message, IFudgeSerializer s)
        {
            message.Add("name", Name);
            if (_uniqueId != null)
            {
                message.Add("uniqueId", _uniqueId.ToString());
            }

            s.WriteInline(message, "identifier", PortfolioId);
            s.WriteInline(message, "user", MarketDataUser);
            s.WriteInline(message, "resultModelDefinition", ResultModelDefinition);

            if (DefaultCurrency != null)
            {
                message.Add("currency", DefaultCurrency.ISOCode);
            }

            WriteNullableTimeSpanField(message, "minDeltaCalcPeriod", MinDeltaCalcPeriod);
            WriteNullableTimeSpanField(message, "maxDeltaCalcPeriod", MaxDeltaCalcPeriod);
            WriteNullableTimeSpanField(message, "fullDeltaCalcPeriod", MinFullCalcPeriod);
            WriteNullableTimeSpanField(message, "maxFullCalcPeriod", MaxFullCalcPeriod);

            foreach (var calcConfig in CalculationConfigurationsByName.Values)
            {
                FudgeMsg calcConfigMsg = s.Context.NewMessage();

                calcConfig.ToFudgeMsg(calcConfigMsg, s);

                message.Add("calculationConfiguration", calcConfigMsg);
            }
        }
    }
}