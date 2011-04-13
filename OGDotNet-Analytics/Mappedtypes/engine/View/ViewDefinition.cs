//-----------------------------------------------------------------------
// <copyright file="ViewDefinition.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.LiveData;

namespace OGDotNet.Mappedtypes.engine.view
{
    [DebuggerDisplay("ViewDefinition {_name}")]
    public class ViewDefinition
    {
        private string _name;
        private readonly UniqueIdentifier _portfolioIdentifier;
        private readonly UserPrincipal _user;
        private readonly UniqueIdentifier _uniqueID;

        private readonly ResultModelDefinition _resultModelDefinition;

        private readonly long? _minDeltaCalcPeriod;
        private readonly long? _maxDeltaCalcPeriod;

        private readonly long? _minFullCalcPeriod;
        private readonly long? _maxFullCalcPeriod;
        private readonly Currency _defaultCurrency;

        private readonly Dictionary<string, ViewCalculationConfiguration> _calculationConfigurationsByName;
        

        public ViewDefinition(string name, ResultModelDefinition resultModelDefinition = null, UniqueIdentifier portfolioIdentifier = null, UserPrincipal user = null, Currency defaultCurrency = null, long? minDeltaCalcPeriod = null, long? maxDeltaCalcPeriod = null, long? minFullCalcPeriod = null, long? maxFullCalcPeriod = null, Dictionary<string, ViewCalculationConfiguration> calculationConfigurationsByName = null, UniqueIdentifier uniqueID = null)
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

        public UniqueIdentifier PortfolioIdentifier
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

        public long? MinDeltaCalcPeriod
        {
            get { return _minDeltaCalcPeriod; }
        }

        public long? MaxDeltaCalcPeriod
        {
            get { return _maxDeltaCalcPeriod; }
        }

        public long? MinFullCalcPeriod
        {
            get { return _minFullCalcPeriod; }
        }

        public long? MaxFullCalcPeriod
        {
            get { return _maxFullCalcPeriod; }
        }

        public Dictionary<string, ViewCalculationConfiguration> CalculationConfigurationsByName
        {
            get { return _calculationConfigurationsByName; }
        }

        public UniqueIdentifier UniqueID
        {
            get { return _uniqueID; }
        }

        public static ViewDefinition FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var name = ffc.GetValue<string>("name");
            var uniqueIdString = ffc.GetString("uniqueId");
            var uniqueId = uniqueIdString == null ? null : UniqueIdentifier.Parse(uniqueIdString);

            var resultModelDefinition = deserializer.FromField<ResultModelDefinition>(ffc.GetByName("resultModelDefinition"));
            var portfolioIdentifier =ffc.GetAllByName("identifier").Any()  ? UniqueIdentifier.Parse(ffc.GetValue<String>("identifier")) : null;
            var user = deserializer.FromField<UserPrincipal>(ffc.GetByName("user"));

            var currency = ffc.GetByName("currency")==null ? null : Currency.Create(ffc.GetValue<string>("currency"));

            
            var minDeltaCalcPeriod = ffc.GetLong("minDeltaCalcPeriod");
            var maxDeltaCalcPeriod = ffc.GetLong("maxDeltaCalcPeriod");

            var minFullCalcPeriod = ffc.GetLong("fullDeltaCalcPeriod");
            var maxFullCalcPeriod = ffc.GetLong("maxFullCalcPeriod");



            var calculationConfigurationsByName = ffc.GetAllByName("calculationConfiguration")
                                                    .Select(deserializer.FromField<ViewCalculationConfiguration>)
                                                    .ToDictionary(vcc => vcc.Name);

            return new ViewDefinition(name, resultModelDefinition, portfolioIdentifier, user, currency, minDeltaCalcPeriod, maxDeltaCalcPeriod, minFullCalcPeriod, maxFullCalcPeriod, calculationConfigurationsByName, uniqueId);
        }

        private static void WriteNullableLongField(IAppendingFudgeFieldContainer message, string name, long? value)
        {
            if (value.HasValue)
            {message.Add(name,value.Value);}
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer message, IFudgeSerializer s)
        {
            message.Add("name",Name);
            if (_uniqueID != null)
            {
                message.Add("uniqueId", _uniqueID.ToString());
            }
            s.WriteInline(message,"identifier", PortfolioIdentifier);
            s.WriteInline(message, "user", User);
            s.WriteInline(message, "resultModelDefinition", ResultModelDefinition);

            if (DefaultCurrency != null)
            {
                message.Add("currency",DefaultCurrency.ISOCode);
            }

            WriteNullableLongField(message, "minDeltaCalcPeriod", MinDeltaCalcPeriod);
            WriteNullableLongField(message, "maxDeltaCalcPeriod", MaxDeltaCalcPeriod);
            WriteNullableLongField(message, "fullDeltaCalcPeriod", MinFullCalcPeriod);
            WriteNullableLongField(message, "maxFullCalcPeriod", MinFullCalcPeriod);


            foreach (var calcConfig in CalculationConfigurationsByName.Values)
            {
                FudgeMsg calcConfigMsg = s.Context.NewMessage();

                calcConfig.ToFudgeMsg(calcConfigMsg, s);

                message.Add("calculationConfiguration",calcConfigMsg);
            }
            
        }

    }
}