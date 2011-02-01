using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;

using OGDotNet_Analytics.Mappedtypes.engine.Value;
using OGDotNet_Analytics.Mappedtypes.Id;
using OGDotNet_Analytics.Mappedtypes.LiveData;

namespace OGDotNet_Analytics.Mappedtypes.engine.View
{
    public class ViewDefinition
    {
        private readonly string _name;
        private readonly UniqueIdentifier _portfolioIdentifier;
        private readonly UserPrincipal _user;
        private readonly ResultModelDefinition _resultModelDefinition;
        private readonly string _currency;
        private readonly long _minDeltaCalcPeriod;
        private readonly long _maxDeltaCalcPeriod;
        private readonly long _minFullCalcPeriod;
        private readonly long _maxFullCalcPeriod;
        private readonly Dictionary<string, ViewCalculationConfiguration> _calculationConfigurationsByName;

        private ViewDefinition(string name, UniqueIdentifier portfolioIdentifier, UserPrincipal user, ResultModelDefinition resultModelDefinition, string currency, long minDeltaCalcPeriod, long maxDeltaCalcPeriod, long minFullCalcPeriod, long maxFullCalcPeriod, Dictionary<string, ViewCalculationConfiguration> calculationConfigurationsByName)
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

        public string Currency
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

            var currency= ffc.GetValue<string>("currency");

            var minDeltaCalcPeriod = ffc.GetValue<long>("minDeltaCalcPeriod");
            var maxDeltaCalcPeriod = ffc.GetValue<long>("maxDeltaCalcPeriod");
            long minFullCalcPeriod = 0;
            if (ffc.GetMessage("minFullCalcPeriod") != null)
            {
                minFullCalcPeriod = ffc.GetValue<long>("minFullCalcPeriod");
            }

            var calculationConfigurationsByName = new Dictionary<string, ViewCalculationConfiguration>();
            foreach (var fudgeField in ffc.GetAllByName("calculationConfiguration"))
            {
                var vcc = deserializer.FromField<ViewCalculationConfiguration>(fudgeField);
                calculationConfigurationsByName.Add(vcc.Name, vcc);
            }

            var maxFullCalcPeriod = ffc.GetValue<long>("maxFullCalcPeriod");
            return new ViewDefinition(name, portfolioIdentifier, user, resultModelDefinition, currency, minDeltaCalcPeriod, maxDeltaCalcPeriod, minFullCalcPeriod, maxFullCalcPeriod, calculationConfigurationsByName);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }

    }

    public class ViewCalculationConfiguration
    {
        private readonly string _name;
        private readonly List<ValueRequirement> _specificRequirements;
        private readonly Dictionary<string, ValueProperties> _portfolioRequirementsBySecurityType;


        private ViewCalculationConfiguration(string name, List<ValueRequirement> specificRequirements, Dictionary<string, ValueProperties> portfolioRequirementsBySecurityType)
        {
            _name = name;
            _specificRequirements = specificRequirements;
            _portfolioRequirementsBySecurityType = portfolioRequirementsBySecurityType;
        }

        public string Name
        {
            get { return _name; }
        }

        public List<ValueRequirement> SpecificRequirements
        {
            get { return _specificRequirements; }
        }

        public Dictionary<string, ValueProperties> PortfolioRequirementsBySecurityType
        {
            get { return _portfolioRequirementsBySecurityType; }
        }

        public static ViewCalculationConfiguration FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var name = ffc.GetValue<string>("name");

            List<ValueRequirement> specificRequirements = GetList<ValueRequirement>(ffc, "specificRequirement", deserializer);

            //TODO MAP deserializer by magic
            var portfolioRequirementsBySecurityType = new Dictionary<string, ValueProperties>();
            foreach (var portfolioReqField in ffc.GetAllByName("portfolioRequirementsBySecurityType"))
            {
                var securityType = ((IFudgeFieldContainer) portfolioReqField.Value).GetValue<String>("securityType");
                var valueProperties = deserializer.FromField<ValueProperties>(portfolioReqField);
                portfolioRequirementsBySecurityType.Add(securityType, valueProperties);
            }

            
            return new ViewCalculationConfiguration(name, specificRequirements, portfolioRequirementsBySecurityType);
        }

        private static List<T> GetList<T>(IFudgeFieldContainer ffc, string fieldName, IFudgeDeserializer deserializer) where T : class
        {
            var specificRequirements = new List<T>();
            foreach (var fudgeField in ffc.GetAllByName(fieldName))
            {
                var specificRequirement = deserializer.FromField<T>(fudgeField);
                specificRequirements .Add(specificRequirement);
            }
            return specificRequirements;
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }

    public class ValueProperties
    {
        private readonly Dictionary<string, HashSet<string>> _properties;

        private ValueProperties(Dictionary<string, HashSet<string>> properties)
        {
            _properties = properties;
        }

        public Dictionary<string, HashSet<string>> Properties//TODO kill this
        {
            get { return _properties; }
        }

        public static ValueProperties FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            //TODO this properly
            var properties = new Dictionary<string, HashSet<string>>();
            foreach (var field in ffc.GetAllByName("portfolioRequirement"))
            {
                HashSet<string> propertyValueSet;
                if (! properties.TryGetValue(field.Name, out propertyValueSet))
                {
                    propertyValueSet = new HashSet<string>();
                    properties[field.Name] = propertyValueSet;
                }
                propertyValueSet.Add((string)field.Value);
            }
            return new ValueProperties(properties);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }

    }

    public class Bob
    {
        public string SecurityType { get; set; }
        public IList<String> PortfolioRequirement { get; set; }
        //public IList<String> PortfolioRequirements { get; set; }
        //public ValueProperties ValueProperties { get; set; }
    }
    
    public class ValueRequirement
    {
        private readonly string _valueName;
        private readonly ComputationTargetType _computationTargetType;
        private readonly UniqueIdentifier _computationTargetIdentifier;

        public string ValueName { get { return _valueName; }}
        public ComputationTargetType ComputationTargetType { get { return _computationTargetType; } }
        public UniqueIdentifier ComputationTargetIdentifier { get { return _computationTargetIdentifier; } }

        public ValueRequirement(string valueName,string computationTargetType, string computationTargetIdentifier)
        {
            _valueName = valueName;
            if (!Enum.TryParse(computationTargetType, true, out _computationTargetType))
            {
                throw new ArgumentException();
            }
            _computationTargetIdentifier = UniqueIdentifier.Parse(computationTargetIdentifier);
        }

        public ComputationTargetSpecification GetTargetSpec()
        {
            return new ComputationTargetSpecification(ComputationTargetType, ComputationTargetIdentifier);
        }
        public ValueSpecification ToSpecification()
        {
            return new ValueSpecification(ValueName, GetTargetSpec());
        }

        public static ValueRequirement FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new ValueRequirement(ffc.GetValue<string>("valueName"), ffc.GetValue<string>("computationTargetType"), ffc.GetValue<string>("computationTargetIdentifier"));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}