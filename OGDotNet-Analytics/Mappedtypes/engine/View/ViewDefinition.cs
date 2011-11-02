//-----------------------------------------------------------------------
// <copyright file="ViewDefinition.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.LiveData;
using OGDotNet.Mappedtypes.Util.Money;

namespace OGDotNet.Mappedtypes.Engine.View
{
    public class ViewDefinition
    {
        private readonly UniqueId _portfolioIdentifier;
        private readonly UserPrincipal _user;

        private readonly ResultModelDefinition _resultModelDefinition;

        private readonly Currency _defaultCurrency;

        private readonly Dictionary<string, ViewCalculationConfiguration> _calculationConfigurationsByName;

        private UniqueId _uniqueID;

        private string _name;

        private TimeSpan? _minDeltaCalcPeriod;
        private TimeSpan? _maxDeltaCalcPeriod;

        private TimeSpan? _minFullCalcPeriod;
        private TimeSpan? _maxFullCalcPeriod;

        public ViewDefinition(string name, ResultModelDefinition resultModelDefinition = null, UniqueId portfolioIdentifier = null, UserPrincipal user = null, Currency defaultCurrency = null, TimeSpan? minDeltaCalcPeriod = null, TimeSpan? maxDeltaCalcPeriod = null, TimeSpan? minFullCalcPeriod = null, TimeSpan? maxFullCalcPeriod = null, Dictionary<string, ViewCalculationConfiguration> calculationConfigurationsByName = null, UniqueId uniqueID = null)
        {
            _name = name;
            _uniqueID = uniqueID;
            _portfolioIdentifier = portfolioIdentifier;
            _user = user ?? UserPrincipal.DefaultUser;
            _resultModelDefinition = resultModelDefinition ?? new ResultModelDefinition();
            _defaultCurrency = defaultCurrency;
            _minDeltaCalcPeriod = minDeltaCalcPeriod;
            _maxDeltaCalcPeriod = maxDeltaCalcPeriod;
            _minFullCalcPeriod = minFullCalcPeriod;
            _maxFullCalcPeriod = maxFullCalcPeriod;
            _calculationConfigurationsByName = calculationConfigurationsByName ?? new Dictionary<string, ViewCalculationConfiguration>();
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public UniqueId PortfolioIdentifier
        {
            get { return _portfolioIdentifier; }
        }

        public UserPrincipal User
        {
            get { return _user; }
        }

        public ResultModelDefinition ResultModelDefinition
        {
            get { return _resultModelDefinition; }
        }

        public Currency DefaultCurrency
        {
            get { return _defaultCurrency; }
        }

        public TimeSpan? MinDeltaCalcPeriod
        {
            get { return _minDeltaCalcPeriod; }
            set { _minDeltaCalcPeriod = value; }
        }

        public TimeSpan? MaxDeltaCalcPeriod
        {
            get { return _maxDeltaCalcPeriod; }
            set { _maxDeltaCalcPeriod = value; }
        }

        public TimeSpan? MinFullCalcPeriod
        {
            get { return _minFullCalcPeriod; }
            set { _minFullCalcPeriod = value; }
        }

        public TimeSpan? MaxFullCalcPeriod
        {
            get { return _maxFullCalcPeriod; }
            set { _maxFullCalcPeriod = value; }
        }

        public Dictionary<string, ViewCalculationConfiguration> CalculationConfigurationsByName
        {
            get { return _calculationConfigurationsByName; }
        }

        public UniqueId UniqueID
        {
            get { return _uniqueID; }
            set { _uniqueID = value; }
        }

        public override string ToString()
        {
            return string.Format("[ViewDefinition {0}]", Name);
        }

        public static ViewDefinition FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var name = ffc.GetValue<string>("name");
            var uniqueIdString = ffc.GetString("uniqueId");
            var uniqueId = uniqueIdString == null ? null : UniqueId.Parse(uniqueIdString);

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
            if (_uniqueID != null)
            {
                message.Add("uniqueId", _uniqueID.ToString());
            }
            s.WriteInline(message, "identifier", PortfolioIdentifier);
            s.WriteInline(message, "user", User);
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