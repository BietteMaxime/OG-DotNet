using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet_Analytics.Mappedtypes.engine.View
{
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

        public IEnumerable<ValueRequirement> SpecificRequirements
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
            return ffc.GetAllByName(fieldName).Select(deserializer.FromField<T>).ToList();
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}