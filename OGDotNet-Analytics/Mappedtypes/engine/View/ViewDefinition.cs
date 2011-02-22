using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.LiveData;

namespace OGDotNet.Mappedtypes.engine.View
{
    public class ViewDefinition
    {
        private readonly string _name;
        private readonly UniqueIdentifier _portfolioIdentifier;
        private readonly UserPrincipal _user;
        private readonly ResultModelDefinition _resultModelDefinition;
        private readonly Currency _currency;
        private readonly long _minDeltaCalcPeriod;
        private readonly long _maxDeltaCalcPeriod;
        private readonly long _minFullCalcPeriod;
        private readonly long _maxFullCalcPeriod;
        private readonly Dictionary<string, ViewCalculationConfiguration> _calculationConfigurationsByName;

        private ViewDefinition(string name, UniqueIdentifier portfolioIdentifier, UserPrincipal user, ResultModelDefinition resultModelDefinition, Currency currency, long minDeltaCalcPeriod, long maxDeltaCalcPeriod, long minFullCalcPeriod, long maxFullCalcPeriod, Dictionary<string, ViewCalculationConfiguration> calculationConfigurationsByName)
        {
            _name = name;
            _portfolioIdentifier = portfolioIdentifier;
            _user = user;
            _resultModelDefinition = resultModelDefinition;
            _currency = currency;
            _minDeltaCalcPeriod = minDeltaCalcPeriod;
            _maxDeltaCalcPeriod = maxDeltaCalcPeriod;
            _minFullCalcPeriod = minFullCalcPeriod;
            _maxFullCalcPeriod = maxFullCalcPeriod;
            _calculationConfigurationsByName = calculationConfigurationsByName;
        }

        public string Name
        {
            get { return _name; }
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

        public Currency Currency
        {
            get { return _currency; }
        }

        public long MinDeltaCalcPeriod
        {
            get { return _minDeltaCalcPeriod; }
        }

        public long MaxDeltaCalcPeriod
        {
            get { return _maxDeltaCalcPeriod; }
        }

        public long MinFullCalcPeriod
        {
            get { return _minFullCalcPeriod; }
        }

        public long MaxFullCalcPeriod
        {
            get { return _maxFullCalcPeriod; }
        }

        public Dictionary<string, ViewCalculationConfiguration> CalculationConfigurationsByName
        {
            get { return _calculationConfigurationsByName; }
        }

        public static ViewDefinition FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var name = ffc.GetValue<string>("name");
            var resultModelDefinition = deserializer.FromField<ResultModelDefinition>(ffc.GetByName("resultModelDefinition"));
            var portfolioIdentifier =ffc.GetAllByName("identifier").Any()  ? UniqueIdentifier.Parse(ffc.GetValue<String>("identifier")) : null;
            var user = deserializer.FromField<UserPrincipal>(ffc.GetByName("user"));

            var currency = Core.Common.Currency.Create(ffc.GetValue<string>("currency"));

            var minDeltaCalcPeriod = ffc.GetValue<long>("minDeltaCalcPeriod");
            var maxDeltaCalcPeriod = ffc.GetValue<long>("maxDeltaCalcPeriod");
            long minFullCalcPeriod = 0;
            if (ffc.GetMessage("minFullCalcPeriod") != null)
            {
                minFullCalcPeriod = ffc.GetValue<long>("minFullCalcPeriod");
            }

            var calculationConfigurationsByName = ffc.GetAllByName("calculationConfiguration")
                                                    .Select(deserializer.FromField<ViewCalculationConfiguration>)
                                                    .ToDictionary(vcc => vcc.Name);

            var maxFullCalcPeriod = ffc.GetValue<long>("maxFullCalcPeriod");
            return new ViewDefinition(name, portfolioIdentifier, user, resultModelDefinition, currency, minDeltaCalcPeriod, maxDeltaCalcPeriod, minFullCalcPeriod, maxFullCalcPeriod, calculationConfigurationsByName);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }

    }
}