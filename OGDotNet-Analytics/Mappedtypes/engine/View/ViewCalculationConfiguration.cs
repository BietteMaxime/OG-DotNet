using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.engine.value;

namespace OGDotNet.Mappedtypes.engine.View
{
    public class ViewCalculationConfiguration
    {
        private readonly string _name;
        private readonly List<ValueRequirement> _specificRequirements;
        private readonly Dictionary<string, ValueProperties> _portfolioRequirementsBySecurityType;
        private readonly ValueProperties _defaultProperties;

        public ViewCalculationConfiguration(string name, List<ValueRequirement> specificRequirements, Dictionary<string, ValueProperties> portfolioRequirementsBySecurityType) : this(name,specificRequirements, portfolioRequirementsBySecurityType, new ValueProperties())
        {
            
        }
        public ViewCalculationConfiguration(string name, List<ValueRequirement> specificRequirements, Dictionary<string, ValueProperties> portfolioRequirementsBySecurityType, ValueProperties defaultProperties)
        {
            _name = name;
            _specificRequirements = specificRequirements;
            _portfolioRequirementsBySecurityType = portfolioRequirementsBySecurityType;
            _defaultProperties = defaultProperties;
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

        public ValueProperties DefaultProperties
        {
            get { return _defaultProperties; }
        }

        public static ViewCalculationConfiguration FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var name = ffc.GetValue<string>("name");

            List<ValueRequirement> specificRequirements = GetList<ValueRequirement>(ffc, "specificRequirement", deserializer);

            //TODO MAP deserializer by magic
            var portfolioRequirementsBySecurityType = new Dictionary<string, ValueProperties>();
            foreach (var portfolioReqField in ffc.GetAllByName("portfolioRequirementsBySecurityType"))
            {
                const string securitytypeKey = "securityType";

                var securityType = ((IFudgeFieldContainer) portfolioReqField.Value).GetValue<String>(securitytypeKey);
                var valueProperties = deserializer.FromField<ValueProperties>(portfolioReqField);
                portfolioRequirementsBySecurityType.Add(securityType, valueProperties.Filter(p => p.Key != securitytypeKey));
            }

            var defaultProperties = deserializer.FromField<ValueProperties>(ffc.GetByName("defaultProperties"));

            return new ViewCalculationConfiguration(name, specificRequirements, portfolioRequirementsBySecurityType, defaultProperties);
        }

        private static List<T> GetList<T>(IFudgeFieldContainer ffc, string fieldName, IFudgeDeserializer deserializer) where T : class
        {
            return ffc.GetAllByName(fieldName).Select(deserializer.FromField<T>).ToList();
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer calcConfigMsg, IFudgeSerializer s)
        {

            calcConfigMsg.Add("name", Name);
            foreach (var securityTypeRequirements in PortfolioRequirementsBySecurityType)
            {
                FudgeMsg securityTypeRequirementsMsg = new FudgeMsg(s.Context);
                securityTypeRequirementsMsg.Add("securityType", securityTypeRequirements.Key);
                foreach (var requirement in securityTypeRequirements.Value.Properties)
                {
                    foreach (var var in requirement.Value)
                    {
                        securityTypeRequirementsMsg.Add("portfolioRequirement", var);
                    }

                    // TODO put the value constraints into the message if they're specified
                }

                calcConfigMsg.Add("portfolioRequirementsBySecurityType", securityTypeRequirementsMsg);
            }

            var fudgeSerializer = new FudgeSerializer(s.Context);

            foreach (var specificRequirement in SpecificRequirements)
            {
                var sReqMsg = fudgeSerializer.SerializeToMsg(specificRequirement);
                calcConfigMsg.Add("specificRequirement", sReqMsg);

            }

            var defaultPropsMessage = fudgeSerializer.SerializeToMsg(DefaultProperties);
            calcConfigMsg.Add("defaultProperties", defaultPropsMessage);

            //TODO delta defn
            calcConfigMsg.Add("deltaDefinition", new FudgeMsg());
        }
    }
}